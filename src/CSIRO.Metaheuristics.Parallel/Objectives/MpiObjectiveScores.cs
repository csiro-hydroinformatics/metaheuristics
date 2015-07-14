using System;
using System.Text;
using CSIRO.Metaheuristics.Parallel.SystemConfigurations;

namespace CSIRO.Metaheuristics.Parallel.Objectives
{
    /// <summary>
    /// A serializable implementation of IObjectiveScore, for use in Message Passing.
    /// </summary>
    [Serializable]
    public struct MpiObjectiveScore : IObjectiveScore
    {
        public string text;
        public bool maximise;
        public string name;
        public double value;

        public string GetText()
        {
            return text;
        }

        public bool Maximise
        {
            get { return maximise; }
        }

        public string Name
        {
            get { return name; }
        }

        public IComparable ValueComparable
        {
            get { return value; }
        }

        public override string ToString()
        {
            return Name + ":" + ValueComparable.ToString();
        }
    }

    /// <summary>
    /// A serializable implementation of IObjectiveScores, for use in Message Passing.
    /// </summary>
    [Serializable]
    public struct MpiObjectiveScores : IObjectiveScores<MpiSysConfig>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MpiObjectiveScores"/> struct
        /// from an IObjectiveScore array.
        /// </summary>
        /// <param name="scores">The scores.</param>
        /// <param name="sysConfig">The sys config.</param>
        /// <param name="catchmentId">The catchment id.</param>
        public MpiObjectiveScores(IObjectiveScore[] scores, MpiSysConfig sysConfig, string catchmentId = "")
        {
            config = sysConfig;
            this.CatchmentId = catchmentId;
            this.scores = new MpiObjectiveScore[scores.Length];
            for (int i = 0; i < scores.Length; i++)
            {
                var score = scores[i];
                this.scores[i] = new MpiObjectiveScore
                    {
                        name = score.Name,
                        maximise = score.Maximise,
                        text = score.GetText(),
                        value = (double)score.ValueComparable
                    };
            }
        }

        public MpiObjectiveScores(IObjectiveScores<MpiSysConfig> scores, string catchmentId = "")
        {
            this.config = scores.SystemConfiguration;
            this.CatchmentId = catchmentId;
            this.scores = new MpiObjectiveScore[scores.ObjectiveCount];
            for (int i = 0; i < this.scores.Length; i++)
            {
                var score = scores.GetObjective(i);
                this.scores[i] = new MpiObjectiveScore
                {
                    name = score.Name,
                    maximise = score.Maximise,
                    text = score.GetText(),
                    value = (double)score.ValueComparable
                };
            }
        }

        public MpiObjectiveScore[] scores;
        public MpiSysConfig config;
        public string CatchmentId;

        public MpiSysConfig SystemConfiguration
        {
            get { return config; }
        }

        public IObjectiveScore GetObjective(int i)
        {
            return scores[i];
        }

        public ISystemConfiguration GetSystemConfiguration()
        {
            return config;
        }

        public int ObjectiveCount
        {
            get { return scores.Length; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            // if we have a catchment Id, prepend it to the string
            if (!String.IsNullOrEmpty(CatchmentId))
                sb.AppendFormat("Catchment {0}: ", CatchmentId);

            for (int i = 0; i < scores.Length; i++)
			{

                sb.Append(scores[i].ToString());
                sb.Append(", ");
			}
            sb.Append(SystemConfiguration.ToString());
            return sb.ToString();
        }
    }
}
