using System;

namespace CSIRO.Metaheuristics
{
    /// <summary>
    /// Interface for objects that derive fitness scores from a population. Mostly used for Pareto multi-objective optimizations.
    /// </summary>
    /// <typeparam name="T">The type of fitness used to compare system configuration.</typeparam>
    public interface IFitnessAssignment<T> where T : IComparable
    {
        /// <summary>
        /// Given a population of objective results, assign fitness scores to each result.
        /// </summary>
        /// <param name="scores"></param>
        /// <returns></returns>
        FitnessAssignedScores<T>[] AssignFitness(IObjectiveScores[] scores);
    }

    /// <summary>
    /// Capture a fitness score derived from a candidate system configuration and its objective scores.
    /// </summary>
    /// <typeparam name="T">The type of fitness used to compare system configuration.</typeparam>
    public class FitnessAssignedScores<T> : IComparable<FitnessAssignedScores<T>> where T : IComparable
    {
        /// <summary>
        /// Creates a FitnessAssignedScores, a union of a candidate system configuration and its objective scores, and an overall fitness score.
        /// </summary>
        /// <param name="scores">Objective scores</param>
        /// <param name="fitnessValue">Fitness value, derived from the scores and context information such as a candidate population.</param>
        public FitnessAssignedScores(IObjectiveScores scores, T fitnessValue)
        {
            this.Scores = scores;
            this.FitnessValue = fitnessValue;
        }
        /// <summary>
        /// Gets the objective scores
        /// </summary>
        public IObjectiveScores Scores { get; private set; }

        /// <summary>
        /// Gets the fitness value that has been assigned to the candidate system configuration and its objective scores
        /// </summary>
        public T FitnessValue { get; private set; }

        /// <summary>
        /// Compares two FitnessAssignedScores<T>.
        /// </summary>
        /// <param name="other">Object to compare with this object</param>
        /// <returns>an integer as necessary to implement IComparable</returns>
        public int CompareTo(FitnessAssignedScores<T> other)
        {
            return this.FitnessValue.CompareTo(other.FitnessValue);
        }

        public override string ToString()
        {
            return FitnessValue.ToString() + ", " + Scores.ToString();
        }
    }

}
