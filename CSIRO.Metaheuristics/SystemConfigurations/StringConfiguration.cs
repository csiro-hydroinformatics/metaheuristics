using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.SystemConfigurations
{
    public class StringConfiguration : ISystemConfiguration
    {
        public StringConfiguration( string value )
        {
            this.Value = value;
        }

        public string Value { get; private set; }

        public string GetConfigurationDescription( )
        {
            return Value;
        }

        public void ApplyConfiguration( object system )
        {
            // do nothing. 
        }
    }
}
