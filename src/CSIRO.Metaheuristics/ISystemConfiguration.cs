
﻿using System;
using CSIRO.Sys;

namespace CSIRO.Metaheuristics
{
    /// <summary>
    /// The representation of the tunable 'parameters' of the optimisation problem, usually a parameter set.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This apparently inconspicuous interface is actually crucial. This is the expression of the "point" in the 
    /// parameter space. In practice it is often a latin hypercube in most cases in hydrology 
    /// (i.e. a parameter set and its feasible bounds). However a more abstract definition allows more flexibility to 
    /// define the optimisation problem at hand, arguably the most critical step in an optimisation task.
    /// </para>
    /// </remarks>
    public interface ISystemConfiguration
    {
        /// <summary>
        /// Gets an alphanumeric description for this system configuration
        /// </summary>
        string GetConfigurationDescription();

        /// <summary>
        /// Apply this system configuration to a compatible system, usually a 'model' in the broad sense of the term.
        /// </summary>
        /// <param name="system">A compatible system, usually a 'model' in the broad sense of the term</param>
        /// <exception cref="ArgumentException">thrown if this system configuration cannot be meaningfully applied to the specified system</exception>
        void ApplyConfiguration( object system );
    }

    /// <summary>
    /// Interface for system configurations that support cloning. Helps to support parallelism in solvers.
    /// </summary>
    public interface ICloneableSystemConfiguration : ISystemConfiguration, ICloningSupport<ICloneableSystemConfiguration>
    {
    }
}
