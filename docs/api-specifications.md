# API Specifications

The documentation site covers details about the various [Network APIs](https://docs.radixdlt.com/docs/network-apis) exposed in the Radix stack.

## Gateway API

The current [Gateway API schema](https://radix-babylon-gateway-api.redoc.ly/) of the foundation Gateway is available on Redocly.

The Gateway API schema source-of-truth lives in this repository as an Open API definition in [gateway-api-schema.yaml](../src/RadixDlt.NetworkGateway.GatewayApi/gateway-api-schema.yaml).

## Model and Client Generation

The Gateway Service uses the `openapi-generator` to generate models and clients against the Gateway API spec and the Core API spec. More information can be found in the [generation folder README](../generation).
