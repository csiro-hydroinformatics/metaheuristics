using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace CSIRO.Metaheuristics.Logging
{
    public class InMemoryLogger : ILoggerMh, IEnumerable<ILogInfo>
    {
        ConcurrentQueue<ILogInfo> queue = new ConcurrentQueue<ILogInfo>();

        public void Write(IObjectiveScores[] scores, IDictionary<string, string> tags)
        {
            queue.Enqueue(new SysConfigLogInfo(scores, tags));
        }

        public void Write(FitnessAssignedScores<double> worstPoint, IDictionary<string, string> tags)
        {
            queue.Enqueue(new SysConfigLogInfo(new IObjectiveScores[] { worstPoint.Scores }, tags));
        }

        public void Write(IHyperCube<double> newPoint, IDictionary<string, string> tags)
        {
            queue.Enqueue(new SysConfigLogInfo(LoggerMhHelper.CreateNoScore(newPoint), tags));
        }

        public void Write(string message, IDictionary<string, string> tags)
        {
            queue.Enqueue(new StringOnlyLogInfo(message, tags));
        }

        IEnumerator<ILogInfo> IEnumerable<ILogInfo>.GetEnumerator()
        {
            return queue.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return queue.GetEnumerator();
        }
    }
}
