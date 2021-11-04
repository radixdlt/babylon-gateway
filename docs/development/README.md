# Developer Set-up

* We use [dotnet 6](https://dotnet.microsoft.com/download/dotnet/6.0) - ensure `dotnet --version` returns at least 6.x
* Install docker
* Recommended IDE: Rider - but turn on "Show All Files" in the explorer

## Running in development

You can change the configuration in `src/DataAggregator/appsettings.Development.json`. By default, it is set up to point at the localhost from the first docker node of [radixdlt](https://github.com/radixdlt/radixdlt).

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

We use xUnit: https://xunit.net/docs/comparisons

Run `dotnet test` from the repo root.

## Reformatting

Run `dotnet format` to fix whitespace and other formatting issues across all files. Most IDEs should run this on any file you edit, so this might be needed.
