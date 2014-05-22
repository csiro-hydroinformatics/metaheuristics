namespace CSIRO.Metaheuristics
{
    /// <summary>
    /// An interface for constructs where the optimization problem is given to a solver, and ready to execute.
    /// </summary>
    public interface IEvolutionEngine<out T> where T : ISystemConfiguration
    {
        /// TODO: think of ways to monitor the execution.

        /// <summary>
        /// Solve the metaheuristic this object defines.
        /// </summary>
        /// <returns>The results of the optimization process</returns>
        IOptimizationResults<T> Evolve();

        /// <summary>
        /// Gets a description of this solver
        /// </summary>
        /// <returns></returns>
        string GetDescription();

        /// <summary>
        /// Request a cancellation of the process.
        /// </summary>
        void Cancel();

    }

}
