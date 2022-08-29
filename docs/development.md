# Developer Set-up

## Pre-requisites

The following are pre-requisites:
* We use [dotnet 6](https://dotnet.microsoft.com/download/dotnet/6.0) - ensure `dotnet --version` returns at least 6.x
* Install docker

Whilst any IDE supporting dotnet core can be used for development, we would recommend Jetbrains Rider.

## Configuration

In development, configuration comes from a few places, with items lower down the list taking priority for a given field. `[X]` is `DataAggregator` or `GatewayApi`.

* `samples/[X]/appsettings.json`
* `samples/[X]/appsettings.Development.json`
* `samples/[X]/appsettings.PersonalOverrides.json` (under .gitignore)
* Environment variables

By default, the configuration is set up to point to a full node's Core API running locally at http://localhost:3333, but you may wish to use non-local Core API, to have a synced-up system to read from, and to avoid hammering your computer too much! (see sections below).

If you wish to easily spin up a local Core API, follow the instructions in [the deployment folder](../deployment), for set-up and running only a full node.

### Custom development configuration

As referenced above, the `appsettings.PersonalOverrides.json` files can be used to (eg) override configuration locally without risking updating source control. The schema for this file is the same as the main `appsettings.json` and `appsettings.Development.json`. Some example use cases include:

* Changing the `NetworkName` so it matches the core nodes you're connected to
* Connecting to a non-local Core Node (typically by adjusting `DisableCoreApiHttpsCertificateChecks`, and the `CoreApiAddress`, `CoreApiAuthorizationHeader` fields under the first configured node in `CoreApiNodes`)

### Connecting to a non-local Core API

A syncing full node and the data aggregator are both quite resource intensive, so it can help to run at least the full node off of your local machine.

If at RDX Works, we have some Core APIs you can connect to off your local machine - talk to your team lead about getting access to these.

If not at RDX Works, please see [https://docs.radixdlt.com/](https://docs.radixdlt.com/) for a how-to on running a full node.
You'll need to run a version 1.1.0 or higher in order for the node to have the Core API that the Network Gateway requires.

## Developing in Rider

### Recommended configuration

* In the explorer panel, in the dropdown, use "File System" mode instead of "Solution" mode.
* In the explorer panel, under the cod, select "Always Select Open File"

### Running the solution

Run following tasks:

* `PostgreSQL & PgAdmin` (this runs `docker-compose up`)

And then, depending on what you're working on, you can run one or both of these. Note that the `Data Aggregator` needs to have run successfully at least once to create the Database, and start the ledger, for the `Gateway API` to be able to connect.

* `Data Aggregator`
* `Gateway API`

You can use the `Wipe Database` task if you ever need to clear the database. (Say, because the DB ledger got corrupted; or you wish to change which network you're connected to)

## Developing using the command line

All the commands should be run from the repo root.

Run the following in separate terminals:

```bash
# Spin up PostgreSQL and PgAdmin first
docker-compose up
```

```bash
# Run the DataAggregator
dotnet run --project samples/DataAggregator --launch-profile "Data Aggregator"
```

```bash
# Run the Gateway API
dotnet run --project samples/GatewayAPI --launch-profile "Gateway Api"
```

And, if you need to wipe the database, you should stop all of the above processes, and then either delete the `.postgresdata` folder, or run:

```bash
# Wipe the database
dotnet run --project samples/DatabaseMigrations --launch-profile "Wipe Database"
```

## Looking at the database

To inspect the database, we have included a pgAdmin docker container.

After doing `docker-compose up` from the repo root, a pgAdmin container is also booted up.

* Location: http://localhost:5050/
* Local server is at "Servers / Local Radix Public Gateway"
* Password is `db_dev_password` (click "save" and you should only need to ass it once)

## Testing

It is assumed all commands are executed from the root folder `/babylon-gateway/`
, and postgres is running.

If you're running for CI, just run `docker-compose up db` to avoid spinning up pgAdmin as well.


There are three different ways of running tests.

- All tests: `dotnet test` 

- Unit tests only: `dotnet test --filter RadixDlt.NetworkGateway.UnitTests`

- Integration tests only: `dotnet test --filter RadixDlt.NetworkGateway.IntegrationTests`


For more information, see the [Tests project](../../src/Tests).

## Reformatting

Run `dotnet format` to fix whitespace and other formatting issues across all files. Rider runs this as you save each file, so this likely won't be needed regularly.

## Code Generation - Migrations, Open API specs etc

See the [generation](../generation) folder.
