using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics;
using CSIRO.Metaheuristics.Logging;
using RDotNet;
using CSIRO.Metaheuristics.SystemConfigurations;
using System.Collections;

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

        public static DataFrame AsDataFrame(IEnumerable<IObjectiveScores> objScores, bool stringsAsFactors = true)
        {
            var e = REngine.GetInstance();

            IObjectiveScores[] results = objScores.ToArray();
            List<IEnumerable> columns = new List<IEnumerable>();
            var colNames = new List<string>();
            if (results.Length == 0)
            {
                colNames.Add("empty.result.set");
                columns.Add(new double[] { });
            }
            else
            {
                var objValues = new Dictionary<string, double[]>();
                var scoreNames = new List<string>();
                var firstResult = results.First();
                for (int i = 0; i < firstResult.ObjectiveCount; i++)
                    scoreNames.Add(firstResult.GetObjective(i).Name);

                var pNames = new List<string>();

                colNames.AddRange(scoreNames);
                foreach (var name in scoreNames)
                {
                    var c = new double[results.Length];
                    columns.Add(c);
                    objValues.Add(name, c);
                }

                // Add objective values
                for (int i = 0; i < results.Length; i++)
                {
                    var obj = results[i];
                    for (int j = 0; j < obj.ObjectiveCount; j++)
                    {
                        var objVal = obj.GetObjective(j);
                        var d = (double)objVal.ValueComparable;
                        objValues[objVal.Name][i] = d; // objVal.Maximise ? -d : d;
                    }
                }

                var hc = firstResult.GetSystemConfiguration() as IHyperCube<double>;
                if (hc != null)
                {
                    var pValues = new Dictionary<string, double[]>();
                    pNames.AddRange(hc.GetVariableNames());
                    colNames.AddRange(pNames);
                    foreach (var name in pNames)
                    {
                        var c = new double[results.Length];
                        columns.Add(c);
                        pValues.Add(name, c);
                    }
                    for (int i = 0; i < results.Length; i++)
                    {
                        var obj = results[i];
                        hc = (IHyperCube<double>)obj.GetSystemConfiguration();
                        foreach (var name in pNames)
                            pValues[name][i] = hc.GetValue(name);
                    }
                }
                else
                {
                    colNames.Add("SysConfig");
                    var c = new string[results.Length];
                    for (int i = 0; i < results.Length; i++)
                    {
                        var obj = results[i];
                        c[i] = obj.GetSystemConfiguration().ToString();
                    }
                    columns.Add(c);
                }
            }
            return e.CreateDataFrame(columns.ToArray(), colNames.ToArray(), stringsAsFactors: stringsAsFactors);
        }
    }
}
