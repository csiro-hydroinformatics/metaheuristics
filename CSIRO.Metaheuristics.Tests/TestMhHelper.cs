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

        [Test]
        public void TestParseCsvScores()
        {
            var template = createPoint(3, 2, 1);
            var csvContent = @"0,1,2,Score:0-0,Score:0-1
4.4,3.3,2.2,0,3.14159265358979
4.5,3.4,2.1,3.14159265358979,6.28318530717959
";
            var points = MetaheuristicsHelper.ParseConfigsFromCsv(csvContent, template);
            Assert.AreEqual(4.4, points[0].GetValue("0"));
            Assert.AreEqual(3.3, points[0].GetValue("1"));
            Assert.AreEqual(3.4, points[1].GetValue("1"));
        }

        [Test]
        public void TestMakeBins()
        {
            int[] population = new int[13];
            for (int i = 0; i < population.Length; i++)
			    population[i] = i+1;

            var result = MetaheuristicsHelper.MakeBins(population, 2);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7 }, result[0]);
            Assert.AreEqual(new[] { 8, 9, 10, 11, 12, 13 }, result[1]);
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
            return TestHyperCube.CreatePoint(0, 0, 10, x, y, z);
        }
    }
}
