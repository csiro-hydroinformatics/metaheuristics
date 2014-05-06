using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.Utils;

namespace CSIRO.Metaheuristics.SystemConfigurations
{
    [Serializable]
    public abstract class HyperCube<T> : IHyperCube<T> where T : IComparable
    {
        protected HyperCube( string[] variableNames )
        {
            this.ThrowOnOutOfBounds = false;
            this.variableNames = variableNames.Clone( ) as string[];
            values = new Dictionary<string, ValMinMax>( );
            foreach( var varName in variableNames )
                values[varName] = new ValMinMax( );
        }
        string[] variableNames;
        [Serializable]
        private class ValMinMax
        {
            public T Val;
            public T Min;
            public T Max;

            public ValMinMax()
            {
            }
            public ValMinMax( ValMinMax valMinMax )
            {
                this.Val = valMinMax.Val;
                this.Min = valMinMax.Min;
                this.Max = valMinMax.Max;
            }
        };
        IDictionary<string, ValMinMax> values;

        #region IHyperCube<T> Members

        public string[] GetVariableNames( )
        {
            return variableNames.Clone( ) as string[];
        }

        public int Dimensions
        {
            get { return variableNames.Length; }
        }

        public virtual T GetValue( string variableName )
        {
            return values[variableName].Val;
        }

        public virtual T GetMaxValue( string variableName )
        {
            return values[variableName].Max;
        }

        public virtual T GetMinValue( string variableName )
        {
            return values[variableName].Min;
        }

        public virtual void SetValue( string variableName, T value )
        {
            bool throwOnOutOfBounds = true;
            SetValue(variableName, value, throwOnOutOfBounds);
        }

        protected void SetValue(string variableName, T value, bool throwOnOutOfBounds)
        {
            checkCorrectArg(variableName);
            T maxValue = GetMaxValue(variableName);
            T minValue = GetMinValue(variableName);
            if (throwOnOutOfBounds)
                MetaheuristicsHelper.CheckInBounds(value, minValue, maxValue, throwOnOutOfBounds);
            this.values[variableName].Val = value;
        }

        public void SetMaxValue( string variableName, T value )
        {
            checkCorrectArg( variableName );
            this.values[variableName].Max = value;
        }

        public void SetMinValue(string variableName, T value)
        {
            checkCorrectArg( variableName );
            this.values[variableName].Min = value;
        }

        public void SetMinMaxValue(string variableName, T min, T max, T value)
        {
            this.SetMinValue(variableName, min);
            this.SetMaxValue(variableName, max);
            this.SetValue(variableName, value);
        }

        protected void checkCorrectArg( string variableName )
        {
            if( !( variableNames.Contains( variableName ) ) )
                throw new ArgumentException( "Incorrect variable name: " + variableName );
        }

        public abstract IHyperCube<T> HomotheticTransform( IHyperCube<T> point, double factor );

        #endregion

        #region ISystemConfiguration Members

        public virtual string GetConfigurationDescription( )
        {
            StringBuilder sb = new StringBuilder( );
            foreach( var item in this.values )
            {
                sb.Append( item.Key );
                sb.Append( ": {" );
                var tmp = item.Value;
                sb.Append( tmp.Val.ToString( ) );
                sb.Append( ", " );
                sb.Append( tmp.Min.ToString( ) );
                sb.Append( ", " );
                sb.Append( tmp.Max.ToString( ) );
                sb.Append( "} " );
            }
            return sb.ToString( );
        }

        public abstract void ApplyConfiguration( object system );

        #endregion

        #region ICloningSupport<ICloneableSystemConfiguration> Members

        public virtual bool SupportsDeepCloning
        {
            get { return true; }
        }

        public virtual bool SupportsThreadSafeCloning
        {
            get { return true; }
        }

        public virtual ICloneableSystemConfiguration Clone( )
        {
            var result = this.MemberwiseClone( ) as HyperCube<T>;
            result.variableNames = this.variableNames.Clone( ) as string[];
            result.values = new Dictionary<string, ValMinMax>( );
            foreach( var varName in variableNames )
                result.values[varName] = new ValMinMax( this.values[varName] );
            return result;
        }

        #endregion

        protected void performHomoteticTransform(IHyperCube<T> point, T factor, ref HyperCube<T> result)
        {
            foreach (var varName in this.GetVariableNames())
            {
                var min = this.GetMinValue(varName);
                var max = this.GetMaxValue(varName);
                result.SetMinValue(varName, min);
                result.SetMaxValue(varName, max);
                var newVal = reflect(point.GetValue(varName), this.GetValue(varName), factor);
                var isInBounds = MetaheuristicsHelper.CheckInBounds(newVal, min, max, throwIfFalse: this.ThrowOnOutOfBounds);
                if (!isInBounds)
                {
                    result = null;
                    break;
                }
                result.SetValue(varName, newVal);
            }
        }

        protected abstract T reflect(T point, T reference, T factor);


        public bool ThrowOnOutOfBounds { get; set; }
    }
}
