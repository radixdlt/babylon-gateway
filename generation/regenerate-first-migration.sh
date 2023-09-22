#!/bin/sh

set -e

SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
ROOT_DIR="$SCRIPT_DIR/../"
cd "$ROOT_DIR"

# This script should only be used whilst we're in development and completely wiping the database each boot-up

# Check it builds
dotnet build apps/DatabaseMigrations

# Remove existing migrations
find "src/RadixDlt.NetworkGateway.PostgresIntegration/Migrations" -name \*.cs -exec rm {} \;
dotnet ef migrations add InitialCreate --project src/RadixDlt.NetworkGateway.PostgresIntegration --startup-project apps/DatabaseMigrations --context MigrationsDbContext

./generation/ensure-license-headers.sh
./generation/regenerate-idempotent-sql-script.sh
