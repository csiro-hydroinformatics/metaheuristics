
﻿namespace CSIRO.Metaheuristics
{
    /// <summary>
    /// Interface for constructs calculating the objective scores on an ensemble of models (e.g multi-catchment streamflow evaluation).
    /// </summary>
    /// <remarks>This interface is to promote hiding the implementation details of batch model evaluations, e.g. MPI. multi-threading, etc.</remarks>
    /// <typeparam name="T">A type implementing ISystemConfiguration</typeparam>
     public interface IEnsembleObjectiveEvaluator<T> where T : ISystemConfiguration
    {
        IObjectiveScores<T>[] EvaluateScore(T systemConfiguration);
    }
}
