Gateway API depends on the node’s Core API.

When the Core API schema changes, we need to update the generated Core API client to consume new features. Follow these steps:

1. Copy spec file from [node repository](https://github.com/radixdlt/babylon-node/blob/develop/core-rust/core-api-server/core-api-schema.yaml).
   - Rename it to `core-api-spec-copy.yaml`
   - Place it in `babylon-gateway\src\RadixDlt.CoreApiSdk\core-api-spec-copy.yaml`
2. Execute [this script](regenerate-core-api.py) to regenerate Core API Client.
```
cd babylon-gateway\generation\
python regenerate-core-api.py
```
3. Ensure the project builds successfully after upgrading the Core API client:
```
dotnet build
```
