# Signal 11 — Local Dev

## Prerequisites
- .NET 10 SDK
- Docker (optional, for containerized server)
- `jq` (optional, for script output formatting)

## Quick Start

    ./dev/build.sh
    ./dev/run.sh server    # terminal 1
    ./dev/run.sh client    # terminal 2

## Scripts

| Script | Usage | Description |
|--------|-------|-------------|
| `build.sh` | `build.sh [all\|server\|client]` | Build projects |
| `run.sh` | `run.sh [server\|client]` | Run a project |
| `test.sh` | `test.sh [all\|server\|client\|domain]` | Run tests |

## Defaults
- Server URL: `http://localhost:5000`
- Game data: `dev/seed/games/`
- Boards: `dev/seed/boards/`
