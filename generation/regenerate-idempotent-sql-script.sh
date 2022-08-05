#!/bin/sh

set -e

SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
ROOT_DIR="$SCRIPT_DIR/../"
cd "$ROOT_DIR"

dotnet ef migrations script --idempotent --project samples/DatabaseMigrations --output samples/DatabaseMigrations/Migrations/IdempotentApplyMigrations.sql --context MigrationsDbContext
