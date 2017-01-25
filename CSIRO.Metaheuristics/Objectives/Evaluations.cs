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
            if (population.Length == 0)
                return new IObjectiveScores[0];
            if(parallelOptions == null)
                parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = -1 };

            var procCount = System.Environment.ProcessorCount;

            IObjectiveScores[] result;
            if (evaluator.SupportsThreadSafeCloning) {
                // There is presumably no point cloning 
                // the system more times than the max level of parallelism
                int nParallel=procCount;
                if (parallelOptions.MaxDegreeOfParallelism > 0)
                    nParallel = Math.Min(nParallel, parallelOptions.MaxDegreeOfParallelism);
                T[][] subPop = MetaheuristicsHelper.MakeBins(population, nParallel);
                var taskPkgs = new List<Tuple<T[],IClonableObjectiveEvaluator<T>>>();
                taskPkgs.Add(Tuple.Create(subPop[0], evaluator));
                for (int i = 1; i < subPop.Length; i++)
                    taskPkgs.Add(Tuple.Create(subPop[i], evaluator.Clone()));
                // Need to use Parallel.ForEach rather than Parallel.For to work around a Parallel.For 
                // oddity in Mono 3.12.1. Need an identity to iterate over...
                var ramp = new int[subPop.Length];
                // Map of index of subpopulations to indices in the variable result:
                //var offsets = new int[subPop.Length];
                var resultBins = new IObjectiveScores[ramp.Length][];
                for (int i = 1; i < ramp.Length; i++)
                    ramp[i] = i;
                Parallel.ForEach(ramp, parallelOptions, 
                    (i => {
                        resultBins[i] = EvaluateScoresSerial(taskPkgs[i], isCancelled);
                    } )
				);
                result = Gather(resultBins);
                    
			} else {
                result = new IObjectiveScores[population.Length];
				for (int i = 0; i < population.Length; i++) {
                    if (!isCancelled ())
                        result [i] = evaluator.EvaluateScore (population [i]);
                    else
                        result [i] = null;
				}
			}
            return result;
        }

        static IObjectiveScores[] Gather(IObjectiveScores[][] resultBins)
        {
            var result = new List<IObjectiveScores>();
            for (int i = 0; i < resultBins.Length; i++)
            {
                for (int j = 0; j < resultBins[i].Length; j++)
                {
                    result.Add(resultBins[i][j]);
                }
            }
            return result.ToArray();
        }

        static IObjectiveScores[] EvaluateScoresSerial<T>(Tuple<T[],IClonableObjectiveEvaluator<T>> task, Func<bool> isCancelled) where T : ISystemConfiguration
        {
            var pop = task.Item1;
            var eval = task.Item2;
            return EvaluateScoresSerial(pop, isCancelled, eval);
        }

        static IObjectiveScores[] EvaluateScoresSerial<T>(T[] population, Func<bool> isCancelled, 
            IClonableObjectiveEvaluator<T> evaluator) where T : ISystemConfiguration
        {
            IObjectiveScores[] result = new IObjectiveScores[population.Length];
            for (int j = 0; j < population.Length; j++) {
                if (!isCancelled ())
                    result [j] = evaluator.EvaluateScore (population [j]);
                else
                    result [j] = null;
            }
            return result;
        }
    }
}
