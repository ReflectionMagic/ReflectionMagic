using System;
using System.Reflection;

namespace ReflectionMagic
{
    /// <summary>
    /// Provides a mechanism to access fields through the <see cref="IProperty"/> abstraction.
    /// </summary>
    internal class Field : IProperty
    {
        private readonly FieldInfo _fieldInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class wrapping the specified field.
        /// </summary>
        /// <param name="field">The field info to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="field"/> is <c>null</c>.</exception>
        internal Field(FieldInfo field)
        {
            _fieldInfo = field ?? throw new ArgumentNullException(nameof(field));
        }

        public Type PropertyType => _fieldInfo.FieldType;

        string IProperty.Name => _fieldInfo.Name;

        object IProperty.GetValue(object obj, object[] index)
        {
            return _fieldInfo.GetValue(obj);
        }

        void IProperty.SetValue(object obj, object value, object[] index)
        {
            _fieldInfo.SetValue(obj, value);
        }
    }

    [Obsolete("Will be made internal in a future release.")]
    public static class FieldInfoExtensions
    {
        [Obsolete("This is an internal API. If you are using this consider opening an issue on GitHub.")]
        public static IProperty ToIProperty(this FieldInfo info)
        {
            return new Field(info);
        }
    }
}
