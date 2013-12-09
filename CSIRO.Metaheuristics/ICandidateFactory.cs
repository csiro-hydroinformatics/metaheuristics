namespace CSIRO.Metaheuristics
{
    /// <summary>
    /// Creates new candidate system configurations in the optimization problem
    /// </summary>
    /// <typeparam name="T">The type of ISystemConfiguration this factory creates</typeparam>
    public interface ICandidateFactory<out T> where T : ISystemConfiguration
    {
        /// <summary>
        /// Gets a randomly generated candidate system configuration
        /// </summary>
        /// <remarks>Most candidates generated in an optimisation process would not be naively random. 
        /// Whether this is this interface that provides them or not remains TBD</remarks>
        T CreateRandomCandidate();

    }
}
