using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CSIRO.Metaheuristics.Tests
{
    [TestFixture]
    public class TestCovarianceContravariance
    {
        public void TestObjectiveEvaluators( )
        {
            var general = new ObjEvalA( );
            var scores = general.EvaluateScore( new SysConfigA() );
            var sysConfig = scores.SystemConfiguration;
            Assert.IsAssignableFrom( typeof( SysConfigA ), sysConfig );
        }

        private class SysConfigA : ISystemConfiguration
        {

            #region ISystemConfiguration Members

            public string GetConfigurationDescription( )
            {
                throw new NotImplementedException( );
            }

            public void ApplyConfiguration( object system )
            {
                throw new NotImplementedException( );
            }

            #endregion
        }

        private class ObjEvalA : IObjectiveEvaluator<SysConfigA>
        {
            #region IObjectiveEvaluator<SysConfigA> Members

            public IObjectiveScores<SysConfigA> EvaluateScore( SysConfigA systemConfiguration )
            {
                throw new NotImplementedException( );
            }

            #endregion
        }
    }
}
