#!/bin/sh
# Quick health check on the VPS. Usage: ./diagnose-containers.sh [compose-dir]
set -e
COMPOSE_DIR="${1:-/etc/bonusesapp}"
COMPOSE_FILE="${BONUSESAPP_COMPOSE_FILE:-remote_docker-compose.yml}"
cd "$COMPOSE_DIR"

echo "=== disk / memory ==="
df -h / | tail -1
free -h

echo ""
echo "=== compose ps ==="
docker compose -f "$COMPOSE_FILE" ps -a

echo ""
echo "=== app logs (last 40 lines) ==="
docker compose -f "$COMPOSE_FILE" logs app --tail 40 2>&1 || true

echo ""
echo "=== sqlite file ==="
ls -lh /etc/bonusesapp/data/bonuses.db 2>&1 || echo "missing /etc/bonusesapp/data/bonuses.db"

echo ""
echo "=== ssl on host ==="
ls -la /etc/bonusesapp/ssl 2>&1 || echo "missing /etc/bonusesapp/ssl"

echo ""
echo "=== listen ports ==="
ss -tlnp | grep -E ':80|:443' || true
