using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CSIRO.Metaheuristics.Optimization
{
    public class CombinedSCEwithRosenbrock<T, U> : IEvolutionEngine<T>
        where T : IHyperCube<U>, ICloneableSystemConfiguration
        where U : struct, IComparable, IConvertible
    {
        private ShuffledComplexEvolution<T> shuffledCE;
        private RosenbrockOptimizer<T, U> rosenbrock;

        public CombinedSCEwithRosenbrock( ShuffledComplexEvolution<T> shuffledComplexEvolution, IClonableObjectiveEvaluator<T> evaluator, 
            ITerminationCondition<T> rosenTerminationCondition, object AlgabraProvider )
        {
            this.evaluator = evaluator;
            this.shuffledCE = shuffledComplexEvolution;
            this.rosenTerminationCondition = rosenTerminationCondition;
            this.rosenAlgabraProvider = AlgabraProvider;

        }

        private ITerminationCondition<T> rosenTerminationCondition;
        private IClonableObjectiveEvaluator<T> evaluator;
        private object rosenAlgabraProvider;

        public IOptimizationResults<T> Evolve()
        {
            //Run SCE
            shuffleFinished = false;
            cancelled = false;

            IOptimizationResults<T> Results = shuffledCE.Evolve();
            shuffleFinished = true;
            
            //Take the new MetaParameterSet and pass it to Rosenbrock
            if( cancelled )
                return Results;

            IList<IObjectiveScores> results = Results.ToList( );
            var startingPoint = (T) results.FirstOrDefault().GetSystemConfiguration();
            rosenbrock = new RosenbrockOptimizer<T, U>( evaluator, startingPoint,
                                                        rosenTerminationCondition )
                         { AlgebraProvider = (IAlgebraProvider) rosenAlgabraProvider };
            ////Run Rosenbrock
            try
            {
                 Results = rosenbrock.Evolve();
            }
            catch
            {
                
            }
                return Results;
        }

        public string GetDescription()
        {
            return "SCE Optimizer followed by Rosenbrock Optimizer";
        }

        private bool shuffleFinished = false;
        private bool cancelled = false;
        public void Cancel()
        {
            if( !shuffleFinished )
            {
                cancelled = true;
                shuffledCE.Cancel();
                return;
            }
            rosenbrock.Cancel( );
        }
    }
}
