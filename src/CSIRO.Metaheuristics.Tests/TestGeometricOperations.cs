using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CSIRO.Metaheuristics.SystemConfigurations;
using CSIRO.Metaheuristics.RandomNumberGenerators;

namespace CSIRO.Metaheuristics.Tests
{
    public interface ITestHypercubeFactory<T> where T: IHyperCube<double>
    {
        T Create(int dim, int value, int min, int max);
    }

    [TestFixture]
    public class TestGeometricOperationsTestHypercube : AbstractTestGeometricOperations<TestHyperCube>
    {
        protected override ITestHypercubeFactory<TestHyperCube> createFactory()
        {
            return new TestHypercubeFactory();
        }

        private class TestHypercubeFactory : ITestHypercubeFactory<TestHyperCube>
        {
            public TestHyperCube Create(int dim, int value, int min, int max)
            {
                return new TestHyperCube(dim, value, min, max);
            }
        }

    }

    public abstract class AbstractTestGeometricOperations<T> where T : IHyperCube<double>
    {
        protected AbstractTestGeometricOperations()
        {
            this.factory = createFactory();
        }

        protected abstract ITestHypercubeFactory<T> createFactory();
        
        protected ITestHypercubeFactory<T> factory = null;

        [Test]
        public void TestGetCentroid()
        {
            var hco = new HyperCubeOperations(new BasicRngFactory(0));

            var hc_1 = factory.Create(3, 0, 0, 4);
            var hc_2 = factory.Create(3, 0, 0, 4);
            var hc_3 = factory.Create(3, 0, 0, 4);


            //var hc_1 = new TestHyperCube(3, 0, 0, 4);
            //var hc_2 = (TestHyperCube)hc_1.Clone();
            //var hc_3 = (TestHyperCube)hc_1.Clone();
            var varNames = hc_1.GetVariableNames();
            hc_2.SetValue(varNames[0], 1);
            hc_3.SetValue(varNames[1], 2);

            TestCentroidCalculation(hco, hc_1, hc_2, hc_3, varNames);

        }

        public static void TestCentroidCalculation(HyperCubeOperations hco, IHyperCube<double> hc_1, IHyperCube<double> hc_2, IHyperCube<double> hc_3, string[] varNames)
        {
            IHyperCube<double>[] points = new IHyperCube<double>[] { hc_1, hc_2, hc_3 };
            var centroid = hco.GetCentroid(points);
            //[0,0,0]
            //[1,0,0]
            //[0,2,0]
            TestAreEqual(new double[] { 1 / 3.0, 2 / 3.0, 0 }, centroid, varNames);
        }

        public static void TestAreEqual(double[] values, IHyperCube<double> point, string[] varNames, double delta=1e-12)
        {
            for (int i = 0; i < varNames.Length; i++)
            {
                Assert.AreEqual(values[i], point.GetValue(varNames[i]), delta, "variable " + varNames[i]);
            }
        }

        [Test]
        public void TestHomotecy()
        {
            var centroid = factory.Create(3, 0, 0, 4);
            setValues(centroid, 1.0, 2.0, 3.0);
            var point = factory.Create(3, 0, 0, 4);
            setValues(point, 2.0, 2.1, 2.2);
            var varNames = point.GetVariableNames();

            double delta = 1e-10;
            TestAreEqual(new double[] { 3.0, 2.2, 1.4 }, centroid.HomotheticTransform(point, 2.0), varNames, delta);
            TestAreEqual(new double[] { 0.0, 1.9, 3.8 }, centroid.HomotheticTransform(point, -1.0), varNames, delta);
        }

        protected void setValues(T point, params double[] values)
        {
            var varNames = point.GetVariableNames();
            for (int i = 0; i < values.Length; i++)
            {
                point.SetValue(varNames[i], values[i]);
            }
        }
    }
}
