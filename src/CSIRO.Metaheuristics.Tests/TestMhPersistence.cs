using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CSIRO.Metaheuristics.Tests;

namespace CSIRO.Metaheuristics.DataModel.Tests
{
    [TestFixture]
    public class TestMhPersistence
    {
        [Test]
        public void TestWrite()
        {
            // TODO: make this machine independent. Needs an SQL server for tests
            if (Environment.MachineName.ToLower() != "chrome-bu")
                return;
            using (var db = new OptimizationResultsContext())
            {
                var resultsSet = new ObjectivesResultsCollection { Name = "Result Set" };
                var aPointWithScores = new ObjectiveScoreCollection();
                aPointWithScores.Scores = new List<ObjectiveScore> {
                    new ObjectiveScore { Name = "NSE", Value = "0.5", Maximize = true },
                    new ObjectiveScore { Name = "Bias", Value = "0.6", Maximize = false } };

                var hc = new HyperCube { Name = "HyperCube Name" };

                hc.Variables = new List<VariableSpecification> {
                    new VariableSpecification { Maximum = 1, Minimum = 0, Name = "aParameter_1", Value = 0.5f },
                    new VariableSpecification { Maximum = 1, Minimum = 0, Name = "aParameter_2", Value = 0.5f } };
                aPointWithScores.SysConfiguration = hc;

                resultsSet.ScoresSet = new List<ObjectiveScoreCollection> { aPointWithScores };
                var attributes = new TagCollection();
                attributes.Tags = new List<Tag> {
                    new Tag { Name = "ModelId", Value = "GR4J" },
                    new Tag { Name = "CatchmentId", Value = "123456" } };
                resultsSet.Tags = attributes;

                db.ObjectivesResultsCollectionSet.Add(resultsSet);

                int recordsAffected = db.SaveChanges();

                //Console.WriteLine(
                //    "Saved {0} entities to the database, press any key to exit.",
                //    recordsAffected);

                //Console.ReadKey();
            }
        }

        [Test]
        public void TestConversionToPersistenceDataModel()
        {
            // TODO: make this machine independent. Needs an SQL server for tests
            if (Environment.MachineName.ToLower() != "chrome-bu")
                return;
            var optResults = new MockOptResults();
            optResults.objectives = new List<IObjectiveScores<TestHyperCube>>
            {
                new MockObjScores {scores = new List<MockObjScore>
                    {
                        new MockObjScore {maximize = true, name = "NSE", value = 0.5},
                        new MockObjScore {maximize = false, name = "Bias", value = 0.55}
                    },
                    sysConfig = new TestHyperCube(3, 0.1, 0, 1) },
                new MockObjScores {scores = new List<MockObjScore>
                    {
                        new MockObjScore {maximize = true, name = "NSE", value = 0.6},
                        new MockObjScore {maximize = false, name = "Bias", value = 0.66}
                    },
                    sysConfig = new TestHyperCube(3, 0.3, 0, 1) }
            };
            var attributes = new Dictionary<string, string>();
            attributes.Add("ModelId", "GR4J");

            var result = ConvertOptimizationResults.Convert(optResults, resultsSetName: "result set name", attributes: attributes);

            Assert.AreEqual("result set name", result.Name);

            var tags = result.Tags.Tags.ToList();
            Assert.AreEqual(1, tags.Count);
            Assert.AreEqual("ModelId", tags[0].Name);
            Assert.AreEqual("GR4J", tags[0].Value);

            var resultsList = result.ScoresSet.ToList();
            var score = resultsList[0].Scores.ToList()[0];
            Assert.AreEqual("0.5", score.Value);
            Assert.AreEqual("NSE", score.Name);
            Assert.AreEqual(true, score.Maximize);

            score = resultsList[1].Scores.ToList()[1];
            Assert.AreEqual("0.66", score.Value);
            Assert.AreEqual("Bias", score.Name);
            Assert.AreEqual(false, score.Maximize);

            var varList = ((HyperCube)resultsList[0].SysConfiguration).Variables.ToList();
            Assert.AreEqual(3, varList.Count);
            Assert.AreEqual(0.1f, varList[0].Value);
            Assert.AreEqual(0f, varList[0].Minimum);
            Assert.AreEqual(1f, varList[0].Maximum);
    
        }

        private class MockOptResults : IOptimizationResults<TestHyperCube>
        {
            public List<IObjectiveScores<TestHyperCube>> objectives;
            public IEnumerator<IObjectiveScores> GetEnumerator()
            {
                return objectives.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

        }

        private class MockObjScores : IObjectiveScores<TestHyperCube>
        {
            public List<MockObjScore> scores;
            public TestHyperCube sysConfig;
            public IObjectiveScore GetObjective(int i)
            {
                return scores[i];
            }

            public ISystemConfiguration GetSystemConfiguration()
            {
                return sysConfig;
            }

            public int ObjectiveCount
            {
                get { return this.scores.Count; }
            }

            public TestHyperCube SystemConfiguration
            {
                get { return sysConfig; }
            }
        }

        private class MockObjScore : IObjectiveScore
        {
            public bool maximize;
            public string name;
            public double value;

            public string GetText()
            {
                throw new NotImplementedException();
            }

            public bool Maximise
            {
                get { return maximize; }
            }

            public string Name
            {
                get { return name; }
            }

            public IComparable ValueComparable
            {
                get { return value; }
            }
        }
    }
}
