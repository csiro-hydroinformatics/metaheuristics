using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.Objectives;

namespace CSIRO.Metaheuristics.Fitness
{
    public class ZitlerThieleFitnessAssignment : IFitnessAssignment<double>
    {
        public ZitlerThieleFitnessAssignment( )
        {

        }
        public FitnessAssignedScores<double>[] AssignFitness( IObjectiveScores[] scores )
        {
            var paretoRanking = new ParetoRanking<IObjectiveScores>( scores );
            IObjectiveScores[] nonDominated = paretoRanking.GetParetoRank( 1 );
            IObjectiveScores[] dominated = paretoRanking.GetDominatedByParetoRank( 1 );
            double[] fitnessesNonDominated = new double[nonDominated.Length];
            double[] fitnessesDominated = new double[dominated.Length];
            for( int j = 0; j < nonDominated.Length; j++ )
                fitnessesNonDominated[j] = (double)paretoRanking.GetNumDominated( nonDominated[j], dominated ) / scores.Length;
            for( int j = 0; j < dominated.Length; j++ )
            {
                double fitness = 1.0;
                for( int k = 0; k < nonDominated.Length; k++ )
                {
                    if( paretoRanking.IsDominated( dominated[j], nonDominated[k] ) )
                        fitness += fitnessesNonDominated[k];
                }
                fitnessesDominated[j] = fitness;
            }
            List<double> fitnesses = new List<double>( );
            fitnesses.AddRange( fitnessesNonDominated );
            fitnesses.AddRange( fitnessesDominated );
            List<IObjectiveScores> orderedScores = new List<IObjectiveScores>( );
            orderedScores.AddRange( nonDominated );
            orderedScores.AddRange( dominated );
            FitnessAssignedScores<double>[] result = new FitnessAssignedScores<double>[fitnesses.Count];
            for( int i = 0; i < fitnesses.Count; i++ )
                result[i] = new FitnessAssignedScores<double>( orderedScores[i], fitnesses[i] );
            return result;
        }
    }
}
