namespace EnvModellingSample
{
    public interface ITimeStepModel
    {
        void RunOneTimeStep();
        void Reset();
        ITimeStepModel Clone();
        bool IsClonable { get; }
    }
}
