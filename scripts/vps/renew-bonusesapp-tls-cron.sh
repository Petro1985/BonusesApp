#!/bin/bash
set -euo pipefail
# Intended for cron every ~2 months. certbot renew only replaces the cert when due (typically <30 days to expiry).

if [[ -f /etc/default/bonusesapp-tls ]]; then
  set -a
  # shellcheck source=/dev/null
  . /etc/default/bonusesapp-tls
  set +a
fi

: "${BONUSESAPP_TLS_DOMAIN:?}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

certbot renew --quiet --no-random-sleep-on-renew || true
"${SCRIPT_DIR}/bonusesapp-deploy-pfx.sh"
