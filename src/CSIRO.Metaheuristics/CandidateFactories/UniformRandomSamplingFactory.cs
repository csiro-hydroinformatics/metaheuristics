using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.Optimization;
using CSIRO.Metaheuristics.SystemConfigurations;

namespace CSIRO.Metaheuristics.CandidateFactories
{
    public class UniformRandomSamplingFactory<TSysConfig> : ICandidateFactory<TSysConfig>, IHyperCubeOperationsFactory 
        where TSysConfig : IHyperCube<double>, ICloneableSystemConfiguration
    {
        public UniformRandomSamplingFactory( IRandomNumberGeneratorFactory rng, TSysConfig template)
        {
            this.rng = rng;
            if( !template.SupportsThreadSafeCloning )
                throw new ArgumentException( "This URS factory requires cloneable and thread-safe system configurations" );
            this.template = template;
            this.hcOps = CreateIHyperCubeOperations( );
        }
        IRandomNumberGeneratorFactory rng;
        TSysConfig template;
        IHyperCubeOperations hcOps;

        private IHyperCubeOperations CreateIHyperCubeOperations( )
        {
            return new HyperCubeOperations( rng.CreateFactory( ) );
        }
        public IHyperCubeOperations CreateNew( IRandomNumberGeneratorFactory rng )
        {
            return new HyperCubeOperations( rng.CreateFactory( ) );
        }

        public TSysConfig CreateRandomCandidate( )
        {
            return (TSysConfig) hcOps.GenerateRandom( template );
        }
    }
}
