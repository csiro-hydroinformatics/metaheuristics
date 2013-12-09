using System;
using CSIRO.Metaheuristics.Parallel.SystemConfigurations;
using CSIRO.Metaheuristics.Objectives;
using System.Reflection;
using System.IO;
using MPI;

namespace CSIRO.Metaheuristics.Parallel.Objectives
{
    /// <summary>
    /// An objective evaluator for the 'master' MPI process, 
    /// that gathers all the individual scores from the 'slaves' to calculate one or more "global" scores
    /// </summary>
    public class MpiObjectiveEvaluator : IClonableObjectiveEvaluator<MpiSysConfig>
    {
        static MpiObjectiveEvaluator()
        {
            log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public MpiObjectiveEvaluator()
        {
#if DEBUG_LAUNCH
            Debugger.Launch();
#endif
            if (!log.IsDebugEnabled && Assembly.GetEntryAssembly() != null)
            {
                var file = new FileInfo(Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().EscapedCodeBase
                    .Replace(@"file:///", string.Empty)
                    .Replace(@"file:", string.Empty)
                    ),
                    "MpiObjectiveEvaluator.log4net"));
                if (file.Exists)
                {
                    log4net.Config.XmlConfigurator.Configure(file);
                    log.Debug("Configured the log with " + file.FullName);
                }
            }
        }

        public static readonly log4net.ILog log;

        Intracommunicator comm = Communicator.world;
        public IObjectiveScores<MpiSysConfig> EvaluateScore(MpiSysConfig systemConfiguration)
        {
            int rank = comm.Rank;
            if (rank == 0)
            {
                for (int i = 1; i < comm.Size; i++)
                {
                    comm.Send(systemConfiguration, i, Convert.ToInt32(MpiMessageTags.SystemConfigurationMsgTag));
                }
                if (log.IsDebugEnabled)
                    log.Info("Process " + comm.Rank + " has sent the sys configs to evaluate");
                IObjectiveScores[] allscores = new IObjectiveScores[comm.Size - 1];
                IObjectiveScores objscore = null;
                if (log.IsDebugEnabled)
                    log.Info("Process " + comm.Rank + " waiting to receive results from all slave processes");
                for (int i = 1; i < comm.Size; i++)
                {
                    comm.Receive(i, Convert.ToInt32(MpiMessageTags.EvalSlaveResultMsgTag), out objscore);
                    allscores[i - 1] = objscore;
                }
                if (log.IsDebugEnabled)
                    log.Info("Process " + comm.Rank + " has received all the scores evaluated ");
                var result = CalculateCompositeObjectives(allscores, systemConfiguration);
                return result;
            }
            else
            {
                throw new NotSupportedException("MpiObjectiveEvaluator is designed to work with MPI process rank 0 only");
            }
        }

        protected virtual IObjectiveScores<MpiSysConfig> CalculateCompositeObjectives(IObjectiveScores[] allscores, MpiSysConfig sysConfig)
        {
            if (!Array.TrueForAll(allscores, (x => x.ObjectiveCount == 1)))
                throw new ArgumentException("Only single objective is supported");
            bool maximise = true;
            int count = 0;
            double sum = 0;
            double mean;
            for (int i = 0; i < allscores.Length; i++)
            {
                if (allscores[i] != null)
                {
                    count++;
                    sum += (double)allscores[i].GetObjective(0).ValueComparable;
                }
            }
            if (count == 0)
                throw new ArgumentException("no scores found in the array to reduce");
            mean = sum / count;
            return new MultipleScores<MpiSysConfig>(new IObjectiveScore[] { new DoubleObjectiveScore("Arithmetic mean", mean, maximise) }, sysConfig);
        }

        public IClonableObjectiveEvaluator<MpiSysConfig> Clone()
        {
            throw new NotSupportedException();
        }

        public bool SupportsDeepCloning
        {
            get { return false; }
        }

        public bool SupportsThreadSafeCloning
        {
            get { return false; }
        }
    }
}
