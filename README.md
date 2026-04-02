# Signal 11

A digital adaptation of a robot-programming board game. Named for `SIGSEGV`.
When your robot falls into a pit, your core is dumped.

Built in .NET 10 as a client-server system. The first client is a Zork-style
command-line REPL. No graphics. Pure narration.

## Quick Start

    ./dev/build.sh
    ./dev/run.sh server    # in one terminal
    ./dev/run.sh client    # in another terminal

At the REPL prompt:

    > CONNECT http://localhost:5000
    > MAKE GAME "THE FOUNDRY"
    > START GAME
    > LOOK BOARD

## Project Structure

    src/Signal11.Domain/          Pure C# game engine
    src/Signal11.Server/          ASP.NET Core 10 REST API
    src/Signal11.Client.Repl/     Zork-style CLI client
    tests/                        Unit and integration tests
    designs/                      C4 diagrams and design specs
    requirements/                 Mission statement and versioned requirements
    dev/                          Local dev scripts and seed data

## Development

See `dev/README.md` for full local dev instructions.

    ./dev/build.sh [all|server|client]
    ./dev/run.sh   [server|client]
    ./dev/test.sh  [all|server|client|domain]

## License

Apache 2.0 — see `LICENSE`.
