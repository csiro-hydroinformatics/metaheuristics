namespace CSIRO.Metaheuristics
{
    public interface IObjectiveScores
    {
        int ObjectiveCount { get; }
        IObjectiveScore GetObjective( int i );
        ISystemConfiguration GetSystemConfiguration( );
    }
    public interface IObjectiveScores<out TSysConfig> : IObjectiveScores where TSysConfig : ISystemConfiguration
    {
        TSysConfig SystemConfiguration { get; }
    }

}
