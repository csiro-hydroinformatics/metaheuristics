using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics;
using CSIRO.Metaheuristics.SystemConfigurations;
using CSIRO.Metaheuristics.Optimization;
using CSIRO.Metaheuristics.RandomNumberGenerators;
using CSIRO.Metaheuristics.Utils;
using CSIRO.Modelling.Core;
using EnvModellingSample;
using NativeModelWrapper;

namespace NativeModelSample
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
             * This program primarily demonstrates how to wrap and calibrate a model written 
             * in a native language. While technological aspects of what it demonstrates 
             * are not inherently limited nor specific to optimisation, this is 
             * a use case often encountered (e.g. to calibrate existing models written in 
             * C++ , C, or Fortran)
             */
            IClonableObjectiveEvaluator<BasicHyperCube> evaluator;
            IEvolutionEngine<BasicHyperCube> uniformRandomSampling;

            using (var simulation = new AwbmWrapper())
            {
                var data = DataHandling.GetSampleClimate();
                SimulationFactory.SetSampleSimulation(simulation, data);
                int from = simulation.GetStart();
                int to = simulation.GetEnd();

                evaluator = AWBM_URS.MainClass.BuildUrsEvaluator(simulation, data.Runoff, from, to);

                var paramSpace = AWBM_URS.MainClass.CreateFeasibleAwbmParameterSpace();
                uniformRandomSampling = new UniformRandomSampling<BasicHyperCube>(evaluator, new BasicRngFactory(0), paramSpace, 3000);
                var ursResults = uniformRandomSampling.Evolve();
                Console.WriteLine(MetaheuristicsHelper.GetHumanReadable(ursResults));
            }
        }
    }
}
