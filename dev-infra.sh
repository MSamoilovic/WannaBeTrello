#!/usr/bin/env bash
# Starts postgres, redis and pgadmin for local API development.
# Usage:
#   ./dev-infra.sh          # start
#   ./dev-infra.sh stop     # stop
#   ./dev-infra.sh logs     # tail logs

set -euo pipefail

SERVICES="postgres redis"
TOOLS_SERVICES="pgadmin mailpit"

case "${1:-start}" in
  start)
    echo "Starting dev infrastructure (postgres + redis + pgadmin + mailpit)..."
    docker compose up -d $SERVICES
    docker compose --profile tools up -d $TOOLS_SERVICES
    echo "Done."
    echo "  API:     dotnet run --project Feezbow.API"
    echo "  pgAdmin: http://localhost:${PGADMIN_PORT:-5050}"
    echo "  Mailpit: http://localhost:${MAILPIT_UI_PORT:-8025}"
    ;;
  stop)
    docker compose --profile tools stop $TOOLS_SERVICES
    docker compose stop $SERVICES
    ;;
  logs)
    docker compose --profile tools logs -f $SERVICES $TOOLS_SERVICES
    ;;
  *)
    echo "Usage: $0 [start|stop|logs]"
    exit 1
    ;;
esac
