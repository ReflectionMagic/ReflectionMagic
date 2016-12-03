@echo off
dotnet restore 
dotnet test test/ReflectionMagicTests
dotnet pack --version-suffix "%CI_VERSION%" -c Release -o . src/ReflectionMagic
