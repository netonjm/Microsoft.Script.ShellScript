#!/bin/bash

# nuget restoring
if [ ! -f ./tools/nuget.exe ]; then
	mkdir -p tools
    echo "nuget.exe not found! downloading latest version"
    curl -O https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
    mv nuget.exe tools/
fi
echo "Restoring nugets packages..."
mono tools/nuget.exe restore src/Xamarin.ShellScript.sln

echo "build project in release mode"
xbuild /p:Configuration=Release src/Xamarin.ShellScript/Xamarin.ShellScript.csproj 

echo "Generating nuget..."
mono tools/nuget.exe pack templates/nuget/Xamarin.ShellScript.nuspec -outputdirectory templates/nuget/packages/

echo "Finished."
