using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.Utils;
using System.Reflection;

namespace CSIRO.Metaheuristics.Logging
{
    public class Log4netAdapter : ILoggerMh
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public void Write(IObjectiveScores[] scores, IDictionary<string, string> tags)
        {
            if (log.IsInfoEnabled)
                log.Info(new SysConfigLogInfo(scores, tags));
        }

        public void Write(FitnessAssignedScores<double> worstPoint, IDictionary<string, string> tags)
        {
            if (log.IsInfoEnabled)
                log.Info(new SysConfigLogInfo(new IObjectiveScores[] { worstPoint.Scores }, tags));
        }

        public void Write(IHyperCube<double> newPoint, IDictionary<string, string> tags)
        {
            if (log.IsInfoEnabled)
                log.Info(new SysConfigLogInfo(LoggerMhHelper.CreateNoScore(newPoint), tags));
        }

        public void Write(string message)
        {
            if (log.IsInfoEnabled)
                log.Info(message);
        }


        public void Write(string message, IDictionary<string, string> tags)
        {
            throw new NotImplementedException();
        }
    }
}
