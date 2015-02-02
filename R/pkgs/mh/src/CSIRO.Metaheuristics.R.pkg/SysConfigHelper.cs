using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics;
using CSIRO.Metaheuristics.Logging;
using RDotNet;

namespace CSIRO.Metaheuristics.R.Pkgs
{
    public static class SysConfigHelper
    {
        public static void SetHyperCube(IHyperCubeSetBounds<double> paramSet, string[] pnames, double[] vals, double[] mins = null, double[] maxs = null)
        {
            for (int i = 0; i < pnames.Length; i++)
            {
                paramSet.SetValue(pnames[i], vals[i]);
                if (mins != null)
                    paramSet.SetMinValue(pnames[i], mins[i]);
                if (maxs != null)
                    paramSet.SetMaxValue(pnames[i], maxs[i]);
            }
        }

        public static Type TypeofIHyperCubeSetBounds { get { return typeof(IHyperCubeSetBounds<double>); } }

        public static Type TypeofIHyperCube { get { return typeof(IHyperCube<double>); } }

        public static HyperCubeInterop ToDataFrame(IHyperCube<double> paramSet)
        {
            var names = paramSet.GetVariableNames();
            var r = new HyperCubeInterop(names.Length);
            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i];
                r.Name[i] = name;
                r.Description[i] = "";
                r.Value[i] = paramSet.GetValue(name);
                r.Min[i] = paramSet.GetMinValue(name);
                r.Max[i] = paramSet.GetMaxValue(name);
            }
            return r;
        }

        public class HyperCubeInterop
        {
            public string[] Name;
            public string[] Description;
            public double[] Value;
            public double[] Min;
            public double[] Max;

            public HyperCubeInterop(int p)
            {
                this.Name = new string[p];
                this.Description = new string[p];
                this.Min = new double[p];
                this.Max = new double[p];
                this.Value = new double[p];
            }
        }

        public static DataFrame GetContent(IEnumerable<ILogInfo> logInfo)
        {
            return logInfo.ToDataFrame();
        }
    }
}
