#!/bin/bash
# Fathom — SessionStart hook for Claude Code on the web.
# Installs the .NET 10 SDK + Angular UI deps so web sessions can build/test the
# ASP.NET Core backend (Kavita.sln) and the Angular app (UI/Web).
set -euo pipefail

# Only run in Claude-on-the-web remote environments.
if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

PROJECT_DIR="${CLAUDE_PROJECT_DIR:-$PWD}"
ENV_FILE="${CLAUDE_ENV_FILE:-/dev/null}"
DOTNET_DIR="$HOME/.dotnet"

# 1) .NET 10 SDK (best-effort). Requires the env network policy to allow Microsoft's
#    .NET CDN (builds.dotnet.microsoft.com / dotnetcli.azureedge.net). NuGet is already reachable.
if [ -x "$DOTNET_DIR/dotnet" ]; then
  echo ".NET SDK already present: $("$DOTNET_DIR/dotnet" --version 2>/dev/null || echo unknown)"
else
  echo "Installing .NET 10 SDK..."
  if curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh 2>/dev/null; then
    bash /tmp/dotnet-install.sh --channel 10.0 --install-dir "$DOTNET_DIR" \
      || echo "WARN: .NET SDK install failed — allow builds.dotnet.microsoft.com / dotnetcli.azureedge.net in the network policy, then relaunch."
  else
    echo "WARN: cannot reach the .NET install script (dot.net returned 403). Allow Microsoft's .NET CDN in the env network policy, then relaunch."
  fi
fi

# Expose dotnet on PATH for the rest of the session, if it installed.
if [ -x "$DOTNET_DIR/dotnet" ]; then
  {
    echo "export DOTNET_ROOT=\"$DOTNET_DIR\""
    echo "export PATH=\"$DOTNET_DIR:\$PATH\""
  } >> "$ENV_FILE"
fi

# 2) Angular UI dependencies. UI/Web/.npmrc already sets legacy-peer-deps=true.
if [ -d "$PROJECT_DIR/UI/Web" ] && [ ! -d "$PROJECT_DIR/UI/Web/node_modules" ]; then
  echo "Installing UI dependencies (npm install)..."
  ( cd "$PROJECT_DIR/UI/Web" && npm install )
else
  echo "UI dependencies already present (skipping)."
fi

echo "Session start hook complete."
