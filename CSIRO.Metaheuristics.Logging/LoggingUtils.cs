using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.DataModel;
using System.IO;

namespace CSIRO.Metaheuristics.Logging
{
    public class LoggingUtils
    {

        private static ObjectivesResultsCollection createResultsSet(SysConfigLogInfo item, string resultsName = "")
        {
            IObjectiveScores[] arrayScores = item.Scores as IObjectiveScores[];
            if (arrayScores != null)
            {
                var tags = item.Tags;
                var result = ConvertOptimizationResults.Convert(arrayScores, attributes: tags);
                result.Name = resultsName;
                return result;
            }
            return null;
        }
    }
}
