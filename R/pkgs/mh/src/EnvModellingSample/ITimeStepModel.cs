using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
