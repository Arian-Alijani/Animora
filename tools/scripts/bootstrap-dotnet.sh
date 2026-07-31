#!/usr/bin/env bash
# Installs the SDK pinned in global.json for environments that ship without one (agent sandboxes,
# fresh CI images). Idempotent: exits early when a matching SDK already resolves.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
CHANNEL="10.0"

if command -v dotnet >/dev/null 2>&1 && (cd "$REPO_ROOT" && dotnet --version >/dev/null 2>&1); then
  echo "dotnet $(cd "$REPO_ROOT" && dotnet --version) already satisfies global.json"
  exit 0
fi

# The installer extracts a ~1 GB tree through TMPDIR; the default /tmp is a small tmpfs in the
# agent sandbox, so point it at a path on the main filesystem.
WORK_DIR="$(mktemp -d "${TMPDIR:-$HOME}/animora-dotnet-XXXXXX")"
trap 'rm -rf "$WORK_DIR"' EXIT

curl -fsSL https://dot.net/v1/dotnet-install.sh -o "$WORK_DIR/dotnet-install.sh"
chmod +x "$WORK_DIR/dotnet-install.sh"
TMPDIR="$WORK_DIR" "$WORK_DIR/dotnet-install.sh" \
  --channel "$CHANNEL" --install-dir "$DOTNET_ROOT" --no-path

export DOTNET_ROOT
export PATH="$DOTNET_ROOT:$PATH"

echo "installed dotnet $(cd "$REPO_ROOT" && dotnet --version)"
echo "add to your shell profile: export DOTNET_ROOT=\"$DOTNET_ROOT\"; export PATH=\"\$DOTNET_ROOT:\$PATH\""
