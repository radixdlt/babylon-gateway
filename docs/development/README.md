# Developer Set-up

* We use [dotnet 6](https://dotnet.microsoft.com/download/dotnet/6.0) - ensure `dotnet --version` returns at least 6.x
* Install docker
* Recommended IDE: Rider - but turn on "Show All Files" in the explorer

## Running in development

### If in Rider

* Run the `Postgres` task, then the `DataAggregator` task in parallel

### From the command line

Before running the system, boot-up postgres with:
```bash
docker-compose up
```

You can then run the codebase in a new terminal:

```bash
cd src/DataAggregator
dotnet run
```

## Before pushing

Run `dotnet format` to fix whitespace and other formatting issues.
