
using System;

namespace CSIRO.Metaheuristics
{
    /// <summary>
    /// An interface for objects that are capable to create new random number generators or new 'child factories'
    /// </summary>
    /// <remarks>
    /// The rational for defining this interface is to have a way to initialise 
    /// replicate parallel tasks that may run in isolation and concurrently yet always behave predictably. 
    /// This is the case notably for 
    /// </remarks>
    public interface IRandomNumberGeneratorFactory
    {
        /// <summary>
        /// Creates a new random number generator instance
        /// </summary>
        Random CreateRandom( );

        /// <summary>
        /// Create a new factory for a sub-process that will itself need 
        /// to reproduceably generate independent random number generators.
        /// </summary>
        IRandomNumberGeneratorFactory CreateFactory( );

        /// <summary>
        /// Gets a new random number.
        /// </summary>
        int Next( );
    }
}
