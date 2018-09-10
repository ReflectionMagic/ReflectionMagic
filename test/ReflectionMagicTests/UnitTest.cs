using LibraryWithPrivateMembers;
using ReflectionMagic;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace ReflectionMagicTests
{
    public class UnitTest
    {
        private dynamic dynamicFooType;
        private dynamic dynamicFoo;

        public UnitTest()
        {
            dynamicFooType = typeof(MarkerType).GetTypeInfo().Assembly.GetDynamicType("LibraryWithPrivateMembers.Foo");
            dynamicFoo = typeof(MarkerType).GetTypeInfo().Assembly.CreateDynamicInstance("LibraryWithPrivateMembers.Foo");
        }

        [Fact]
        public void TestPropertyGetAndSetInternalInteger()
        {
            dynamicFoo.SomeInternalInteger = 17;
            Assert.Equal(17, dynamicFoo.SomeInternalInteger);
        }

        [Fact]
        public void TestPropertyGetAndSetPublicBool()
        {
            dynamicFoo.SomePublicBool = true;
            Assert.Equal(true, dynamicFoo.SomePublicBool);
        }

        [Fact]
        public void TestPropertyGetAndSetPrivateString()
        {
            dynamicFoo.SomePrivateString = "Hello";
            Assert.Equal("Hello", dynamicFoo.SomePrivateString);
        }

        [Fact]
        public void TestPrivateIntegerField()
        {
            Assert.Equal(-1, dynamicFoo._somePrivateIntegerField);
            dynamicFoo._somePrivateIntegerField = 12;
            Assert.Equal(12, dynamicFoo._somePrivateIntegerField);
        }

        [Fact]
        public void TestOperatorOnIntegers()
        {
            dynamicFoo.SomeInternalInteger = 17;
            Assert.Equal(35, dynamicFoo.SomeInternalInteger * 2 + 1);
        }

        [Fact]
        public void TestOperatorOnStrings()
        {
            dynamicFoo.SomePrivateString = "Hello";
            Assert.Equal("Hello world", dynamicFoo.SomePrivateString + " world");
            Assert.Equal("Say Hello", "Say " + dynamicFoo.SomePrivateString);
        }

        [Fact]
        public void TestMissingProperty()
        {
            Assert.Throws<MissingMemberException>(() => dynamicFoo.NotExist = "Hello");
        }

        [Fact]
        public void TestMissingMethod()
        {
            var exception = Assert.Throws<MissingMethodException>(() => dynamicFoo.NotExist());

            Assert.Contains("NotExist", exception.Message);
        }

        [Fact]
        public void FactCalls()
        {
            dynamicFoo.SomeInternalInteger = 17;

            // This one is defined on the base type
            var sum = dynamicFoo.AddIntegers(dynamicFoo.SomeInternalInteger, 3);
            Assert.Equal(20, sum);

            // Different overload defined on the type itself
            sum = dynamicFoo.AddIntegers(dynamicFoo.SomeInternalInteger, 3, 4);
            Assert.Equal(24, sum);
        }

        [Fact]
        public void TestCallToMethodThatHidesBaseMethod()
        {
            var val = dynamicFoo.SomeMethodThatGetsHiddenInDerivedClass();

            Assert.Equal("Foo.SomeMethod", val);
        }

        [Fact]
        public void TestCallToPropertyThatHidesBaseProperty()
        {
            var val = dynamicFoo.SomePropertyThatGetsHiddenInDerivedClass;

            Assert.Equal("Foo.SomePropertyThatGetsHiddenInDerivedClass", val);
        }

        [Fact]
        public void FactCallThatTakesObject()
        {
            dynamicFoo._bar.SomeBarStringProperty = "Blah1";
            var barString = dynamicFoo.ReturnStringFromBarObject(dynamicFoo._bar.RealObject);
            Assert.Equal("Blah1", barString);

            dynamicFoo._bar.SomeBarStringProperty = "Blah2";
            barString = dynamicFoo.ReturnStringFromBarObject(dynamicFoo._bar);
            Assert.Equal("Blah2", barString);
        }

        [Fact]
        public void FactCallThatTakesType()
        {
            var typeName = dynamicFoo.ReturnTypeName(dynamicFooType);
            Assert.Equal("LibraryWithPrivateMembers.Foo", typeName);
        }

        [Fact]
        public void TestPropertyGetAndSetOnSubObject()
        {
            dynamicFoo._bar.SomeBarStringProperty = "Blah";
            Assert.Equal("Blah", dynamicFoo._bar.SomeBarStringProperty);
        }

        [Fact]
        public void TestGettingBackRealObject()
        {
            object realBar = dynamicFoo._bar.RealObject;
            Assert.Equal("Bar", realBar.GetType().Name);
        }

        [Fact]
        public void TestObjectInstantiationWithNonDefaultCtor()
        {
            var foo = dynamicFooType.New(123);
            Assert.Equal(123, foo.SomeInternalInteger);
        }

        [Fact]
        public void TestStaticPropertyGetAndSet()
        {
            dynamicFooType.SomeFooStaticStringProperty = "zzz";
            Assert.Equal("zzz", dynamicFooType.SomeFooStaticStringProperty);
        }

        [Fact]
        public void TestStaticFieldGetAndSet()
        {
            Assert.Equal(17, dynamicFooType._somePrivateStaticIntegerField);
            dynamicFooType._somePrivateStaticIntegerField++;
            Assert.Equal(18, dynamicFooType._somePrivateStaticIntegerField);
        }

        [Fact]
        public void TestStaticMethodCall()
        {
            var sum = dynamicFooType.AddDoubles(2.5, 3.5);
            Assert.Equal(6, sum);
        }

        [Fact]
        public void TestNullSubObject()
        {
            Assert.Equal(null, dynamicFoo._barNull);
        }

        [Fact]
        public void TestIndexedProperty()
        {
            dynamicFoo["Hello"] = "qqq";
            dynamicFoo["Hello2"] = "qqq2";
            Assert.Equal("qqq", dynamicFoo["Hello"]);
            Assert.Equal("qqq2", dynamicFoo["Hello2"]);
        }

        [Fact]
        public void TestDictionaryAccess()
        {
            dynamicFoo._dict["Hello"] = "qqq";
            dynamicFoo._dict["Hello2"] = "qqq2";
            Assert.Equal("qqq", dynamicFoo._dict["Hello"]);
            Assert.Equal("qqq2", dynamicFoo._dict["Hello2"]);
        }

        [Fact]
        public void TestGenericMethod()
        {
            var result = dynamicFoo.SomeGenericMethod<string>("test");
            Assert.Equal("test", result);
        }

        [Fact]
        public void TestGenericMethodWithTwoArguments()
        {
            var result = dynamicFoo.SomeGenericMethod<string, int>("test", 1234);
            Assert.Equal(1234, result);
        }

        [Fact]
        public void MethodWithNoPrimitiveResult()
        {
            var result = (Exception)dynamicFoo.SomeMethodWithNoPrimitiveResult();

            Assert.NotNull(result);
        }

        [Fact]
        public void NullToGenericMethod()
        {
            var result = dynamicFoo.SomeGenericMethod<string>(null);

            Assert.Null(result);
        }

        [Fact]
        public void TestNullToMethod()
        {
            var result = dynamicFoo.SomeMethod(null);

            Assert.Null(result);
        }

#if !NETCOREAPP1_0 && !NETCOREAPP1_1
        [Fact]
        public void TestAddingByRef()
        {
            int a = 127;
            int b = 128;
            int c = 0;

            dynamicFoo.AddTwoRefParameters(ref a, ref b, ref c);

            Assert.Equal(255, c);
        }

        [Fact]
        public void TestAddingByOut()
        {
            int a = 127;
            int b = 110;

            dynamicFoo.AddTwoParametersWithOut(a, b, out int c);

            Assert.Equal(237, c);
        }

        [Fact]
        public void TestRefArgument()
        {
            object obj = "Test thingie";

            var returned = dynamicFoo.ReturnRefObject(ref obj);

            Assert.Equal(obj, returned);
        }

        [Fact]
        public void TestRefNullArgument()
        {
            object obj = null;

            var returned = dynamicFoo.ReturnRefObject(ref obj);

            Assert.Null(returned);
        }

        [Fact]
        public void TestPassNullToValueTypeRefParameter()
        {
            object first = null;
            int second = 2;
            int third = 0;

            Assert.Throws<MissingMethodException>(() => dynamicFoo.AddTwoRefParameters(ref first, ref second, ref third));
        }
#endif

        [Fact]
        public void TestFieldsAndProperties()
        {
            var fooBar = new FooBar();

#pragma warning disable CS0618 // Type or member is obsolete
            var properties = typeof(FooBar).GetProperties().Select(pi => pi.ToIProperty())
                                 .Union(
                             typeof(FooBar).GetFields().Select(fi => fi.ToIProperty()));
#pragma warning restore CS0618 // Type or member is obsolete

            foreach (var property in properties)
            {
                property.SetValue(fooBar, "test", null);
                Assert.Equal(typeof(string), property.PropertyType);
            }

            Assert.Equal("test", fooBar._field);
            Assert.Equal("test", fooBar.Property);
        }
    }
}
