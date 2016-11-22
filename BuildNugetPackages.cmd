dotnet restore src/ReflectionMagic
dotnet restore test/ReflectionMagicTests
dotnet test -f netcoreapp1.0 test/ReflectionMagicTests
dotnet test -f net451 test/ReflectionMagicTests
dotnet pack -c Release -o . src/ReflectionMagic
