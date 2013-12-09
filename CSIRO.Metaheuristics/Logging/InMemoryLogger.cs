using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace CSIRO.Metaheuristics.Logging
{
    public class InMemoryLogger : ILoggerMh
    {
        ConcurrentQueue<SysConfigLogInfo> queue = new ConcurrentQueue<SysConfigLogInfo>();

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
            queue.Enqueue(new SysConfigLogInfo(new IObjectiveScores[] { new ZeroScores<IHyperCube<double>>() { SystemConfiguration = newPoint } }, tags));
        }

        public void Write(string message)
        {
            // Nohing?
        }

        public SysConfigLogInfo Dequeue()
        {
            SysConfigLogInfo result;
            queue.TryDequeue(out result);
            return result;
        }

        public int Count { get { return queue.Count; } }


        private class ZeroScores<TSysConfig> : IObjectiveScores<TSysConfig>
            where TSysConfig : ISystemConfiguration
        {
            public TSysConfig SystemConfiguration { get; set; }

            public IObjectiveScore GetObjective(int i)
            {
                throw new IndexOutOfRangeException();
            }

            public ISystemConfiguration GetSystemConfiguration()
            {
                return this.SystemConfiguration;
            }

            public int ObjectiveCount
            {
                get { return 0; }
            }
        }

    }
}
