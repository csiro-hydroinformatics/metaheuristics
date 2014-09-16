using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Modelling.Core.Interop
{
    public interface INativeHandle
    {
        IntPtr DangerousGetHandle();
    }
}
