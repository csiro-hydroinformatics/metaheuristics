

namespace CSIRO.Sys
{
    public interface ICloningSupport
    {
        /// <summary>
        /// Gets whether this object returns a deep clone of itself and its properties. 
        /// This may vary through its lifetime.
        /// </summary>
        bool SupportsDeepCloning { get; }

        /// <summary>
        /// Gets whether this object returns a clone deemed thread-safe, i.e. 
        /// for write access. This may vary through its lifetime.
        /// </summary>
        /// <example>
        /// A TIME model runner may return a clone of itself with the same input time series, 
        /// but deep-copy the output time series recorded.
        /// </example>
        bool SupportsThreadSafeCloning { get; }
    }

    /// <summary>
    /// Provides strongly typed cloning, qualifying whether the clone is 'deep' and 
    /// thread-safe.
    /// </summary>
    /// <typeparam name="T">The type of the cloned object returned</typeparam>
    /// <remarks>
    /// <para>
    /// The use of the ICloneable interface in the Base Class Library is 
    /// not recommended (Cwalina and Abrams, 2008). The generality of that interface 
    /// returning an 'object' makes it near useless in a strongly typed language, for a start. 
    /// The lack of information as to what type of 'clone' is returned (shallow? deep? etc.) is 
    /// also criticised. However, an interface for cloning objects is still 
    /// clearly needed in some contexts, notably to spawn parallel threads of executions for ensembles of simulations.
    /// </para>
    /// <para>
    /// This interface tries to be more explicit as to the type of 'clone' that 
    /// can be returned by the implementer. In practice this interface is primarily for 
    /// the 'model runners' or similar executable engines in TIME.
    /// </para>
    /// <para>
    /// This interface should only be implemented after a careful assessment. 
    /// The use of generics to keep strict typing has the potential 
    /// to lead to rather complicated situations (The type constraint on the generic can be 
    /// particularly confusing). This is one case where it usually a good idea 
    /// to implement it explicitely.     
    /// </para>
    /// <para>
    /// References:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Krzysztof Cwalina; Brad Abrams, Framework Design Guidelines: Conventions, Idioms, 
    /// and Patterns for Reusable .NET Libraries, Second Edition, Addison-Wesley Professional, 
    /// October 22, 2008, ISBN-13: 978-0-321-54561-9    
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// A strongly typed ICloneable interface has been used in 
    /// production in the Gallio Automation Platform (http://www.gallio.org/Default.aspx and http://www.gallio.org/api/html/T_Gallio_Common_ICloneable_1.htm)
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    ///using CSIRO.Sys;
    ///using System;
    ///using TIME.Core;
    ///
    ///namespace CSIRO.Metaheuristics.TIMEWrappers
    ///{
    ///    public class SimulationTask : ICloningSupport<SimulationTask>
    ///    {
    ///        public SimulationTask( IModel model )
    ///        {
    ///            this.model = model;
    ///        }
    ///        IModel model;
    ///
    ///        public void Execute( )
    ///        {
    ///            // model is run
    ///        }
    ///
    ///        SimulationTask ICloningSupport<SimulationTask>.Clone( )
    ///        {
    ///            return new SimulationTask( (IModel)( (ICloneable)model ).Clone( ) );
    ///        }
    ///
    ///        bool ICloningSupport<SimulationTask>.SupportsDeepCloning
    ///        {
    ///            get { return false; } // err on the side of caution
    ///        }
    ///
    ///        bool ICloningSupport<SimulationTask>.SupportsThreadSafeCloning
    ///        {
    ///            get { return (model is ICloneable); } // for the sake of example...
    ///        }
    ///    }
    ///}
    ///</code>
    /// </example>
    public interface ICloningSupport<out T> : ICloningSupport where T : ICloningSupport<T>
    {
        /// <summary>
        /// Gets a clone of this object, with the characteristics as currently described.
        /// </summary>
        T Clone();
    }
}
