# Signal 11 — Mission Statement

Signal 11 is a digital adaptation of a robot-programming board game, built as a
client-server system in .NET 10. The name is `SIGSEGV` — Unix signal 11, sent
when a process touches memory it has no business accessing. When your robot falls
into a pit: your core was dumped.

## Design Values

1. **Domain purity** — game logic has no I/O dependencies, ever
2. **Inject don't hardcode** — randomness, time, and input are all injectable
3. **Server is truth** — all game state lives on the server; clients are views
4. **Corruption isolation** — one JSON file per game; one corrupt game affects nothing else
5. **Determinism first** — same inputs always produce the same outputs
6. **Zork soul** — the REPL narrates events, not state; things happen *to* your robot

## Versioning Roadmap

### v1.0 — Full Base Game
- 2-8 players
- Full base game rules: conveyor belts, lasers, pits, gears, repair stations, flags
- Upgrade/customization cards excluded
- Zork-style REPL client
- REST API server
- Diplomacy-mode round timer OR open-ended (Quake mode)

### v1.1 — Upgrade Cards
- Add upgrade/customization card mechanic
- Board cell word expands from 16-bit to 32-bit

### v2.0 — Puzzle Mode
- Deterministic environmental interactions
- Single-player support
- Signal 11 as a puzzle game: predictable complexity, perfect information

### Future Horizons
- Up to 256-player support
- `LOOK AROUND` first-person REPL view
- Map editor / board builder tooling

## Rules Scope (v1.0)

**Included:**
- Move 1/2/3, Back Up, Rotate Left/Right, U-Turn program cards
- Priority-based execution order per register
- Conveyor belts (normal + express), gears, pits, repair stations
- Wall lasers and robot lasers
- Flags 1-4 (touch in order to win)
- Start positions (1-8)
- Archive markers (set by touching a flag or repair station)
- Damage tokens (0-9), powered-down state
- Robots pushing each other

**Excluded in v1.0:**
- Upgrade/customization option cards (v1.1)
- Multi-board tile layouts (future)
