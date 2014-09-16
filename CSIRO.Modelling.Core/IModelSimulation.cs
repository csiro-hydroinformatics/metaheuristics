using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Modelling.Core
{
    public interface IModelSimulation<D, V, T>
    {
        void Execute();
        void SetSpan(T start, T end);
        void Play(string modelPropertyId, D values);
        void Record(string modelPropertyId);
        D GetRecorded(string modelPropertyId);
        void SetVariable(string modelPropertyId, V value);
        V GetVariable(string modelPropertyId);
        T GetStart();
        T GetEnd();
    }

    public interface IModelSimulation<D> : IModelSimulation<D, double, DateTime>
    {
    }
}
