using System;
using System.Collections.Generic;

namespace CSIRO.Metaheuristics.Objectives
{
    public class SingleObjectiveComparer<T> : IComparer<T> where T : IObjectiveScores
    {
        public int Compare( T x, T y )
        {
            if( x.ObjectiveCount != 1 || y.ObjectiveCount != 1 )
                throw new ArgumentException( "This comparer works only for single objective scores" );
            IObjectiveScore xScore = x.GetObjective( 0 );
            IObjectiveScore yScore = y.GetObjective( 0 );
            int comparison = xScore.ValueComparable.CompareTo( yScore.ValueComparable );
            if( xScore.Maximise )
                comparison = -comparison;
            return comparison;
        }
    }
}
