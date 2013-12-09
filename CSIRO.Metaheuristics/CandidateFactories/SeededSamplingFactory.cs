using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.Optimization;

namespace CSIRO.Metaheuristics.CandidateFactories
{
    public class SeededSamplingFactory<TSysConfig> : ICandidateFactory<TSysConfig>, IHyperCubeOperationsFactory
        where TSysConfig : IHyperCube<double>, ICloneableSystemConfiguration
    {
        private ICandidateFactory<TSysConfig> uniformRandomSamplingFactory;

        public SeededSamplingFactory(ICandidateFactory<TSysConfig> uniformRandomSamplingFactory, IEnumerable<TSysConfig> seeds)
        {
            this.HcFactory = (IHyperCubeOperationsFactory)uniformRandomSamplingFactory;
            this.uniformRandomSamplingFactory = uniformRandomSamplingFactory;
            this.seeds = seeds == null ? null : seeds.ToArray();
        }
        private int counterSeeds = 0;
        public TSysConfig CreateRandomCandidate()
        {
            if (this.seeds != null && (counterSeeds < this.seeds.Length))
            {
                counterSeeds++;
                return this.seeds[counterSeeds - 1];
            }
            else
                return uniformRandomSamplingFactory.CreateRandomCandidate();
        }
        private TSysConfig[] seeds;
        private IHyperCubeOperationsFactory HcFactory;

        public IHyperCubeOperations CreateNew(IRandomNumberGeneratorFactory rng)
        {
            return HcFactory.CreateNew(rng);
        }
    }
}
