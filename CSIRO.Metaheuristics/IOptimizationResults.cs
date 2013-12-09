
﻿using System.Collections.Generic;

namespace CSIRO.Metaheuristics
{
    public interface IOptimizationResults<out T> : IEnumerable<IObjectiveScores>
        where T : ISystemConfiguration
    {
    }
}
