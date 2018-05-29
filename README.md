# ReflectionMagic
[![Build status icon](https://teamcity.vandertil.net/app/rest/builds/buildType:(id:PublicProjects_ReflectionMagic_CI)/statusIcon)](https://teamcity.vandertil.net/viewLog.html?buildTypeId=PublicProjects_ReflectionMagic_CI&buildId=lastFinished)

Private reflection allows you to access private and internal members in other assemblies.  Generally, it’s considered to be a bad thing to do, as it ties you to undocumented implementation details which can later break you.  Also, it’s not usable in medium trust.

The purpose of this library is not to encourage anyone to use private reflection in situations where you would not have done it anyway.  Instead, the purpose is to allow you to do it much more easily if you decide that you need to use it. 

_Putting it a different way, I’m not telling you to break the law, but I’m telling you how to break the law more efficiently if that’s what you’re into!_

## The scenario

Assume you are using an assembly that has code like this:

```cs
public class Foo1 
{
    private Foo2 GetOtherClass() 
    { 
        // Omitted
    }
}

internal class Foo2 
{
    private string SomeProp { get { /* Omitted */ } }
}
```
And assume you have an instance _foo1_ of the public class Foo1 and your evil self tells you that you want to call the private method _GetOtherClass()_ and then get the _SomeProp_ property off that.

### Using reflection
Using plain old reflection this would be something like this:

```cs
object foo2 = typeof(Foo1).InvokeMember("GetOtherClass", 
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                null, foo1, null);
                
PropertyInfo propInfo = foo2.GetType().GetProperty("SomeProp",    
                BindingFlags.Instance | BindingFlags.NonPublic);

string val = (string)propInfo.GetValue(foo2, null);
```
Which works, but is pretty ugly.

### Using ReflectionMagic
Doing the same but using the ReflectionMagic library:
```cs
string val = foo1.AsDynamic().GetOtherClass().SomeProp;
```

## More info

For more information look at the original blog post by David Ebbo: https://blogs.msdn.microsoft.com/davidebb/2010/01/18/use-c-4-0-dynamic-to-drastically-simplify-your-private-reflection-code/

## Known limitations
* Support for 'out' and 'ref' parameters is not available on .NET Core 1.x runtimes. This is a runtime limitation and results in a PlatformNotSupportedException.
