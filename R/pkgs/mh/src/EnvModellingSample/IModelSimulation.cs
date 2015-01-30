using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EnvModellingSample
{
    public interface IModelSimulation
    {
        void Execute();
        void SetTimeSpan(int startIndex, int endIndex);
        void Play(string inputIdentifier, double[] values);
        double[] GetRecorded(string outputIdentifier);
        void Record(string outputIdentifier);
        void SetValue(string modelPropertyId, double values);
    }
}
