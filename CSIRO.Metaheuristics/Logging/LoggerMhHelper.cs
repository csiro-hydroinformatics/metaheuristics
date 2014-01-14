using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Logging
{
    internal class LoggerMhHelper
    {
        internal static void Write(FitnessAssignedScores<double> scores, IDictionary<string, string> tags, ILoggerMh logger)
        {
            if (logger != null)
                logger.Write(scores, tags);
        }

        internal static void Write(IObjectiveScores[] scores, IDictionary<string, string> tags, ILoggerMh logger)
        {
            if (logger != null)
                logger.Write(scores, tags);
        }

        internal static void Write(string infoMsg, ILoggerMh logger)
        {
            if (logger != null)
                logger.Write(infoMsg);
        }

        internal static IDictionary<string, string> MergeDictionaries(params IDictionary<string, string>[] dicts)
        {
            var d = dicts[0].AsEnumerable();
            for (int i = 1; i < dicts.Length; i++)
            {
                d = d.Union(dicts[i]);
            }
            return d.ToDictionary(x => x.Key, y => y.Value);
        }

        internal static IDictionary<string, string> CreateTag(params Tuple<string, string>[] tuples)
        {
            return tuples.ToDictionary((x => x.Item1), (y => y.Item2));
        }

        internal static Tuple<string, string> MkTuple(string key, string value)
        {
            return Tuple.Create(key, value);
        }
    }
}
