using System;

namespace CSIRO.Metaheuristics
{
    public interface IFitnessAssignment<T> where T : IComparable
    {
        FitnessAssignedScores<T>[] AssignFitness( IObjectiveScores[] scores );
    }

    public class FitnessAssignedScores<T> : IComparable<FitnessAssignedScores<T>> where T : IComparable
    {
        public FitnessAssignedScores( IObjectiveScores scores, T fitnessValue )
        {
            this.Scores = scores;
            this.FitnessValue = fitnessValue;
        }
        public IObjectiveScores Scores { get; private set; }
        public T FitnessValue { get; private set; }

        public int CompareTo( FitnessAssignedScores<T> other )
        {
            return this.FitnessValue.CompareTo( other.FitnessValue );
        }

        public override string ToString( )
        {
            return FitnessValue.ToString() + ", " + Scores.ToString();
        }
    }

}
