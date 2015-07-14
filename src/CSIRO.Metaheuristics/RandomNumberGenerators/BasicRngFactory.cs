using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.RandomNumberGenerators
{
    public class BasicRngFactory : IRandomNumberGeneratorFactory
    {
        public BasicRngFactory( int seed )
        {
            random = new Random( seed );
        }
        private Random random;

        public Random CreateRandom( )
        {
            return new Random( random.Next( ) );
        }

        public IRandomNumberGeneratorFactory CreateFactory( )
        {
            return new BasicRngFactory( random.Next( ) );
        }

        public int Next( )
        {
            return random.Next( );
        }
    }
}
