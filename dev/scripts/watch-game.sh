#!/usr/bin/env bash
# Polls game state every 2 seconds and prints it.
# Usage: ./watch-game.sh <game_id> [server_url]
GAME_ID="${1:?Usage: watch-game.sh <game_id> [server_url]}"
SERVER="${2:-http://localhost:5000}"
while true; do
  clear
  curl -s "$SERVER/games/$GAME_ID" | jq .
  sleep 2
done
