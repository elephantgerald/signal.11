# Signal 11 — Claude Working Conventions

## Project Structure

- `src/Signal11.Domain` — pure game engine, zero HTTP/IO dependencies. Hard rule.
- `src/Signal11.Server` — REST API wrapping Domain. Owns persistence and HTTP surface.
- `src/Signal11.Client.Repl` — Zork-style CLI. Owns IUserInput and IDisplay.
- `tests/unit/` — fast, in-process, no I/O
- `tests/integration/` — may use file I/O or spin up a test server

## Key Conventions

### Injectable interfaces
Non-determinism is always injected, never called directly:
- `IRandom` — card shuffling (Domain)
- `IClock` — round deadlines (Domain)
- `IUserInput` — terminal input (Client.Repl)
- `IDisplay` — terminal output (Client.Repl)

### Domain purity
`Signal11.Domain.csproj` must never reference:
- Any `Microsoft.AspNetCore.*` package
- Any file I/O namespace beyond what the injected interfaces provide
- Any HTTP or networking library

### File formats
- Board files: binary SN11 format — see `designs/board-format.puml`
- Game state: JSON on disk at `data/games/{id}/state.json`
- Auth tokens: `data/games/{id}/tokens.json`
- Board snapshot per game: `data/games/{id}/board.bin`

### Naming
- Projects: `Signal11.<Component>` (Pascal case, dot-separated)
- Namespaces match project names
- Tests mirror the namespace of the thing they test

### Dev scripts
Always use `dev/build.sh`, `dev/run.sh`, `dev/test.sh` rather than raw dotnet commands.
This keeps CI and local dev consistent.

## Design Docs
- Specs: `designs/YYYY-MM-DD-<topic>-design.md`
- Plans: `designs/plans/YYYY-MM-DD-<topic>.md`
- Diagrams: `designs/*.puml` (C4 + PlantUML)
- Requirements: `requirements/MISSION.md`
