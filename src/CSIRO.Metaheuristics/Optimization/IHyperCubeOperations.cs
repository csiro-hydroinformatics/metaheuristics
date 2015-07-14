using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Optimization
{
    public interface IHyperCubeOperations
    {
        IHyperCube<double> GetCentroid( IHyperCube<double>[] points );
        IHyperCube<double> GenerateRandomWithinHypercube( IHyperCube<double>[] points );
        IHyperCube<double> GenerateRandom( IHyperCube<double> point );
    }

    public interface IHyperCubeOperationsFactory
    {
        IHyperCubeOperations CreateNew( IRandomNumberGeneratorFactory rng );
    }

}
