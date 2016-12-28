@echo off
IF "%CI_CONFIG%"=="" (SET CI_CONFIG=Release) 
echo Starting build in %CI_CONFIG% mode.

dotnet restore 
dotnet test -c %CI_CONFIG% test/ReflectionMagicTests
dotnet pack --version-suffix "%CI_VERSION%" -c %CI_CONFIG% -o . src/ReflectionMagic
