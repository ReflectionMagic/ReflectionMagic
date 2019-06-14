namespace ReflectionMagic
{
    public static class DynamicHelper
    {
        /// <summary>
        /// Unwraps the specified dynamic object.
        /// </summary>
        /// <param name="d">A wrapped object</param>
        /// <returns>The unwrapped object.</returns>
        /// <seealso cref="PrivateReflectionDynamicObjectInstance.RealObject"/>
        /// <seealso cref="PrivateReflectionDynamicObjectStatic.RealObject"/>
        public static object Unwrap(dynamic d)
        {
            // If it's a wrapped object, unwrap it and return the real thing.
            if (d is PrivateReflectionDynamicObjectBase wrapper)
                return wrapper.RealObject;

            // Otherwise, return it unchanged.
            return d;
        }
    }
}
