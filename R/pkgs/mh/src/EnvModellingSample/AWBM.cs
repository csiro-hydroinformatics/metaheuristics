using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EnvModellingSample
{
    /// <summary>
    /// An implementation of the Australian Water Balance Model - for tutorial documentation purpose only.
    /// </summary>
    /// <remarks>
    /// This code is solely for use in tutorial documentation. Do not use in any other context.
    /// </remarks>
    public class AWBM : ITimeStepModel
    {
        public AWBM()
        {
            partialAreas = new double[] { 0.134, 0.433, 0.433 };
            capacities = new double[] { 7, 70, 150 };
            Store = new double[] { 0, 0, 0 };
            PartialExcess = new double[] { 0, 0, 0 };
            BFI = 0.35;
            KBase = 0.95;
            KSurf = 0.35;
        }

        // Inputs
        public double Rainfall, Evapotranspiration;
        // Primary outputs
        public double Runoff, Baseflow;

        // State values, can be considered as outputs depending on the modelling objective.
        private double EffectiveRainfall;
        private double Excess;
        private double[] Store, PartialExcess, capacities, partialAreas;
        private double BaseflowRecharge;
        private double SurfaceRunoff;
        private double BaseflowStore;
        private double SurfaceStore;
        private double RoutedSurfaceRunoff;

        // Parameters
        public double BFI { get; set; }
        public double KSurf { get; set; }
        public double KBase { get; set; }
        public double C1 { get { return capacities[0]; } set { capacities[0] = value; } }
        public double C2 { get { return capacities[1]; } set { capacities[1] = value; } }
        public double C3 { get { return capacities[2]; } set { capacities[2] = value; } }


        public void RunOneTimeStep()
        {
            EffectiveRainfall = Math.Max(0.0, (Rainfall - Evapotranspiration));

            Excess = 0.0;
            for (int i = 0; i < 3; i++)
            {
                double prevS = Store[i];
                PartialExcess[i] = 0.0;
                Store[i] += (Rainfall - Evapotranspiration);
                if (Store[i] >= capacities[i])
                {
                    PartialExcess[i] = ((Store[i] - capacities[i]) * partialAreas[i]);
                    Store[i] = capacities[i];
                    Excess += PartialExcess[i];
                }
                if (Store[i] < 0.0)
                {
                    Store[i] = 0.0;
                }
            }

            BaseflowRecharge = BFI * Excess;
            SurfaceRunoff = (1 - BFI) * Excess;

            BaseflowStore += BaseflowRecharge;
            SurfaceStore += SurfaceRunoff;

            RoutedSurfaceRunoff = (1 - KSurf) * SurfaceStore;
            Baseflow = (1 - KBase) * BaseflowStore;

            BaseflowStore *= (KBase);
            SurfaceStore *= (KSurf);

            Runoff = Baseflow + RoutedSurfaceRunoff;

        }


        public void Reset()
        {
            // inputs
            Rainfall = 0;
            Evapotranspiration = 0;
            // Outputs
            Runoff = 0;
            Baseflow = 0;
            // states
            EffectiveRainfall = 0;
            Excess = 0;
            BaseflowRecharge = 0;
            SurfaceRunoff = 0;
            BaseflowStore = 0;
            SurfaceStore = 0;
            RoutedSurfaceRunoff = 0;
            for (int i = 0; i < Store.Length; i++)
            {
                Store[i] = 0;
                PartialExcess[i] = 0;
            }
        }


        public ITimeStepModel Clone()
        {
            lock (this)
            {
                var res = (AWBM)this.MemberwiseClone();
                res.Store = (double[])this.Store.Clone();
                res.PartialExcess = (double[])this.PartialExcess.Clone();
                res.partialAreas = (double[])this.partialAreas.Clone();
                res.capacities = (double[])this.capacities.Clone();
                return res;
            }
        }


        public bool IsClonable
        {
            get { return true; }
        }
    }
}
