using CSIRO.Sys;

namespace CSIRO.Metaheuristics
{
    /// <summary>
    /// A superset of the <see cref="IObjectiveEvaluator"/> interface that is clonable, most notably to spawn evaluators that are thread safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// This interface is used for instance by the parallel version of the shuffled complex evolution algorithm, 
    /// when spawning complexes that will be run in parallel and for which we want thread safe structures, 
    /// e.g. not sharing the same model runner.
    /// </remarks>
    public interface IClonableObjectiveEvaluator<TSysConfig> :
        IObjectiveEvaluator<TSysConfig>, ICloningSupport<IClonableObjectiveEvaluator<TSysConfig>>
        where TSysConfig : ISystemConfiguration
    {
    }
}
