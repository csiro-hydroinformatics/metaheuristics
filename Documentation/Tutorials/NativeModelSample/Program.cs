﻿using System;
using CSIRO.Metaheuristics;
using CSIRO.Metaheuristics.Optimization;
using CSIRO.Metaheuristics.RandomNumberGenerators;
using CSIRO.Metaheuristics.Utils;
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
            IClonableObjectiveEvaluator<IHyperCube<double>> evaluator;
            IEvolutionEngine<IHyperCube<double>> uniformRandomSampling;

            NativeModelWrapper.AwbmWrapper.PermitMultiThreading = true;

            using (var simulation = new AwbmWrapper())
            {
                var data = DataHandling.GetSampleClimate();
                SimulationFactory.SetSampleSimulation(simulation, data);
                int from = simulation.GetStart();
                int to = simulation.GetEnd();

                evaluator = AWBM_URS.MainClass.BuildUrsEvaluator(simulation, data.Runoff, from, to);

                var paramSpace = AWBM_URS.MainClass.CreateFeasibleAwbmParameterSpace(simulation);
                uniformRandomSampling = new UniformRandomSampling<IHyperCube<double>>(evaluator, new BasicRngFactory(0), paramSpace, 3000);
                var ursResults = uniformRandomSampling.Evolve();
                Console.WriteLine(MetaheuristicsHelper.GetHumanReadable(ursResults));
            }
        }
    }
}
