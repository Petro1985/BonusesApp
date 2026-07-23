#!/bin/bash
set -euo pipefail

if [[ -f /etc/default/bonusesapp-tls ]]; then
  set -a
  # shellcheck source=/dev/null
  . /etc/default/bonusesapp-tls
  set +a
fi

COMPOSE_DIR="${BONUSESAPP_COMPOSE_DIR:-/root/bonuses-app}"
COMPOSE_FILE="${BONUSESAPP_COMPOSE_FILE:-}"
cd "${COMPOSE_DIR}"
if [[ -n "${COMPOSE_FILE}" ]]; then
  docker compose -f "${COMPOSE_FILE}" "$@"
elif [[ -f "${COMPOSE_DIR}/docker-compose.yml" ]]; then
  docker compose -f docker-compose.yml "$@"
else
  docker compose "$@"
fi
