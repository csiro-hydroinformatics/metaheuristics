using System;
using System.Reflection;

namespace CSIRO.Sys
{
    public class Accessor
    {
        public Accessor (Type type, string name)
        {
            var b = BindingFlags.Instance | BindingFlags.Public;
            field = type.GetField (name, b);
            if (field == null)
            {
                property = type.GetProperty (name, b);
                if(property == null)
                    throw new ArgumentException(
                        string.Format("Public instance field or property '{0}' not found on type '{1}'",
                        name, type.FullName));
            }
        }

        FieldInfo field;
        PropertyInfo property;

        public void SetValue(object obj, object value)
        {
            if (field != null)
                field.SetValue (obj, value);
            else if (property != null)
                property.SetValue (obj, value, null);
            else
                throw new NotSupportedException ("This point should never be reached");
        }
        public object GetValue(object obj)
        {
            if (field != null)
                return field.GetValue (obj);
            else if (property != null)
                return property.GetValue (obj, null);
            else
                throw new NotSupportedException ("This point should never be reached");
        }
    }
}

