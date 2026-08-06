#!/usr/bin/env bash
set -Eeuo pipefail
script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$script_dir/.."
COMPOSE_BUILD=1 "$script_dir/project-local.sh" start --compose;
echo 'Banking API: http://localhost:5000 | Swagger: http://localhost:5000/swagger'
