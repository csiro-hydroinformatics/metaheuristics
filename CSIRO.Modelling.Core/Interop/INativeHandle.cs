using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Modelling.Core.Interop
{
    /// <summary>
    /// An interface for managed objects that can return the address of corresponding unmanaged objects (e.g. C++ models)
    /// </summary>
    public interface INativeHandle
    {
        IntPtr DangerousGetHandle();
    }
}
