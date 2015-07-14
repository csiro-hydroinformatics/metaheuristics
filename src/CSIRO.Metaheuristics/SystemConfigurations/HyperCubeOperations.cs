using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.Optimization;
using CSIRO.Metaheuristics.Utils;

namespace CSIRO.Metaheuristics.SystemConfigurations
{
    public class HyperCubeOperations : IHyperCubeOperations
    {
         public HyperCubeOperations( IRandomNumberGeneratorFactory rng )
        {
            this.rng = rng;
        }

        IRandomNumberGeneratorFactory rng;

        public IHyperCube<double> GenerateRandom( IHyperCube<double> point )
        {
            string[] varNames = point.GetVariableNames( );
            IHyperCube<double> result = point.Clone( ) as IHyperCube<double> ;
            for( int i = 0; i < varNames.Length; i++ )
            {
                string v = varNames[i];
                // We take the bounds of the result point, to cater for cascading parameter constraints.
                double min = result.GetMinValue(v); 
                double max = result.GetMaxValue(v);
                checkFeasibleInterval(min, max, v);
                result.SetValue( varNames[i], GetRandomisedValue(min, max) );
            }
            return result;
        }

        #region IHyperCubeOperations Members

        public IHyperCube<double> GetCentroid( IHyperCube<double>[] points )
        {

            if( points == null )
            {
                return null;
            }
            if( points.Length == 1 )
            {
                return points[0].Clone( ) as IHyperCube<double>;
            }

            IHyperCube<double> p = points[0].Clone( ) as IHyperCube<double>;

            string[] varNames = p.GetVariableNames();
            foreach( string varName in varNames )
            {
                double val = 0.0;
                for( int i = 0; i < points.Length; i++ )
                {
                    val += points[i].GetValue(varName);
                }
                p.SetValue( varName, val / ( (double)points.Length ) );
            }
            return p;

        }

        public IHyperCube<double> GenerateRandomWithinHypercube( IHyperCube<double>[] points )
        {
            if( points.Length == 0 )
            {
                return this.GenerateRandom(points[0]);
            }
            else
            {
                IHyperCube<double> p = points[0].Clone() as IHyperCube<double>;
                string[] varNames = p.GetVariableNames();
                for (int i = 0; i < varNames.Length; i++)
                {
                    string v = varNames[i];
                    double minimum;
                    double maximum;
                    GetSmallestIntervalForValues(points, v, out minimum, out maximum);
                    minimum = Math.Max(minimum, p.GetMinValue(v));
                    maximum = Math.Min(maximum, p.GetMaxValue(v));
                    checkFeasibleInterval(minimum, maximum, v);
                    p.SetValue(varNames[i], GetRandomisedValue(minimum, maximum));
                }

                return p;
            }

        }

        private void checkFeasibleInterval(double minimum, double maximum, string varName)
        {
            if (maximum < minimum)
                throw new NotSupportedException(string.Format("Impossible to generate random value for variable {0}: min={1}, max={2}", varName, minimum, maximum));
        }

        #endregion

        private void GetSmallestIntervalForValues( IHyperCube<double>[] points, string varName, out double minimum, out double maximum )
        {
            int n = points.Length;
            double[] values = new double[n];

            for( int j = 0; j < n; j++ )
                values[j] = points[j].GetValue(varName);

            minimum = MetaheuristicsHelper.GetMinimum( values );
            maximum = MetaheuristicsHelper.GetMaximum( values );
        }

        private double GetRandomisedValue( double minimum, double maximum )
        {
            if( rng == null )
                throw new NotSupportedException( "A random number generator was not provided. This hyper-cube cannot be randomized" );
            Random random = rng.CreateRandom( );
            double delta = maximum - minimum;
            return minimum + random.NextDouble( ) * delta;
        }

        public static double Reflect (double point, double reference, double factor)
        {
            return reference + ((point - reference) * factor);
        }
    }
}
