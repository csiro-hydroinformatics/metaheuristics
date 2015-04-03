using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Modelling.Core;
using CSIRO.Modelling.Core.Interop;

namespace NativeModelWrapper
{
    /// <summary>
    /// An object that is an adapter between the AWBM model wrapper and the interop function calls to native code.
    /// </summary>
    /// <typeparam name="M">The type of the managed model wrapper</typeparam>
    /// <remarks>While it may seem overkill to have this besides , the intent is to decouple the model 
    /// wrapper from the details of how the interop is implemented; there are different techniques and 
    /// software patterns to do so</remarks>
    internal class NativeApi<M> : IDisposable
        where M : IModelSimulation<double[], double, int>, INativeHandle
    {
        protected virtual void Dispose(bool disposing)
        {
            // In this intance, there is nothing to do to clean up, because no 
            // native resources are held, as such.
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal IntPtr CreateSimulation()
        {
            return NativeApiPInvoke.CreateSimulation();
        }

        internal IntPtr Clone(M modelWrapper)
        {
            return NativeApiPInvoke.Clone(modelWrapper.DangerousGetHandle());
        }

        internal void Dispose(M modelWrapper)
        {
            NativeApiPInvoke.Dispose(modelWrapper.DangerousGetHandle());
        }

        internal void Execute(M modelWrapper)
        {
            NativeApiPInvoke.Execute(modelWrapper.DangerousGetHandle());
        }

        internal void SetSpan(M modelWrapper, int start, int end)
        {
            NativeApiPInvoke.SetSpan(modelWrapper.DangerousGetHandle(), start, end);
        }

        internal void Play(M modelWrapper, string modelPropertyId, double[] values)
        {
            NativeApiPInvoke.Play(modelWrapper.DangerousGetHandle(), modelPropertyId, values, values.Length);
        }

        internal void Record(M modelWrapper, string modelPropertyId)
        {
            NativeApiPInvoke.Record(modelWrapper.DangerousGetHandle(), modelPropertyId);
        }

        internal double[] GetRecorded(M modelWrapper, string modelPropertyId)
        {
            var handle = modelWrapper.DangerousGetHandle();
            int length = GetSimulationLength(modelWrapper);
            double[] data = new double[length];
            NativeApiPInvoke.GetRecorded(modelWrapper.DangerousGetHandle(), modelPropertyId, data, length);
            return data;
        }

        private int GetSimulationLength(M modelWrapper)
        {
            return GetEnd(modelWrapper) - GetStart(modelWrapper) + 1;
        }

        internal void SetVariable(M modelWrapper, string modelPropertyId, double value)
        {
            NativeApiPInvoke.SetVariable(modelWrapper.DangerousGetHandle(), modelPropertyId, value);
        }

        internal double GetVariable(M modelWrapper, string modelPropertyId)
        {
            return NativeApiPInvoke.GetVariable(modelWrapper.DangerousGetHandle(), modelPropertyId);
        }

        internal int GetStart(M modelWrapper)
        {
            return NativeApiPInvoke.GetStart(modelWrapper.DangerousGetHandle());
        }

        internal int GetEnd(M modelWrapper)
        {
            return NativeApiPInvoke.GetEnd(modelWrapper.DangerousGetHandle());
        }

        internal bool SupportsThreadSafeCloning(M modelWrapper)
        {
            return NativeApiPInvoke.SupportsThreadSafeCloning(modelWrapper.DangerousGetHandle());
        }
    }
}
