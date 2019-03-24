using System;
using EnvModellingSample;
using CSIRO.Metaheuristics;
using CSIRO.Metaheuristics.Optimization;
using CSIRO.Metaheuristics.RandomNumberGenerators;
using CSIRO.Metaheuristics.Utils;
using CSIRO.Modelling.Core;

namespace AWBM_URS
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            /*
             * The context of this tutorial is that you have an existing time stepping simulation
             * model. 
             */
            IClonableObjectiveEvaluator<IHyperCube<double>> evaluator;
            IEvolutionEngine<IHyperCube<double>> uniformRandomSampling;


            var simulation = SimulationFactory.CreateAwbmSimulation();
            var data = DataHandling.GetSampleClimate();
            SimulationFactory.SetSampleSimulation(simulation, data);
            int from = simulation.GetStart();
            int to = simulation.GetEnd();

            evaluator = BuildUrsEvaluator(simulation, data.Runoff, from, to);

            var paramSpace = CreateFeasibleAwbmParameterSpace(simulation);
            uniformRandomSampling = new UniformRandomSampling<IHyperCube<double>>(evaluator, new BasicRngFactory(0), paramSpace, 3000);
            var ursResults = uniformRandomSampling.Evolve();
            Console.WriteLine(MetaheuristicsHelper.GetHumanReadable(ursResults));
        }

        public static IClonableObjectiveEvaluator<IHyperCube<double>> BuildUrsEvaluator(IModelSimulation<double[], double, int> simulation, double[] observation, int from, int to)
        {
            return ModellingSampleAdapter.OptimizationAdapter.BuildEvaluator(simulation, observation, from, to);
        }

        public static IHyperCube<double> CreateFeasibleAwbmParameterSpace(IModelSimulation<double[], double, int> simulation)
        {
            return ModellingSampleAdapter.OptimizationAdapter.BuildParameterSpace(simulation);
        }
    }
}
