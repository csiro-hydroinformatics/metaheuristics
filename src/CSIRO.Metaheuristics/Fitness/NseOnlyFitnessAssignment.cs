using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Fitness
{
    public class NseOnlyFitnessAssignment : IFitnessAssignment<double>
    {
        public NseOnlyFitnessAssignment()
        {

        }
        public FitnessAssignedScores<double>[] AssignFitness(IObjectiveScores[] scores)
        {
            FitnessAssignedScores<double>[] result = new FitnessAssignedScores<double>[scores.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new FitnessAssignedScores<double>(scores[i], (- (double)scores[i].GetObjective(0).ValueComparable));
            }

            return result;
        }
    }
}
