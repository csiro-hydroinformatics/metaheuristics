using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Objectives
{
	/// <summary>
	/// An object that is an aggregate of scores for a given 
	/// system configuration. Useful mostly for multi-objective optimisation.
	/// </summary>
	[Serializable] // TODO: I don't think I want this attribute. Was needed for MPI enabled calibration however.
    public class MultipleScores<TSysConfig> : IObjectiveScores<TSysConfig>
        where TSysConfig : ISystemConfiguration
    {
        public MultipleScores( IObjectiveScore[] scores, TSysConfig sysConfig )
        {
            this.scores = (IObjectiveScore[])scores.Clone( );
            this.sysConfig = sysConfig;
        }
        private IObjectiveScore[] scores;
        private TSysConfig sysConfig;
        public int ObjectiveCount
        {
            get { return scores.Length; }
        }

        public IObjectiveScore GetObjective( int i )
        {
            return scores[i];
        }

        public ISystemConfiguration GetSystemConfiguration( )
        {
            return sysConfig;
        }

        public override string ToString( )
        {
            StringBuilder sb = new StringBuilder( );
            for( int i = 0; i < this.scores.Length; i++ )
            {
                sb.Append( scores[i].GetText( ) );
                sb.Append( " " );
            }
            return sb.ToString( );
        }

        public TSysConfig SystemConfiguration
        {
            get { return sysConfig; }
        }
    }

	public class SingleScore<TSysConfig> : MultipleScores<TSysConfig>
			where TSysConfig : ISystemConfiguration
	{
		public SingleScore( IObjectiveScore score, TSysConfig sysConfig ) 
			: base(new[]{score}, sysConfig)
		{
		}
	}
}