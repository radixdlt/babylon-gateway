# Generated Dependencies

Details on each generated dependency are below.

## RadixNodeApi.GeneratedApiClient

This is generated from the node's transactions api spec.

Prerequisites:
* Install the [openapi-generator](https://github.com/OpenAPITools/openapi-generator) (eg with `brew install openapi-generator`)
* Installed the dotnet SDK version 5+

With all paths relative to the repo root, proceed as follows:

* Update `./generation/core-api-spec.yml` with the latest spec. You may need to change the spec version to "3.0.0" so that it works with code gen, until [openapi-generator supports 3.1.0](https://github.com/OpenAPITools/openapi-generator/issues/9083).
* Update the target client library version in `./generation/regenerate-core-api-client.sh`
* Run `./generation/regenerate-transactions-client.sh`

### Docs

* Docs on [general openapi-generator configuration are here](https://openapi-generator.tech/docs/configuration/).
* Docs on the [csharp-netcore specific configuration are here](https://openapi-generator.tech/docs/generators/csharp-netcore).
* The PR implementing the [httpclient implementation is here](https://github.com/OpenAPITools/openapi-generator/pull/8821).

### Transitive dependencies

For reference, at initial build time, the dependencies specified were:
  * "JsonSubTypes": "1.8.0",
  * "Newtonsoft.Json": "12.0.3",
  * "Polly": "7.2.1", (https://github.com/App-vNext/Polly - for retry logic)
  * "System.ComponentModel.Annotations": "5.0.0"
