namespace CSIRO.Metaheuristics
{
    /// <summary>
    /// An interface for one or more objective scores derived from the evaluation of a candidate system configuration.
    /// </summary>
    /// <remarks>This interface is defined without generics on purpose, to reduce complexity. Limits the unnecessary proliferation of generic classes</remarks>
    public interface IObjectiveScores
    {
        /// <summary>
        /// Gets the number of objective scores in this instance.
        /// </summary>
        int ObjectiveCount { get; }

        /// <summary>
        /// Gets one of the objective 
        /// </summary>
        /// <param name="i">zero-based inex of the objective</param>
        /// <returns></returns>
        IObjectiveScore GetObjective(int i);

        /// <summary>
        /// Gets the system configuration that led to these scores.
        /// </summary>
        /// <returns></returns>
        ISystemConfiguration GetSystemConfiguration();
    }

    /// <summary>
    /// A generic interface for one or more objective scores derived from the evaluation of a candidate system configuration.
    /// </summary>
    /// <typeparam name="TSysConfig">The type of the system configuration</typeparam>
    public interface IObjectiveScores<out TSysConfig> : IObjectiveScores where TSysConfig : ISystemConfiguration
    {
        /// <summary>
        /// Gets the system configuration that led to these scores.
        /// </summary>
        TSysConfig SystemConfiguration { get; }
    }

}
