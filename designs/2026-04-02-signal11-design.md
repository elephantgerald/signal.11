# Signal 11 — System Design

**Date:** 2026-04-02
**Status:** Approved

---

## Overview

Signal 11 is a digital adaptation of a robot-programming board game, built as a .NET 10 client-server project. The name derives from `SIGSEGV` — Unix signal 11, sent when a process accesses memory it has no business touching. When your robot falls into a pit, your core is dumped.

The first client is a Zork-style command-line REPL. No graphics. Pure narration.

---

## Versioning Roadmap

- **v1.0** — Full base game, 2-8 players, minus upgrade/customization cards
- **v1.1** — Add upgrade/customization cards
- **v2.0** — Deterministic puzzle mode with complex environmental interactions

Future horizons: single-player mode, up to 256-player support.

---

## Solution Structure

```
signal11/
├── src/
│   ├── Signal11.Domain/              # Pure C# game engine — no I/O, no HTTP
│   ├── Signal11.Server/              # ASP.NET Core 10 REST API
│   └── Signal11.Client.Repl/        # Zork-style CLI client
├── tests/
│   ├── unit/
│   │   ├── Signal11.Domain.Tests/
│   │   ├── Signal11.Server.Tests/
│   │   └── Signal11.Client.Repl.Tests/
│   └── integration/
│       ├── Signal11.Domain.Integration.Tests/
│       └── Signal11.Server.Integration.Tests/
├── designs/                          # C4 + PlantUML decision docs
├── requirements/                     # Mission statement + versioned requirements
├── dev/                              # Local dev support
├── docs/                             # Design specs (this file)
├── README.md
├── CLAUDE.md
├── ETHOS.md
├── LICENSE                           # Apache 2.0
└── Signal11.sln
```

---

## Architecture

**Option B — Domain Library + Hosted Server + Clients**

- `Signal11.Domain` — pure C# class library, zero HTTP/IO dependencies. All game logic lives here. Fully deterministic and unit-testable in isolation.
- `Signal11.Server` — ASP.NET Core 10 minimal API, wraps Domain, owns persistence and REST surface.
- `Signal11.Client.Repl` — console app, talks to Server via HTTP, owns the Zork-style narration layer.

**Hard rule:** `Signal11.Domain` has no NuGet references to anything web, HTTP, or I/O. All non-determinism is injected.

### Injectable Interfaces (Domain)

| Interface | Purpose |
|---|---|
| `IRandom` | Card shuffling, any randomness |
| `IClock` | Round deadlines, timestamps |

### Injectable Interfaces (REPL)

| Interface | Purpose |
|---|---|
| `IUserInput` | Terminal input — injectable for scripted test sessions |
| `IDisplay` | Terminal output — injectable for test assertions |

---

## Domain Model

### Board
- Grid of `Cell` structs: floor type, conveyor direction + speed, gear spin, wall edges
- Loaded from binary `.board` files (see File Formats)
- Named per board convention: `1a.board`, `2b.board`, etc.

### Cell (binary encoding — see File Formats)

### Robot
- Position, facing direction, health (damage tokens), archive marker, powered-down state
- Owned by a `Player`

### Player
- Identity (id, name, description), hand of `ProgramCard`s, submitted `Program` (5 registers), connection state

### ProgramCard
- Type: Move 1/2/3, Back Up, Rotate Left/Right, U-Turn
- Priority number (determines execution order within a register)

### Game
- Players + robots, board reference + orientation, round number, phase state machine
- Phase states: `Lobby` → `Dealing` → `Programming` → `Executing` → `Cleanup` → `Dealing` (or `GameOver`)
- Configurable round timer: deadline-based (Diplomacy mode) or open-ended (Quake mode)

### ExecutionEngine
- Pure function: `GameState + Programs → GameState + ExecutionLog`
- `ExecutionLog` is an ordered list of events for the REPL to narrate

---

## REST API

All responses return JSON. Player identity via token in `Authorization: Bearer <token>` header.

### Lobby
```
POST   /games                    # Create game (caller becomes owner)
POST   /games/{id}/join          # Join with player name, returns playerId + token
GET    /games                    # List active games
GET    /games/{id}               # Full game state snapshot
POST   /games/{id}/start         # Start game (owner only)
DELETE /games/{id}               # Abandon game
```

### Programming Phase
```
GET    /games/{id}/hand          # Your current card hand
POST   /games/{id}/program       # Submit 5-register program
DELETE /games/{id}/program       # Retract submission (before execution)
GET    /games/{id}/status        # How many players have submitted
```

### Execution
```
GET    /games/{id}/log           # Ordered execution log for current/last round
GET    /games/{id}/board         # Board state with all robot positions
```

### Health
```
GET    /health
```

---

## REPL Client

### Command Vocabulary

**Connection & Identity**
```
CONNECT <url>
SET NAME <name>                # Local state — used as default name when joining
SET DESC <description>         # MUD/MUSH style robot description, sent on join
```

**Game Lifecycle**
```
MAKE GAME "<name>"             # Create game, become owner
LIST GAMES
JOIN GAME <id> AS <name>
START GAME                     # Owner only
LEAVE GAME
```

**Awareness**
```
LOOK BOARD                     # ASCII render of board + robot positions
LOOK CLOCK                     # Round phase, deadline, submission count
LOOK HAND                      # Your current cards
LOOK PROGRAM                   # Your submitted program
LOOK ROBOTS                    # All players: stats + position
LOOK ROBOT <name>              # One robot: SET DESC + stats
```

**Programming**
```
PROGRAM <card numbers>         # e.g. PROGRAM 1 3 7 9 2
RETRACT                        # Withdraw submission
WATCH                          # Narrate the execution log
```

Future v2: `LOOK AROUND` — first-person adjacent cell view.

### Sample Session
```
SIGNAL 11 v0.1.0
> CONNECT http://localhost:5000
CONNECTED. SERVER ONLINE. 3 ACTIVE GAMES.
> MAKE GAME "THE FOUNDRY"
GAME CREATED. ID: abc123. YOU ARE THE OWNER.
> SET DESC A battered cleaning bot repurposed for glory. Smells of pine.
DESCRIPTION SET.
> START GAME
GAME STARTED. BOARD: 1A. 4 PLAYERS. ROUND 1 BEGINS.
> PROGRAM 1 3 7 9 2
PROGRAM SUBMITTED. WAITING FOR 3 OTHER PLAYERS...
> WATCH
REGISTER 1 — MOVE 3 (priority 790, VOLTRON moves first)
  VOLTRON lurches forward 3 spaces.
  BENDER is in the way. BENDER absorbs 1 damage.
  BENDER falls into a pit.
  YOUR CORE WAS DUMPED, BENDER. YOU RESPAWN AT ARCHIVE MARKER 2.
```

---

## File Formats

### Board File (`boards/1a.board`)

**Header block:**
```
[4 bytes]  magic: "SN11" (0x534E3131)
[1 byte]   version
[1 byte]   width (cells)
[1 byte]   height (cells)
[1 byte]   flag count
[N bytes]  flag positions (2 bytes each: x, y)
```

**Data block** (width × height × 2 bytes, row-major):

Per cell — 2 bytes (16 bits), 6 bits reserved for future expansion:
```
bits 15-12: floor type (4 bits)
bits 11-9:  conveyor direction (3 bits)
bit  8:     conveyor speed (1 bit)
bits 7-6:   gear (2 bits)
bits 5-0:   reserved
```

Floor type values:
| Value | Type |
|---|---|
| 0 | Normal |
| 1 | Pit |
| 2 | Repair (wrench) |
| 3 | Double repair (wrench + hammer) |
| 4-7 | Flag 1-4 |
| 8-15 | Start position 1-8 |

Conveyor direction: `000`=none, `001`=N, `010`=E, `011`=S, `100`=W

Gear: `00`=none, `01`=CW, `10`=CCW

Note: cell word will expand from 16-bit to 32-bit in v1.1 to accommodate upgrade card interactions. Version byte in header controls parser behaviour.

**Wall block** (width × height × 1 byte, parallel to data block):
```
bit 7: north wall
bit 6: east wall
bit 5: south wall
bit 4: west wall
bit 3: north wall has laser
bit 2: east wall has laser
bit 1: south wall has laser
bit 0: west wall has laser
```

### Game State (`games/{id}/state.json`)
```json
{
  "id": "abc123",
  "name": "THE FOUNDRY",
  "board": {
    "name": "1a",
    "orientation": 90
  },
  "round": 3,
  "phase": "programming",
  "deadline": "2026-04-02T23:59:00Z",
  "players": [...],
  "robots": [...],
  "log": [...]
}
```

Board orientation: `0`, `90`, `180`, or `270` degrees. The execution engine applies rotation transforms; the `.board` file is always stored at 0°.

### Auth (`games/{id}/tokens.json`)
```json
{
  "p1": "<token>",
  "p2": "<token>"
}
```

### Game folder structure
```
data/
  games/
    {id}/
      state.json     # game truth
      tokens.json    # auth truth
      board.bin      # snapshot of board at game creation
```

---

## Dev Environment (`./dev`)

```
dev/
  README.md
  docker-compose.yml
  build.sh              # build.sh [all|server|client]
  run.sh                # run.sh [server|client]
  test.sh               # test.sh [all|server|client|domain]
  scripts/
    new-game.sh
    watch-game.sh
  seed/
    boards/
    games/
```

### Script contracts

```bash
./dev/build.sh [all|server|client]       # defaults to all
./dev/run.sh [server|client]
./dev/test.sh [all|server|client|domain] # defaults to all
```

All scripts exit non-zero on failure for CI composability.

---

## Documentation

- `designs/system-context.puml` — C4 Level 1: Signal 11 in the world
- `designs/container.puml` — C4 Level 2: Domain / Server / Client
- `designs/board-format.puml` — binary format diagram
- `requirements/MISSION.md` — versioned roadmap, rules scope, player targets

PlantUML with C4 extension is the standard for all technical diagrams.

---

## Key Design Principles

1. **Domain purity** — game logic has no I/O dependencies, ever
2. **Inject don't hardcode** — randomness, time, and input are all injectable
3. **Server is truth** — all game state lives on the server; clients are views
4. **Corruption isolation** — one JSON file per game; one corrupt game doesn't affect others
5. **Determinism first** — same inputs always produce same outputs; foundation for v2 puzzle mode
6. **Zork soul** — the REPL narrates events, not state; things happen *to* your robot
