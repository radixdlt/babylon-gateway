#!/bin/bash

# This script should only be used whilst we're in development and completely wiping the database each boot-up

# Check it builds
dotnet build src/DataAggregator

# Remove existing migrations
rm -rf src/DataAggregator/Migrations
dotnet ef migrations add InitialCreate --project src/DataAggregator
