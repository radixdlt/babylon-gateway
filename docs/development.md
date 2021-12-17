# Developer Set-up

## Pre-requisites

The following are pre-requisites:
* We use [dotnet 6](https://dotnet.microsoft.com/download/dotnet/6.0) - ensure `dotnet --version` returns at least 6.x
* Install docker

Whilst any IDE supporting dotnet core can be used for development, we would recommend Jetbrains Rider.

## Configuration

In development, configuration comes from a few places, with items lower down the list taking priority for a given field. `[X]` is `DataAggregator` or `GatewayAPI`.

* `src/[X]/appsettings.json`
* `src/[X]/appsettings.Development.json`
* `src/[X]/appsettings.PersonalOverrides.json` (under .gitignore)
* Environment variables

By default, the configuration is set up to point to a full node's Core API running locally at http://localhost:3333. 

There is a guide in the radixdlt node repository regarding [run a full node against a development build](https://github.com/radixdlt/radixdlt/blob/develop/docs/development/run-configurations/connecting-to-a-live-network-in-docker.md). Instead of building a full node image, you can also use an image on docker hub at version `1.1.0+`. The latest at time of writing is [release 1.1.0-rc.1](https://github.com/radixdlt/radixdlt/releases/tag/1.1.0-rc.1) available as docker tag [radixdlt/radixdlt-core:1.1.0-rc.1](https://hub.docker.com/layers/radixdlt/radixdlt-core/1.1.0-rc.1/images/sha256-912939c55aa8abf6ecd0b7ae329daf8448a5b0d6137078000dc5a8797a86f045?context=explore). You should run this with similar configuration as per running a full node against a development build, in particular with `api.transactions.enable=true`.

### Custom development configuration

As referenced above, the `appsettings.PersonalOverrides.json` files can be used to (eg) override configuration locally without risking updating source control. The schema for this file is the same as the main `appsettings.json` and `appsettings.Development.json`. Some example use cases include:

* Changing the `NetworkName` so it matches the core nodes you're connected to
* Connecting to a non-local Core Node (typically by adjusting `DisableCoreApiHttpsCertificateChecks`, and the `CoreApiAddress`, `CoreApiAuthorizationHeader` fields under the first configured node in `CoreApiNodes`)

### Connecting to a non-local Core API

A syncing full node and the data aggregator are both quite resource intensive, so it can help to run at least the full node off of your local machine.

If at RDX Works, we have some Core APIs you can connect to off your local machine - talk to your team lead about getting access to these.

If not at RDX Works, please see [https://docs.radixdlt.com/](https://docs.radixdlt.com/) for a how-to on running a full node.

## Developing in Rider

### Recommended configuration

* In the explorer panel, in the dropdown, use "File System" mode instead of "Solution" mode.
* In the explorer panel, under the cod, select "Always Select Open File"

### Running the solution

Run following tasks:

* `Postgres` (this runs `docker-compose up`)

And then, depending on what you're working on, you can run one or both of these. Note that the `Data Aggregator` needs to have run successfully at least once to create the Database, and start the ledger, for the `Gateway API` to be able to connect.

* `Data Aggregator`
* `Gateway API`

You can use the `Wipe Database` task if you ever need to clear the database. (Say, because the DB ledger got corrupted; or you wish to change which network you're connected to)
## Developing using the command line

All the commands should be run from the repo root.

Run the following in separate terminals:

```bash
# Spin up Postgres and PG Adminfirst
docker-compose up
```

```bash
# Run the DataAggregator
dotnet run --project src/DataAggregator --launch-profile "Data Aggregator"
```

```bash
# Run the Gateway API
dotnet run --project src/GatewayAPI --launch-profile "Gateway API"
```

And, if you need to wipe the database, you should stop all of the above processes, and then either delete the `.postgresdata` folder, or run:

```bash
# Wipe the database
dotnet run --project src/GatewayAPI --launch-profile "Wipe Database"
```

## Looking at the database

To inspect the database, we have included a pgAdmin docker container.

After doing `docker-compose up` from the repo root, a pgAdmin container is also booted up.

* Location: http://localhost:5050/
* Local server is at "Servers / Local Radix Public Gateway"
* Password is `db_dev_password` (click "save" and you should only need to ass it once)

## Testing

Run `dotnet test` from the repo root. This assumes postgres is running. If you're running for CI, just run `docker-compose up db` to avoid spinning up pgAdmin as well.

For more information, see the [Tests project](../../src/Tests).

## Reformatting

Run `dotnet format` to fix whitespace and other formatting issues across all files. Rider runs this as you save each file, so this likely won't be needed regularly.

## Code Generation - Migrations, Open API specs etc

See the [generation](../generation) folder.
