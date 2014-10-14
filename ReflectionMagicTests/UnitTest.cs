using System;
using System.Web.UI;
using LibraryWithPrivateMembers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReflectionMagic;

namespace ReflectionMagicTests
{
    [TestClass]
    public class UnitTest
    {
        private dynamic dynamicFoo;
        private dynamic dynamicFooType;

        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
        }

        [TestInitialize()]
        public void TestInitialize()
        {
            dynamicFooType = typeof(MarkerType).Assembly.GetDynamicType("LibraryWithPrivateMembers.Foo");
            dynamicFoo = typeof(MarkerType).Assembly.CreateDynamicInstance("LibraryWithPrivateMembers.Foo");
        }

        [TestMethod]
        public void TestPropertyGetAndSetInternalInteger()
        {
            dynamicFoo.SomeInternalInteger = 17;
            Assert.AreEqual(17, dynamicFoo.SomeInternalInteger);
        }

        [TestMethod]
        public void TestPropertyGetAndSetPublicBool()
        {
            dynamicFoo.SomePublicBool = true;
            Assert.AreEqual(true, dynamicFoo.SomePublicBool);
        }

        [TestMethod]
        public void TestPropertyGetAndSetPrivateString()
        {
            dynamicFoo.SomePrivateString = "Hello";
            Assert.AreEqual("Hello", dynamicFoo.SomePrivateString);
        }

        [TestMethod]
        public void TestPrivateIntegerField()
        {
            Assert.AreEqual(-1, dynamicFoo._somePrivateIntegerField);
            dynamicFoo._somePrivateIntegerField = 12;
            Assert.AreEqual(12, dynamicFoo._somePrivateIntegerField);
        }

        [TestMethod]
        public void TestOperatorOnIntegers()
        {
            dynamicFoo.SomeInternalInteger = 17;
            Assert.AreEqual(35, dynamicFoo.SomeInternalInteger * 2 + 1);
        }

        [TestMethod]
        public void TestOperatorOnStrings()
        {
            dynamicFoo.SomePrivateString = "Hello";
            Assert.AreEqual("Hello world", dynamicFoo.SomePrivateString + " world");
            Assert.AreEqual("Say Hello", "Say " + dynamicFoo.SomePrivateString);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMissingProperty()
        {
            dynamicFoo.NotExist = "Hello";
        }

        [TestMethod]
        public void TestMethodCalls()
        {
            dynamicFoo.SomeInternalInteger = 17;

            // This one is defined on the base type
            var sum = dynamicFoo.AddIntegers(dynamicFoo.SomeInternalInteger, 3);
            Assert.AreEqual(20, sum);

            // Different overload defined on the type itself
            sum = dynamicFoo.AddIntegers(dynamicFoo.SomeInternalInteger, 3, 4);
            Assert.AreEqual(24, sum);
        }

        [TestMethod]
        public void TestCallToMethodThatHidesBaseMethod()
        {
            var val = dynamicFoo.SomeMethodThatGetsHiddenInDerivedClass();

            Assert.AreEqual("Foo.SomeMethod", val);
        }

        [TestMethod]
        public void TestCallToPropertyThatHidesBaseProperty()
        {
            var val = dynamicFoo.SomePropertyThatGetsHiddenInDerivedClass;

            Assert.AreEqual("Foo.SomePropertyThatGetsHiddenInDerivedClass", val);
        }

        [TestMethod]
        public void TestMethodCallThatTakesObject()
        {
            dynamicFoo._bar.SomeBarStringProperty = "Blah1";
            var barString = dynamicFoo.ReturnStringFromBarObject(dynamicFoo._bar.RealObject);
            Assert.AreEqual("Blah1", barString);

            dynamicFoo._bar.SomeBarStringProperty = "Blah2";
            barString = dynamicFoo.ReturnStringFromBarObject(dynamicFoo._bar);
            Assert.AreEqual("Blah2", barString);
        }

        [TestMethod]
        public void TestMethodCallThatTakesType()
        {
            var typeName = dynamicFoo.ReturnTypeName(dynamicFooType);
            Assert.AreEqual("LibraryWithPrivateMembers.Foo", typeName);
        }

        [TestMethod]
        public void TestPropertyGetAndSetOnSubObject()
        {
            dynamicFoo._bar.SomeBarStringProperty = "Blah";
            Assert.AreEqual("Blah", dynamicFoo._bar.SomeBarStringProperty);
        }

        [TestMethod]
        public void TestGettingBackRealObject()
        {
            object realBar = dynamicFoo._bar.RealObject;
            Assert.AreEqual("Bar", realBar.GetType().Name);
        }

        [TestMethod]
        public void TestObjectInstantiationWithNonDefaultCtor()
        {
            var foo = dynamicFooType.New(123);
            Assert.AreEqual(123, foo.SomeInternalInteger);
        }

        [TestMethod]
        public void TestStaticPropertyGetAndSet()
        {
            dynamicFooType.SomeFooStaticStringProperty = "zzz";
            Assert.AreEqual("zzz", dynamicFooType.SomeFooStaticStringProperty);
        }

        [TestMethod]
        public void TestStaticFieldGetAndSet()
        {
            Assert.AreEqual(17, dynamicFooType._somePrivateStaticIntegerField);
            dynamicFooType._somePrivateStaticIntegerField++;
            Assert.AreEqual(18, dynamicFooType._somePrivateStaticIntegerField);
        }

        [TestMethod]
        public void TestStaticMethodCall()
        {
            var sum = dynamicFooType.AddDoubles(2.5, 3.5);
            Assert.AreEqual(6, sum);
        }

        [TestMethod]
        public void TestNullSubObject()
        {
            Assert.AreEqual(null, dynamicFoo._barNull);
        }

        [TestMethod]
        public void TestIndexedProperty()
        {
            dynamicFoo["Hello"] = "qqq";
            dynamicFoo["Hello2"] = "qqq2";
            Assert.AreEqual("qqq", dynamicFoo["Hello"]);
            Assert.AreEqual("qqq2", dynamicFoo["Hello2"]);
        }

        [TestMethod]
        public void TestDictionaryAccess()
        {
            dynamicFoo._dict["Hello"] = "qqq";
            dynamicFoo._dict["Hello2"] = "qqq2";
            Assert.AreEqual("qqq", dynamicFoo._dict["Hello"]);
            Assert.AreEqual("qqq2", dynamicFoo._dict["Hello2"]);
        }

        [TestMethod]
        public void TestMiscSystemWebCode()
        {
            var sysWebAssembly = typeof(Control).Assembly;

            var page = (new Page()).AsDynamic();

            var control1 = new Control() { ID = "foo1" };
            page.Controls.Add(control1);
            var control2 = sysWebAssembly.CreateDynamicInstance("System.Web.UI.Control");
            control2.ID = "foo2";
            page._controls.Add(control2);


            Assert.AreEqual("foo1", control2._page._controls[0]._id);

            // 5 is the default capacity in the ControlCollection
            Assert.AreEqual(5, control2._page._controls._controls.Length);

            control2._page._maxResourceOffset = 77;
            Assert.AreEqual(77, page._maxResourceOffset);

            Assert.AreEqual(ClientIDMode.Inherit, (ClientIDMode)page.EffectiveClientIDModeValue);

            Assert.AreEqual("__Page", page.GetClientID());

            var buildManagerHostType = sysWebAssembly.GetDynamicType("System.Web.Compilation.BuildManagerHost");
            buildManagerHostType.InClientBuildManager = true;
            Assert.AreEqual(true, buildManagerHostType.InClientBuildManager);

            var buildManagerHost = buildManagerHostType.New();
            var buildManager = sysWebAssembly.CreateDynamicInstance("System.Web.Compilation.BuildManager");
            buildManagerHost._buildManager = buildManager;
            Assert.AreEqual(buildManagerHost._buildManager.RealObject, buildManager.RealObject);
        }

        [TestMethod]
        public void TestGenericMethod()
        {
            var result = dynamicFoo.SomeGenericMethod<string>("test");
            Assert.AreEqual("test", result);
        }

        [TestMethod]
        public void MethodWithNoPrimitiveResult()
        {
            var result = (Exception)dynamicFoo.SomeMethodWithNoPrimitiveResult();
        }
    }
}
