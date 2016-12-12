using System;
using ReflectionMagic;
using Xunit;

namespace ReflectionMagicTests
{
    public class ExtensionMethodFacts
    {
        public class TheAsDynamicMethod
        {
            [Fact]
            public void Should_WrapObjects()
            {
                var obj = new object();

                var wrapped = obj.AsDynamic();

                Assert.IsType<PrivateReflectionDynamicObjectInstance>(wrapped);
            }

            [Fact]
            public void Should_WrapStructs()
            {
                var @struct = new CustomStruct();

                var wrapped = @struct.AsDynamic();

                Assert.IsType<PrivateReflectionDynamicObjectInstance>(wrapped);
            }

            [Fact]
            public void Should_BeIdempotent()
            {
                var obj = new object();

                object wrapped = obj.AsDynamic();

                Assert.Equal(wrapped, wrapped.AsDynamic());
            }

            [Fact]
            public void Should_NotWrapNull()
            {
                object instance = null;

                Assert.Null(instance.AsDynamic());
            }

            [Fact]
            public void Should_NotWrapString()
            {
                string value = "Test value";

                object wrapped = value.AsDynamic();

                Assert.Same(value, wrapped);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(0.0F)]
            [InlineData(0.0D)]
            [InlineData(0U)]
            [InlineData(0L)]
            // TODO: Maybe add some more examples of primitive types here.
            public void Should_NotWrapPrimitiveTypes(object primitive)
            {
                object wrapped = primitive.AsDynamic();

                Assert.Same(primitive, wrapped);
            }

        }
    }

    internal struct CustomStruct
    {

    }
}
