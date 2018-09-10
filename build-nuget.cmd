@echo off
IF "%CI_CONFIG%"=="" (SET CI_CONFIG=Release) 
echo Starting build in %CI_CONFIG% mode.

dotnet clean -c %CI_CONFIG%
dotnet restore 
dotnet build -c %CI_CONFIG%
dotnet test --no-build --verbosity=normal -c %CI_CONFIG% test/ReflectionMagicTests/ReflectionMagicTests.csproj
dotnet pack --no-build /p:VersionSuffix="%CI_VERSION%" -c %CI_CONFIG% -o ..\..\ --include-symbols src/ReflectionMagic
