dotnet restore src/ReflectionMagic
dotnet restore test/ReflectionMagicTests
dotnet test test/ReflectionMagicTests
dotnet pack -c Release -o . src/ReflectionMagic
