using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CSIRO.Metaheuristics.SystemConfigurations;
using CSIRO.Metaheuristics.RandomNumberGenerators;
using CSIRO.Metaheuristics.CandidateFactories;

namespace CSIRO.Metaheuristics.Tests
{
    [TestFixture]
    public class TestUrsPointGeneration
    {
        [Test]
        public void TestConstrainedFeasibleRegion()
        {
            // Test that the parameter set does not deal with its constraints, 
            // because the URS should, to try to preserve uniformity of sampling with varying bounds.
            var p = new TwoParamsConstraints();
            Assert.IsTrue(p.IsWithinBounds);
            var names = p.GetVariableNames();
            Assert.AreEqual(5, p.GetValue(names[1]));
            Assert.AreEqual(5, p.GetMaxValue(names[1]));
            p.SetValue(names[1], 10);
            Assert.IsFalse(p.IsWithinBounds);
            Assert.AreEqual(10, p.GetValue(names[1]));


            var rng = new BasicRngFactory(0);
            var urs = new UniformRandomSamplingFactory<TwoParamsConstraints>(rng.CreateFactory(), new TwoParamsConstraints());
            int n = 200;
            var pop = new TwoParamsConstraints[n];
            for (int i = 0; i < n; i++)
            {
                pop[i] = urs.CreateRandomCandidate();
                Assert.IsTrue(pop[i].IsWithinBounds);
            }
            var hcOps = urs.CreateNew(rng.CreateFactory());
            var hcArray = Array.ConvertAll(pop, x => (IHyperCube<double>)x);
            for (int i = 0; i < n; i++)
            {
                p = (TwoParamsConstraints)hcOps.GenerateRandomWithinHypercube(hcArray);
                Assert.IsTrue(p.IsWithinBounds);                
            }
        }
    }
}
