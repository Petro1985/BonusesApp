#!/bin/bash
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <fqdn> [extra certbot args, e.g. -d www.example.com]"
  exit 1
fi

PRIMARY="$1"
shift

: "${BONUSESAPP_TLS_EMAIL:?Set BONUSESAPP_TLS_EMAIL for Lets Encrypt}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export BONUSESAPP_TLS_DOMAIN="${PRIMARY}"

"${SCRIPT_DIR}/bonusesapp-certbot-pre.sh"

if ! certbot certonly --standalone --non-interactive --agree-tos \
  -m "${BONUSESAPP_TLS_EMAIL}" \
  -d "${PRIMARY}" "$@"; then
  "${SCRIPT_DIR}/bonusesapp-certbot-post.sh" || true
  exit 1
fi

"${SCRIPT_DIR}/bonusesapp-certbot-post.sh"
"${SCRIPT_DIR}/bonusesapp-deploy-pfx.sh"

echo "Configure renewal hooks in /etc/letsencrypt/renewal/${PRIMARY}.conf — see scripts/vps/00-SETUP-TLS.txt"
