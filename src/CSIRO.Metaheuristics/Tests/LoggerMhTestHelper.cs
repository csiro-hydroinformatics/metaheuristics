using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.Logging;

namespace CSIRO.Metaheuristics.Tests
{
    /// <summary>
    /// Facilities to support unit tests on logs of optimisation processes. This is placed in this assembly to limit dependencies. 
    /// </summary>
    public class LoggerMhTestHelper
    {
        public static void CreateTestLogContent(out InMemoryLogger logger, out Dictionary<string, string> strMsg, out Dictionary<string, string> popTags)
        {
            logger = new InMemoryLogger();
            strMsg =
                      new Dictionary<string, string>()
                      {
                        {"Message", "the string message"},
                        {"Category", "the string category"}
                      };

            logger.Write("String message", strMsg);

            var evaluator = new IdentityObjEval();
            var inPoints = new List<TestHyperCube>();
            inPoints.Add(createPoint(1.0, 2.0, 3.0));
            inPoints.Add(createPoint(1.0, 2.2, 4.0));
            inPoints.Add(createPoint(2.0, 3.0, 5.0));
            var inScores = Array.ConvertAll(inPoints.ToArray(), (x => (IObjectiveScores)evaluator.EvaluateScore(x)));


            popTags = new Dictionary<string, string>()
                      {
                        {"Message", "initial population msg"},
                        {"Category", "initial population category"}
                      };

            logger.Write(inScores, popTags);
        }

        private static TestHyperCube createPoint(double x, double y, double z)
        {
            return TestHyperCube.CreatePoint(0, 0, 10, x, y, z);
        }

    }
}
