using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Objectives
{
    public sealed class ScoreComparison
    {
        public static T[] SortSingleObjective<T>( T[] scores ) where T: IObjectiveScores
        {
            var result = (T[])scores.Clone( );
            Array.Sort( result, new SingleObjectiveComparer<T>( ) );
            return result;
        }

        public static IObjectiveScores[] GetParetoFront( IObjectiveScores[] scores )
        {
            return new ParetoRanking<IObjectiveScores>( scores ).GetParetoRank( 1 );
        }
    }
}
