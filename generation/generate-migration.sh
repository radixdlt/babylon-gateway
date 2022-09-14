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

dotnet ef migrations add "$MigrationName" --project src/RadixDlt.NetworkGateway.PostgresIntegration --startup-project samples/DatabaseMigrations --context MigrationsDbContext

./generation/ensure-license-headers.sh
./generation/regenerate-idempotent-sql-script.sh

echo
echo "== Successfully generated the migration =="
echo
echo "You may need to manually go in and manually improve the migration code, as per https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing?tabs=dotnet-core-cli"
echo "After any manual changes, run ./generation/regenerate-idempotent-sql-script.sh"
