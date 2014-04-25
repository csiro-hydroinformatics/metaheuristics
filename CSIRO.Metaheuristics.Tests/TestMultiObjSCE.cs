using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CSIRO.Metaheuristics.RandomNumberGenerators;
using CSIRO.Metaheuristics.Optimization;
using CSIRO.Metaheuristics.CandidateFactories;
using CSIRO.Metaheuristics.Fitness;
using CSIRO.Metaheuristics.Objectives;
using System.Threading;

namespace CSIRO.Metaheuristics.Tests
{
    [TestFixture]
    public class TestMultiObjSCE
    {
        [Test]
        public void TestSceDualObj()
        {
            var rng = new BasicRngFactory(0);
            var evaluator = new SCH1ObjectiveEvaluator();
            var engine = new ShuffledComplexEvolution<ICloneableSystemConfiguration>(
                evaluator,
                new UniformRandomSamplingFactory<IHyperCube<double>>(rng.CreateFactory(), new UnivariateReal(0)),
                new ShuffledComplexEvolution<ICloneableSystemConfiguration>.MaxShuffleTerminationCondition(),
                5, 20, 10, 3, 20, 7,
                rng,
                new ZitlerThieleFitnessAssignment());
            Results = engine.Evolve();

        }

        [Test]
        public void TestCoeffVariationTerminationCriteria()
        {
            double threshold = 2.5e-2;
            var termination = new ShuffledComplexEvolution<TestHyperCube>.CoefficientOfVariationTerminationCondition(threshold: threshold);
            IObjectiveScores[] population = createSample(converged: false);
            double cv = termination.GetMaxParameterCoeffVar(population);
            Assert.AreEqual(0.1104401, cv, 1e-6);
            Assert.IsFalse(termination.IsBelowCvThreshold(population));

            population = createSample(converged: true);
            cv = termination.GetMaxParameterCoeffVar(population);
            Assert.AreEqual(0.01741291, cv, 1e-6);
            Assert.IsTrue(termination.IsBelowCvThreshold(population));


            termination = new ShuffledComplexEvolution<TestHyperCube>.CoefficientOfVariationTerminationCondition(threshold: threshold, maxHours: 0.1);
            var rng = new BasicRngFactory(0);
            var evaluator = new ParaboloidObjEvalTest(bestParam: 2);
            var engine = new ShuffledComplexEvolution<TestHyperCube>(
                evaluator,
                new UniformRandomSamplingFactory<TestHyperCube>(rng.CreateFactory(), new TestHyperCube(2, 0, -10, 10)),
                termination,
                5, 20, 10, 3, 20, 1,
                rng,
                new DefaultFitnessAssignment());
            var results = engine.Evolve();
            Console.WriteLine("Current shuffle: {0}", engine.CurrentShuffle);
            Assert.IsFalse(termination.HasReachedMaxTime());
        }

        [Test]
        public void TestCoeffVariationTerminationCriteriaBackupThreshold()
        {
            double threshold = 2.5e-2;
            double hours = 1.0 / 3600;
            var termination = new ShuffledComplexEvolution<ICloneableSystemConfiguration>.CoefficientOfVariationTerminationCondition(threshold: threshold, maxHours: hours);
            Assert.IsFalse(termination.HasReachedMaxTime());
            Thread.Sleep(2000);
            Assert.IsTrue(termination.HasReachedMaxTime());
        }

        private IObjectiveScores[] createSample(bool converged = false)
        {
            /*
            > s = rnorm(7, 1000, 12)
            [1] 1024.0471  980.6007 1006.0446  972.9169 1000.0527 1004.0899 1008.8202
            > sd(s)/mean(s)
            [1] 0.01741291
            [1] 1001.3914 1005.0520 1015.2980 1016.9730  981.3532 1000.1558  999.8205
            > sd(s)/mean(s)
            [1] 0.01179792
            [1]  960.8459  880.2550  918.0862  964.6065  994.5268  910.9359 1204.6796
            > sd(s)/mean(s)
            [1] 0.1104401
            [1]  ,  955.1453 1155.3053 1147.6358 1020.7405  910.9917  723.9004
            > sd(s)/mean(s)
            [1] 0.1555959
            */

            var pset = new TestHyperCube[7];
            double[] narrow_one = { 1024.0471, 980.6007, 1006.0446, 972.9169, 1000.0527, 1004.0899, 1008.8202 };
            double[] narrow_two = { 1001.3914, 1005.0520, 1015.2980, 1016.9730, 981.3532, 1000.1558, 999.8205 };
            double[] large_one = { 960.8459, 880.2550, 918.0862, 964.6065, 994.5268, 910.9359, 1204.6796 };
            double[] large_two = { 899.5851, 955.1453, 1155.3053, 1147.6358, 1020.7405, 910.9917, 723.9004 };

            for (int i = 0; i < narrow_one.Length; i++)
            {
                pset[i] = new TestHyperCube(2, 1000, 0, 4000);
                pset[i].SetValues(narrow_one[i], (converged ? narrow_two[i] : large_one[i]));
            }
            var d = new DoubleObjectiveScore("FakeNSE", 0.5, true);
            return Array.ConvertAll(pset, (x => new MultipleScores<TestHyperCube>(new IObjectiveScore[] { d }, x)));
        }

        public IOptimizationResults<ICloneableSystemConfiguration> Results;

        private class UnivariateRealUniformRandomSampler
        {
            public UnivariateRealUniformRandomSampler(double min, double max, int seed)
            {
                this.rand = new Random(seed);
                this.min = min;
                this.max = max;
            }
            private double min, max;
            private Random rand;
            public double GetNext()
            {
                return min + rand.NextDouble() * (max - min);
            }
        }

        private class UnivariateReal : IHyperCube<double>
        {
            public UnivariateReal(double value)
            {
                this.value = value;
            }
            private double value;
            public double min = -5, max = +5;

            public double Value
            {
                get { return this.value; }
            }

            public string GetConfigurationDescription()
            {
                return "Value: " + this.value;
            }

            #region ISystemConfiguration Members


            public void ApplyConfiguration(object system)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IHyperCube<double> Members

            public string[] GetVariableNames()
            {
                return new string[] { "variableName" };
            }

            public int Dimensions
            {
                get { return 1; }
            }

            public double GetValue(string variableName)
            {
                return this.Value;
            }

            public double GetMaxValue(string variableName)
            {
                return max;
            }

            public double GetMinValue(string variableName)
            {
                return min;
            }

            public void SetValue(string variableName, double value)
            {
                this.value = value;
            }

            public IHyperCube<double> HomotheticTransform(IHyperCube<double> point, double factor)
            {
                var p = point as UnivariateReal;
                var result = new UnivariateReal(p.value + factor * (this.value - p.value));
                if (result.value > result.max || result.value < result.min)
                    return null;
                return result;

            }

            #endregion

            #region ICloningSupport<ICloneableSystemConfiguration> Members

            public bool SupportsDeepCloning
            {
                get { return true; }
            }

            public bool SupportsThreadSafeCloning
            {
                get { return true; }
            }

            public ICloneableSystemConfiguration Clone()
            {
                return new UnivariateReal(this.Value);
            }

            #endregion
        }

        private class SCH1ObjectiveEvaluator : IClonableObjectiveEvaluator<ICloneableSystemConfiguration>
        {
            public IObjectiveScores<ICloneableSystemConfiguration> EvaluateScore(ICloneableSystemConfiguration systemConfiguration)
            {
                double x = ((UnivariateReal)systemConfiguration).Value;
                double y = x - 2;
                var result = new SCH1ObjectiveScores(x * x, y * y);
                result.SystemConfiguration = systemConfiguration;
                return result;
            }

            private class SCH1ObjectiveScores : IObjectiveScores<ICloneableSystemConfiguration> // TODO: there should be intermediary abstract classes for common cases.
            {
                public SCH1ObjectiveScores(double firstScore, double secondScore)
                {
                    scores = new double[] { firstScore, secondScore };
                }
                private double[] scores;
                public int ObjectiveCount
                {
                    get { return 2; }
                }

                private ICloneableSystemConfiguration systemConfiguration;

                public ISystemConfiguration GetSystemConfiguration()
                {
                    return this.systemConfiguration;
                }

                public ICloneableSystemConfiguration SystemConfiguration
                {
                    get { return systemConfiguration; }
                    internal set { systemConfiguration = value; }
                }

                public IObjectiveScore GetObjective(int i)
                {
                    return new DoubleObjectiveScore("p" + i, scores[i]);
                }

                private class DoubleObjectiveScore : IObjectiveScore
                {
                    public DoubleObjectiveScore(string name, double value)
                    {
                        this.Name = name;
                        this.value = value;
                    }
                    double value;

                    public bool Maximise
                    {
                        get { return false; }
                    }

                    public string GetText()
                    {
                        return Name + " " + this.ValueComparable.ToString();
                    }

                    public string Name
                    {
                        get;
                        private set;
                    }

                    public IComparable ValueComparable
                    {
                        get { return value; }
                    }
                }

            }

            #region ICloningSupport<IClonableObjectiveEvaluator<ICloneableSystemConfiguration>> Members

            public bool SupportsDeepCloning
            {
                get { return false; }
            }

            public bool SupportsThreadSafeCloning
            {
                get { return false; }
            }

            public IClonableObjectiveEvaluator<ICloneableSystemConfiguration> Clone()
            {
                throw new NotImplementedException();
            }

            #endregion
        }
    }
}