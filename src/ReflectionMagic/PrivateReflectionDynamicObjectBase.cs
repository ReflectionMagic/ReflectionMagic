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
#if NET45
        private static readonly Type[] _emptyTypes = new Type[0];
#endif

        // We need to virtualize this so we use a different cache for instance and static props
        protected abstract IDictionary<Type, IDictionary<string, IProperty>> PropertiesOnType { get; }

        protected abstract Type TargetType { get; }

        protected abstract object Instance { get; }

        protected abstract BindingFlags BindingFlags { get; }

        public abstract object RealObject { get; }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder is null)
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
            if (binder is null)
                throw new ArgumentNullException(nameof(binder));

            IProperty prop = GetProperty(binder.Name);

            // Set the property value.  Make sure to unwrap it first if it's one of our dynamic objects
            prop.SetValue(Instance, DynamicHelper.Unwrap(value), index: null);

            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (binder is null)
                throw new ArgumentNullException(nameof(binder));

            IProperty prop = GetIndexProperty();
            result = prop.GetValue(Instance, indexes);

            // Wrap the sub object if necessary. This allows nested anonymous objects to work.
            result = result.AsDynamic();

            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (binder is null)
                throw new ArgumentNullException(nameof(binder));

            IProperty prop = GetIndexProperty();
            prop.SetValue(Instance, DynamicHelper.Unwrap(value), indexes);

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (binder is null)
                throw new ArgumentNullException(nameof(binder));

            if (args is null)
                throw new ArgumentNullException(nameof(args));

            for (int i = 0; i < args.Length; i++)
            {
                args[i] = DynamicHelper.Unwrap(args[i]);
            }

            var typeArgs = GetGenericMethodArguments(binder);

            result = InvokeMethodOnType(TargetType, Instance, binder.Name, args, typeArgs);

            // Wrap the sub object if necessary. This allows nested anonymous objects to work.
            result = result.AsDynamic();

            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder is null)
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
            if (!(type.GetTypeInfo().BaseType is null))
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
                {
                    typeProperties[prop.Name] = new Property(prop);
                }
            }

            // Finally, add all the fields from the current type
            foreach (FieldInfo field in type.GetTypeInfo().GetFields(BindingFlags))
            {
                if (field.DeclaringType == type)
                {
                    typeProperties[field.Name] = new Field(field);
                }
            }

            // Cache it for next time
            PropertiesOnType[type] = typeProperties;

            return typeProperties;
        }

        private static bool ParametersCompatible(MethodInfo method, object[] passedArguments)
        {
            Debug.Assert(method != null);
            Debug.Assert(passedArguments != null);

            var parametersOnMethod = method.GetParameters();

            if (parametersOnMethod.Length != passedArguments.Length)
                return false;

            for (int i = 0; i < parametersOnMethod.Length; ++i)
            {
                var parameterType = parametersOnMethod[i].ParameterType.GetTypeInfo();
                ref var argument = ref passedArguments[i];

                if (argument is null && parameterType.IsValueType)
                {
                    // Value types can not be null.
                    return false;
                }

                if (!parameterType.IsInstanceOfType(argument))
                {
                    // Parameters should be instance of the parameter type.
                    if (parameterType.IsByRef)
                    {
                        var typePassedByRef = parameterType.GetElementType().GetTypeInfo();

                        Debug.Assert(typePassedByRef != null);

                        if (typePassedByRef.IsValueType && argument is null)
                        {
                            return false;
                        }

                        if (!(argument is null))
                        {
                            var argumentType = argument.GetType().GetTypeInfo();
                            var argumentByRefType = argumentType.MakeByRefType().GetTypeInfo();
                            if (parameterType != argumentByRefType)
                            {
                                try
                                {
                                    argument = Convert.ChangeType(argument, typePassedByRef.AsType());
                                }
                                catch (InvalidCastException)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    else if (argument is null)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static object InvokeMethodOnType(Type type, object target, string name, object[] args, Type[] typeArgs)
        {
            Debug.Assert(type != null);
            Debug.Assert(args != null);
            Debug.Assert(typeArgs != null);

            const BindingFlags allMethods =
                BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Instance | BindingFlags.Static;

            MethodInfo method = null;
            Type currentType = type;

            while (method is null && !(currentType is null))
            {
                var methods = currentType.GetTypeInfo().GetMethods(allMethods);

                MethodInfo candidate;
                for (int i = 0; i < methods.Length; ++i)
                {
                    candidate = methods[i];

                    if (candidate.Name == name)
                    {
                        // Check if the method is called as a generic method.
                        if (typeArgs.Length > 0 && candidate.ContainsGenericParameters)
                        {
                            var candidateTypeArgs = candidate.GetGenericArguments();
                            if (candidateTypeArgs.Length == typeArgs.Length)
                            {
                                candidate = candidate.MakeGenericMethod(typeArgs);
                            }
                        }

                        if (ParametersCompatible(candidate, args))
                        {
                            method = candidate;
                            break;
                        }
                    }
                }

                if (method is null)
                {
                    // Move up in the type hierarchy.
                    // If there is no base type, then this will set currentType to null, terminating the loop.
                    currentType = currentType.GetTypeInfo().BaseType;
                }
            }

            if (method is null)
            {
                throw new MissingMethodException($"Method with name '{name}' not found on type '{type.FullName}'.");
            }

            return method.Invoke(target, args);
        }

        private static Type[] GetGenericMethodArguments(InvokeMemberBinder binder)
        {
            var csharpInvokeMemberBinderType = binder
                    .GetType().GetTypeInfo()
                    .GetInterface("Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder")
                    .GetTypeInfo();

            var typeArgsList = (IList<Type>)csharpInvokeMemberBinderType.GetProperty("TypeArguments").GetValue(binder, null);

            Type[] typeArgs;
            if (typeArgsList.Count == 0)
            {
#if NET45
                typeArgs = _emptyTypes;
#else
                typeArgs = Array.Empty<Type>();
#endif
            }
            else
            {
                typeArgs = typeArgsList.ToArray();
            }

            return typeArgs;
        }
    }
}
