
namespace CSIRO.Metaheuristics
{
    /// <summary>
    /// Interface for objects used by solvers to determine whether they have completed their task.
    /// </summary>
    /// <typeparam name="T">The type of system configuration dealt with</typeparam>
    public interface ITerminationCondition<in T> where T : ISystemConfiguration
    {

        /// <summary>
        /// Sets the evolution engine this object deals with, so that this object can inspect it and determine the status. This method is meant to be called by the engine.
        /// </summary>
        /// <param name="engine">The evolution engine (e.g. optimizer) that uses this termination condition</param>
        void SetEvolutionEngine(IEvolutionEngine<T> engine);

        /// <summary>
        /// Gets whether the termination condition is met.
        /// </summary>
        /// <returns></returns>
        bool IsFinished();
    }
}
