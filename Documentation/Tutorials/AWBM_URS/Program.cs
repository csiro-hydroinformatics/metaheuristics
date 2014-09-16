using System;
using EnvModellingSample;
using CSIRO.Metaheuristics;
using CSIRO.Metaheuristics.SystemConfigurations;
using CSIRO.Metaheuristics.Optimization;
using CSIRO.Metaheuristics.RandomNumberGenerators;
using CSIRO.Metaheuristics.Utils;
using System.IO;
using System.Collections.Generic;
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
            IClonableObjectiveEvaluator<BasicHyperCube> evaluator;
            IEvolutionEngine<BasicHyperCube> uniformRandomSampling;


            var simulation = SimulationFactory.CreateAwbmSimulation();
            var data = DataHandling.GetSampleClimate();
            SimulationFactory.SetSampleSimulation(simulation, data);
            int from = simulation.GetStart();
            int to = simulation.GetEnd();

            evaluator = BuildUrsEvaluator(simulation, data.Runoff, from, to);

            var paramSpace = CreateFeasibleAwbmParameterSpace();
            uniformRandomSampling = new UniformRandomSampling<BasicHyperCube>(evaluator, new BasicRngFactory(0), paramSpace, 3000);
            var ursResults = uniformRandomSampling.Evolve();
            Console.WriteLine(MetaheuristicsHelper.GetHumanReadable(ursResults));
        }

        public static IClonableObjectiveEvaluator<BasicHyperCube> BuildUrsEvaluator(IModelSimulation<double[], double, int> simulation, double[] observation, int from, int to)
        {
            return new SumSquareRunoffEvaluator(simulation, observation, from, to);
        }

        private class SumSquareRunoffEvaluator : IClonableObjectiveEvaluator<BasicHyperCube>
        {
            public SumSquareRunoffEvaluator(IModelSimulation<double[], double, int> simulation, double[] observedData, int from, int to)
            {
                this.simulation = simulation;
                this.observedData = observedData;
                this.from = from;
                this.to = to;
            }

            IModelSimulation<double[], double, int> simulation;
            double[] observedData;
            int from, to;

            public IClonableObjectiveEvaluator<BasicHyperCube> Clone()
            {
                throw new NotImplementedException();
            }

            public IObjectiveScores<BasicHyperCube> EvaluateScore(BasicHyperCube systemConfiguration)
            {
                // What follows needs explanation. 
                // systemConfiguration is general purpose; we need to find the appropriate model itself.
                systemConfiguration.ApplyConfiguration(((ModelSimulation)simulation).TsModel);
                simulation.Execute();
                double result = sumSquares(simulation.GetRecorded("Runoff"), this.observedData);
                return MetaheuristicsHelper.CreateSingleObjective(systemConfiguration, result, "Sum Squares");
            }

            double sumSquares(double[] calculated, double[] observedData)
            {
                double res = 0, d;
                for (int i = from; i < to; i++)
                {
                    if (observedData[i] < 0)
                        continue;
                    d = calculated[i] - observedData[i];
                    res += d * d;
                }
                return res;
            }

            public bool SupportsDeepCloning
            {
                get
                {
                    return false;
                }
            }

            public bool SupportsThreadSafeCloning
            {
                get
                {
                    return false;
                }
            }
        }

        public static BasicHyperCube CreateFeasibleAwbmParameterSpace()
        {
            BasicHyperCube paramSpace = new BasicHyperCube(new[] {
                "C1",
                "C2",
                "C3",
                "BFI",
                "KBase",
                "KSurf"
            });
            paramSpace.SetMinMaxValue("C1", 0.0, 50.0, 7.0);
            paramSpace.SetMinMaxValue("C2", 0.0, 200.0, 20.0);
            paramSpace.SetMinMaxValue("C3", 0.0, 1000.0, 50.0);
            paramSpace.SetMinMaxValue("BFI", 0.01, 0.99, 0.7);
            paramSpace.SetMinMaxValue("KBase", 0.01, 0.99, 0.3);
            paramSpace.SetMinMaxValue("KSurf", 0.01, 0.99, 0.9);
            return paramSpace;
        }
    }
}
