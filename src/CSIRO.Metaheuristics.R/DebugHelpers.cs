using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CSIRO.Metaheuristics.R
{
    static class DebugHelpers
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static void MpiSleep(int ms, string msg = "")
        {
#if DEBUG_MODELS
            Log.DebugFormat("Rank {0} sleeping for {1}s: {2}", MPI.Communicator.world.Rank, ms / 1000, msg);
            Thread.Sleep(ms);
            Log.DebugFormat("Rank {0} awake", MPI.Communicator.world.Rank);
#endif
        }
    }
}
