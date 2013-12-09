using System;
using EnvModellingSample;
using CSIRO.Metaheuristics;
using CSIRO.Metaheuristics.SystemConfigurations;
using CSIRO.Metaheuristics.Optimization;
using CSIRO.Metaheuristics.RandomNumberGenerators;
using CSIRO.Metaheuristics.Utils;
using CSIRO.Metaheuristics.Objectives;
using System.IO;
using System.Collections.Generic;

namespace AWBM_URS
{
    class MainClass
    {
        public static void Main (string[] args)
        {
            /*
             * The context of this tutorial is that you have an existing time stepping simulation
             * model. 
             */ 
            IClonableObjectiveEvaluator<BasicHyperCube> evaluator;
            IEvolutionEngine<BasicHyperCube> uniformRandomSampling;


            var simulation = SimulationFactory.CreateAwbmSimulation ();
            double[][] data = parse ("/home/per202/src/mh/MH/trunk/Solutions/Metaheuristics/Tutorials/AWBM_URS/CatData.csv");
            simulation.Play ("Rainfall", data [0]);
            simulation.Play ("Evapotranspiration", data [1]);

            var startTs = new DateTime (1980, 01, 01);
            int from = (new DateTime (1985, 01, 01) - startTs).Days;
            int to = (new DateTime (1999, 12, 31) - startTs).Days;
            simulation.SetTimeSpan (0, to);
            simulation.Record ("Runoff");

            evaluator = buildEvaluator(simulation, data[2], from, to);

            var paramSpace = createFeasibleParameterSpace ();
            uniformRandomSampling = new UniformRandomSampling<BasicHyperCube> (evaluator, new BasicRngFactory (0), paramSpace, 3000);
            var ursResults = uniformRandomSampling.Evolve ();
            Console.WriteLine (MetaheuristicsHelper.GetHumanReadable(ursResults));
        }

        static IClonableObjectiveEvaluator<BasicHyperCube> buildEvaluator (IModelSimulation simulation, double[] observation, int from, int to)
        {
            return new SumSquareRunoffEvaluator (simulation, observation, from, to);
        }

        private class SumSquareRunoffEvaluator : IClonableObjectiveEvaluator<BasicHyperCube>
        {
            public SumSquareRunoffEvaluator (IModelSimulation simulation, double[] observedData, int from, int to)
            {
                this.simulation = simulation;
                this.observedData = observedData;
                this.from = from;
                this.to = to;
            }

            IModelSimulation simulation;
            double[] observedData;
            int from, to;

            public IClonableObjectiveEvaluator<BasicHyperCube> Clone ()
            {
                throw new NotImplementedException ();
            }

            public IObjectiveScores<BasicHyperCube> EvaluateScore (BasicHyperCube systemConfiguration)
            {
                // What follows needs explanation. 
                // systemConfiguration is general purpose; we need to find the appropriate model itself.
                systemConfiguration.ApplyConfiguration (((ModelSimulation)simulation).TsModel);
                simulation.Execute ();
                double result = sumSquares (simulation.GetRecorded("Runoff"), this.observedData);
                return MetaheuristicsHelper.CreateSingleObjective (systemConfiguration, result, "Sum Squares");
            }

            double sumSquares (double[] calculated, double[] observedData)
            {
                double res = 0, d;
                for (int i = from; i < to; i++) {
                    if (observedData [i] < 0)
                        continue;
                    d = calculated [i] - observedData [i];
                    res += d * d;
                }
                return res;
            }

            public bool SupportsDeepCloning {
                get {
                    return false;
                }
            }

            public bool SupportsThreadSafeCloning {
                get {
                    return false;
                }
            }
        }

        static BasicHyperCube createFeasibleParameterSpace ()
        {
            BasicHyperCube paramSpace = new BasicHyperCube (new[] {
                "C1",
                "C2",
                "C3",
                "BFI",
                "KBase",
                "KSurf"
            });
            paramSpace.SetMinMaxValue ("C1", 0.0, 50.0, 7.0);
            paramSpace.SetMinMaxValue ("C2", 0.0, 200.0, 20.0);
            paramSpace.SetMinMaxValue ("C3", 0.0, 1000.0, 50.0);
            paramSpace.SetMinMaxValue ("BFI", 0.01, 0.99, 0.7);
            paramSpace.SetMinMaxValue ("KBase", 0.01, 0.99, 0.3);
            paramSpace.SetMinMaxValue ("KSurf", 0.01, 0.99, 0.9);
            return paramSpace;
        }

        static double[][] parse (string file)
        {
            List<double[]> result = new List<double[]> ();
            using (var f = File.OpenText (file)) {
                var line = f.ReadLine ();
                while (!string.IsNullOrEmpty(line)) {
                    var items = line.Split (',');
                    double [] values = Array.ConvertAll (items, parseDouble);
                    if (values.Length != 3)
                        throw new ArgumentException ();
                    result.Add (values);
                    line = f.ReadLine ();
                }
            }
            int n = result.Count;
            var res = new double[][] {new double[n],new double[n],new double[n]};
            for (int i = 0; i < n; i++) {
                for (int j = 0; j < 3; j++) {
                    res [j] [i] = result [i] [j];
                }
            }
            return res;
        }
        static double parseDouble(string str)
        {
            if (str.ToLower () == "na")
                return -9999;
            else
                return double.Parse(str);
        }
    }
}
