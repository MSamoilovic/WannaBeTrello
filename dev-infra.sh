#!/usr/bin/env bash
# Starts only postgres and redis for local API development.
# Usage:
#   ./dev-infra.sh          # start
#   ./dev-infra.sh stop     # stop (keep data)
#   ./dev-infra.sh down     # stop + remove containers (keep volumes)

set -euo pipefail

SERVICES="postgres redis"

case "${1:-start}" in
  start)
    echo "Starting dev infrastructure (postgres + redis)..."
    docker compose up -d $SERVICES
    echo ""
    echo "Waiting for services to be healthy..."
    docker compose ps $SERVICES
    echo ""
    echo "postgres -> localhost:${POSTGRES_PORT:-5432}"
    echo "redis    -> localhost:${REDIS_PORT:-6379}"
    echo ""
    echo "Run the API with: dotnet run --project Feezbow.API"
    ;;
  stop)
    echo "Stopping dev infrastructure..."
    docker compose stop $SERVICES
    ;;
  down)
    echo "Removing dev infrastructure containers..."
    docker compose rm -sf $SERVICES
    ;;
  *)
    echo "Usage: $0 [start|stop|down]"
    exit 1
    ;;
esac
