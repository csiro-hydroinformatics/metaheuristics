using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CSIRO.Utilities
{
    /// <summary>
    /// A helper class to dynamically (reflectively) call methods on generic types, to prevent switch statements
    /// </summary>
    public class GenericTypesHelper
    {
        /// <summary>
        /// Creates a new helper for a type, using specific binding flags to find methods and method definitions on this type
        /// </summary>
        public GenericTypesHelper( Type reflectedType, BindingFlags bindingFlags )
        {
            if( reflectedType == null )
                throw new ArgumentNullException( );
            this.ReflectedType = reflectedType;
            this.BindingFlags = bindingFlags;
        }

        /// <summary>
        /// Creates a new helper for a type, looking for public static methods and method definitions on this type
        /// </summary>
        public GenericTypesHelper( Type reflectedType )
            : this( reflectedType, BindingFlags.Static | BindingFlags.Public )
        {
        }

        /// <summary>
        /// Gets the reflected type.
        /// </summary>
        /// <value>The reflected type.</value>
        public Type ReflectedType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the BindingFlags used to reflect on the type.
        /// </summary>
        /// <value>The BindingFlags used to reflect on the type.</value>
        public BindingFlags BindingFlags
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a method on the reflected type, given the binding flags
        /// </summary>
        public MethodInfo GetMethod( string methodName )
        {
            return ReflectedType.GetMethod( methodName, this.BindingFlags );
        }

        public MethodInfo MakeGenericMethod(string methodName, params Type[] typeArguments)
        {
            return MakeGenericMethod(this.GetMethod(methodName), typeArguments);
        }

        public MethodInfo MakeGenericMethod(string methodName, object[] parameters, params Type[] typeArguments)
        {
            return MakeGenericMethod(this.getMethod(methodName, parameters), typeArguments);
        }

        /// <summary>
        /// Gets a generic method method on the reflected type, given the binding flags
        /// </summary>
        /// <param name="methodInfo">The generic method definition</param>
        /// <param name="typeArguments">The type arguments required by the generic method definition</param>
        /// <returns>The 'typed' generic method</returns>
        public MethodInfo MakeGenericMethod( MethodInfo methodInfo, params Type[] typeArguments )
        {
            return methodInfo.MakeGenericMethod( typeArguments );
        }

        /// <summary>
        /// Call a generic method that is known to return a result.
        /// </summary>
        /// <typeparam name="U">A valid expected returned type of this method, given the method parameters</typeparam>
        /// <param name="methodInfo">The generic method definition</param>
        /// <param name="obj">The object on which to call the method (if an instance method)</param>
        /// <param name="parameters">The parameters of the generic method called once that method has been created</param>
        /// <param name="typeArguments">The type arguments required by the generic method definition</param>
        /// <returns></returns>
        public U CallGenericMethod<U>( MethodInfo methodInfo, object obj, object[] parameters, params Type[] typeArguments )
        {
            return (U)MakeGenericMethod( methodInfo, typeArguments ).Invoke( obj, parameters );
        }

        /// <summary>
        /// Call a generic method that is known to return a result.
        /// </summary>
        /// <typeparam name="U">A valid expected returned type of this method, given the method parameters</typeparam>
        /// <param name="methodName">The name of the generic method definition</param>
        /// <param name="obj">The object on which to call the method (if an instance method)</param>
        /// <param name="parameters">The parameters of the generic method called once that method has been created</param>
        /// <param name="typeArguments">The type arguments required by the generic method definition</param>
        /// <returns></returns>
        public U CallGenericMethod<U>( string methodName, object obj, object[] parameters, params Type[] typeArguments )
        {
            return (U)MakeGenericMethod( this.getMethod( methodName, parameters ), typeArguments ).Invoke( obj, parameters );
        }

        private MethodInfo getMethod( string methodName, object[] parameters )
        {
            if( parameters == null )
                throw new ArgumentNullException( "You cannot specify a null reference as method parameters when discovering a MethodInfo" );

            Type[] methodParameterTypes = getTypes( parameters );
            return this.getMethod( methodName, methodParameterTypes );
        }

        private MethodInfo getMethod( string methodName, Type[] methodParameterTypes )
        {
            return ReflectedType.GetMethod( methodName, this.BindingFlags, null, CallingConventions.Any, methodParameterTypes, null );
        }

        private Type[] getTypes( object[] parameters )
        {
            return Array.ConvertAll( parameters, ( x => x.GetType( ) ) );
        }

        /// <summary>
        /// Call a generic method that is known to return no results (void)
        /// </summary>
        /// <param name="methodName">The name of the generic method definition</param>
        /// <param name="obj">The object on which to call the method (if an instance method)</param>
        /// <param name="parameters">The parameters of the generic method called once that method has been created</param>
        /// <param name="typeArguments">The type arguments required by the generic method definition</param>
        public void CallGenericMethod( string methodName, object obj, object[] parameters, params Type[] typeArguments )
        {
            MakeGenericMethod( methodName, typeArguments ).Invoke( obj, parameters );
        }
    }
}
