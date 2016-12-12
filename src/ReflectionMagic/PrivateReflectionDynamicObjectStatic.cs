using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ReflectionMagic
{
    public class PrivateReflectionDynamicObjectStatic : PrivateReflectionDynamicObjectBase
    {
        private static readonly ConcurrentDictionary<Type, IDictionary<string, IProperty>> _propertiesOnType = new ConcurrentDictionary<Type, IDictionary<string, IProperty>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateReflectionDynamicObjectStatic"/> class, wrapping the specified type.
        /// </summary>
        /// <param name="type">The type to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <c>null</c>.</exception>
        public PrivateReflectionDynamicObjectStatic(Type type)
        {
            if(type == null)
                throw new ArgumentNullException(nameof(type));

            TargetType = type;
        }

        protected override IDictionary<Type, IDictionary<string, IProperty>> PropertiesOnType => _propertiesOnType;

        // For static calls, we have the type and the instance is always null
        protected override Type TargetType { get; }

        protected override object Instance => null;

        public override object RealObject => TargetType;

        protected override BindingFlags BindingFlags => BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public dynamic New(params object[] args)
        {
            Debug.Assert(args != null);

#if NET45
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
                throw new MissingMethodException($"Constructor that accepts parameters: '{string.Join(", ", argumentTypes.Select(x => x.ToString()))}' not found.");

            return result.AsDynamic();
#endif
        }
    }
}
