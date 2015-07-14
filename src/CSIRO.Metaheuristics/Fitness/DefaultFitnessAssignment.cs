using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Fitness
{
    //this fitness assignment is for a single objective search.
    public class DefaultFitnessAssignment : IFitnessAssignment<double>
    {
        public DefaultFitnessAssignment( )
        {

        }
        public FitnessAssignedScores<double>[] AssignFitness( IObjectiveScores[] scores )
        {
            FitnessAssignedScores<double>[] result = new FitnessAssignedScores<double>[scores.Length];

            for( int i = 0; i < result.Length; i++ )
            {
                if( scores[i].GetObjective( 0 ).Maximise )
                    result[i] = new FitnessAssignedScores<double>( scores[i], ( - (double)scores[i].GetObjective(0).ValueComparable ) );
                else
                    result[i] = new FitnessAssignedScores<double>( scores[i], ( (double)scores[i].GetObjective( 0 ).ValueComparable ) );

            }

            return result;
        }
    }
}
