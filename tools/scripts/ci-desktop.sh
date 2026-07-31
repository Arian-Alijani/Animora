#!/usr/bin/env bash
# Local stand-in for the desktop CI job (TECH_STACK §15): restore -> build -> test over the desktop
# solution, in the same order and configuration CI uses, so a local red equals a CI red.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
SOLUTION="$REPO_ROOT/desktop/Animora.Desktop.sln"
CONFIGURATION="${CONFIGURATION:-Release}"

# bootstrap-dotnet.sh installs into $HOME/.dotnet without editing PATH, so resolve it here too.
if ! command -v dotnet >/dev/null 2>&1; then
  export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
  export PATH="$DOTNET_ROOT:$PATH"
fi

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet not found: run tools/scripts/bootstrap-dotnet.sh first" >&2
  exit 1
fi

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

echo "== restore =="
dotnet restore "$SOLUTION"

echo "== build ($CONFIGURATION) =="
dotnet build "$SOLUTION" --configuration "$CONFIGURATION" --no-restore

# Zero tests in a project is not a failure here: the test projects exist before the code they cover.
echo "== test ($CONFIGURATION) =="
dotnet test "$SOLUTION" --configuration "$CONFIGURATION" --no-build

echo "desktop ci script completed: restore + build + test green"
