
﻿using System;

namespace CSIRO.Metaheuristics
{
    /// <summary>
    /// Interface for scores used to quantify the performance of a system
    /// </summary>
    /// <remarks>This interface is defined without generics on purpose, to reduce complexity. Limits the unnecessary proliferation of generic classes</remarks>
    public interface IObjectiveScore
    {
        /// <summary>
        /// Gets whether this objective is a maximizable one (higher is better).
        /// </summary>
        bool Maximise { get; }

        /// <summary>
        /// Get a text represtattion of this score
        /// </summary>
        /// <returns></returns>
        string GetText();

        /// <summary>
        /// Get name of the objective measure, typically a bivariate statistic.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the value of the objective. Inheritors should return the real value, and not worry about negating or not. This is taken care elsewhere.
        /// </summary>
        IComparable ValueComparable { get; }
    }

    /// <summary>
    /// Interface for scores used to quantify the performance of a system, defining the type of the score.
    /// </summary>
    /// <typeparam name="T">The type of the objective (score) value, e.g. double, int, float</typeparam>
    public interface IObjectiveScore<out T> where T : IComparable
    {
        /// <summary>
        /// Gets the value of the objective.
        /// </summary>
        T Value { get; }
    }
}
