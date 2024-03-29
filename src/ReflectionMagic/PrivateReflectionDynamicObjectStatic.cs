using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace ReflectionMagic
{
    public class PrivateReflectionDynamicObjectStatic : PrivateReflectionDynamicObjectBase
    {
        private static readonly ConcurrentDictionary<Type, IDictionary<string, IProperty>> _propertiesOnType = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateReflectionDynamicObjectStatic"/> class, wrapping the specified type.
        /// </summary>
        /// <param name="type">The type to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <c>null</c>.</exception>
        public PrivateReflectionDynamicObjectStatic(Type type)
        {
            TargetType = type ?? throw new ArgumentNullException(nameof(type));
        }

        protected override IDictionary<Type, IDictionary<string, IProperty>> PropertiesOnType => _propertiesOnType;

        // For static calls, we have the type and the instance is always null
        protected override Type TargetType { get; }

        protected override object Instance => null;

        /// <summary>
        /// The type that the <see cref="PrivateReflectionDynamicObjectStatic"/> wraps.
        /// </summary>
        public override object RealObject => TargetType;

        protected override BindingFlags BindingFlags => BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public dynamic New(params object[] args)
        {
            if (args is null)
                throw new ArgumentNullException(nameof(args));

            Debug.Assert(TargetType != null);

            return Activator.CreateInstance(TargetType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, args, null).AsDynamic();
        }
    }
}
