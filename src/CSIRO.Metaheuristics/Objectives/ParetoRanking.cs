using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Objectives
{
    public class ParetoRanking<T> where T : IObjectiveScores
    {
        public ParetoRanking( IEnumerable<T> scores, IComparer<T> comparer )
        {
            this.comparer = comparer;
            paretoRanking = doParetoRanking( scores, comparer );
        }

        public ParetoRanking( IEnumerable<T> scores ) : this( scores, new ParetoComparer<T>( ) ) { }

        private T[][] doParetoRanking( IEnumerable<T> scores, IComparer<T> comparer )
        {
            List<T[]> result = new List<T[]>();
            var rankOne = doOneRanking(scores, comparer);
            while (rankOne.Dominated.Length > 0) // Warning Expects the comparer to not be buggy!
            {
                result.Add(rankOne.NonDominated);
                rankOne = doOneRanking(rankOne.Dominated, comparer);
            }
            result.Add(rankOne.NonDominated);
            return result.ToArray();
        }

        private struct ParetoRanked
        {
            public ParetoRanked( T[] nonDominated, T[] dominated )
            {
                this.nonDominated = nonDominated;
                this.dominated = dominated;
            }
            private T[] nonDominated;

            public T[] NonDominated
            {
                get { return nonDominated; }
            }
            private T[] dominated;

            public T[] Dominated
            {
                get { return dominated; }
            }
        }

        private ParetoRanked doOneRanking( IEnumerable<T> scores, IComparer<T> comparer )
        {
            if (scores.Count() == 0)
                throw new ArgumentException("Cannot (pareto) rank on an array of zero length");
            List<T> nonDominated = new List<T>();
            List<T> dominated = new List<T>();
            foreach ( var scoreA in scores )
            {
                bool isNonDominant = true;
                foreach( var scoreB in scores )
                {
                    if (object.ReferenceEquals(scoreB, scoreA))
                        continue;
                    int comparison = comparer.Compare(scoreA, scoreB);
                    if (comparison > 0) // then scores[j] dominates score[i].
                    {
                        isNonDominant = false;
                        break;
                    }
                }
                if (isNonDominant)
                    nonDominated.Add(scoreA);
                else
                    dominated.Add(scoreA);
            }
            if (nonDominated.Count == 0)
                throw new Exception("Invalid result: the Pareto front should always have at least one element. The comparer " +
                    comparer.GetType().FullName + " may have errors");
            return new ParetoRanked(nonDominated.ToArray(), dominated.ToArray());
        }

        private T[][] paretoRanking;
        private IComparer<T> comparer;

        public T[] GetParetoRank(int rankNumber)
        {
            return (T[])paretoRanking[rankNumber - 1].Clone();
        }

        public static U[] GetParetoFront<U>( IEnumerable<U> scores ) where U: IObjectiveScores
        {
            var paretoRanking = new ParetoRanking<U>( scores, new ParetoComparer<U>( ) );
            return paretoRanking.GetParetoRank( 1 );
        }

        public T[] GetDominatedByParetoRank( int i )
        {
            List<T> result = new List<T>( );
            for( int j = i; j < paretoRanking.Length; j++ )
                result.AddRange(paretoRanking[j]);
            return result.ToArray();
        }

        public int GetNumDominated( T nonDominated, IEnumerable<T> dominated )
        {
            int count = 0;
            for( int i = 0; i < dominated.Count(); i++ )
                if( comparer.Compare( nonDominated, dominated.ElementAt(i) ) < 0 ) // think: is nonDominated 'before' dominated[i]
                    count++;
            return count;
        }

        public bool IsDominated( T testedPoint, T dominatorPoint )
        {
            return ( comparer.Compare( testedPoint, dominatorPoint ) > 0 );// think: is testedPoint 'after' dominatorPoint
        }
    }
}
