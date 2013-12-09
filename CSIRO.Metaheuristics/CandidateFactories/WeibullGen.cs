using System;
using CSIRO.Metaheuristics.Optimization;
using CSIRO.Metaheuristics.SystemConfigurations;

namespace CSIRO.Metaheuristics.CandidateFactories
{
    public class WeibullGen<TSysConfig> : ICandidateFactory<TSysConfig>, IHyperCubeOperationsFactory
        where TSysConfig : IHyperCube<double>, ICloneableSystemConfiguration
    {
        private readonly Random random;
        private readonly TSysConfig template;

        public WeibullGen(IRandomNumberGeneratorFactory rng, TSysConfig template)
        {
            this.template = template;
            random = rng.CreateRandom();
        }

        #region ICandidateFactory<TSysConfig> Members

        public TSysConfig CreateRandomCandidate()
        {
            var result = (TSysConfig) template.Clone();
            var varNames = result.GetVariableNames();
            foreach (var v in varNames)
            {
                result.SetValue(v, CreateValue(result.GetMinValue(v), result.GetMaxValue(v), template.GetValue(v)));
            }
            return result;
        }

        #endregion

        #region IHyperCubeOperationsFactory Members

        public IHyperCubeOperations CreateNew(IRandomNumberGeneratorFactory rng)
        {
            return new HyperCubeOperations(rng.CreateFactory());
        }

        #endregion

        private double CreateValue(double min, double max, double seedValue)
        {
            double r = random.NextDouble();
            double v = Math.Sqrt(1.2*(-Math.Log(r*.99999 + .000005)));
            // ??? double range = max - min;
            double result = v*seedValue;
            return Math.Max(min, Math.Min(max, result));
        }
    }
}