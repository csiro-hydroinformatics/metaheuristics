using System;
using System.Runtime.InteropServices;
using CSIRO.Modelling.Core;
using CSIRO.Modelling.Core.Interop;

namespace NativeModelWrapper
{
    /// <summary>
    /// A wrapper in the managed code around the C++ implementation of AWBM we are using in this sample.
    /// </summary>
    /// <remarks>It is possible here to inherit from SafeHandle, but this is not compulsory to have a working interoperability</remarks>
    public class AwbmWrapper : SafeHandle,
        IModelSimulation<double[], double, int>, 
        INativeHandle
    {
        public AwbmWrapper()
            : base(IntPtr.Zero, true)
        {
            api = new NativeApi<AwbmWrapper>();
            IntPtr pointer = api.CreateSimulation();
            SetHandle(pointer);
        }

        private AwbmWrapper(AwbmWrapper src)
            : base(IntPtr.Zero, true)
        {
            api = new NativeApi<AwbmWrapper>();
            if (!src.SupportsThreadSafeCloning)
                throw new NotSupportedException("source model says it cannot be cloned in a thread-safe manner");
            IntPtr pointer = api.Clone(src);
            SetHandle(pointer);
        }

        NativeApi<AwbmWrapper> api;

        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
                return true;
            this.api.Dispose();
            this.SetHandleAsInvalid();
            return true;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        public void Execute()
        {
            api.Execute(this);
        }

        public void SetSpan(int start, int end)
        {
            api.SetSpan(this, start, end);
        }

        public void Play(string modelPropertyId, double[] values)
        {
            api.Play(this, modelPropertyId, values);
        }

        public void Record(string modelPropertyId)
        {
            api.Record(this, modelPropertyId);
        }

        public double[] GetRecorded(string modelPropertyId)
        {
            return api.GetRecorded(this, modelPropertyId);
        }

        public void SetVariable(string modelPropertyId, double value)
        {
            api.SetVariable(this, modelPropertyId, value);
        }

        public double GetVariable(string modelPropertyId)
        {
            return api.GetVariable(this, modelPropertyId);
        }

        public int GetStart()
        {
            return api.GetStart(this);
        }

        public int GetEnd()
        {
            return api.GetEnd(this);
        }

        public IModelSimulation<double[], double, int> Clone()
        {
            return new AwbmWrapper(this);
        }

        public static bool PermitMultiThreading = true;

        public bool SupportsDeepCloning
        {
            get
            {
                return (PermitMultiThreading && api.SupportsThreadSafeCloning(this));
            }
        }

        public bool SupportsThreadSafeCloning
        {
            get
            {
                return (PermitMultiThreading && api.SupportsThreadSafeCloning(this));
            }
        }
    }
}
