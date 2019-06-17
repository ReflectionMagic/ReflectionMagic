using System;
using System.Reflection;

namespace ReflectionMagic
{
    /// <summary>
    /// Provides an mechanism to access properties through the <see cref="IProperty"/> abstraction.
    /// </summary>
    internal class Property : IProperty
    {
        private readonly PropertyInfo _propertyInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> class wrapping the specified property.
        /// </summary>
        /// <param name="property">The property info to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is <c>null</c>.</exception>
        internal Property(PropertyInfo property)
        {
            _propertyInfo = property ?? throw new ArgumentNullException(nameof(property));
        }

        public Type PropertyType => _propertyInfo.PropertyType;

        string IProperty.Name => _propertyInfo.Name;

        object IProperty.GetValue(object obj, object[] index)
        {
            return _propertyInfo.GetValue(obj, index);
        }

        void IProperty.SetValue(object obj, object value, object[] index)
        {
            _propertyInfo.SetValue(obj, value, index);
        }
    }

    [Obsolete("Will be made internal in a future release.")]
    public static class PropertyInfoExtensions
    {
        [Obsolete("This is an internal API. If you are using this consider opening an issue on GitHub.")]
        public static IProperty ToIProperty(this PropertyInfo info)
        {
            return new Property(info);
        }
    }
}
