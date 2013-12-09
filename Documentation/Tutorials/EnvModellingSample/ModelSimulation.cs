using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EnvModellingSample
{
    public class ModelSimulation : IModelSimulation
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
                item.Value[i] = (double) fi.GetValue(TsModel);
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

        public void SetTimeSpan(int startIndex, int endIndex)
        {
            this.startIndex = startIndex;
            this.endIndex = endIndex;
        }

        private Dictionary<string, double[]> inputs = new Dictionary<string,double[]>();
        private Dictionary<string, double[]> outputs = new Dictionary<string,double[]>();

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

        public void SetValue(string modelPropertyId, double value)
        {
            var pInfo = TsModel.GetType().GetProperty(modelPropertyId);
            pInfo.SetValue(TsModel, value, null);
        }
    }
}
