using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace CSIRO.Sys
{
    public static class ReflectionHelper
    {
        public static Dictionary<string,Accessor> GetAccessorMap(Type type, string[] names)
        {
            var result = new Dictionary<string,Accessor> ();
            foreach (var name in names) {
                result [name] = new Accessor (type, name);
            }
            return result;
        }
    }
}

