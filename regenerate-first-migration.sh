#!/bin/sh

# This script should only be used whilst we're in development and completely wiping the database each boot-up

# Check it builds
dotnet build src/DataAggregator

# Remove existing migrations
find "src/DataAggregator/Migrations" -name \*.cs -exec rm {} \;
dotnet ef migrations add InitialCreate --project src/DataAggregator
