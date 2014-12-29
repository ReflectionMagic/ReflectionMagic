using System;
using System.Reflection;

namespace ReflectionMagic
{
    // IProperty implementation over a FieldInfo
    public class Field : IProperty
    {
        internal FieldInfo FieldInfo { get; set; }

        string IProperty.Name
        {
            get
            {
                return FieldInfo.Name;
            }
        }

        object IProperty.GetValue(object obj, object[] index)
        {
            return FieldInfo.GetValue(obj);
        }

        void IProperty.SetValue(object obj, object val, object[] index)
        {
            FieldInfo.SetValue(obj, val);
        }

        public Type PropertyType { get { return FieldInfo.FieldType; } }
    }

    public static class FieldInfoExtensions
    {
        public static IProperty ToIProperty(this FieldInfo info)
        {
            return new Field { FieldInfo = info };
        }
    }
}
