using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EnvModellingSample
{
    public class SimulationFactory
    {
        public static IModelSimulation CreateAwbmSimulation()
        {
            return new ModelSimulation(new AWBM());
        }
    }
}
