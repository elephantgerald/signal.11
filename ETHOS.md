# Signal 11 — Ethos

## Why Signal 11?

Because the best board games are systems — emergent, chaotic, deterministic — and
the best software is the same. Signal 11 is an attempt to build both at once.

The name is `SIGSEGV`. Unix signal 11. What the kernel sends your process when it
reaches for memory it has no right to touch. On this board, that's every round.

## The Zork Principle

The first client is a command-line REPL. Not because we lack ambition —
because we want to nail the system before we dress it up. A game that plays
beautifully in text is a game that works. Every rule, every interaction, every
edge case: rendered in plain narration.

When your robot gets shoved into a pit by a conveyor belt you didn't account for,
the terminal simply says:

    YOUR CORE WAS DUMPED. YOU RESPAWN AT ARCHIVE MARKER 2.
    IT IS PROBABLY YOUR FAULT.

That's the tone. Dry. Precise. A little cruel.

## The Determinism Imperative

Same inputs. Same outputs. Always.

This is not just good engineering hygiene — it's the foundation of v2.0, where
Signal 11 becomes a deterministic puzzle game. Every board, every card, every
robot interaction produces exactly one outcome. The chaos is in the programming
phase. The execution phase is physics.

## Versioning Philosophy

- **v1.0** gets the game right.
- **v1.1** gets the cards right.
- **v2.0** gets the puzzles right.

No feature creep. No speculative abstractions. Build the thing that works,
then build the next thing on top of it.

## The 256-Player Horizon

Yes, eventually. A 256-player Signal 11 game on a tiled multi-board with a
Diplomacy-style 24-hour round timer is an absurd and beautiful thing to imagine.
We keep it on the horizon not because we'll get there soon,
but because knowing it's possible keeps the architecture honest.
