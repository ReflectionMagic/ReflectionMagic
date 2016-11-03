using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace ReflectionMagic
{
    public class PrivateReflectionDynamicObjectStatic : PrivateReflectionDynamicObjectBase
    {
        private static readonly IDictionary<Type, IDictionary<string, IProperty>> _propertiesOnType = new ConcurrentDictionary<Type, IDictionary<string, IProperty>>();
        private readonly Type _type;

        public PrivateReflectionDynamicObjectStatic(Type type)
        {
            _type = type;
        }

        internal override IDictionary<Type, IDictionary<string, IProperty>> PropertiesOnType
        {
            get { return _propertiesOnType; }
        }

        // For static calls, we have the type and the instance is always null
        protected override Type TargetType { get { return _type; } }
        protected override object Instance { get { return null; } }

        public override object RealObject
        {
            get { return TargetType; }
        }

        protected override BindingFlags BindingFlags
        {
            get { return BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic; }
        }

        public dynamic New(params object[] args)
        {
            return Activator.CreateInstance(TargetType,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, args, null).AsDynamic();
        }
    }
}
