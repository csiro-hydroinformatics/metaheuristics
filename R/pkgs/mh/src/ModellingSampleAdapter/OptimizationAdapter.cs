using CSIRO.Metaheuristics;
using CSIRO.Metaheuristics.SystemConfigurations;
using CSIRO.Metaheuristics.Utils;
using CSIRO.Modelling.Core;
using System;

namespace ModellingSampleAdapter
{
    /// <summary>
    /// A class that wraps the tutorial's modelling system with constructs for calibration with this metaheuristics framework. 
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class OptimizationAdapter
    {
        public static IClonableObjectiveEvaluator<IHyperCube<double>> BuildEvaluator(IModelSimulation<double[], double, int> simulation, double[] observation, int from, int to, string statisticsId="ss")
        {
            if (statisticsId != "ss")
                throw new NotSupportedException("Only the sum of squared differences objective function is supported in this sample code...");
            return new SumSquareRunoffEvaluator(simulation, observation, from, to);
        }

        private class SumSquareRunoffEvaluator : IClonableObjectiveEvaluator<IHyperCube<double>>
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

            public IClonableObjectiveEvaluator<IHyperCube<double>> Clone()
            {
                return new SumSquareRunoffEvaluator(
                    simulation.Clone(),
                    observedData,
                    from,
                    to
                    );
            }

            public IObjectiveScores<IHyperCube<double>> EvaluateScore(IHyperCube<double> systemConfiguration)
            {
                apply(systemConfiguration, simulation);
                simulation.Execute();
                double result = sumSquares(simulation.GetRecorded("Runoff"), this.observedData);
                return MetaheuristicsHelper.CreateSingleObjective(systemConfiguration, result, "Sum Squares");
            }

            private void apply(IHyperCube<double> systemConfiguration, IModelSimulation<double[], double, int> simulation)
            {
                var varNames = systemConfiguration.GetVariableNames();
                foreach (var varName in varNames)
                {
                    simulation.SetVariable(varName, systemConfiguration.GetValue(varName));
                }
            }

            double sumSquares(double[] calculated, double[] observedData)
            {
                double res = 0, d;
                for (int i = from; i < to; i++)
                {
                    if (double.IsNaN(observedData[i]) || observedData[i] < 0)
                        continue;
                    d = calculated[i] - observedData[i];
                    res += d * d;
                }
                return res;
            }

            public static bool PermitMultiThreading = true;

            public bool SupportsDeepCloning
            {
                get
                {
                    return (PermitMultiThreading && simulation.SupportsDeepCloning);
                }
            }

            public bool SupportsThreadSafeCloning
            {
                get
                {
                    return (PermitMultiThreading && simulation.SupportsDeepCloning);
                }
            }
        }

        public static IHyperCube<double> BuildParameterSpace(IModelSimulation<double[], double, int> simulation)
        {
            // TODO there could be a protocol to at least verify that the parameters defined are compatible with a given simulation. 
            // However this is a lot of work for a very generic system.
            var paramSpace = new BasicHyperCube(new[] {
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
