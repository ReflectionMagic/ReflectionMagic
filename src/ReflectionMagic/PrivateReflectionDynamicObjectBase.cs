using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace ReflectionMagic
{
    public abstract class PrivateReflectionDynamicObjectBase : DynamicObject
    {
        // We need to virtualize this so we use a different cache for instance and static props
        protected abstract IDictionary<Type, IDictionary<string, IProperty>> PropertiesOnType { get; }

        protected abstract Type TargetType { get; }

        protected abstract object Instance { get; }

        protected abstract BindingFlags BindingFlags { get; }

        public abstract object RealObject { get; }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

            IProperty prop = GetProperty(binder.Name);

            // Get the property value
            result = prop.GetValue(Instance, index: null);

            // Wrap the sub object if necessary. This allows nested anonymous objects to work.
            result = result.AsDynamic();

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

            IProperty prop = GetProperty(binder.Name);

            // Set the property value.  Make sure to unwrap it first if it's one of our dynamic objects
            prop.SetValue(Instance, Unwrap(value), index: null);

            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

            IProperty prop = GetIndexProperty();
            result = prop.GetValue(Instance, indexes);

            // Wrap the sub object if necessary. This allows nested anonymous objects to work.
            result = result.AsDynamic();

            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

            IProperty prop = GetIndexProperty();
            prop.SetValue(Instance, Unwrap(value), indexes);

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Unwrap(args[i]);
            }

            var csharpBinder = binder.GetType().GetTypeInfo().GetInterface("Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder");
            var typeArgs = (IList<Type>)csharpBinder.GetTypeInfo().GetProperty("TypeArguments").GetValue(binder, null);

            result = InvokeMethodOnType(TargetType, Instance, binder.Name, args, typeArgs);

            // Wrap the sub object if necessary. This allows nested anonymous objects to work.
            result = result.AsDynamic();

            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

            result = binder.Type.GetTypeInfo().IsInstanceOfType(RealObject) ? RealObject : Convert.ChangeType(RealObject, binder.Type);

            return true;
        }

        public override string ToString()
        {
            Debug.Assert(Instance != null);

            return Instance.ToString();
        }

        private IProperty GetIndexProperty()
        {
            // The index property is always named "Item" in C#
            return GetProperty("Item");
        }

        private IProperty GetProperty(string propertyName)
        {
            // Get the list of properties and fields for this type
            IDictionary<string, IProperty> typeProperties = GetTypeProperties(TargetType);

            // Look for the one we want
            if (typeProperties.TryGetValue(propertyName, out IProperty property))
                return property;

            // The property doesn't exist

            // Get a list of supported properties and fields and show them as part of the exception message
            // For fields, skip the auto property backing fields (which name start with <)
            var propNames = typeProperties.Keys.Where(name => name[0] != '<').OrderBy(name => name);

            throw new MissingMemberException(
                $"The property {propertyName} doesn\'t exist on type {TargetType}. Supported properties are: {string.Join(", ", propNames)}");
        }

        private IDictionary<string, IProperty> GetTypeProperties(Type type)
        {
            // First, check if we already have it cached
            if (PropertiesOnType.TryGetValue(type, out IDictionary<string, IProperty> typeProperties))
                return typeProperties;

            // Not cached, so we need to build it
            typeProperties = new Dictionary<string, IProperty>();

            // First, recurse on the base class to add its fields
            if (type.GetTypeInfo().BaseType != null)
            {
                foreach (IProperty prop in GetTypeProperties(type.GetTypeInfo().BaseType).Values)
                {
                    typeProperties[prop.Name] = prop;
                }
            }

            // Then, add all the properties from the current type
            foreach (PropertyInfo prop in type.GetTypeInfo().GetProperties(BindingFlags))
            {
                if (prop.DeclaringType == type)
                    typeProperties[prop.Name] = new Property(prop);
            }

            // Finally, add all the fields from the current type
            foreach (FieldInfo field in type.GetTypeInfo().GetFields(BindingFlags))
            {
                if (field.DeclaringType == type)
                    typeProperties[field.Name] = new Field(field);
            }

            // Cache it for next time
            PropertiesOnType[type] = typeProperties;

            return typeProperties;
        }

        private static bool ParametersCompatible(MethodInfo method, object[] passedArguments, IList<Type> typeArgs)
        {
            Debug.Assert(method != null);
            Debug.Assert(passedArguments != null);
            Debug.Assert(typeArgs != null);

            if (typeArgs.Count > 0)
            {
                method = method.MakeGenericMethod(typeArgs.ToArray());
            }

            var parametersOnMethod = method.GetParameters();

            if (parametersOnMethod.Length != passedArguments.Length)
                return false;

            for (int i = 0; i < parametersOnMethod.Length; ++i)
            {
                var parameterType = parametersOnMethod[i].ParameterType.GetTypeInfo();
                var argument = passedArguments[i];
                var argumentType = argument.GetType().GetTypeInfo();

                if (argument == null && parameterType.IsValueType)
                {
                    // Value types can not be null.
                    return false;
                }

                if (!parameterType.IsInstanceOfType(passedArguments[i]))
                {
                    // Parameters should be instance of the parameter type.
                    if (parameterType.IsByRef)
                    {
                        // Handle parameters passed by ref
                        var argumentByRefType = argumentType.MakeByRefType().GetTypeInfo();
                        if (parameterType != argumentByRefType)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static object InvokeMethodOnType(Type type, object target, string name, object[] args, IList<Type> typeArgs)
        {
            Debug.Assert(type != null);
            Debug.Assert(args != null);
            Debug.Assert(typeArgs != null);

            const BindingFlags allMethods =
                BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Instance | BindingFlags.Static;

            MethodInfo method = null;
            Type currentType = type;

            while (method == null && currentType != null)
            {
                var methods = currentType.GetTypeInfo().GetMethods(allMethods);

                foreach (var m in methods)
                {
                    if (m.Name == name && ParametersCompatible(m, args, typeArgs))
                    {
                        method = m;
                        break;
                    }
                }

                if (method == null)
                {
                    // Move up in the type hierarchy.
                    currentType = currentType.GetTypeInfo().BaseType;
                }
            }

            if (method == null)
                throw new MissingMethodException($"Method with name '{name}' not found on type '{type.FullName}'.");

            if (typeArgs.Count > 0)
            {
                method = method.MakeGenericMethod(typeArgs.ToArray());
            }

            return method.Invoke(target, args);
        }

        private static object Unwrap(object o)
        {
            // If it's a wrap object, unwrap it and return the real thing
            if (o is PrivateReflectionDynamicObjectBase wrappedObj)
                return wrappedObj.RealObject;

            // Otherwise, return it unchanged
            return o;
        }
    }
}
