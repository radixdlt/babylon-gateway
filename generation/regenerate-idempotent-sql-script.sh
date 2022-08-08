#!/bin/sh

set -e

SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
ROOT_DIR="$SCRIPT_DIR/../"
cd "$ROOT_DIR"

dotnet ef migrations script --idempotent --output src/RadixDlt.NetworkGateway/Migrations/IdempotentApplyMigrations.sql --project src/RadixDlt.NetworkGateway --startup-project samples/DatabaseMigrations --context MigrationsDbContext
