#!/bin/bash
set -euo pipefail
# Creates a long-lived self-signed PFX for OpenIddict signing/encryption (not the same as HTTPS yourcert.pfx).
# Password must match OIDC:Certificates:Password in appsettings.json (override via BONUSESAPP_OIDC_PFX_PASSWORD).

if [[ -f /etc/default/bonusesapp-tls ]]; then
  set -a
  # shellcheck source=/dev/null
  . /etc/default/bonusesapp-tls
  set +a
fi

PFX_DIR="${BONUSESAPP_PFX_DIR:-/etc/bonusesapp/ssl}"
PFX_UID="${BONUSESAPP_PFX_UID:-1654}"
PFX_GID="${BONUSESAPP_PFX_GID:-1654}"
: "${BONUSESAPP_OIDC_PFX_PASSWORD:?Set BONUSESAPP_OIDC_PFX_PASSWORD to match appsettings OIDC Certificates Password}"

TMP="$(mktemp -d)"
trap 'rm -rf "${TMP}"' EXIT

openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes \
  -keyout "${TMP}/key.pem" \
  -out "${TMP}/cert.pem" \
  -subj "/CN=BonusesApp-OpenIddict"

mkdir -p "${PFX_DIR}"
chmod 755 "${PFX_DIR}"

openssl pkcs12 -export \
  -out "${PFX_DIR}/authcert.pfx.new" \
  -inkey "${TMP}/key.pem" \
  -in "${TMP}/cert.pem" \
  -passout "env:BONUSESAPP_OIDC_PFX_PASSWORD"

mv "${PFX_DIR}/authcert.pfx.new" "${PFX_DIR}/authcert.pfx"
chown "${PFX_UID}:${PFX_GID}" "${PFX_DIR}/authcert.pfx"
chmod 640 "${PFX_DIR}/authcert.pfx"

echo "Wrote ${PFX_DIR}/authcert.pfx — restart the app container if it is already running."
