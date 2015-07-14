using CSIRO.Metaheuristics.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.R.Pkgs.Tests
{
    public class TestCases
    {

        // WORKAROUND : rClr 0.7-4 converts arrays to R Lists. in this instance this is 
        // in the way of this unit test.
        private class ObjResultsWrapper : IEnumerable<IObjectiveScores>
        {
            public ObjResultsWrapper(IEnumerable<IObjectiveScores> data)
            {
                this.data = data;
            }
            IEnumerable<IObjectiveScores> data;
            public IEnumerator<IObjectiveScores> GetEnumerator()
            {
                return data.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return data.GetEnumerator();
            }
        }

        public static IEnumerable<IObjectiveScores> IdentityScores(params TestHyperCube[] inPoints)
        {
            var evaluator = new IdentityObjEval();
            return new ObjResultsWrapper(Array.ConvertAll(inPoints.ToArray(), (x => (IObjectiveScores)evaluator.EvaluateScore(x))));
        }

        //static TestHyperCube CreateTestHc(params double[] coords)
        //{
        //    var inPoints = new List<TestHyperCube>();
        //    for (int i = 0; i < coords.Length; i++)
        //    {
        //        inPoints[i] = TestHyperCube.CreatePoint(0, 0, 1, coords)
        //    }
        //    inPoints.Add(createPoint(1.0, 2.0, 3.0));
        //    inPoints.Add(createPoint(1.0, 2.2, 4.0));
        //    inPoints.Add(createPoint(2.0, 3.0, 5.0));
        //    return inPoints.ToArray();
        //}

    }
}
