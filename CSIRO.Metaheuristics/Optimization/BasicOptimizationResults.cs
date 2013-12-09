using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Optimization
{
    public class BasicOptimizationResults<TSysConfig> : IOptimizationResults<TSysConfig> where TSysConfig: ISystemConfiguration
    {
        //I know adapting all kinds of optimization results into one type may not be a good idea.
        //It is just a temporal solution. I will discuss it with JM later.

        public IObjectiveScores[] ObjectiveScores { get; private set; }

        public BasicOptimizationResults( IObjectiveScores[] objectiveScores )
        {
            this.ObjectiveScores = objectiveScores;
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("\n--------------------------------------------------------------\n");
            for(int i = 0; i < ObjectiveScores.Length; i ++)
            {
                for(int k = 0; k < ObjectiveScores[i].ObjectiveCount; k ++)
                {
                    output.Append(ObjectiveScores[i].GetObjective(k).GetText() + "\t");
                }

                IHyperCube<double> pSet = (IHyperCube<double>) ObjectiveScores[i].GetSystemConfiguration();
                string[] keyNames = pSet.GetVariableNames();
                foreach (string key in keyNames)
                {
                    output.Append("\n" + key + "\t\t" + pSet.GetValue(key));
                }
                output.Append("\n\n");
            }

            return output.ToString();
        }

        private IObjectiveScores[] GetAll( )
        {
            return (IObjectiveScores[]) this.ObjectiveScores.Clone( );
        }


        #region IEnumerable<IObjectiveScores> Members

        public IEnumerator<IObjectiveScores> GetEnumerator( )
        {
            return ( (IEnumerable<IObjectiveScores>)GetAll( ) ).GetEnumerator( );
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( )
        {
            return GetAll( ).GetEnumerator( );
        }

        #endregion
    }
}
