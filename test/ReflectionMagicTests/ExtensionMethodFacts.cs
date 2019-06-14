using ReflectionMagic;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ReflectionMagicTests
{
    public class ExtensionMethodFacts
    {
        public class TheAsDynamicMethod
        {
            public static IEnumerable<object[]> PrimitiveValues = (new object[]
            {
                (int)1,
                (uint)1,
                (long)1,
                (byte)1,
                (double)1,
                (char)'a',
                (bool)true,
            }).Select(value => new object[] { value });

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
            [MemberData(nameof(PrimitiveValues))]
            public void Should_NotWrapPrimitiveTypes(object primitive)
            {
                object wrapped = primitive.AsDynamic();

                Assert.Same(primitive, wrapped);
            }

            [Fact]
            public void Should_UnwrapObjects()
            {
                object obj = new object();
                dynamic wrapped = obj.AsDynamic();
                object unwrapped = DynamicHelper.Unwrap(wrapped);

                Assert.Same(unwrapped, obj);
            }

            [Fact]
            public void Should_UnwrapStructs()
            {
                var @struct = Guid.NewGuid();
                var wrapped = @struct.AsDynamic();
                object unwrapped = DynamicHelper.Unwrap(wrapped);

                Assert.Equal(unwrapped, @struct);
            }

            [Theory]
            [MemberData(nameof(PrimitiveValues))]
            public void Should_UnwrapPrimitiveTypes(object primitive)
            {
                dynamic wrapped = primitive.AsDynamic();
                object unwrapped = DynamicHelper.Unwrap(wrapped);

                Assert.Equal(unwrapped, primitive);
            }
        }
    }

    internal struct CustomStruct
    {

    }
}
