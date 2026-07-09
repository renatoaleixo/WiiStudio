#!/usr/bin/env bash
set -euo pipefail

policy_id="com.wiistudio.wwt.manage"
policy_target="/usr/share/polkit-1/actions/com.wiistudio.wwt.policy"
script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
template_path="${script_dir}/wiistudio-wwt.pkexec.policy.template"

wwt_path="${1:-}"
if [[ -z "${wwt_path}" ]]; then
  if ! wwt_path="$(command -v wwt 2>/dev/null)"; then
    echo "Nao encontrei o wwt no PATH. Informe o caminho: $0 /caminho/para/wwt" >&2
    exit 2
  fi
fi

if [[ ! -x "${wwt_path}" ]]; then
  echo "O caminho informado nao e executavel: ${wwt_path}" >&2
  exit 2
fi

if [[ "${wwt_path}" != /* ]]; then
  wwt_dir="$(cd -- "$(dirname -- "${wwt_path}")" && pwd)"
  wwt_path="${wwt_dir}/$(basename -- "${wwt_path}")"
fi

if [[ ! -f "${template_path}" ]]; then
  echo "Template nao encontrado: ${template_path}" >&2
  exit 2
fi

if [[ "${EUID}" -ne 0 ]]; then
  if ! command -v pkexec >/dev/null 2>&1; then
    echo "pkexec nao encontrado. Execute com sudo: sudo $0 ${wwt_path}" >&2
    exit 2
  fi

  echo "Sera solicitada a senha uma vez para instalar a politica Polkit do WiiStudio."
  exec pkexec /usr/bin/env bash "${script_dir}/install-wiistudio-polkit-policy.sh" "${wwt_path}"
fi

tmp_file="$(mktemp)"
trap 'rm -f "${tmp_file}"' EXIT

escaped_wwt_path="${wwt_path//&/&amp;}"
escaped_wwt_path="${escaped_wwt_path//</&lt;}"
escaped_wwt_path="${escaped_wwt_path//>/&gt;}"

sed "s#__WWT_PATH__#${escaped_wwt_path}#g" "${template_path}" > "${tmp_file}"
install -o root -g root -m 0644 "${tmp_file}" "${policy_target}"

if command -v pkaction >/dev/null 2>&1; then
  if ! pkaction --action-id "${policy_id}" >/dev/null 2>&1; then
    echo "A politica foi instalada, mas o Polkit ainda nao listou a action ${policy_id}." >&2
    echo "Tente reiniciar a sessao ou o servico polkit." >&2
    exit 1
  fi
fi

echo "Politica instalada: ${policy_target}"
echo "WWT autorizado pela action ${policy_id}: ${wwt_path}"
echo "As proximas chamadas do WiiStudio via pkexec devem reutilizar a autorizacao enquanto o Polkit mantiver o cache ativo."
