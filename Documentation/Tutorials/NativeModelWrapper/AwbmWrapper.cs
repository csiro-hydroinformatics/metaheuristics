using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CSIRO.Modelling.Core;
using CSIRO.Modelling.Core.Interop;

namespace NativeModelWrapper
{

    internal class NativeApi<M> : IDisposable
        where M : IModelSimulation<double[], double, int>, INativeHandle
    {
        protected virtual void Dispose(bool disposing)
        {
            // Nothing / todo
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Execute(M modelWrapper)
        {
            Execute(modelWrapper.DangerousGetHandle());
        }

        internal void SetSpan(M modelWrapper, int start, int end)
        {
            SetSpan(modelWrapper.DangerousGetHandle(), start, end);
        }

        internal void Play(M modelWrapper, string modelPropertyId, double[] values)
        {
            Play(modelWrapper.DangerousGetHandle(), modelPropertyId, values, values.Length);
        }

        internal void Record(M modelWrapper, string modelPropertyId)
        {
            Record(modelWrapper.DangerousGetHandle(), modelPropertyId);
        }

        internal double[] GetRecorded(M modelWrapper, string modelPropertyId)
        {
            var handle = modelWrapper.DangerousGetHandle();
            int length = GetSimulationLength(modelWrapper);
            double[] data = new double[length];
            GetRecorded(modelWrapper.DangerousGetHandle(), modelPropertyId, data, length);
            return data;
        }

        private int GetSimulationLength(M modelWrapper)
        {
            throw new NotImplementedException();
        }

        internal void SetVariable(M modelWrapper, string modelPropertyId, double value)
        {
            SetVariable(modelWrapper.DangerousGetHandle(), modelPropertyId, value);
        }

        internal double GetVariable(M modelWrapper, string modelPropertyId)
        {
            return GetVariable(modelWrapper.DangerousGetHandle(), modelPropertyId);
        }

        internal int GetStart(M modelWrapper)
        {
            return GetStart(modelWrapper.DangerousGetHandle());
        }

        internal int GetEnd(M modelWrapper)
        {
            return GetEnd(modelWrapper.DangerousGetHandle());
        }

        [DllImport("NativeModelCpp.dll", EntryPoint = "Execute", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Execute(IntPtr nativeModel);

        [DllImport("NativeModelCpp.dll", EntryPoint = "GetRecorded", CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetRecorded(
            [In] IntPtr modelSimulation,
            [In] string variableIdentifier,
            [Out] [MarshalAs(UnmanagedType.LPArray)] double[] values,
            [Out, In] int arrayLength);

        [DllImport("NativeModelCpp.dll", EntryPoint = "SetSpan", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetSpan(
            [In] IntPtr modelSimulation,
            [In] int start,
            [In] int end);

        [DllImport("NativeModelCpp.dll", EntryPoint = "Play", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Play(
            [In] IntPtr modelInstance,
            [In] string varId,
            [In] [MarshalAs(UnmanagedType.LPArray)] double[] values,
            [In] int arrayLength
        );

        [DllImport("NativeModelCpp.dll", EntryPoint = "Record", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Record(
            [In] IntPtr modelSimulation,
            [In] string variableIdentifier);

        [DllImport("NativeModelCpp.dll", EntryPoint = "SetVariable", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetVariable(
            [In] IntPtr modelSimulation,
            [In] string variableIdentifier,
            [In] double value);

        [DllImport("NativeModelCpp.dll", EntryPoint = "GetVariable", CallingConvention = CallingConvention.Cdecl)]
        private static extern double GetVariable(
            [In] IntPtr modelSimulation,
            [In] string variableIdentifier);

        [DllImport("NativeModelCpp.dll", EntryPoint = "GetStart", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetStart(
            [In] IntPtr modelSimulation);


        [DllImport("NativeModelCpp.dll", EntryPoint = "GetEnd", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetEnd(
            [In] IntPtr modelSimulation);


    
    }

    public class AwbmWrapper : SafeHandle,
        IModelSimulation<double[], double, int>, INativeHandle
    {
        public AwbmWrapper() : base(IntPtr.Zero, true)
        {
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
    }
}
