using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.Fitness;
using System.Threading.Tasks;
using System.Threading;
using CSIRO.Metaheuristics.Objectives;
using CSIRO.Metaheuristics.Logging;
using CSIRO.Metaheuristics.CandidateFactories;

namespace CSIRO.Metaheuristics.Optimization
{
    public class UniformRandomSampling<T> : IEvolutionEngine<T> where T : IHyperCube<double>, ICloneableSystemConfiguration 
    {
        public UniformRandomSampling( IClonableObjectiveEvaluator<T> evaluator, 
                                     IRandomNumberGeneratorFactory rng,
                                     T template,
                                     int numShuffle, IDictionary<string, string> tags = null)
        {
            this.evaluator = evaluator;
            this.populationInitializer = new UniformRandomSamplingFactory<T> (rng, template);
            this.numShuffle = numShuffle;
            this.logTags = tags;
        }

        public UniformRandomSampling( IClonableObjectiveEvaluator<T> evaluator, 
                                     ICandidateFactory<T> populationInializer,
                                     int numShuffle, IDictionary<string, string> tags = null)
        {
            this.evaluator = evaluator;
            this.populationInitializer = populationInializer;
            this.numShuffle = numShuffle;
            this.logTags = tags;
        }

        IClonableObjectiveEvaluator<T> evaluator;
        ICandidateFactory<T> populationInitializer;
        public ILoggerMh Logger { get; set; }


        private readonly int numShuffle = 3000;
        private bool isCancelled = false;
        private IDictionary<string, string> logTags;

        public IOptimizationResults<T> Evolve( )
        {
            IObjectiveScores[] scores = evaluateScores(evaluator, initialisePopulation());
            var tags = LoggerMhHelper.CreateTag(LoggerMhHelper.MkTuple("Category", "URS"));
            loggerWrite(scores, tags);
            IObjectiveScores[] paretoScores = ParetoRanking<IObjectiveScores>.GetParetoFront(scores);
            return new BasicOptimizationResults<T>(paretoScores);
        }

        private void loggerWrite(IObjectiveScores[] scores, IDictionary<string, string> tags)
        {
            if ( logTags != null )
                tags = LoggerMhHelper.MergeDictionaries(logTags, tags);
            LoggerMhHelper.Write(scores, tags, Logger);
        }

        public string GetDescription( )
        {
            throw new NotImplementedException( );
        }

        
        private IObjectiveScores[] evaluateScores( IObjectiveEvaluator<T> evaluator, T[] population )
        {
            IObjectiveScores[] tmp = new IObjectiveScores[population.Count()];
            for (int i = 0; i < tmp.Length; i++)
            {
                if (!isCancelled)
                {
                    tmp[i] = evaluator.EvaluateScore(population[i]);
                }
                else
                {
                    tmp[i] = tmp[i - 1];
                }
            }
            return tmp;
        }

        private T[] initialisePopulation( )
        {
            T[] result = new T[numShuffle];
            for( int i = 0; i < result.Length; i++ )
                result[i] = populationInitializer.CreateRandomCandidate( );
            return result;
        }


        public int CurrentGeneration
        {
            get { throw new NotImplementedException(); }
        }

        public void Cancel()
        {
            isCancelled = true;
        }
    }
}
