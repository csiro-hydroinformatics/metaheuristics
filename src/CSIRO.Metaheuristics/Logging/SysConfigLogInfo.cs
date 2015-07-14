using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.Utils;

namespace CSIRO.Metaheuristics.Logging
{
    public interface ILogInfo { 
        IObjectiveScores[] Scores { get; }
        IDictionary<string, string> Tags { get; }
    }

    public interface IResultsSetInfo : IEnumerable<IKeyValueInfoProvider>
    {
    }

    public interface IKeyValueInfoProvider
    {
        Dictionary<string, string> AsDictionary();
    }

    public class StringOnlyLogInfo : ILogInfo
    {
        private string message;

        public StringOnlyLogInfo(string message, IDictionary<string, string> tags)
        {
            this.message = message;
            this.Tags = tags;
        }
        public IObjectiveScores[] Scores
        {
            get { return new IObjectiveScores[0]; }
        }

        public IDictionary<string, string> Tags
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return message;
        }
    }

    public class SysConfigLogInfo : ILogInfo
    {
        public SysConfigLogInfo(IObjectiveScores[] scores, IDictionary<string, string> tags)
        {
            this.Tags = tags;
            this.Scores = scores;
        }

        public override string ToString()
        {
            if (Scores!=null)
                return ScoresString(Scores);
            return string.Empty;
        }

        public static string ScoresString(IObjectiveScores[] allScores)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < allScores.Length; i++)
            {
                sb.Append(allScores[i]);
                if (i != allScores.Length-1)
                    sb.Append(",");
            }
            return sb.ToString();
        }

        public IObjectiveScores[] Scores
        {
            get;
            private set;
        }

        public IDictionary<string, string> Tags
        {
            get;
            private set;
        }
    }
}
