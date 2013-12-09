
namespace CSIRO.Metaheuristics
{
    public interface ITerminationCondition<in T> where T : ISystemConfiguration
    {
        void SetEvolutionEngine( IEvolutionEngine<T> engine ) ;
        bool IsFinished( );
    }
}
