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

    [Obsolete()]
    public static class PropertyInfoExtensions
    {
        [Obsolete("This is an internal API. If you are using this consider opening an issue on GitHub.")]
        public static IProperty ToIProperty(this PropertyInfo info)
        {
            return new Property { PropertyInfo = info };
        }
    }
}
