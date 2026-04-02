#!/usr/bin/env bash
set -e
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TARGET="${1:-}"

case "$TARGET" in
  server)
    dotnet run --project "$REPO_ROOT/src/Signal11.Server/Signal11.Server.csproj"
    ;;
  client)
    dotnet run --project "$REPO_ROOT/src/Signal11.Client.Repl/Signal11.Client.Repl.csproj"
    ;;
  *)
    echo "Usage: run.sh [server|client]"
    exit 1
    ;;
esac
