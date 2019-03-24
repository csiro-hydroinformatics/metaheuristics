using System;
using System.Runtime.InteropServices;

namespace NativeModelWrapper
{
    /// <summary>
    /// P/Invoke function calls, implemented using the DllImport() attribute.
    /// </summary>
    internal static class NativeApiPInvoke
    {

        [DllImport("NativeModelCpp.dll", EntryPoint = "CreateSimulation", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr CreateSimulation();

        [DllImport("NativeModelCpp.dll", EntryPoint = "Clone", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Clone(IntPtr nativeModel);

        [DllImport("NativeModelCpp.dll", EntryPoint = "Dispose", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Dispose(IntPtr nativeModel);

        [DllImport("NativeModelCpp.dll", EntryPoint = "Execute", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Execute(IntPtr nativeModel);

        [DllImport("NativeModelCpp.dll", EntryPoint = "GetRecorded", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GetRecorded(
            [In] IntPtr nativeModel,
            [In] string variableIdentifier,
            [Out] [MarshalAs(UnmanagedType.LPArray)] double[] values,
            [Out, In] int arrayLength);

        [DllImport("NativeModelCpp.dll", EntryPoint = "SetSpan", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetSpan(
            [In] IntPtr nativeModel,
            [In] int start,
            [In] int end);

        [DllImport("NativeModelCpp.dll", EntryPoint = "Play", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Play(
            [In] IntPtr modelInstance,
            [In] string varId,
            [In] [MarshalAs(UnmanagedType.LPArray)] double[] values,
            [In] int arrayLength);

        [DllImport("NativeModelCpp.dll", EntryPoint = "Record", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Record(
            [In] IntPtr nativeModel,
            [In] string variableIdentifier);

        [DllImport("NativeModelCpp.dll", EntryPoint = "SetVariable", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetVariable(
            [In] IntPtr nativeModel,
            [In] string variableIdentifier,
            [In] double value);

        [DllImport("NativeModelCpp.dll", EntryPoint = "GetVariable", CallingConvention = CallingConvention.Cdecl)]
        internal static extern double GetVariable(
            [In] IntPtr nativeModel,
            [In] string variableIdentifier);

        [DllImport("NativeModelCpp.dll", EntryPoint = "GetStart", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStart(
            [In] IntPtr nativeModel);

        [DllImport("NativeModelCpp.dll", EntryPoint = "GetEnd", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetEnd(
            [In] IntPtr nativeModel);

        [DllImport("NativeModelCpp.dll", EntryPoint = "SupportsThreadSafeCloning", CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool SupportsThreadSafeCloning(
            [In] IntPtr nativeModel);

    }
}
