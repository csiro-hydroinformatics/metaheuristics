
﻿using System;

namespace CSIRO.Metaheuristics
{
    /// <summary>
    /// Interface for system configurations that are a set of numeric parameters, each with min and max feasible values.
    /// </summary>
    /// <typeparam name="T">A comparable type; typically a float or double, but possibly integer or more esoteric type</typeparam>
    public interface IHyperCube<T> : ICloneableSystemConfiguration where T : IComparable
    {
        /// <summary>
        /// Gets the names of the variables defined for this hypercube.
        /// </summary>
        /// <returns></returns>
        string[] GetVariableNames();

        /// <summary>
        /// Gets the number of dimensions in this hypercube
        /// </summary>
        int Dimensions { get; }

        /// <summary>
        /// Gets the value for a variable
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        T GetValue(string variableName);

        /// <summary>
        /// Gets the maximum feasible value for a variable
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        T GetMaxValue(string variableName);

        /// <summary>
        /// Gets the minimum feasible value for a variable
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        T GetMinValue(string variableName);

        /// <summary>
        /// Sets the value of one of the variables in the hypercube
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        void SetValue(string variableName, T value);

        /// <summary>
        /// Perform a homotetic transformation centered around this hypercube, of a second hypercube.
        /// </summary>
        /// <param name="point">The hypercube from which to derive. This object must not be modified by the method.</param>
        /// <param name="factor">The factor in the homotecie. a value 1 leaves the effectively point unchanged</param>
        /// <returns>A new instance of an hypercube, result of the transformation</returns>
        IHyperCube<T> HomotheticTransform(IHyperCube<T> point, double factor);
    }
}
