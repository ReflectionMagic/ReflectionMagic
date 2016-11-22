using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReflectionMagic
{
    public class PrivateReflectionDynamicObjectStatic : PrivateReflectionDynamicObjectBase
    {
        private static readonly IDictionary<Type, IDictionary<string, IProperty>> _propertiesOnType = new ConcurrentDictionary<Type, IDictionary<string, IProperty>>();

        public PrivateReflectionDynamicObjectStatic(Type type)
        {
            TargetType = type;
        }

        internal override IDictionary<Type, IDictionary<string, IProperty>> PropertiesOnType => _propertiesOnType;

        // For static calls, we have the type and the instance is always null
        protected override Type TargetType { get; }

        protected override object Instance => null;

        public override object RealObject => TargetType;

        protected override BindingFlags BindingFlags
        {
            get { return BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic; }
        }

        public dynamic New(params object[] args)
        {
#if NET40 || NET45
            return Activator.CreateInstance(TargetType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, args, null).AsDynamic();
#else
            var constructors = TargetType.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var argumentTypes = args.Select(x => x.GetType()).ToArray();

            object result = (from t in constructors
                             let parameters = t.GetParameters()
                             let parameterTypes = parameters.Select(x => x.ParameterType)
                             where parameters.Length == args.Length
                             where parameterTypes.SequenceEqual(argumentTypes)
                             select t.Invoke(args)).SingleOrDefault();

            if (result == null)
                throw new MissingMethodException("Constructor not found.");

            return result.AsDynamic();
#endif
        }
    }
}
