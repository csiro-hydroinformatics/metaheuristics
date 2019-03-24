using System;
using System.Collections.Generic;
using System.Reflection;
using CSIRO.Modelling.Core;

namespace EnvModellingSample
{
    public class ModelSimulation : IModelSimulation<double[], double, int>
    {
        public ModelSimulation(ITimeStepModel model)
        {
            this.tsModel = model;
        }

        private ITimeStepModel tsModel;
        private int startIndex, endIndex;

        public ITimeStepModel TsModel
        {
            get { return tsModel; }
        }

        public void Execute()
        {
            TsModel.Reset();
            for (int i = startIndex; i <= endIndex; i++)
            {
                setInputs(i);
                TsModel.RunOneTimeStep();
                getOutputs(i);
            }
        }

        private void getOutputs(int i)
        {
            foreach (var item in this.outputs)
            {
                FieldInfo fi = getFi(item.Key);
                item.Value[i] = (double)fi.GetValue(TsModel);
            }
        }

        private FieldInfo getFi(string p)
        {
            return TsModel.GetType().GetField(p);
        }

        private void setInputs(int i)
        {
            foreach (var item in this.inputs)
            {
                FieldInfo fi = getFi(item.Key);
                fi.SetValue(TsModel, item.Value[i]);
            }
        }

        public void SetSpan(int startIndex, int endIndex)
        {
            this.startIndex = startIndex;
            this.endIndex = endIndex;
        }

        private Dictionary<string, double[]> inputs = new Dictionary<string, double[]>();
        private Dictionary<string, double[]> outputs = new Dictionary<string, double[]>();

        public void Play(string inputIdentifier, double[] values)
        {
            this.inputs[inputIdentifier] = values;
        }

        public double[] GetRecorded(string outputIdentifier)
        {
            return this.outputs[outputIdentifier];
        }

        public void Record(string outputIdentifier)
        {
            this.outputs[outputIdentifier] = new double[GetSimulationLength()];
        }

        private int GetSimulationLength()
        {
            return endIndex - startIndex + 1;
        }

        public void SetVariable(string modelPropertyId, double value)
        {
            var pInfo = GetPropertyInfo(modelPropertyId);
            pInfo.SetValue(TsModel, value, null);
        }

        public double GetVariable(string modelPropertyId)
        {
            var pInfo = GetPropertyInfo(modelPropertyId);
            return (double)pInfo.GetValue(TsModel, null);
        }

        private PropertyInfo GetPropertyInfo(string modelPropertyId)
        {
            return TsModel.GetType().GetProperty(modelPropertyId);
        }

        public int GetStart()
        {
            return startIndex;
        }

        public int GetEnd()
        {
            return endIndex;
        }

        public IModelSimulation<double[], double, int> Clone()
        {
            if (!SupportsThreadSafeCloning)
                throw new NotSupportedException();
            var res = new ModelSimulation(TsModel.Clone());
            res.startIndex = startIndex;
            res.endIndex = endIndex;
            foreach (var input in this.inputs)
            {
                res.inputs[input.Key] = inputs[input.Key];
            }
            foreach (var output in outputs)
            {
                res.outputs[output.Key] = (double[])outputs[output.Key].Clone();
            }
            return res;
        }

        public bool SupportsDeepCloning
        {
            get { return TsModel.IsClonable; }
        }

        public bool SupportsThreadSafeCloning
        {
            get { return TsModel.IsClonable; }
        }
    }
}
