using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CSIRO.Metaheuristics.Objectives;
using CSIRO.Metaheuristics.Parallel.SystemConfigurations;

namespace CSIRO.Metaheuristics.Parallel.Objectives
{
    public abstract class CompositeObjectiveCalculation<TSysConfig> : IDisposable where TSysConfig : ISystemConfiguration
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected bool disposed;

        protected abstract bool IsMaximisable { get; }
        protected abstract string ObjectiveName { get; }
        protected abstract string[] VariableNames { get; }

        #region IDisposable Members

        /// <summary>
        ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        ///   Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"> <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources. </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here
                }
                // dispose unmanaged resources here
                // disposal is done. Set the flag so we don't get disposed more than once.
                disposed = true;
            }
        }

        public MpiObjectiveScores CalculateCatchmentObjective(MpiObjectiveScores[] gridCellScores, MpiSysConfig sysConfig)
        {
            throw new NotImplementedException("Todo: implement if we require an R function to calculate a catchment score from the gridded scores");
        }

        public IObjectiveScores<TSysConfig> CalculateCompositeObjective(IObjectiveScores[] allscores, TSysConfig sysConfig)
        {
            if (!Array.TrueForAll(allscores, (x => x.ObjectiveCount == VariableNames.Length)))
                throw new ArgumentException("Inconsistent length between at least one score set, and the number of variable names specified");

            var objValues = getByObjectiveVarname(allscores);

            var score = calculateComposite(objValues);


            if (double.IsNaN(score) && IsMaximisable)
                score = -999;
            Log.InfoFormat("Score: {0}", score);

            return
                new MultipleScores<TSysConfig>(
                    new IObjectiveScore[] { new DoubleObjectiveScore(ObjectiveName, score, maximise: IsMaximisable) }, sysConfig);
        }

        protected abstract double calculateComposite(double[][] objValues);

        protected double[][] getByObjectiveVarname(IObjectiveScores[] allscores)
        {
            var objValues = new double[VariableNames.Length][];
            for (int i = 0; i < VariableNames.Length; i++)
            {
                double[] array = Array.ConvertAll(allscores, (x => Convert.ToDouble(x.GetObjective(i).ValueComparable)));
                Log.Info(AssignVector(VariableNames[i], new List<double>(array)));
                objValues[i] = array;
            }
            return objValues;
        }

        private static string AssignVector(string variableName, IList<double> values)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(variableName);
            sb.Append(@" <- c(");
            sb.Append(ListToString(values));
            sb.Append(")");
            return sb.ToString();
        }

        private static string ListToString<T>(IList<T> values)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < values.Count; i++)
            {
                sb.Append(values[i]);
                if (i != (values.Count - 1))
                    sb.Append(",");
            }
            return sb.ToString();
        }
    }
}
