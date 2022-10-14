# Configuration

## How to configure

The Network Gateway services can be configured in line with the [configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0) paradigm, with a few notable additions:

* Environment variables must be prefixed by application specific prefix (`GatewayApi__` or `DataAggregator__`) to disambiguate from other environment variables. 
* There's support for defining an environment variable `CustomJsonConfigurationFilePath` which is configured with a path to a JSON file, in ASP.NET JSON format.
  Note that configuration configured there takes priority over environment variables.

## Configuration options

Check out the .json files in [/apps/DataAggregator](../apps/DataAggregator) and [/apps/GatewayApi](../apps/GatewayApi) respectively. 
