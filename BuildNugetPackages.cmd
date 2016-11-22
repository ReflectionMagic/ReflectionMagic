dotnet restore 
dotnet test test/ReflectionMagicTests
dotnet pack -c Release -o . src/ReflectionMagic
