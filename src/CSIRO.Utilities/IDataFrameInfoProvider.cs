using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Utilities
{
    public interface IDataFrameInfoProvider
    {
        IDictionary<string, string> GetRow( );
    }
}
