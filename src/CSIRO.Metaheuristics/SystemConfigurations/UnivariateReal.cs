using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.SystemConfigurations
{
    public class UnivariateReal : IHyperCube<double>
    {
        public UnivariateReal( double value, double minValue = 0, double maxValue = 1 )
        {
            this.value = value;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
        private double value;

        public double Value
        {
            get { return this.value; }
        }

        public string GetConfigurationDescription( )
        {
            return "Value: " + this.value;
        }

        public virtual void ApplyConfiguration( object system )
        {
            throw new NotImplementedException( );
        }


        #region IHyperCube<double> Members

        public string[] GetVariableNames( )
        {
            return new string[]{VariableName};
        }

        public string VariableName { get; set; }

        public int Dimensions
        {
            get { return 1; }
        }

        public double GetValue( string variableName )
        {
            checkCorrectArg( variableName );
            return Value;
        }

        private void checkCorrectArg( string variableName )
        {
            if( !( variableName == VariableName ) )
                throw new ArgumentException( "Incorrect variable name" );
        }

        public double GetMaxValue( string variableName )
        {
            checkCorrectArg( variableName );
            return maxValue;
        }
        double maxValue, minValue;

        public double GetMinValue( string variableName )
        {
            checkCorrectArg( variableName );
            return minValue;
        }

        public void SetValue( string variableName, double value )
        {
            checkCorrectArg( variableName );
            if( value > maxValue || value < minValue ) throw new ArgumentException( "Value to set is out of min-max bounds" );
            this.value = value;
        }

        public IHyperCube<double> HomotheticTransform( IHyperCube<double> point, double factor )
        {
            var centralPoint = (UnivariateReal)point;
            var reflectedValue = centralPoint.value + ( this.value - centralPoint.value ) * factor;
            if (reflectedValue > this.maxValue || reflectedValue < this.minValue)
                return null;
            var result = (UnivariateReal)this.MemberwiseClone( );
            result.SetValue( this.VariableName, reflectedValue);
            return result;
        }

        #endregion

        #region ICloningSupport<ICloneableSystemConfiguration> Members

        public bool SupportsDeepCloning
        {
            get { return true; }
        }

        public bool SupportsThreadSafeCloning
        {
            get { return true; }
        }

        public ICloneableSystemConfiguration Clone( )
        {
            return (ICloneableSystemConfiguration)this.MemberwiseClone( );
        }

        #endregion
    }
}
