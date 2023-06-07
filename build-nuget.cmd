@echo off
IF "%Configuration%"=="" (SET Configuration=Release)
IF "%Configuration%"=="Release" (SET ContinuousIntegrationBuild=true)
echo Starting build in %Configuration% mode (ContinuousIntegrationBuild=%ContinuousIntegrationBuild%).

dotnet clean
dotnet restore
dotnet build --no-restore
dotnet test --no-build
dotnet pack --no-build

set Configuration=
set ContinuousIntegrationBuild=

dotnet tool restore
dotnet validate package local **\*.nupkg
