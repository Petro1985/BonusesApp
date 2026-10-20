#!/bin/sh
# Run on the VPS as root before/after first deploy.
# Container app user is uid 1654 (.NET aspnet image).
set -e
LOG_DIR="${1:-/etc/bonusesapp/logs}"
mkdir -p "$LOG_DIR"
chown -R 1654:1654 "$LOG_DIR"
chmod 755 "$LOG_DIR"
echo "OK: $LOG_DIR owned by 1654:1654"
