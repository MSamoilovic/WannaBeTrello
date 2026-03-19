#!/usr/bin/env bash
# Builds and runs the Feezbow API.
# Usage:
#   ./dev-run.sh            # build + run
#   ./dev-run.sh --no-build # skip build, just run

set -euo pipefail

SKIP_BUILD=false

for arg in "$@"; do
  [[ "$arg" == "--no-build" ]] && SKIP_BUILD=true
done

if [[ "$SKIP_BUILD" == false ]]; then
  echo "Building solution..."
  dotnet build Feezbow.sln --configuration Debug
fi

echo "Starting API..."
dotnet run --project Feezbow.API --configuration Debug
