@echo off
IF "%CI_CONFIG%"=="" (SET CI_CONFIG=Release) 
echo Starting build in %CI_CONFIG% mode.

dotnet restore 
dotnet test --verbosity=normal -c %CI_CONFIG% test/ReflectionMagicTests/ReflectionMagicTests.csproj
dotnet pack /p:VersionSuffix="%CI_VERSION%" -c %CI_CONFIG% -o ..\..\ --include-symbols src/ReflectionMagic
