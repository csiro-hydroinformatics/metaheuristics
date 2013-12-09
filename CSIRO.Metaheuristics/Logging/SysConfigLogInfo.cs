using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.Utils;

namespace CSIRO.Metaheuristics.Logging
{
    public class SysConfigLogInfo
    {
        public IDictionary<string, string> Tags = new Dictionary<string, string>();
        public object Scores;

        public SysConfigLogInfo(object scores, IDictionary<string,string> tags)
        {
            this.Tags = tags;
            this.Scores = scores;
        }

        public override string ToString()
        {
            string scoreString = string.Empty;
            if (Scores is IObjectiveScores[] && Scores!=null)
                scoresString((IObjectiveScores[]) Scores);

            if (scoreString != string.Empty)
                return scoreString;
            else
            {
                return string.Empty;
            }
        }

        public string scoresString(IObjectiveScores[] allScores)
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
    }
}
