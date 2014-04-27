
﻿namespace CSIRO.Metaheuristics
 {
     /// <summary>
     /// Interface for constructs calculating the objective scores on an ensemble of models (e.g multi-catchment streamflow evaluation).
     /// </summary>
     /// <remarks>This interface is to promote hiding the implementation details of batch model evaluations, e.g. MPI. multi-threading, etc.</remarks>
     /// <typeparam name="T">A type implementing ISystemConfiguration</typeparam>
     public interface IEnsembleObjectiveEvaluator<T> where T : ISystemConfiguration
     {
         /// <summary>
         /// Given a system configuration, evaluate the scores for each of the systems in this ensemble.
         /// </summary>
         /// <param name="systemConfiguration">The system configuration to assess.</param>
         /// <returns>The set of scores, one set for each system in this ensemble.</returns>
         IObjectiveScores<T>[] EvaluateScore(T systemConfiguration);
     }
 }
