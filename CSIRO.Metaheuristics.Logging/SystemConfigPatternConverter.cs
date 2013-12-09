using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Util;
using log4net.Layout;
using CSIRO.Metaheuristics.Utils;
using log4net.Core;

namespace CSIRO.Metaheuristics.Logging
{
    public class SystemConfigPatternConverter : PatternConverter
    {
        protected override void Convert(System.IO.TextWriter writer, object state)
        {
            LoggingEvent logEvent = state as LoggingEvent;
            SysConfigLogInfo sysConfig = logEvent.MessageObject as SysConfigLogInfo;
            if (sysConfig == null)
                throw new ArgumentException("Object is not of type SysConfigLogInfo");

            Tuple<string, string>[] prepend = new Tuple<string, string>[]
            {
                Tuple.Create("Category", sysConfig.Tags["Category"])
            };
            //prepend = null; // Not sure how to prevent repeats reliably.
            IEnumerable<IHyperCube<double>> points = sysConfig.Scores as IEnumerable<IHyperCube<double>>;
            IEnumerable<IObjectiveScores> scores = sysConfig.Scores as IEnumerable<IObjectiveScores>;

            if (points != null)
                writer.Write(MetaheuristicsHelper.BuildCsvFileContent<IHyperCube<double>>(points, false, prepend));
            if (scores != null)
                writer.Write(MetaheuristicsHelper.BuildCsvFileContent<IHyperCube<double>>(scores, false, prepend));
        }
    }

    public class SystemConfigLayout : PatternLayout
    {
        public SystemConfigLayout()
        {
            AddConverter(new ConverterInfo { Name = "sysconfig", Type = typeof(SystemConfigPatternConverter) });
        }
    }
}
