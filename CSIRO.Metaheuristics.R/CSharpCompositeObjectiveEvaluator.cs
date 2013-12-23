using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics;
using CSIRO.Metaheuristics.Parallel.Objectives;

namespace CSIRO.Metaheuristics.R
{
    // A drop-in replacement for the R composite objective function evaluator, as a back up plan if problems persist running on the CentOS raijin from NCI.
    public class CSharpCompositeObjectiveEvaluator<TSysConfig> : CompositeObjectiveCalculation<TSysConfig>
        where TSysConfig : ISystemConfiguration
    {
        protected override bool IsMaximisable
        {
            get { return true; }
        }

        protected override string ObjectiveName
        {
            get { return "NSE(day-boxcox/mth) w Bias"; }
        }

        protected override string[] VariableNames
        {
            get { return new[] { "nse_bc", "nse_month", "bias" }; }
        }

        // mean(quantile((nse_bc+nse_month)/2 - 5 * abs(log(1 + bias))^2.5, probs = c(0.25, 0.50, 0.75, 1.0), na.rm = TRUE, names = FALSE, type = 5))
        protected override double calculateComposite(double[][] objValues)
        {
            double[] obj = new double[objValues[0].Length];
            for (int i = 0; i < obj.Length; i++)
            {
                //(nse_bc+nse_month)/2 - 5 * abs(log(1 + bias))^2.5
                obj[i] = (objValues[0][i] + objValues[1][i]) / 2 + Math.Pow(Math.Abs(Math.Log(1 + objValues[2][i])), 2.5);
            }
            var percentiles = new[] { 0.25, 0.50, 0.75, 1.0 };

            // TODO: the following is a DRAFT
            // IT IS NOT YET REPLICATING THE INTERPOLATION 'TYPE=5' IN THE R QUANTILE FUNCTION
            var result = Array.ConvertAll(percentiles, x => PercentilesCalculationsHelper.BasicPercentilesInterpolated(x, obj));
            return result.Average();
        }

    }

    internal class PercentilesCalculationsHelper
    {
        public static double BasicPercentilesInterpolated(double percentile, double[] percValues)
        {

            if (percValues.Length < 2)
                throw new ArgumentException("Data must be of length more than one for this composite to work");

            Array.Sort(percValues);

            var ind = percentile * (percValues.Length - 1);
            var iprev = (int)Math.Floor(ind);
            var inext = (int)Math.Ceiling(ind);
            var deltax = 1.0 / (percValues.Length - 1);
            var x_zero = iprev * deltax;
            var result = percValues[iprev] + (percentile - x_zero)/deltax * (percValues[inext] - percValues[iprev]);
            return result;
        }
    }
}
