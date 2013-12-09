using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Objectives
{
    //this comparer is to try to judge whether there is a strict dominance which means anyone of objectives in one element
    //is better than one in another element.
    public class ParetoComparer<T> : IComparer<T> where T : IObjectiveScores
    {
        public int Compare(T x, T y)
        {
            int n = x.ObjectiveCount;
            if (n < 1)
                throw new ArgumentException("There must be at least one objective to compare");
            int result = 0;
            int i = 0;
            result = CompareObjectiveScore(x, y, 0);
            if (result == 0) // at least one measure is identical: there cannot be strict dominance.
                return 0;
            for (i = 1; i < n; i++)
            {
                int comparison = CompareObjectiveScore(x, y, i);
                if (comparison == 0) // at least one measure is identical: there cannot be strict dominance.
                    return 0;
                else if (comparison * result < 0) // partial order for this measure contradicts the order so far: no pairwise dominance
                    return 0;
                // otherwise result is of same sign as the current comparison.
            }
            return result;
        }

        private static int CompareObjectiveScore(T x, T y, int i)
        {
            IObjectiveScore xScore = x.GetObjective(i);
            IObjectiveScore yScore = y.GetObjective(i);
            int comparison = xScore.ValueComparable.CompareTo(yScore.ValueComparable);
            if (xScore.Maximise)
                comparison = -comparison;
            return comparison;
        }
    }

}
