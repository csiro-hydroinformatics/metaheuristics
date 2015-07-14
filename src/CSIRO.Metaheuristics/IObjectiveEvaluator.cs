
﻿namespace CSIRO.Metaheuristics
 {
     /// <summary>
     /// Interface for constructs calculating the objective scores defined for the optimization problem.
     /// </summary>
     /// <typeparam name="T">A type implementing ISystemConfiguration</typeparam>
     public interface IObjectiveEvaluator<T> where T : ISystemConfiguration
     {
         /// <summary>
         /// Evaluate the objective values for a candidate system configuration
         /// </summary>
         /// <param name="systemConfiguration">candidate system configuration</param>
         /// <returns>An object with one or more objective scores</returns>
         IObjectiveScores<T> EvaluateScore(T systemConfiguration);
     }
 }
