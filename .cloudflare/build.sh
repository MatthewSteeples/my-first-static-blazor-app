#!/bin/sh
set -eu
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 10.0 -InstallDir ./dotnet
./dotnet/dotnet workload install wasm-tools
./dotnet/dotnet --version
./dotnet/dotnet test ../Shared.Tests/Shared.Tests.csproj -c Release
./dotnet/dotnet publish ../Client/Client.csproj -c Release -o ./public
