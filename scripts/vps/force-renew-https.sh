#!/bin/bash
set -euo pipefail
# Full HTTPS renew: stop app → certbot renew → rebuild yourcert.pfx → start app.
# Does NOT touch authcert.pfx (OpenIddict).
#
# Usage (as root):
#   ~/bonuses-app/scripts/vps/force-renew-https.sh
#   FORCE_RENEW=0 ~/bonuses-app/scripts/vps/force-renew-https.sh   # only if due (<~30d)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ -f /etc/default/bonusesapp-tls ]]; then
  set -a
  # shellcheck source=/dev/null
  . /etc/default/bonusesapp-tls
  set +a
fi

: "${BONUSESAPP_TLS_DOMAIN:?Set BONUSESAPP_TLS_DOMAIN in /etc/default/bonusesapp-tls}"
CERT_NAME="${BONUSESAPP_TLS_DOMAIN}"
FORCE_RENEW="${FORCE_RENEW:-1}"

echo "==> Domain: ${CERT_NAME}"

# Fix CRLF if scripts were copied from Windows
if grep -q $'\r' "${SCRIPT_DIR}/bonusesapp-deploy-pfx.sh" 2>/dev/null; then
  echo "==> Stripping CRLF from scripts/vps/*.sh"
  sed -i 's/\r$//' "${SCRIPT_DIR}"/*.sh
fi
chmod +x "${SCRIPT_DIR}"/*.sh

echo "==> Stopping app (free port 80 for standalone)"
"${SCRIPT_DIR}/bonusesapp-certbot-pre.sh"

cleanup_start_app() {
  echo "==> Ensuring app is up"
  "${SCRIPT_DIR}/bonusesapp-certbot-post.sh" || true
}
trap cleanup_start_app EXIT

if ss -tlnp 2>/dev/null | grep -q ':80 '; then
  echo "ERROR: port 80 still in use after stop:" >&2
  ss -tlnp | grep ':80 ' >&2 || true
  exit 1
fi
echo "==> Port 80 is free"

RENEW_ARGS=(renew --no-random-sleep-on-renew --cert-name "${CERT_NAME}" -v)
if [[ "${FORCE_RENEW}" == "1" ]]; then
  RENEW_ARGS+=(--force-renewal)
  echo "==> certbot renew --force-renewal"
else
  echo "==> certbot renew (only if due)"
fi

certbot "${RENEW_ARGS[@]}"

# deploy-pfx rebuilds PFX and restarts app; disable trap so we don't double-start
trap - EXIT
echo "==> Building yourcert.pfx and restarting app"
"${SCRIPT_DIR}/bonusesapp-deploy-pfx.sh"

echo "==> LE cert dates:"
openssl x509 -in "/etc/letsencrypt/live/${CERT_NAME}/fullchain.pem" -noout -dates -subject

echo "==> Live HTTPS check (127.0.0.1:443):"
if echo | openssl s_client -connect 127.0.0.1:443 -servername "${CERT_NAME}" 2>/dev/null \
  | openssl x509 -noout -dates -subject; then
  echo "==> Done."
else
  echo "WARN: 443 not answering yet — try: docker compose -f remote_docker-compose.yml up -d" >&2
  echo "      then: docker compose -f remote_docker-compose.yml logs --tail=80 app" >&2
  exit 1
fi
