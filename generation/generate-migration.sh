#!/bin/sh

set -e

SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
ROOT_DIR="$SCRIPT_DIR/../"
cd "$ROOT_DIR"

MigrationName="$1"

if [ -z "$MigrationName" ]; then
    echo "ERROR: Please provide a name for the migration in UpperCamelCase as a command line parameter to this script."
    exit 1;
fi

# Check it builds
dotnet build src/DataAggregator

dotnet ef migrations add "$MigrationName" --project src/DataAggregator

./generation/ensure-license-headers.sh
