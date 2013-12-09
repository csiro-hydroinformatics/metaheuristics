using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CSIRO.Metaheuristics.Utils;

namespace CSIRO.Metaheuristics.Tests
{
    [TestFixture]
    public class TestMhHelper
    {
        [Test]
        public void TestInterpolator()
        {
            var evaluator = new IdentityObjEval();
            var inPoints = new List<TestHyperCube>();
            inPoints.Add(createPoint(1.0, 2.0, 3.0));
            inPoints.Add(createPoint(1.0, 2.0, 4.0));
            inPoints.Add(createPoint(2.0, 3.0, 5.0));
            var inScores = Array.ConvertAll( inPoints.ToArray(), (x => evaluator.EvaluateScore(x) ));
            var outPoints = MetaheuristicsHelper.InterpolateBetweenPoints<TestHyperCube>(evaluator, inScores, 0.1+1e-11);
            Assert.AreEqual(21, outPoints.Length);
            assertPoint(outPoints[0], 1.0, 2.0, 3.0);
            assertPoint(outPoints[1], 1.0, 2.0, 3.1);
            assertPoint(outPoints[10], 1.0, 2.0, 4.0);
            assertPoint(outPoints[15], 1.5, 2.5, 4.5);
        }

        private void assertPoint(IObjectiveScores iObjectiveScores, double x, double y, double z)
        {
            var s = (IObjectiveScores<TestHyperCube>)iObjectiveScores;
            Assert.AreEqual(x, (double)s.GetObjective(0).ValueComparable, 1e-9);
            Assert.AreEqual(y, (double)s.GetObjective(1).ValueComparable, 1e-9);
            Assert.AreEqual(z, (double)s.GetObjective(2).ValueComparable, 1e-9);
            Assert.AreEqual(x, s.SystemConfiguration.GetValue("0"), 1e-9);
            Assert.AreEqual(y, s.SystemConfiguration.GetValue("1"), 1e-9);
            Assert.AreEqual(z, s.SystemConfiguration.GetValue("2"), 1e-9);
        }

        private static TestHyperCube createPoint(double x, double y, double z)
        {
            var hc = new TestHyperCube(3, 0, 0, 10);
            hc.SetValue("0", x);
            hc.SetValue("1", y);
            hc.SetValue("2", z);
            return hc;
        }
    }
}
