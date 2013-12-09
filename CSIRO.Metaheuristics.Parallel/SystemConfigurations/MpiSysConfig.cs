using System;
using System.Collections.Generic;
using System.Text;

namespace CSIRO.Metaheuristics.Parallel.SystemConfigurations
{
    /// <summary>
    /// A serializable implementation of a traditional univariate parameter range, fir use in message passing.
    /// </summary>
    [Serializable]
    public struct MpiParameterConfig
    {
        public string name;
        public double min;
        public double max;
        public double value;
    }

    public enum MpiMessageTags
    {
        SystemConfigurationMsgTag = 1,
        EvalSlaveResultMsgTag = 2,
        ModelDefinition = 3
    }

    /// <summary>
    /// A serializable implementation of a traditional hypercube, for use in Message Passing.
    /// </summary>
    [Serializable]
    public class MpiSysConfig : IHyperCube<double>
    {
        // In order to avoid multiple threads with communication, add a boolean property.
        public bool calibrationIsFinished = false;
        public MpiParameterConfig[] parameters;

        protected MpiSysConfig()
        {
        }

        protected MpiSysConfig(MpiSysConfig template)
        {
            this.parameters = (MpiParameterConfig[])template.parameters.Clone();
        }

        public int Dimensions
        {
            get { return this.parameters.Length; }
        }

        public double GetMaxValue(string variableName)
        {
            int index = findIndex(variableName);
            return parameters[index].max;
        }

        private int findIndex(string variableName)
        {
            int index = Array.FindIndex(this.parameters, (x => x.name == variableName));
            checkIsKnownVariable(variableName, index);
            return index;
        }

        private static void checkIsKnownVariable(string variableName, int index)
        {
            if (index < 0) throw new ArgumentException("Unknown system variable: " + variableName);
        }

        public double GetMinValue(string variableName)
        {
            int index = findIndex(variableName);
            return parameters[index].min;
        }

        public double GetValue(string variableName)
        {
            int index = findIndex(variableName);
            return parameters[index].value;
        }

        public string[] GetVariableNames()
        {
            return Array.ConvertAll(this.parameters, (x => x.name));
        }

        public IHyperCube<double> HomotheticTransform(IHyperCube<double> point, double factor)
        {
            var p = point as MpiSysConfig;
            var result = new MpiSysConfig(this);
            //return performMirrorBounceTransform(factor, p, result);
            return performDefaultTransform(factor, p, result);
        }

        private IHyperCube<double> performMirrorBounceTransform(double factor, MpiSysConfig p, MpiSysConfig result)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                var reflectedValue = doHomoteticTransf(factor, p, i);
                if (reflectedValue > this.parameters[i].max)
                {
                    reflectedValue = reflectAgainstBound(reflectedValue, this.parameters[i].max);
                }
                else if (reflectedValue < this.parameters[i].min)
                {
                    reflectedValue = reflectAgainstBound(reflectedValue, this.parameters[i].min);
                }
                result.parameters[i].value = reflectedValue;
            }
            return result;
        }

        private static double reflectAgainstBound(double reflectedValue, double bound)
        {
            reflectedValue = 2 * bound - reflectedValue;
            return reflectedValue;
        }

        private IHyperCube<double> performDefaultTransform(double factor, MpiSysConfig p, MpiSysConfig result)
        {
            bool isOutOfBounds = false;
            for (int i = 0; i < parameters.Length; i++)
            {
                var reflectedValue = doHomoteticTransf(factor, p, i);
                if (reflectedValue > this.parameters[i].max || reflectedValue < this.parameters[i].min)
                {
                    isOutOfBounds = true;
                    break;
                }
                else
                {
                    result.parameters[i].value = reflectedValue;
                }
            }
            if (isOutOfBounds)
                return null;
            else
                return result;
        }

        private double doHomoteticTransf(double factor, MpiSysConfig p, int i)
        {
            var reflectedValue = this.parameters[i].value + (p.parameters[i].value - this.parameters[i].value) * factor;
            return reflectedValue;
        }

        public void SetValue(string variableName, double value)
        {
            parameters[findIndex(variableName)].value = value;
        }

        public virtual void ApplyConfiguration(object system)
        {
            // so far only an inheritor is working with the TIME infrastructure. No point having a default behavior here.
            throw new NotImplementedException("ApplyConfiguration must be overriden by inheritors");
        }

        public string GetConfigurationDescription()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("MPI-friendly parameter set");
            var varNames = GetVariableNames();
            for (int i = 0; i < varNames.Length; i++)
            {
                sb.Append(varNames[i]);
                sb.Append("  ");
                sb.Append(GetValue(varNames[i]));
                sb.Append("  ");
                sb.Append(GetMinValue(varNames[i]));
                sb.Append("  ");
                sb.Append(GetMaxValue(varNames[i]));
                sb.AppendLine("  ");
            }
            return sb.ToString();
        }

        public virtual ICloneableSystemConfiguration Clone()
        {
            return new MpiSysConfig
            {
                parameters = (MpiParameterConfig[])this.parameters.Clone()
            };
        }

        public bool SupportsDeepCloning
        {
            get { return true; }
        }

        public bool SupportsThreadSafeCloning
        {
            get { return true; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < parameters.Length; i++)
            {
                sb.Append(parameters[i].name);
                sb.Append(':');
                sb.Append(parameters[i].value.ToString("N2"));
                sb.Append(", ");
            }
            return sb.ToString();
        }
    }
}
