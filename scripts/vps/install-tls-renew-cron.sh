#!/bin/bash
set -euo pipefail
# Install bi-monthly HTTPS renew cron, then run renew once.
#
# Usage (as root):
#   ~/bonuses-app/scripts/vps/install-tls-renew-cron.sh
#   SKIP_RUN_NOW=1 ~/bonuses-app/scripts/vps/install-tls-renew-cron.sh   # cron only
#   RUN_NOW_FORCE=0 ~/bonuses-app/scripts/vps/install-tls-renew-cron.sh # first run only if due

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CRON_TAG="# bonusesapp-tls-renew"
LOG_FILE="${BONUSESAPP_TLS_CRON_LOG:-/var/log/bonusesapp-tls-renew.log}"
SKIP_RUN_NOW="${SKIP_RUN_NOW:-0}"
RUN_NOW_FORCE="${RUN_NOW_FORCE:-1}"

if [[ "$(id -u)" -ne 0 ]]; then
  echo "Run as root." >&2
  exit 1
fi

if [[ -f /etc/default/bonusesapp-tls ]]; then
  set -a
  # shellcheck source=/dev/null
  . /etc/default/bonusesapp-tls
  set +a
fi

: "${BONUSESAPP_TLS_DOMAIN:?Set BONUSESAPP_TLS_DOMAIN in /etc/default/bonusesapp-tls}"

if grep -q $'\r' "${SCRIPT_DIR}/force-renew-https.sh" 2>/dev/null \
  || grep -q $'\r' "${SCRIPT_DIR}/install-tls-renew-cron.sh" 2>/dev/null; then
  echo "==> Stripping CRLF from scripts/vps/*.sh"
  sed -i 's/\r$//' "${SCRIPT_DIR}"/*.sh
fi
chmod +x "${SCRIPT_DIR}"/*.sh

RENEWAL_CONF="/etc/letsencrypt/renewal/${BONUSESAPP_TLS_DOMAIN}.conf"
if [[ -f "${RENEWAL_CONF}" ]]; then
  echo "==> Ensuring certbot hooks in ${RENEWAL_CONF}"
  ensure_hook() {
    local key="$1"
    local value="$2"
    if grep -qE "^[[:space:]]*${key}[[:space:]]*=" "${RENEWAL_CONF}"; then
      sed -i "s|^[[:space:]]*${key}[[:space:]]*=.*|${key} = ${value}|" "${RENEWAL_CONF}"
    else
      if grep -q '^\[renewalparams\]' "${RENEWAL_CONF}"; then
        sed -i "/^\[renewalparams\]/a ${key} = ${value}" "${RENEWAL_CONF}"
      else
        printf '\n[renewalparams]\n%s = %s\n' "${key}" "${value}" >> "${RENEWAL_CONF}"
      fi
    fi
  }
  ensure_hook pre_hook "${SCRIPT_DIR}/bonusesapp-certbot-pre.sh"
  ensure_hook post_hook "${SCRIPT_DIR}/bonusesapp-certbot-post.sh"
  ensure_hook deploy_hook "${SCRIPT_DIR}/bonusesapp-deploy-pfx.sh"
else
  echo "WARN: ${RENEWAL_CONF} not found — issue the cert first (issue-initial-cert-standalone.sh)." >&2
fi

# Cron: 04:00 on the 1st of every odd month (≈ every 2 months). FORCE_RENEW=0 = only when due.
CRON_CMD="FORCE_RENEW=0 ${SCRIPT_DIR}/force-renew-https.sh >> ${LOG_FILE} 2>&1"
CRON_LINE="0 4 1 Jan,Mar,May,Jul,Sep,Nov * ${CRON_CMD} ${CRON_TAG}"

echo "==> Installing crontab entry"
TMP="$(mktemp)"
crontab -l 2>/dev/null | grep -v 'bonusesapp-tls-renew' | grep -v 'renew-bonusesapp-tls-cron.sh' > "${TMP}" || true
printf '%s\n' "${CRON_LINE}" >> "${TMP}"
crontab "${TMP}"
rm -f "${TMP}"

echo "==> Current bonusesapp TLS cron:"
crontab -l | grep -E 'bonusesapp-tls|force-renew-https' || true
touch "${LOG_FILE}"
chmod 644 "${LOG_FILE}"

if [[ "${SKIP_RUN_NOW}" == "1" ]]; then
  echo "==> Cron installed. Skipped immediate renew (SKIP_RUN_NOW=1)."
  exit 0
fi

echo "==> Running renew now (FORCE_RENEW=${RUN_NOW_FORCE})"
FORCE_RENEW="${RUN_NOW_FORCE}" "${SCRIPT_DIR}/force-renew-https.sh"
echo "==> Done. Log: ${LOG_FILE}"
