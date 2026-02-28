#!/usr/bin/env bash
set -euo pipefail

export PATH="$HOME/.dotnet:$PATH"
dotnet restore
dotnet build GalacticCoopShooter.sln
