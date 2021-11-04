# Generated Dependencies

Details on each generated dependency are below.

## RadixNodeApi.GeneratedApiClient

This is generated from the node's transactions api spec.

Create a temporary dummy directory, and an empty ./output directory inside it. Add the node's transaction api spec as `transactions.json`.

Install the [openapi-generator](https://github.com/OpenAPITools/openapi-generator) (eg with `brew install openapi-generator`), then in the dummy directory, run:


```sh
openapi-generator generate \
    -i ./transactions.json \
    -g csharp-netcore \
    -o ./output/ \
    --library httpclient \
    --additional-properties=packageName=RadixNodeApi.GeneratedApiClient,targetFramework=net5.0
    
cd ./output/

dotnet pack

# Now copy the generated .nupkg from the build directory
```

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
