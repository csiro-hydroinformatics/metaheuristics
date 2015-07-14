using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.Optimization;
using CSIRO.Metaheuristics.SystemConfigurations;
using CSIRO.Metaheuristics.Fitness;

namespace CSIRO.Metaheuristics.CandidateFactories
{
    public class BestOfSampling<TSysConfig> : ICandidateFactory<TSysConfig>, IHyperCubeOperationsFactory 
        where TSysConfig : IHyperCube<double>, ICloneableSystemConfiguration
    {
        private ICandidateFactory<TSysConfig> innerSamplingFactory;

        FitnessAssignedScores<double>[] candidates;
        private int nextIndex = 0;

        public BestOfSampling(ICandidateFactory<TSysConfig> innerSamplingFactory, int poolSize, int bestPoints, 
            IObjectiveEvaluator<TSysConfig> evaluator, IFitnessAssignment<double> fitAssignment = null)
        {
            if (fitAssignment == null)
                fitAssignment = new DefaultFitnessAssignment();
            if (poolSize < bestPoints)
                throw new ArgumentOutOfRangeException("poolSize", poolSize, String.Format("poolSize must be >= bestPoints({0})", bestPoints));
            this.HcFactory = (IHyperCubeOperationsFactory)innerSamplingFactory;
            this.innerSamplingFactory = innerSamplingFactory;
            var tmp = new IObjectiveScores[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                tmp[i] = evaluator.EvaluateScore(innerSamplingFactory.CreateRandomCandidate());
            }
            var points = fitAssignment.AssignFitness(tmp);
            Array.Sort(points);
            candidates = new FitnessAssignedScores<double>[bestPoints];
            for (int i = 0; i < bestPoints; i++)
            {
                candidates[i] = points[i];
            }
        }
        public TSysConfig CreateRandomCandidate()
        {
            if (nextIndex >= this.candidates.Length)
                throw new IndexOutOfRangeException("Exhausted the pool of best points this can generate");
            var result = (TSysConfig)candidates[nextIndex].Scores.GetSystemConfiguration();
            nextIndex++;
            return result;
        }
        private IHyperCubeOperationsFactory HcFactory;

        public IHyperCubeOperations CreateNew(IRandomNumberGeneratorFactory rng)
        {
            return HcFactory.CreateNew(rng);
        }
    }
}
