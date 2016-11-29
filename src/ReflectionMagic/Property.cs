using System;
using System.Reflection;

namespace ReflectionMagic
{
    // IProperty implementation over a PropertyInfo
    public class Property : IProperty
    {
        internal PropertyInfo PropertyInfo { get; set; }

        public Type PropertyType => PropertyInfo.PropertyType;

        string IProperty.Name => PropertyInfo.Name;

        object IProperty.GetValue(object obj, object[] index)
        {
            return PropertyInfo.GetValue(obj, index);
        }

        void IProperty.SetValue(object obj, object val, object[] index)
        {
            PropertyInfo.SetValue(obj, val, index);
        }

    }

    public static class PropertyInfoExtensions
    {
        public static IProperty ToIProperty(this PropertyInfo info)
        {
            return new Property { PropertyInfo = info };
        }
    }
}
