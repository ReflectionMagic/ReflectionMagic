using System;
using System.Diagnostics;
using System.Reflection;

namespace ReflectionMagic
{
    internal static class ReflectionHelper
    {
        public static bool IsPrimitive(object obj)
        {
            Debug.Assert(obj != null);

#if NET40 || NET45
            return obj.GetType().IsPrimitive;
#else
            return obj.GetType().GetTypeInfo().IsPrimitive;
#endif
        }

        public static bool IsInstanceOfType(Type type, object obj)
        {
            Debug.Assert(type != null);

#if NET40 || NET45
            return type.IsInstanceOfType(obj);
#else
            return type.GetTypeInfo().IsInstanceOfType(obj);
#endif
        }

        public static Type GetBaseType(Type type)
        {
            Debug.Assert(type != null);

#if NET40 || NET45
            return type.BaseType;
#else
            return type.GetTypeInfo().BaseType;
#endif
        }

        public static PropertyInfo[] GetProperties(Type type, BindingFlags bindingFlags)
        {
            Debug.Assert(type != null);

#if NET40 || NET45
            return type.GetProperties(bindingFlags);
#else
            return type.GetTypeInfo().GetProperties(bindingFlags);
#endif
        }

        public static FieldInfo[] GetFields(Type type, BindingFlags bindingFlags)
        {
            Debug.Assert(type != null);

#if NET40 || NET45
            return type.GetFields(bindingFlags);
#else
            return type.GetTypeInfo().GetFields(bindingFlags);
#endif
        }

        public static MethodInfo[] GetMethods(Type type, BindingFlags bindingFlags)
        {
            Debug.Assert(type != null);

#if NET40 || NET45
            return type.GetMethods(bindingFlags);
#else
            return type.GetTypeInfo().GetMethods(bindingFlags);
#endif
        }

        public static Type GetInterface(Type type, string name)
        {
            Debug.Assert(type != null);

#if NET40 || NET45
            return type.GetInterface(name);
#else
            return type.GetTypeInfo().GetInterface(name);
#endif
        }

        public static PropertyInfo GetProperty(Type type, string name)
        {
            Debug.Assert(type != null);

#if NET40 || NET45
            return type.GetProperty(name);
#else
            return type.GetTypeInfo().GetProperty(name);
#endif
        }

        public static bool IsValueType(Type type)
        {
#if NET40 || NET45
            return type.IsValueType;
#else
            return type.GetTypeInfo().IsValueType;
#endif
        }
    }
}
