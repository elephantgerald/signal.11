#!/usr/bin/env bash
# Creates a new game on the local server and prints the game ID.
# Usage: ./new-game.sh [server_url] [game_name]
SERVER="${1:-http://localhost:5000}"
NAME="${2:-TEST GAME}"
curl -s -X POST "$SERVER/games" \
  -H "Content-Type: application/json" \
  -d "{\"name\": \"$NAME\"}" | jq .
