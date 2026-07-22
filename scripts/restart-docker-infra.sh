#!/usr/bin/env bash
# Restart Postgres from repo docker-compose.yml (infra only).
set -euo pipefail
cd "$(dirname "$0")/.."
exec docker compose restart postgres
