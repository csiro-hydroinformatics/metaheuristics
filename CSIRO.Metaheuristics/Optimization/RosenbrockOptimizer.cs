using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using CSIRO.Metaheuristics.Objectives;
using CSIRO.Metaheuristics.Logging;

namespace CSIRO.Metaheuristics.Optimization
{
    public class RosenbrockOptimizer<T, U> : IEvolutionEngine<T>
        where T : IHyperCube<U>, ICloneableSystemConfiguration
        where U : struct, IComparable, IConvertible
    {
        public RosenbrockOptimizer( IObjectiveEvaluator<T> evaluator,
            T startingPoint,
            ITerminationCondition<T> terminationCondition,
            double alpha = 1.4,
            double beta = 0.7,
            IAlgebraProvider algebraprovider = null,
            IDictionary<string, string> logTags = null)
        {
            this.countingEvaluator = new CountingEvaluator(evaluator);
            this.startingPoint = evaluator.EvaluateScore( startingPoint );
            this.terminationCondition = terminationCondition;
            terminationCondition.SetEvolutionEngine( this );
            this.alpha = alpha;
            this.beta = beta;
            this.AlgebraProvider = algebraprovider;
            this.logTags = logTags;
        }
        CountingEvaluator countingEvaluator;
        IObjectiveScores<T> startingPoint;
        IObjectiveScores<T> currentPoint;
        ITerminationCondition<T> terminationCondition;

        public IAlgebraProvider AlgebraProvider
        {
            get;
            set;
        }

        public ILoggerMh Logger { get; set; }

        double alpha;
        double beta;
        double initialStep = 0.1;
        private bool isCancelled = false;
        private Stage stage;
        private IDictionary<string, string> logTags;

        public IOptimizationResults<T> Evolve( )
        {
            if (this.AlgebraProvider == null)
                throw new ArgumentNullException("The Rosenbrock optimizer must have its AlgebraProvider property not set to 'null'");
            currentPoint = startingPoint;
            IBase b = createNewBase( currentPoint );
            IVector stepsLength = b.CreateVector( );
            for( int i = 0; i < stepsLength.Length; i++ )
            {
                stepsLength[i] = initialStep;
            }
            while (!terminationCondition.IsFinished() && !isCancelled)
            {
                var endOfStage = performStage( b, currentPoint, stepsLength );
                b = endOfStage.Item1;
                currentPoint = endOfStage.Item2;
            }
            return new BasicOptimizationResults<T>( new IObjectiveScores<T>[] { currentPoint } );
        }

        public string GetDescription( )
        {
            return "Rosenbrock Search Optimizer";
        }

        public int CurrentGeneration
        {
            get { throw new NotImplementedException( ); }
        }

        public void Cancel( )
        {
            isCancelled = true;
            stage.Cancel();
        }

        private class CountingEvaluator
        {
            private IObjectiveEvaluator<T> evaluator;
            public IObjectiveScores<T> Evaluate( T sysConfig )
            {
                Counter++;
                return evaluator.EvaluateScore( sysConfig );
            }
            public int Counter { get; private set; }

            public CountingEvaluator( IObjectiveEvaluator<T> evaluator )
            {
                Counter = 0;
                this.evaluator = evaluator;
            }
        }

        public class RosenbrockOptimizerIterationTermination : ITerminationCondition<T>
        {
            private int maxIteration;

            public RosenbrockOptimizerIterationTermination( int maxIteration )
            {
                this.maxIteration = maxIteration;
            }

            #region ITerminationCondition<UnivariateReal> Members

            public void SetEvolutionEngine( IEvolutionEngine<T> engine )
            {
                this.rosen = engine as RosenbrockOptimizer<T, U>;
            }

            public bool IsFinished( )
            {
                return rosen.countingEvaluator.Counter >= maxIteration;
            }

            #endregion

            private RosenbrockOptimizer<T, U> rosen { get; set; }
        }


        private class Stage
        {
            public Stage( IBase b, IObjectiveScores<T> startingPoint, IVector stepsLength, double alpha, double beta, CountingEvaluator countingEvaluator, IAlgebraProvider algebraProvider, ITerminationCondition<T> terminationCondition, IDictionary<string,string> logTags = null)
            {
                this.b = b;
                this.startingPoint = startingPoint;
                this.currentPoint = startingPoint;
                this.stepsLength = stepsLength;
                this.alpha = alpha;
                this.beta = beta;
                this.countingEvaluator = countingEvaluator;
                this.terminationCondition = terminationCondition;
                this.algebraProvider = algebraProvider;
                this.logTags = logTags;
            }
            private IBase b;
            private IObjectiveScores<T> startingPoint;
            private IObjectiveScores<T> currentPoint;
            private IObjectiveScores<T> endPoint;
            double alpha;
            double beta;
            bool[] moveFailedOnce;
            bool[] moveSuccededOnce;
            IVector stepsLength;
            CountingEvaluator countingEvaluator;
            ITerminationCondition<T> terminationCondition;

            public ILoggerMh Logger { get; set; }

            public Tuple<IBase, IObjectiveScores<T>> Evolve()
            {
                moveFailedOnce = createNegBoolArray( b.NumDimensions );
                moveSuccededOnce = createNegBoolArray( b.NumDimensions );
                while( stageIsComplete( ) == false )
                {
                    currentPoint = makeAMove( b, currentPoint );
                    LoggerMhHelper.Write(new IObjectiveScores[]{ currentPoint },
                        createTag(), 
                        Logger);
                }
                endPoint = currentPoint;
                b = createNewBase( b, startingPoint, endPoint );
                return Tuple.Create( b, currentPoint );
            }

            private IDictionary<string, string> createTag()
            {
                var result = LoggerMhHelper.CreateTag(LoggerMhHelper.MkTuple("Move", countingEvaluator.Counter.ToString("D6")));
                if ( logTags != null)
                    result = LoggerMhHelper.MergeDictionaries(this.logTags, result);
                return result;
            }

            private IBase createNewBase( IBase b, IObjectiveScores<T> startingPoint, IObjectiveScores<T> endPoint )
            {
                IVector pathOfStage = createVector( startingPoint, endPoint );
                if ( pathOfStage.IsNullVector )
                {
                    pathOfStage.SetAllComponents(1.0);
                }
                IBase result = b.CreateNew( pathOfStage.Length );
                int colinearTo = isColinearToBase( pathOfStage );
                if( colinearTo >= 0 )
                    return result;
                if( result.GetBaseVector( 0 ).IsOrthogonal( pathOfStage ) )
                {
                    if( result.NumDimensions > 1 )
                    {
                        if( !result.GetBaseVector( 1 ).IsOrthogonal( pathOfStage ) )
                        {
                            result.SetBaseVector( 1, pathOfStage );
                            result.Orthonormalize( 1 );
                        }
                    }
                    else
                        return result;
                }
                else
                {
                    result.SetBaseVector( 0, pathOfStage );
                    result.Orthonormalize( 0 );
                }
                return result;
            }

            private int isColinearToBase( IVector pathOfStage )
            {
                int result = -1;
                for( int i = 0; i < pathOfStage.Length; i++ )
                {
                    if( pathOfStage[i] != 0.0 )
                        if( result != -1 )
                            return -1;
                        else
                            result = i;
                }
                return result;
            }

            private IAlgebraProvider algebraProvider;

            private IVector createVector( IObjectiveScores<T> startingPoint, IObjectiveScores<T> endPoint )
            {
                IVector result = algebraProvider.CreateVector( startingPoint.SystemConfiguration.Dimensions );
                int count = result.Length;
                var varNames = startingPoint.SystemConfiguration.GetVariableNames( );
                for( int i = 0; i < count; i++ )
                {
                    result[i] =
                        endPoint.SystemConfiguration.GetValue( varNames[i] ).ToDouble( null ) -
                        startingPoint.SystemConfiguration.GetValue( varNames[i] ).ToDouble( null );
                }
                return result;
            }

            private IObjectiveScores<T> makeAMove( IBase b, IObjectiveScores<T> startPoint )
            {
                IObjectiveScores<T> newPoint = startPoint;
                int d = b.NumDimensions;
                for( int i = 0; i < d; i++ )
                {
                    newPoint = makeAMove( i, b, newPoint );
                }
                return newPoint;
            }

            private IObjectiveScores<T> makeAMove( int baseVectorIndex, IBase b, IObjectiveScores<T> startingPoint )
            {
                double stepSize = stepsLength[baseVectorIndex];
                bool success = false;
                bool moveMetConstraint = false;
                bool bumped = false;
                var sysConfig = (T)startingPoint.SystemConfiguration.Clone( );
                var varnames = sysConfig.GetVariableNames( );
                int count = sysConfig.Dimensions;
                for( int variableIndex = 0; variableIndex < count; variableIndex++ )
                {
                    var minMax = Tuple.Create( sysConfig.GetMinValue( varnames[variableIndex] ), sysConfig.GetMaxValue( varnames[variableIndex] ) );
                    bumped = !( tryAdd( varnames[baseVectorIndex], sysConfig, b[baseVectorIndex][variableIndex] * step( stepSize, minMax.Item1.ToDouble( null ), minMax.Item2.ToDouble( null ) ) ) );
                    if( bumped )
                        moveMetConstraint = true;
                }
                IObjectiveScores<T> candidatePoint = evaluate( sysConfig );
                var compareResult = compareObjScores( candidatePoint, startingPoint );
                success = compareResult <= 0;
                var result = success ? candidatePoint : startingPoint;
                if( success && !moveMetConstraint )
                {
                    stepsLength[baseVectorIndex] *= ( alpha );
                    moveSuccededOnce[baseVectorIndex] = true;
                }
                else
                {
                    stepsLength[baseVectorIndex] *= ( -beta );
                    moveFailedOnce[baseVectorIndex] = true;
                }
                return result;
            }

            SingleObjectiveComparer<IObjectiveScores> comparer = new SingleObjectiveComparer<IObjectiveScores>( );
            private bool isCancelled = false;
            private IDictionary<string, string> logTags;

            private int compareObjScores( IObjectiveScores<T> candidatePoint, IObjectiveScores<T> startingPoint )
            {
                return this.comparer.Compare( candidatePoint, startingPoint );
            }

            private bool tryAdd( string varName, IHyperCube<U> sysConfig, double realStepSize )
            {
                var min = sysConfig.GetMinValue( varName ).ToDouble(null);
                var max = sysConfig.GetMaxValue( varName ).ToDouble(null);
                var val = sysConfig.GetValue( varName ).ToDouble(null);
                var newval = val + realStepSize;
                bool result = true;
                if( newval < min )
                {
                    newval = min; result = false;
                }
                else if( newval > max )
                {
                    newval = max; result = false;
                }
                sysConfig.SetValue( varName, (U)((IConvertible)newval).ToType(typeof(U), CultureInfo.InvariantCulture));
                return result;
            }

            private IObjectiveScores<T> evaluate( T sysConfig )
            {
                return countingEvaluator.Evaluate( sysConfig );
            }

            private double step( double fraction, double min, double max )
            {
                double delta = max - min;
                if( double.IsNaN( delta ) )
                    throw new ArgumentException("Parameter range is NaN");
                else
                    return fraction * delta;
            }

            private bool[] createNegBoolArray( int dim )
            {
                bool[] result = new bool[dim];
                for( int i = 0; i < dim; i++ )
                    result[i] = false;
                return result;
            }

            private bool stageIsComplete( )
            {
                if( terminationCondition.IsFinished( ) || isCancelled)
                    return true;
                int count = currentPoint.SystemConfiguration.GetVariableNames( ).Length;
                for( int i = 0; i < count; i++ )
                {
                    if( ( moveSuccededOnce[i] == false ) || ( moveFailedOnce[i] == false ) )
                        return false;
                }
                return true;
            }

            public void Cancel( )
            {
                isCancelled = true;
            }
        }

        private Tuple<IBase, IObjectiveScores<T>> performStage( IBase b, IObjectiveScores<T> currentPoint, IVector stepsLength )
        {
            stage = new Stage( b, currentPoint, stepsLength, alpha, beta, this.countingEvaluator, this.AlgebraProvider, this.terminationCondition, this.logTags);
            stage.Logger = Logger;
            return stage.Evolve( );
        }

        private IBase createNewBase( IObjectiveScores<T> currentPoint )
        {
            return AlgebraProvider.CreateBase( currentPoint.SystemConfiguration.Dimensions );
        }
    }
}
