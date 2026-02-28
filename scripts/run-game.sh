#!/usr/bin/env bash
set -euo pipefail

export PATH="$HOME/.dotnet:$PATH"
dotnet run --project src/GalacticCoopShooter/GalacticCoopShooter.csproj
