#!/usr/bin/env bash
set -e
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TARGET="${1:-all}"

case "$TARGET" in
  all)
    dotnet build "$REPO_ROOT/Signal11.sln"
    ;;
  server)
    dotnet build "$REPO_ROOT/src/Signal11.Server/Signal11.Server.csproj"
    ;;
  client)
    dotnet build "$REPO_ROOT/src/Signal11.Client.Repl/Signal11.Client.Repl.csproj"
    ;;
  *)
    echo "Usage: build.sh [all|server|client]"
    exit 1
    ;;
esac
