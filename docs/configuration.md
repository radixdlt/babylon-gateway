# Configuration

## How to configure

The Network Gateway services can be configured in line with the [configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0) paradigm, with a few notable additions:

* Environment variables may be prefixed by `RADIX_NG_AGGREGATOR__` / `RADIX_NG_API__` to disambiguate from other environment variables. (But this is optional).
* There's support for defining an environment variable `CustomJsonConfigurationFilePath` which is configured with a path to a JSON file, in ASP.NET JSON format.
  Note that configuration configured there takes priority over environment variables.

## Configuration options

Check out the .json files in [/src/DataAggregator](../src/DataAggregator) and [/src/GatewayAPI](../src/GatewayAPI) respectively. 
