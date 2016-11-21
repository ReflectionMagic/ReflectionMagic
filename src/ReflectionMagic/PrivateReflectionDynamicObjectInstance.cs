using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace ReflectionMagic
{
    public class PrivateReflectionDynamicObjectInstance : PrivateReflectionDynamicObjectBase
    {
        private static readonly IDictionary<Type, IDictionary<string, IProperty>> _propertiesOnType = new ConcurrentDictionary<Type, IDictionary<string, IProperty>>();
        private readonly object _instance;

        public PrivateReflectionDynamicObjectInstance(object instance)
        {
            _instance = instance;
        }

        internal override IDictionary<Type, IDictionary<string, IProperty>> PropertiesOnType
        {
            get { return _propertiesOnType; }
        }

        // For instance calls, we get the type from the instance
        protected override Type TargetType { get { return _instance.GetType(); } }

        protected override object Instance { get { return _instance; } }

        public override object RealObject
        {
            get { return Instance; }
        }

        protected override BindingFlags BindingFlags
        {
            get { return BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic; }
        }
    }
}
