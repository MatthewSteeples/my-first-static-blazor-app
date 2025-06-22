#!/bin/sh
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 9.0 -InstallDir ./dotnet
./dotnet/dotnet workload install wasm-tools
./dotnet/dotnet --version
./dotnet/dotnet publish ../Client/Client.csproj -c Release -o ./public