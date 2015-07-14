using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Fitness
{
    public class NseBiasFitnessAssignment : IFitnessAssignment<double>
    {
        public NseBiasFitnessAssignment( )
        {

        }
        public FitnessAssignedScores<double>[] AssignFitness( IObjectiveScores[] scores )
        {
            FitnessAssignedScores<double>[] result = new FitnessAssignedScores<double>[scores.Length];

            for( int i = 0; i < result.Length; i++ )
            {
                double fitness = (double)scores[i].GetObjective( 0 ).ValueComparable
                    - 5 * Math.Pow( Math.Abs( Math.Log( 1 + (double)scores[i].GetObjective( 1 ).ValueComparable ) ), 2.5 );
                result[i] = new FitnessAssignedScores<double>( scores[i], ( -fitness ) );
            }

            return result;
        }
    }
}
