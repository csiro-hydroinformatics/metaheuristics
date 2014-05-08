using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Logging
{
    public  class LoggerMhHelper
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
