using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.DataModel;

namespace CSIRO.Metaheuristics.Logging
{
    public static class LoggerMhHelper
    {
        public  static void Write(FitnessAssignedScores<double> scores, IDictionary<string, string> tags, ILoggerMh logger)
        {
            if (logger != null)
                logger.Write(scores, tags);
        }

        public  static void Write(IObjectiveScores[] scores, IDictionary<string, string> tags, ILoggerMh logger)
        {
            if (logger != null)
                logger.Write(scores, tags);
        }

        public  static void Write(string infoMsg, IDictionary<string, string> tags, ILoggerMh logger)
        {
            if (logger != null)
                logger.Write(infoMsg, tags);
        }

        public  static IDictionary<string, string> MergeDictionaries(params IDictionary<string, string>[] dicts)
        {
            var d = dicts[0].AsEnumerable();
            for (int i = 1; i < dicts.Length; i++)
            {
                d = d.Union(dicts[i]);
            }
            return d.ToDictionary(x => x.Key, y => y.Value);
        }

        public  static IDictionary<string, string> CreateTag(params Tuple<string, string>[] tuples)
        {
            return tuples.ToDictionary((x => x.Item1), (y => y.Item2));
        }

        public  static Tuple<string, string> MkTuple(string key, string value)
        {
            return Tuple.Create(key, value);
        }

        public static IObjectiveScores[] CreateNoScore<TSysConfig>(TSysConfig newPoint) where TSysConfig : ISystemConfiguration
        {
            return new IObjectiveScores[] { new ZeroScores<TSysConfig>() { SystemConfiguration = newPoint } };
        }

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

        /// <summary>
        /// Extract information from a logging object to a format using only classes from the Base Class Library
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="resultsName"></param>
        /// <returns>A tuple - the first item is a list of strings, the unique names (column headers) and the second item is a list of dictionaries (the information for each line)</returns>
        public static Tuple<List<string>, List<Dictionary<string, string>>> ExtractLog(this IEnumerable<ILogInfo> logger, string resultsName = "")
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

        public static void ToColumns(this IEnumerable<ILogInfo> logger, out Dictionary<string,string[]> strInfo, out Dictionary<string,double[]> numericInfo)
        {
            var dataLog = ExtractLog(logger);
            var keys = dataLog.Item1.ToArray();
            var lines = dataLog.Item2.ToArray();

            var list = logger.ToList();
            var firstScoreEntry = list.First(x => x.Scores.Length > 0);
            var firstScore = firstScoreEntry.Scores[0];
            var hc = firstScore.GetSystemConfiguration() as IHyperCube<double>;
            if (hc == null)
                throw new NotSupportedException("The first item in the log must be including information on an IHyperCube<double>");
            var numericColumnsList = new List<string>(hc.GetVariableNames());
            for (int i = 0; i < firstScore.ObjectiveCount; i++)
                numericColumnsList.Add(firstScore.GetObjective(i).Name);
            var numericColumns = numericColumnsList.ToArray();
            var stringColumns = keys.Where(x => !numericColumns.Contains(x)).ToArray();


            strInfo = new Dictionary<string, string[]>();
            for (int i = 0; i < stringColumns.Length; i++)
                strInfo.Add(stringColumns[i], new string[lines.Length]);
            numericInfo = new Dictionary<string, double[]>();
            for (int i = 0; i < numericColumns.Length; i++)
                numericInfo.Add(numericColumns[i], new double[lines.Length]);

            for (int i = 0; i < lines.Length; i++)
            {
                foreach (var p in numericInfo.Keys)
                    numericInfo[p][i] = double.NaN;
                foreach (var k in lines[i].Keys)
                {
                    var value = lines[i][k];
                    if (strInfo.ContainsKey(k))
                        strInfo[k][i] = value;
                    if (numericInfo.ContainsKey(k))
                    {
                        double d = 0;
                        numericInfo[k][i] = Double.TryParse(value, out d) ? d : double.NaN;
                    }
                }
            }
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
            public DictLineInfo(IDictionary<string, string> line) { this.line = new Dictionary<string, string>(line); }
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


        private class ZeroScores<TSysConfig> : IObjectiveScores<TSysConfig>
            where TSysConfig : ISystemConfiguration
        {
            public TSysConfig SystemConfiguration { get; set; }

            public IObjectiveScore GetObjective(int i)
            {
                throw new IndexOutOfRangeException();
            }

            public ISystemConfiguration GetSystemConfiguration()
            {
                return this.SystemConfiguration;
            }

            public int ObjectiveCount
            {
                get { return 0; }
            }
        }


    }
}
