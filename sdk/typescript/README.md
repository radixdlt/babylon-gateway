# Gateway API SDK

This SDK is a thin wrapper around the [Babylon Gateway API](https://docs-babylon.radixdlt.com/main/apis/api-specification.html). It enables clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission. It is designed for use by wallets and explorers. For simple use cases, you can typically use the [Core API](https://github.com/radixdlt/babylon-node/tree/main/sdk/typescript) on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.

## Structure

There are four sub APIs available in the Gateway API

- Status - For status and configuration details for the Gateway / Network.
- Transaction - For transaction construction, preview, submission, and monitoring the status of an individual transaction.
- Stream - For reading committed transactions.
- State - For reading the current or past ledger state of the network.

All APIs generated from OpenAPI specification are available after initialization either through `gatewayApi.lowLevel` or by calling `innerClient` on corresponding sub-api. See examples in [Low Level APIs section](#low-level-apis). On top of that, this package provides high-level counterparts for each sub api. High level APIs will grow over time as we start encountering repeating patterns when dealing with low level APIs. They are supposed to help with most commonly performed task and standardize ways of working with Gateway API and Typescript

## Initialization

Calling static `intialize` method from `GatewayApiClient` class will instantiate all low-level APIs together with their high-level equivalents and return them as a single object.

```typescript
import { GatewayApiClient } from '@radixdlt/babylon-gateway-api-sdk'

const gatewayApi = GatewayApiClient.initialize({
  basePath: 'https://kisharnet-gateway.radixdlt.com',
})
```

## Low Level APIs

Using generated APIs through `innerClient`

```typescript
async function getTransactionStatus(transactionIntentHashHex: string) {
  let response = await gatewayApi.transaction.innerClient.transactionStatus({
    transactionStatusRequest: {
      intent_hash_hex: transactionIntentHashHex,
    },
  })
  return response.status
}
```

Using generated APIs through `lowLevel` object

```typescript
async function getEntityNonFungibleIds(
  accountAddress: string,
  vaultAddress: string,
  nftAddress: string
) {
  return gatewayApi.lowLevel.state.entityNonFungibleIdsPage({
    stateEntityNonFungibleIdsPageRequest: {
      address: accountAddress,
      vault_address: vaultAddress,
      resource_address: nftAddress,
    },
  })
}
```

## State

### Get Entity Details Vault Aggregated

```typescript
const entityDetails = await gatewayApi.state.getVaultEntityDetails(
  'account_tdx_21_1p823h2sq7nsefkdharvvh5'
)
```

## Transaction

### Get Transaction Status

```typescript
const txStatus = await gatewayApi.transaction.getStatus(
  '266cdfe0a28a761909d04761cdbfe33555ee5fdcf1db37fcf71c9a644b53e60b'
)
```

## Fetch polyfill

Behind the scenes, this library uses the fetch API. If in an environment where `fetch` is not available, a polyfill must be used (see eg [node-fetch](https://www.npmjs.com/package/node-fetch)). Pass your own `fetch` implementation to [`initialize` parameter](https://github.com/radixdlt/babylon-gateway/blob/develop/sdk/typescript/lib/generated/runtime.ts#L20).
