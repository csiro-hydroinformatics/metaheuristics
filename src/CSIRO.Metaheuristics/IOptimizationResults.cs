
﻿using System.Collections.Generic;

namespace CSIRO.Metaheuristics
{
    /// <summary>
    /// Interface for objects that capture the results of a solved metaheuristic
    /// </summary>
    /// <typeparam name="T">The type of the system configuration</typeparam>
    public interface IOptimizationResults<out T> : IEnumerable<IObjectiveScores>
        where T : ISystemConfiguration
    {
    }
}
