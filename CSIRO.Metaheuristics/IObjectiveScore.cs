
﻿using System;

namespace CSIRO.Metaheuristics
{
    /// <summary>
    /// Interface for scores used to quantify the performance of a system
    /// </summary>
    public interface IObjectiveScore
    {
        bool Maximise { get; }
        string GetText();
        string Name { get; }
        IComparable ValueComparable { get; }
    }

    /// <summary>
    /// Interface for scores used to quantify the performance of a system, defining the type of the score.
    /// </summary>
    /// <typeparam name="T">The type of the objective (score) value, e.g. double, int, float</typeparam>
    public interface IObjectiveScore<out T> where T : IComparable
    {
        T Value { get; }
    }
}
