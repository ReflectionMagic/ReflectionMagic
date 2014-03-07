pushd ReflectionMagic
msbuild /p:Configuration=Release
nuget pack -symbols -Prop Configuration=Release
move *.nupkg ..
popd
