using System;
using System.Collections.Generic;

namespace LibraryWithPrivateMembers
{
    // Only used as a covenient way to refer to this assembly
    public class MarkerType
    {
    }

    internal class FooBase
    {
        private int _somePrivateIntegerField = -1;

        private static int _somePrivateStaticIntegerField = 17;

        private int AddIntegers(int n1, int n2)
        {
            return n1 + n2;
        }


        private static double AddDoubles(double f1, double f2)
        {
            return f1 + f2;
        }

        internal string SomeMethodThatGetsHiddenInDerivedClass()
        {
            return "FooBase.SomeMethod";
        }

        internal string SomePropertyThatGetsHiddenInDerivedClass { get { return "FooBase.SomePropertyThatGetsHiddenInDerivedClass"; } }
    }

    internal class Foo : FooBase
    {
        public Foo()
        {
        }

        private Foo(int n)
        {
            SomeInternalInteger = n;
        }

        private Bar _bar = new Bar();
        private Bar _barNull;

        internal int SomeInternalInteger { get; set; }
        public bool SomePublicBool { get; set; }
        private string SomePrivateString { get; set; }

        private static string SomeFooStaticStringProperty { get; set; }

        private int AddIntegers(int n1, int n2, int n3)
        {
            return n1 + n2 + n3;
        }

        private string ReturnStringFromBarObject(Bar bar)
        {
            return bar.SomeBarStringProperty;
        }

        private string ReturnTypeName(Type t)
        {
            return t.FullName;
        }

        private void AddTwoParametersWithOut(int a, int b, out int c)
        {
            c = a + b;
        }

        private void AddTwoRefParameters(ref int a, ref int b, ref int result)
        {
            result = a + b;
        }

        private Dictionary<string, string> _dict = new Dictionary<string, string>();
        internal string this[string s]
        {
            get
            {
                return _dict[s];
            }
            set
            {
                _dict[s] = value;
            }
        }

        private T SomeGenericMethod<T>(T value)
        {
            return value;
        }

        private T2 SomeGenericMethod<T, T2>(T value, T2 value2)
        {
            return value2;
        }

        private Exception SomeMethodWithNoPrimitiveResult()
        {
            return new Exception();
        }

        internal new string SomeMethodThatGetsHiddenInDerivedClass()
        {
            return "Foo.SomeMethod";
        }

        internal new string SomePropertyThatGetsHiddenInDerivedClass { get { return "Foo.SomePropertyThatGetsHiddenInDerivedClass"; } }
    }

    internal class Bar
    {
        internal string SomeBarStringProperty { get; set; }
    }

    public class FooBar
    {
        public string _field;
        public string Property { get; set; }
    }
}
