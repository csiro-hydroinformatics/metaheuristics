using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EnvModellingSample
{
    public class DataHandling
    {
        public struct SampleClimate
        {
            public double[] Rainfall { get; internal set; }
            public double[] Evapotranspiration { get; internal set; }
            public double[] Runoff { get; internal set; }
        }
        public static SampleClimate GetSampleClimate()
        {
            List<double[]> result = new List<double[]>();
            using (var f = new System.IO.StringReader(Properties.Resources.SampleCatchmentData))
            {
                var line = f.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    var items = line.Split(',');
                    double[] values = Array.ConvertAll(items, parseDouble);
                    if (values.Length != 3)
                        throw new ArgumentException();
                    result.Add(values);
                    line = f.ReadLine();
                }
            }
            int n = result.Count;
            var res = new double[][] { new double[n], new double[n], new double[n] };
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    res[j][i] = result[i][j];
                }
            }
            return new SampleClimate { Rainfall = res[0], Evapotranspiration = res[1], Runoff = res[2] };
        }

        static double parseDouble(string str)
        {
            if (str.ToLower() == "na")
                return -9999;
            else
                return double.Parse(str);
        }

    }
}
