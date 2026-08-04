#!/usr/bin/env bash
set -euo pipefail

hosted_services_pid=""
web_pid=""

stop_processes() {
    if [[ -n "${web_pid}" ]]; then
        kill -TERM "${web_pid}" 2>/dev/null || true
    fi

    if [[ -n "${hosted_services_pid}" ]]; then
        kill -TERM "${hosted_services_pid}" 2>/dev/null || true
    fi

    wait 2>/dev/null || true
}

trap stop_processes EXIT INT TERM

if [[ "${CCODER_GENERATE_LOCAL_CERTIFICATE:-false}" == "true" ]]; then
    certificate_path="${CCODER_CERTIFICATE_PATH:-/https/ccoder-localhost.crt}"
    certificate_key_path="${CCODER_CERTIFICATE_KEY_PATH:-/https/ccoder-localhost.key}"

    if [[ ! -f "${certificate_path}" || ! -f "${certificate_key_path}" ]]; then
        mkdir -p "$(dirname "${certificate_path}")"
        openssl req \
            -x509 \
            -nodes \
            -newkey rsa:2048 \
            -sha256 \
            -days 825 \
            -subj "/CN=localhost" \
            -addext "subjectAltName=DNS:localhost,DNS:*.localhost" \
            -addext "extendedKeyUsage=serverAuth" \
            -keyout "${certificate_key_path}" \
            -out "${certificate_path}"
        chmod 600 "${certificate_key_path}"
        chmod 644 "${certificate_path}"
    fi
fi

dotnet /app/HostedServices/HostedServices.dll \
    --urls "${CCODER_HOSTED_SERVICES_URLS}" &
hosted_services_pid=$!

for attempt in {1..60}; do
    if curl --fail --silent http://localhost:5100/Health >/dev/null; then
        break
    fi

    if ! kill -0 "${hosted_services_pid}" 2>/dev/null; then
        echo "HostedServices exited before becoming healthy." >&2
        exit 1
    fi

    if [[ "${attempt}" -eq 60 ]]; then
        echo "HostedServices did not become healthy." >&2
        exit 1
    fi

    sleep 1
done

dotnet /app/Web/Web.dll --urls "${CCODER_WEB_URLS}" &
web_pid=$!

set +e
wait -n "${web_pid}" "${hosted_services_pid}"
exit_code=$?
set -e

echo "A cCoder application process exited with code ${exit_code}." >&2
exit "${exit_code}"
