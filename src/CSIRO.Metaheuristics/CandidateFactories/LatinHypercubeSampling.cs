using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.Optimization;
using CSIRO.Metaheuristics.SystemConfigurations;

namespace CSIRO.Metaheuristics.CandidateFactories
{
    public class LatinHypercubeSampling<TSysConfig> : ICandidateFactory<TSysConfig>, IHyperCubeOperationsFactory 
        where TSysConfig : IHyperCube<double>, ICloneableSystemConfiguration
    {
        public LatinHypercubeSampling(IRandomNumberGeneratorFactory rng, TSysConfig template, int nDiv=5)
        {
            this.rng = rng;
            this.rand = rng.CreateRandom();
            if( !template.SupportsThreadSafeCloning )
                throw new ArgumentException( "This URS factory requires cloneable and thread-safe system configurations" );
            this.template = template;
            this.hcOps = new HyperCubeOperations(rng.CreateFactory());
            this.nDiv = nDiv;
        }
        IRandomNumberGeneratorFactory rng;
        TSysConfig template;
        IHyperCubeOperations hcOps;
        private Random rand;
        private int nDiv;

        public TSysConfig CreateRandomCandidate()
        {
            var result = (TSysConfig)this.template.Clone();
            var varNames = result.GetVariableNames();
            for (int i = 0; i < varNames.Length; i++)
            {
                var v = varNames[i];
                result.SetValue(v, createValue(result.GetMinValue(v), result.GetMaxValue(v)));
            }
            return result;
        }

        private double createValue(double pMin, double pMax)
        {
            int r = rand.Next(0, nDiv - 1);
            double delta = (pMax - pMin);
            double d = (delta/nDiv);
            double pMin_cell = pMin + r * d;
            //double pMax_cell = pMin + d + delta;
            return pMin_cell + d * rand.NextDouble();
        }

        public IHyperCubeOperations CreateNew(IRandomNumberGeneratorFactory rng)
        {
            return new HyperCubeOperations(rng.CreateFactory());
        }
    }
}
