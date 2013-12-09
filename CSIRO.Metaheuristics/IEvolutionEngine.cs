namespace CSIRO.Metaheuristics
{
    /// <summary>
    /// An interface for constructs where the optimization problem is given to a solver, and ready to execute.
    /// </summary>
    /// <remarks>
    /// TODO: think of ways to monitor the execution.
    /// </remarks>
    public interface IEvolutionEngine<out T> where T: ISystemConfiguration
    {
        IOptimizationResults<T> Evolve( );
        string GetDescription();
        int CurrentGeneration { get; }
        void Cancel();
        
    }

}
