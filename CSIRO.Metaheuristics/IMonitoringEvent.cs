namespace CSIRO.Metaheuristics
{
    public interface IMonitoringEvent
    {
        IObjectiveScores[] ScoresSet { get; }
    }
}
