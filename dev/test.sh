#!/usr/bin/env bash
set -e
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TARGET="${1:-all}"

case "$TARGET" in
  all)
    dotnet test "$REPO_ROOT/Signal11.sln"
    ;;
  domain)
    dotnet test "$REPO_ROOT/tests/unit/Signal11.Domain.Tests/Signal11.Domain.Tests.csproj"
    dotnet test "$REPO_ROOT/tests/integration/Signal11.Domain.Integration.Tests/Signal11.Domain.Integration.Tests.csproj"
    ;;
  server)
    dotnet test "$REPO_ROOT/tests/unit/Signal11.Server.Tests/Signal11.Server.Tests.csproj"
    dotnet test "$REPO_ROOT/tests/integration/Signal11.Server.Integration.Tests/Signal11.Server.Integration.Tests.csproj"
    ;;
  client)
    dotnet test "$REPO_ROOT/tests/unit/Signal11.Client.Repl.Tests/Signal11.Client.Repl.Tests.csproj"
    ;;
  *)
    echo "Usage: test.sh [all|server|client|domain]"
    exit 1
    ;;
esac
