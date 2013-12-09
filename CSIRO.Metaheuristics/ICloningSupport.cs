using System;

namespace CSIRO.Metaheuristics
{
    // DOCO: See following, and links: http://stackoverflow.com/questions/536349/why-no-icloneablet 
    public interface ICloningSupport<out T> where T : ICloningSupport<T>
    {
        bool SupportsDeepCloning { get; }
        bool SupportsThreadSafeCloning { get; }
        T Clone( );
    }
}
