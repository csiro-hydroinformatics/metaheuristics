using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.Logging;
using RDotNet;

namespace CSIRO.Metaheuristics.R
{
    public static class LogHelper
    {
        public static DataFrame ToDataFrame(this IEnumerable<ILogInfo> log, bool stringsAsFactors = true)
        {
            var e = REngine.GetInstance();

            Dictionary<string, string[]> strInfo;
            Dictionary<string, double[]> numericInfo;
            log.ToColumns(out strInfo, out numericInfo);
            var colNames = new List<string>(strInfo.Keys);
            colNames.AddRange(numericInfo.Keys);
            var columns = new List<IEnumerable>(strInfo.Values);
            columns.AddRange(numericInfo.Values);
            return e.CreateDataFrame(columns.ToArray(), colNames.ToArray(), stringsAsFactors: stringsAsFactors);
            // IEnumerable[] columns, string[] columnNames = null, string[] rowNames = null, bool checkRows = false, bool checkNames = true, bool stringsAsFactors = true);
        }
    }
}
