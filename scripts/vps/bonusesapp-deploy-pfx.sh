#!/bin/bash
set -euo pipefail

if [[ -f /etc/default/bonusesapp-tls ]]; then
  set -a
  # shellcheck source=/dev/null
  . /etc/default/bonusesapp-tls
  set +a
fi

DOMAIN="${BONUSESAPP_TLS_DOMAIN:?Set BONUSESAPP_TLS_DOMAIN in /etc/default/bonusesapp-tls or the environment}"
LETSENCRYPT_LIVE="/etc/letsencrypt/live/${DOMAIN}"
PFX_DIR="${BONUSESAPP_PFX_DIR:-/etc/bonusesapp/ssl}"
PFX_PATH="${PFX_DIR}/yourcert.pfx"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# Official mcr.microsoft.com/dotnet/aspnet:9.0 runs as non-root APP_UID (default 1654).
PFX_UID="${BONUSESAPP_PFX_UID:-1654}"
PFX_GID="${BONUSESAPP_PFX_GID:-1654}"

if [[ ! -f "${LETSENCRYPT_LIVE}/privkey.pem" || ! -f "${LETSENCRYPT_LIVE}/fullchain.pem" ]]; then
  echo "Missing Lets Encrypt files under ${LETSENCRYPT_LIVE}"
  exit 1
fi

mkdir -p "${PFX_DIR}"
chmod 755 "${PFX_DIR}"
openssl pkcs12 -export \
  -out "${PFX_PATH}.new" \
  -inkey "${LETSENCRYPT_LIVE}/privkey.pem" \
  -in "${LETSENCRYPT_LIVE}/fullchain.pem" \
  -passout pass: \
  -name "${DOMAIN}"

mv "${PFX_PATH}.new" "${PFX_PATH}"
chown "${PFX_UID}:${PFX_GID}" "${PFX_PATH}"
chmod 640 "${PFX_PATH}"

"${SCRIPT_DIR}/bonusesapp-compose-cmd.sh" restart app
