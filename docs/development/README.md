# Developer Set-up

* We use [dotnet 6](https://dotnet.microsoft.com/download/dotnet/6.0) - ensure `dotnet --version` returns at least 6.x
* Install docker
* Recommended IDE: Rider - you will likely wish to turn on "Show All Files" in the explorer

## Running in development

You can change the configuration in `src/DataAggregator/appsettings.Development.json`.

By default, it is set up to point at http://localhost:3333, the port registered by the docker node when running the [radixdlt](https://github.com/radixdlt/radixdlt) core.

### If in Rider

Run the following tasks in parallel:

* `Postgres`
* `DataAggregator`

### From the command line

All the commands should be run from the repo root.

Run the following in separate terminals:

```bash
# Spin up Postgres first
docker-compose up
```

```bash
# Run the DataAggregator
dotnet run --project src/DataAggregator
```

## Testing

Run `dotnet test` from the repo root.

For more information, see the [Tests project](../../src/Tests).

## Reformatting

Run `dotnet format` to fix whitespace and other formatting issues across all files. Rider runs this as you save each file, so this likely won't be needed regularly.
