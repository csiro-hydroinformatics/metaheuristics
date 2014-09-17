using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSIRO.Metaheuristics.Utils;

namespace CSIRO.Metaheuristics.Objectives
{
    public static class Evaluations
    {
        public static IObjectiveScores[] EvaluateScores<T>(IClonableObjectiveEvaluator<T> evaluator, T[] population, Func<bool> isCancelled) where T : ISystemConfiguration
        {
            var procCount = System.Environment.ProcessorCount;
            if (!evaluator.SupportsThreadSafeCloning)
                procCount = 1;
            T[][] subPop = MetaheuristicsHelper.MakeBins(population, procCount);
            var offsets = new int[subPop.Length];
            IClonableObjectiveEvaluator<T>[] cloneEval = new IClonableObjectiveEvaluator<T>[subPop.Length];
            offsets[0] = 0;
            cloneEval[0] = evaluator;
            for (int i = 1; i < offsets.Length; i++)
            {
                offsets[i] = offsets[i - 1] + subPop[i - 1].Length;
                cloneEval[i] = evaluator.Clone();
            }

            IObjectiveScores[] result = new IObjectiveScores[population.Length];
            Parallel.For(0, subPop.Length, i =>
            {
                var offset = offsets[i];
                for (int j = 0; j < subPop[i].Length; j++)
                {
                    if (!isCancelled())
                        result[offset + j] = cloneEval[i].EvaluateScore(population[offset + j]);
                    else
                        result[offset + j] = null;
                }
            }

            );
            return result;
        }

    }
}
