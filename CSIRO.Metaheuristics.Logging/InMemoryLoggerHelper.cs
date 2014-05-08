using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics;
using CSIRO.Metaheuristics.DataModel;
using CSIRO.Metaheuristics.Logging;
using CSIRO.Metaheuristics.SystemConfigurations;

namespace CSIRO.Metaheuristics.Logging
{
    public static class InMemoryLoggerHelper
    {
        /// <summary>
        /// Serialise the logger to csv.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="outputCsvLogFile">The output CSV log file.</param>
        /// <param name="resultsName">Name of the results.</param>
        /// <param name="delimiter">The delimiter.</param>
        public static void CsvSerialise(this InMemoryLogger logger, string outputCsvLogFile, string resultsName, string delimiter = ",")
        {
            var logInfo = ExtractLog(logger, resultsName);
            WriteToCsv(outputCsvLogFile, logInfo.Item2, logInfo.Item1);
        }

        public static Tuple<List<string>, List<Dictionary<string, string>>> ExtractLog(this InMemoryLogger logger, string resultsName)
        {
            List<IResultsSetInfo> allResultsSets = new List<IResultsSetInfo>();
            var list = logger.ToList();
            foreach (var item in list)
            {
                IResultsSetInfo resultsSet = createResultsSet(item, resultsName);
                if (resultsSet != null)
                    allResultsSets.Add(resultsSet);
            }

            HashSet<string> uniqueKeys = new HashSet<string>();
            List<Dictionary<string, string>> lines = new List<Dictionary<string, string>>();
            foreach (IResultsSetInfo s in allResultsSets)
            {
                foreach (IKeyValueInfoProvider lineInfo in s)
                {
                    Dictionary<string, string> line = lineInfo.AsDictionary();
                    foreach (string key in line.Keys)
                        uniqueKeys.Add(key);

                    lines.Add(line);
                }
            }

            List<string> keys = uniqueKeys.ToList();
            keys.Sort();
            var logInfo = Tuple.Create(keys, lines);
            return logInfo;
        }

        /// <summary>
        /// Writes to CSV.
        /// With a bit more work, this could become an extension method to dictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">The filename.</param>
        /// <param name="lines">The lines.</param>
        /// <param name="header">The header.</param>
        /// <param name="delimiter">The delimiter.</param>
        private static void WriteToCsv<T>(string filename, IEnumerable<Dictionary<string, T>> lines, IList<string> header, string delimiter = ",")
        {
            using (TextWriter writer = new StreamWriter(filename))
            {
                // write header
                for (int i = 0; i < header.Count; i++)
                {
                    writer.Write("\"{0}\"{1}", header[i], (i < header.Count - 1) ? delimiter : "");
                }
                writer.WriteLine();

                // write each line, in the order specified by the header values
                foreach (var line in lines)
                {
                    for (int i = 0; i < header.Count; i++)
                    {
                        if (line.ContainsKey(header[i]))
                            writer.Write("\"{0}\"{1}", line[header[i]], (i < header.Count - 1) ? delimiter : "");
                        else
                            writer.Write((i < header.Count - 1) ? delimiter : "");
                    }
                    writer.WriteLine();
                }

                writer.Close();
            }
        }

        private class LineEntryCollection : IResultsSetInfo
        {
            protected List<IKeyValueInfoProvider> entries = new List<IKeyValueInfoProvider>();

            IEnumerator<IKeyValueInfoProvider> IEnumerable<IKeyValueInfoProvider>.GetEnumerator()
            {
                return entries.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return entries.GetEnumerator();
            }
        }

        private class ObjResultsInfo : LineEntryCollection
        {
            private ObjectivesResultsCollection result;

            public ObjResultsInfo(ObjectivesResultsCollection result)
            {
                this.result = result;

                foreach (var scores in result.ScoresSet)
                {
                    entries.Add(new ScoreCollectionInfo(scores, result.Tags));
                }
            }
        }

        private class DictLineInfo : IKeyValueInfoProvider
        {
            protected Dictionary<string, string> line;
            public DictLineInfo() { }
            public DictLineInfo(IDictionary<string, string> line) { this.line = new Dictionary<string,string>(line); }
            public Dictionary<string, string> AsDictionary()
            {
                return line;
            }
        }

        private class ScoreCollectionInfo : DictLineInfo
        {
            public ScoreCollectionInfo(ObjectiveScoreCollection scores, TagCollection tagCollection)
            {
                line = new Dictionary<string, string>();
                HyperCube hyperCubeScores = (scores.SysConfiguration) as HyperCube;
                if (hyperCubeScores != null)
                    foreach (var variable in hyperCubeScores.Variables)
                        line.Add(variable.Name, variable.Value.ToString());

                foreach (var score in scores.Scores)
                    line.Add(score.Name, score.Value);

                foreach (var tag in tagCollection.Tags)
                    line.Add(tag.Name, tag.Value);
            }
        }


        private class SingleLineInfo : LineEntryCollection
        {
            public SingleLineInfo(IDictionary<string, string> dictionary)
            {
                this.entries.Add(new DictLineInfo(dictionary));
            }
        }

        private static IResultsSetInfo createResultsSet(ILogInfo item, string resultsName = "")
        {
            IObjectiveScores[] arrayScores = item.Scores as IObjectiveScores[];
            if (arrayScores != null && arrayScores.Length > 0)
            {
                IDictionary<string, string> tags = item.Tags;
                var result = ConvertOptimizationResults.Convert(arrayScores, attributes: tags);
                result.Name = resultsName;
                return new ObjResultsInfo(result);
            }
            else
                return new SingleLineInfo(item.Tags);
        }
    }
}
