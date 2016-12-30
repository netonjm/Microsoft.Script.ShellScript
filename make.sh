#!/bin/bash

# nuget restoring
if [ ! -f ./tools/nuget.exe ]; then
	mkdir -p tools
    echo "nuget.exe not found! downloading latest version"
    curl -O https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
    mv nuget.exe tools/
fi
echo "Restoring nugets packages..."
mono tools/nuget.exe restore src/Microsoft.Script.ShellScript.sln

echo "build project in release mode"
xbuild /p:Configuration=Release src/Microsoft.Script.ShellScript/Microsoft.Script.ShellScript.csproj 

echo "Generating nuget..."
mono tools/nuget.exe pack templates/nuget/Microsoft.Script.ShellScript.nuspec -outputdirectory templates/nuget/packages/

echo "Finished."
