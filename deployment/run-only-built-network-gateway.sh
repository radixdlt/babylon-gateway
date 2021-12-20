#!/bin/sh
set -e
SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR"

./scripts/ensure-env-exists.sh

docker-compose --profile network-gateway-built build
docker-compose --profile network-gateway-built up
