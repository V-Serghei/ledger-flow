#!/usr/bin/env bash
set -Eeuo pipefail

action="${1:?use start ou stop}"
shift

project="$(pwd)"
pids_dir="$project/.local-pids"
logs_dir="$project/.local-logs"
mkdir -p "$pids_dir" "$logs_dir"

wait_ready() {
  local name="$1" url="$2" pid="$3" log="$4"
  local attempt
  for attempt in $(seq 1 60); do
    if ! kill -0 "$pid" 2>/dev/null; then
      echo "[$name] processo encerrou antes de ficar pronto. Veja $log" >&2
      return 1
    fi
    if curl --silent --output /dev/null --connect-timeout 1 "$url" 2>/dev/null; then
      echo "[$name] pronto em $url"
      return 0
    fi
    sleep 1
  done
  echo "[$name] não respondeu em $url após 60s. Veja $log" >&2
  return 1
}

start_service() {
  local spec="$1"
  local name url path profile
  IFS='|' read -r name url path profile <<< "$spec"

  local pid_file="$pids_dir/$name.pid"
  local log_file="$logs_dir/$name.log"

  if [[ -f "$pid_file" ]]; then
    local existing_pid
    existing_pid="$(<"$pid_file")"
    if kill -0 "$existing_pid" 2>/dev/null; then
      echo "[$name] já está executando (PID $existing_pid) em $url"
      return 0
    fi
    rm -f "$pid_file"
  fi

  local args=(dotnet run --project "$path")
  [[ -n "${profile:-}" ]] && args+=(--launch-profile "$profile")
  args+=(--urls "$url")

  echo "[$name] iniciando..."
  nohup env ASPNETCORE_ENVIRONMENT=Development "${args[@]}" \
    >"$log_file" 2>&1 < /dev/null &
  local pid=$!
  echo "$pid" >"$pid_file"
  wait_ready "$name" "$url" "$pid" "$log_file"
}

stop_service_pids() {
  local file pid
  shopt -s nullglob
  for file in "$pids_dir"/*.pid; do
    pid="$(<"$file")"
    if kill -0 "$pid" 2>/dev/null; then
      kill "$pid" 2>/dev/null || true
      local attempt
      for attempt in $(seq 1 10); do
        kill -0 "$pid" 2>/dev/null || break
        sleep 1
      done
      if kill -0 "$pid" 2>/dev/null; then
        kill -9 "$pid" 2>/dev/null || true
      fi
      echo "Processo $pid encerrado ($(basename "$file" .pid))."
    fi
    rm -f "$file"
  done
  shopt -u nullglob
}

if [[ "$action" == start ]]; then
  if [[ "${1:-}" == --compose ]]; then
    shift
    command -v docker >/dev/null || { echo "Docker não encontrado." >&2; exit 1; }
    echo "Subindo dependências (docker compose)..."
    docker compose up ${COMPOSE_BUILD:+--build} -d
  fi

  if (($#)); then
    command -v dotnet >/dev/null || { echo ".NET SDK não encontrado." >&2; exit 1; }
  fi

  local_failed=0
  while (($#)); do
    if ! start_service "$1"; then
      local_failed=1
    fi
    shift
  done

  if ((local_failed)); then
    exit 1
  fi
elif [[ "$action" == stop ]]; then
  stop_service_pids
  if [[ "${1:-}" == --compose ]]; then
    command -v docker >/dev/null || { echo "Docker não encontrado." >&2; exit 1; }
    echo "Parando dependências (docker compose)..."
    docker compose down
  fi
else
  echo "Ação inválida: $action" >&2
  exit 2
fi
