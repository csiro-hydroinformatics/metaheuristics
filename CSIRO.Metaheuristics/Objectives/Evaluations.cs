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
        public static IObjectiveScores[] EvaluateScores<T>(IClonableObjectiveEvaluator<T> evaluator, T[] population, Func<bool> isCancelled, ParallelOptions parallelOptions = null) where T : ISystemConfiguration
        {
            if(parallelOptions == null)
                parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = -1 };

            var procCount = System.Environment.ProcessorCount;
            if (!evaluator.SupportsThreadSafeCloning)
            {
                // Changes needed to solve https://github.com/jmp75/rClr/issues/5
                procCount = 1;
            }
            IObjectiveScores[] result = new IObjectiveScores[population.Length];

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

            if (evaluator.SupportsThreadSafeCloning)
            {
                Parallel.For (0, subPop.Length, parallelOptions, i => {
                    EvaluateScores (population, isCancelled, offsets, subPop, i, result, cloneEval [i]);
                });
                    
            } else {
                EvaluateScores (population, isCancelled, offsets, subPop, 0, result, cloneEval [0]);
            }

            return result;
        }

        static void EvaluateScores<T>(T[] population, Func<bool> isCancelled, int[] offsets, T[][] subPop, int i, IObjectiveScores[] result, IClonableObjectiveEvaluator<T> evaluator) where T : ISystemConfiguration
        {
            var offset = offsets [i];
            for (int j = 0; j < subPop [i].Length; j++) {
                if (!isCancelled ())
                    result [offset + j] = evaluator.EvaluateScore (population [offset + j]);
                else
                    result [offset + j] = null;
            }
        }
    }
}
