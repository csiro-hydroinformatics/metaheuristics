using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Logging
{
    /// <summary>
    /// A facade for logging information from optimisation processes, to avoid coupling to specific frameworks.
    /// </summary>
    public interface ILoggerMh
    {
        void Write(IObjectiveScores[] scores, IDictionary<string, string> tags);
        void Write(FitnessAssignedScores<double> worstPoint, IDictionary<string, string> tags);
        void Write(IHyperCube<double> newPoint, IDictionary<string, string> tags);
        void Write(string message, IDictionary<string, string> tags);
    }
}
