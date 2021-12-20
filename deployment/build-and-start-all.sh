#!/bin/sh
set -e
SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR"

if [[ ! -f "$SCRIPT_DIR/container-volumes/fullnode/keystore.ks" ]]; then
    echo "Node key store doesn't exist - creating it..."
    echo
    ./generate-key.sh
    echo
    echo "Key store created successfully"
    echo
fi

docker-compose --profile network-gateway build
docker-compose --profile fullnode --profile network-gateway up
