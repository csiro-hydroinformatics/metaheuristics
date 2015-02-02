using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Modelling.Core
{
    /// <summary>
    /// An interface characterizing the essence of a model simulations (model structure plus input/output data, ready to execute)
    /// </summary>
    /// <typeparam name="D">The type of the object that handles inputs/outputs for the simulation, typically an type holding time series data</typeparam>
    /// <typeparam name="V"></typeparam>
    /// <typeparam name="T">The type representing the units for the simulation span; typically DateTime, sometimes Int32 for array oriented systems</typeparam>
    public interface IModelSimulation<D, V, T> : CSIRO.Sys.ICloningSupport<IModelSimulation<D, V, T>>
    {
        /// <summary>
        /// Execute the simulation, given the current configuration of this object
        /// </summary>
        void Execute();

        /// <summary>
        /// Sets the simulation span (e.g. time span) to execute over
        /// </summary>
        /// <param name="start">First time step of the simulation</param>
        /// <param name="end">Last time step of the simulation</param>
        void SetSpan(T start, T end);

        /// <summary>
        /// Define a data holder, e.g. time series, as input to a uniquely identified variable of the model simulation.
        /// </summary>
        /// <param name="modelPropertyId">Variable identifier in the model simulation</param>
        /// <param name="values">Data to use as inputs</param>
        void Play(string modelPropertyId, D values);

        /// <summary>
        /// Configure the simulation such that a model variable will be recorded to a data holder at the next execution.
        /// </summary>
        /// <param name="modelPropertyId">Variable identifier in the model simulation</param>
        void Record(string modelPropertyId);

        /// <summary>
        /// Gets the data or data holder recording a model variable over the simulation.
        /// </summary>
        /// <param name="modelPropertyId">Variable identifier in the model simulation</param>
        /// <returns></returns>
        D GetRecorded(string modelPropertyId);

        /// <summary>
        /// Sets the value of a model variable
        /// </summary>
        /// <param name="modelPropertyId">Variable identifier in the model simulation</param>
        /// <param name="value"></param>
        void SetVariable(string modelPropertyId, V value);

        /// <summary>
        /// Gets the current value of a model variable
        /// </summary>
        /// <param name="modelPropertyId">Variable identifier in the model simulation</param>
        /// <returns></returns>
        V GetVariable(string modelPropertyId);
        
        /// <summary>
        /// Gets the first time step of the simulation
        /// </summary>
        T GetStart();
        
        /// <summary>
        /// Gets the last time step of the simulation
        /// </summary>
        T GetEnd();
    }

    /// <summary>
    /// Interface for model simulation where model variables are double precisions, and time steps handles by date-time
    /// </summary>
    /// <typeparam name="D">The type of the object that handles inputs/outputs for the simulation, typically an type holding time series data</typeparam>
    public interface IModelSimulation<D> : IModelSimulation<D, double, DateTime>
    {
    }
}
