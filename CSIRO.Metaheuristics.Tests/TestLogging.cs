using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CSIRO.Metaheuristics.Utils;
using CSIRO.Metaheuristics.Logging;

namespace CSIRO.Metaheuristics.Tests
{
    [TestFixture]
    public class TestLogging
    {
        [Test]
        public void TestInMemoryLogging()
        {
            var logger = new InMemoryLogger();
            var strMsg = 
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


            var popTags = new Dictionary<string, string>()
                      {
                        {"Message", "initial population msg"},
                        {"Category", "initial population category"}
                      };

            logger.Write(inScores, popTags);
            var logInfo = logger.ExtractLog("Results name");

            var columnKeys = logInfo.Item1;
            var lines = logInfo.Item2;
            Assert.AreEqual(4, lines.Count());
            Assert.AreEqual(8, columnKeys.Count());

            foreach (var name in new[] { "Message", "Category", "0", "1", "2", "1_s", "2_s", "0_s" })
                Assert.IsTrue(columnKeys.Contains(name), "Expected column name " + name);

            AssertEquivalentDictionaries(strMsg, lines[0]);

            var expectedLine = new Dictionary<string, string>(){
                    {"0", "1"}, 
                    {"1", "2.2"},
                    {"2", "4"}, 
                    {"1_s", "2.2"}, 
                    {"2_s", "4"}, 
                    {"0_s", "1" }};
            foreach (var item in popTags)
                expectedLine.Add(item.Key, item.Value);
            AssertEquivalentDictionaries(expectedLine, lines[2]);

        }

        private void AssertEquivalentDictionaries(Dictionary<string, string> a, Dictionary<string, string> b)
        {
            if (a.Count() != b.Count()) throw new AssertionException("Dictionaries have a different length - cannot be equivalent in content");
            foreach (var k in a.Keys)
            {
                Assert.AreEqual(a[k], b[k]);
            }
        }

        private static TestHyperCube createPoint(double x, double y, double z)
        {
            return TestHyperCube.CreatePoint(0, 0, 10, x, y, z);
        }

    }
}
