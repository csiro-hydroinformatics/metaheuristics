using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Modelling.Core;

namespace EnvModellingSample
{
    public class SimulationFactory
    {
        public static IModelSimulation<double[], double, int> CreateAwbmSimulation()
        {
            return new ModelSimulation(new AWBM());
        }

        public static void SetSampleSimulation(IModelSimulation<double[], double, int> simulation, DataHandling.SampleClimate data)
        {
            simulation.Play("Rainfall", data.Rainfall);
            simulation.Play("Evapotranspiration", data.Evapotranspiration);

            var startTs = new DateTime(1980, 01, 01);
            int from = (new DateTime(1985, 01, 01) - startTs).Days;
            int to = (new DateTime(1999, 12, 31) - startTs).Days;
            
            simulation.SetSpan(0, to);
            simulation.Record("Runoff");
        }
    }
}
