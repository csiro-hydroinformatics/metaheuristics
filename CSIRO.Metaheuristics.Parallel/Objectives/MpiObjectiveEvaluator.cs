using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class MpiObjectiveEvaluator : IClonableObjectiveEvaluator<MpiSysConfig>, IDisposable
    {
        //private class ArithmeticMeanObjective : CompositeObjectiveEvaluator<MpiSysConfig>
        //{

        //    protected override bool IsMaximisable
        //    {
        //        get { throw new NotImplementedException(); }
        //    }

        //    protected override string ObjectiveName
        //    {
        //        get { throw new NotImplementedException(); }
        //    }

        //    protected override string[] VariableNames
        //    {
        //        get { throw new NotImplementedException(); }
        //    }

        //    protected override double calculateComposite(double[][] objValues)
        //    {
        //        if (objValues.Length == 0)
        //            throw new ArgumentException("no scores found in the array to reduce");
        //        if (objValues[0].Length > 1)
        //            throw new NotSupportedException("ArithmeticMeanObjective accepts only arrays of single objectives: e.g. one NSE per catchment. Found several objectives in the first score. You may need a custom CompositeObjectiveEvaluator");
        //        bool maximise = true;
        //        int count = 0;
        //        double sum = 0;
        //        double mean;
        //        for (int i = 0; i < objValues.Length; i++)
        //        {
        //            count++;
        //            sum += objValues[i][0];
        //        }
        //        mean = sum / count;
        //        return mean;
        //    }
        //}

        protected readonly CompositeObjectiveCalculation<MpiSysConfig> evaluator;

        public MpiObjectiveEvaluator(IEnsembleObjectiveEvaluator<MpiSysConfig> systemsEvaluator, CompositeObjectiveCalculation<MpiSysConfig> evaluator)
        {
            if (evaluator == null) throw new ArgumentNullException("evaluator", "The composite objective evaluator cannot be null");
            this.evaluator = evaluator;
            if (systemsEvaluator == null) throw new ArgumentNullException("systemsEvaluator", "systemsEvaluator cannot be null");
            this.systemsEvaluator = systemsEvaluator;
#if DEBUG_LAUNCH
            Debugger.Launch();
#endif
        }

        private readonly Stopwatch simulationTimer = new Stopwatch();
        protected readonly IEnsembleObjectiveEvaluator<MpiSysConfig> systemsEvaluator;

        protected class DefaultEnsembleMpiEvaluator : IEnsembleObjectiveEvaluator<MpiSysConfig>
        {
            public static readonly log4net.ILog log;

            public DefaultEnsembleMpiEvaluator()
            {
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

            static DefaultEnsembleMpiEvaluator()
            {
                log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            }

            Intracommunicator comm = Communicator.world;
            public IObjectiveScores<MpiSysConfig>[] EvaluateScore(MpiSysConfig systemConfiguration)
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
                    IObjectiveScores<MpiSysConfig>[] allscores = new IObjectiveScores<MpiSysConfig>[comm.Size - 1];
                    IObjectiveScores<MpiSysConfig> objscore = null;
                    if (log.IsDebugEnabled)
                        log.Info("Process " + comm.Rank + " waiting to receive results from all slave processes");
                    for (int i = 1; i < comm.Size; i++)
                    {
                        comm.Receive(i, Convert.ToInt32(MpiMessageTags.EvalSlaveResultMsgTag), out objscore);
                        allscores[i - 1] = objscore;
                    }
                    if (log.IsDebugEnabled)
                        log.Info("Process " + comm.Rank + " has received all the scores evaluated ");
                    return allscores;
                }
                else
                {
                    throw new NotSupportedException("MpiObjectiveEvaluator is designed to work with MPI process rank 0 only");
                }
            }
        }

        public TimeSpan SimulationTime
        {
            get { return simulationTimer.Elapsed; }
        }

        public int SimulationCount { get; private set; }

        public virtual IObjectiveScores<MpiSysConfig> EvaluateScore(MpiSysConfig systemConfiguration)
        {
            simulationTimer.Start();
            IObjectiveScores[] scores = new List<IObjectiveScores>(systemsEvaluator.EvaluateScore(systemConfiguration)).ToArray();
            simulationTimer.Stop();
            SimulationCount++;
            return evaluator.CalculateCompositeObjective(scores, systemConfiguration);
        }

        //protected virtual IObjectiveScores<MpiSysConfig> CalculateCompositeObjectives(IObjectiveScores[] allscores, MpiSysConfig sysConfig)
        //{
        //    return evaluator.CalculateCompositeObjective(allscores, sysConfig);
        //}

        public virtual IClonableObjectiveEvaluator<MpiSysConfig> Clone()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///   Gets whether this object returns a deep clone of itself and its properties.
        ///   This may vary through its lifetime.
        /// </summary>
        public virtual bool SupportsDeepCloning
        {
            get { return false; }
        }

        /// <summary>
        ///   Gets whether this object returns a clone deemed thread-safe, i.e.
        ///   for write access. This may vary through its lifetime.
        /// </summary>
        /// <example>
        ///   A TIME model runner may return a clone of itself with the same input time series,
        ///   but deep-copy the output time series recorded.
        /// </example>
        public virtual bool SupportsThreadSafeCloning
        {
            get { return false; }
        }

        public virtual void Dispose()
        {
            Dispose(true);
            // not sure whether the following is needed
            // GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (evaluator != null)
                {
                    evaluator.Dispose();
                }
            }
        }

    }
}
