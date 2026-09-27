#!/bin/sh
set -eu
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 10.0 -InstallDir ./dotnet
./dotnet/dotnet workload install wasm-tools
./dotnet/dotnet --version

mkdir -p "$HOME/.nuget/NuGet"
./dotnet/dotnet nuget add source \
  "https://nuget.pkg.github.com/matthewsteeples/index.json" \
  --name "github-matthewsteeples" \
  --username "$GITHUB_USERNAME" \
  --password "$GITHUB_PACKAGES_TOKEN" \
  --store-password-in-clear-text \
  --configfile "$HOME/.nuget/NuGet/NuGet.Config"

./dotnet/dotnet test ../Shared.Tests/Shared.Tests.csproj -c Release
./dotnet/dotnet publish ../Client/Client.csproj -c Release -o ./public
