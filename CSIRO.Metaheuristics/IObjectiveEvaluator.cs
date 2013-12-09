
﻿namespace CSIRO.Metaheuristics
{
    /// <summary>
    /// Interface for constructs calculating the objective scores defined for the optimization problem.
    /// </summary>
    /// <typeparam name="T">A type implementing ISystemConfiguration</typeparam>
    public interface IObjectiveEvaluator<T> where T: ISystemConfiguration
    {
        IObjectiveScores<T> EvaluateScore(T systemConfiguration);
    }
}
