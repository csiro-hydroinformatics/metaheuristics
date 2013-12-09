using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CSIRO.Metaheuristics.Objectives;
using CSIRO.Metaheuristics.Fitness;

namespace CSIRO.Metaheuristics.Tests
{
    [TestFixture]
    public class TestObjectives
    {
        [Test]
        public void TestParetoRanking()
        {
            ParetoComparer<MockDualObjective> comparer = new ParetoComparer<MockDualObjective>();

            MockDualObjective rank_1_a = new MockDualObjective(1, 1);
            MockDualObjective rank_1_b = new MockDualObjective(0, 2);
            MockDualObjective rank_1_c = new MockDualObjective(2, 0);
            MockDualObjective rank_2_a = new MockDualObjective(2, 2);
            MockDualObjective rank_2_b = new MockDualObjective(3, 1);
            MockDualObjective rank_2_c = new MockDualObjective(1, 3);
            MockDualObjective rank_3_a = new MockDualObjective(3, 3);
            MockDualObjective rank_3_b = new MockDualObjective(2, 4);

            checkParetoRankings(comparer, rank_1_a, rank_1_b, rank_1_c, rank_2_a, rank_2_b, rank_2_c, rank_3_a, rank_3_b);

            rank_1_a = new MockDualObjective(1, -1, true);
            rank_1_b = new MockDualObjective(0, -2, true);
            rank_1_c = new MockDualObjective(2, -0, true);
            rank_2_a = new MockDualObjective(2, -2, true);
            rank_2_b = new MockDualObjective(3, -1, true);
            rank_2_c = new MockDualObjective(1, -3, true);
            rank_3_a = new MockDualObjective(3, -3, true);
            rank_3_b = new MockDualObjective(2, -4, true);

            checkParetoRankings(comparer, rank_1_a, rank_1_b, rank_1_c, rank_2_a, rank_2_b, rank_2_c, rank_3_a, rank_3_b);
        }

        [Test]
        public void TestFitnessAssignment( )
        {
            MockDualObjective rank_1_a = new MockDualObjective( 0,2 );
            MockDualObjective rank_1_b = new MockDualObjective( 1,1 );
            MockDualObjective rank_1_c = new MockDualObjective( 2,0 );
            MockDualObjective d1 = new MockDualObjective( 1.5,1.6 );
            MockDualObjective d2 = new MockDualObjective( 2, 2 );
            MockDualObjective d3 = new MockDualObjective( 3, 1 );
            MockDualObjective d4 = new MockDualObjective( 3,3 );

            ZitlerThieleFitnessAssignment assignment = new ZitlerThieleFitnessAssignment( );
            var fittedScores = assignment.AssignFitness( new IObjectiveScores[]{d1, d2, d3,d4, rank_1_a, rank_1_b, rank_1_c} );
            Assert.AreEqual( 1.0 / 7, getFitness( fittedScores, rank_1_a ), 1e-12 );
            Assert.AreEqual( 3.0 / 7, getFitness( fittedScores, rank_1_b ), 1e-12 );
            Assert.AreEqual( 2.0 / 7, getFitness( fittedScores, rank_1_c ), 1e-12 );
            Assert.AreEqual( 10.0 / 7, getFitness( fittedScores, d1 ), 1e-12 );
            Assert.AreEqual( 10.0 / 7, getFitness( fittedScores, d2 ), 1e-12 );
            Assert.AreEqual( 9.0 / 7, getFitness( fittedScores, d3 ), 1e-12 );
            Assert.AreEqual( 13.0 / 7, getFitness( fittedScores, d4 ), 1e-12 );
        }

        private double getFitness( FitnessAssignedScores<double>[] fittedScores, MockDualObjective objective )
        {
            foreach( var item in fittedScores )
                if( item.Scores == objective )
                    return item.FitnessValue;
            throw new ArgumentException( "Objective not found in the list of points with associated fitnesses" );
        }
        private static void checkParetoRankings(ParetoComparer<MockDualObjective> comparer, MockDualObjective rank_1_a, MockDualObjective rank_1_b, MockDualObjective rank_1_c, MockDualObjective rank_2_a, MockDualObjective rank_2_b, MockDualObjective rank_2_c, MockDualObjective rank_3_a, MockDualObjective rank_3_b)
        {
            Assert.AreEqual(-1, comparer.Compare(rank_1_a, rank_2_a));
            Assert.AreEqual(+1, comparer.Compare(rank_2_a, rank_1_a));

            Assert.AreEqual(-1, comparer.Compare(rank_1_a, rank_3_b));

            Assert.AreEqual(0, comparer.Compare(rank_1_a, rank_1_a));
            Assert.AreEqual(0, comparer.Compare(rank_1_a, rank_1_b));
            Assert.AreEqual(0, comparer.Compare(rank_1_a, rank_1_c));
            Assert.AreEqual(0, comparer.Compare(rank_1_b, rank_1_c));

            MockDualObjective[] scores = new MockDualObjective[] { rank_2_c, rank_1_b, rank_3_b, rank_2_b, rank_1_a, rank_1_c, rank_3_a, rank_2_a };
            ParetoRanking<MockDualObjective> paretoRanking = new ParetoRanking<MockDualObjective>(scores, comparer);
            MockDualObjective[] rank1 = paretoRanking.GetParetoRank(1);
            Assert.AreEqual(3, rank1.Length);
            Assert.IsTrue(rank1.Contains(rank_1_a));
            Assert.IsTrue(rank1.Contains(rank_1_b));
            Assert.IsTrue(rank1.Contains(rank_1_c));
            MockDualObjective[] rank2 = paretoRanking.GetParetoRank(2);
            Assert.AreEqual(3, rank2.Length);
            Assert.IsTrue(rank2.Contains(rank_2_a));
            Assert.IsTrue(rank2.Contains(rank_2_b));
            Assert.IsTrue(rank2.Contains(rank_2_c));
            MockDualObjective[] rank3 = paretoRanking.GetParetoRank(3);
            Assert.AreEqual(2, rank3.Length);
            Assert.IsTrue(rank3.Contains(rank_3_a));
            Assert.IsTrue(rank3.Contains(rank_3_b));
        }

        private class MockDualObjective : IObjectiveScores
        {
            private MockObjectiveScore[] values;
            public MockDualObjective(double value1, double value2, bool maximizeValue2)
            {
                this.values = new MockObjectiveScore[] { new MockObjectiveScore(value1), new MockObjectiveScore(value2, maximizeValue2) };
            }

            public MockDualObjective(double value1, double value2)
                : this(value1, value2, false)
            {
            }
            public int ObjectiveCount
            {
                get { return values.Length; }
            }

            public IObjectiveScore GetObjective(int i)
            {
                return values[i];
            }

            public override string ToString()
            {
                return string.Concat("val1=", values[0].ValueComparable.ToString(), ", val2=", values[1].ValueComparable.ToString());
            }

            public ISystemConfiguration GetSystemConfiguration()
            {
                throw new NotImplementedException();
            }

            private class MockObjectiveScore : IObjectiveScore
            {
                private double value;
                private bool maximize = false;
                public MockObjectiveScore(double value)
                {
                    this.value = value;
                }
                public MockObjectiveScore(double value, bool maximize)
                {
                    this.value = value;
                    this.maximize = maximize;
                }
                public bool Maximise
                {
                    get { return this.maximize; }
                }

                public string GetText()
                {
                    throw new NotImplementedException();
                }

                public IComparable ValueComparable
                {
                    get { return value; }
                }

                public string Name
                {
                    get { throw new NotImplementedException( ); }
                }

            }

        }

    }
}
