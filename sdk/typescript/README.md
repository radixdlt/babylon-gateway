# Gateway API SDK

This SDK is a thin wrapper around the [Babylon Gateway API](https://docs-babylon.radixdlt.com/main/apis/api-specification.html). It enables clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission. It is designed for use by wallets and explorers. For simple use cases, you can typically use the [Core API](https://github.com/radixdlt/babylon-node/tree/main/sdk/typescript) on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.

## Structure

There are four sub APIs available in the Gateway API:

- Status - For status and configuration details for the Gateway / Network.
- Transaction - For transaction construction, preview, submission, and monitoring the status of an individual transaction.
- Stream - For reading committed transactions.
- State - For reading the current or past ledger state of the network.

All APIs generated from OpenAPI specification are available after initialization through `innerClient` object on corresponding sub-api. See examples in [Low Level APIs section](#low-level-apis). On top of that, this package provides [high-level counterparts](#high-level-apis) for each sub api. 

## Versioning

All developers looking for an RCnet V1 compatible API should install versions which have `-rc.1.X` suffix. We'll be publishing internal versions with different suffixes which might not work well with publicly available network. All availble versions are published to [NPM repository](https://www.npmjs.com/package/@radixdlt/babylon-gateway-api-sdk?activeTab=versions)

## Initialization

Calling static `intialize` method from `GatewayApiClient` class will instantiate all low-level APIs together with their high-level equivalents and return them as a single object.

```typescript
import { GatewayApiClient } from '@radixdlt/babylon-gateway-api-sdk'

const gatewayApi = GatewayApiClient.initialize({
  basePath: 'https://rcnet.radixdlt.com',
})
const { status, transaction, stream, state } = gatewayApi
```

## Low Level APIs

Low level APIs are generated automatically based on OpenAPI spec. You can get a good sense of available methods by looking at [Swagger](https://rcnet.radixdlt.com/swagger/index.html). In order to access automatically generated methods you have two options:

### Using generated APIs through `innerClient`

```typescript
async function getTransactionStatus(transactionIntentHashHex: string) {
  let response = await gatewayApi.transaction.innerClient.transactionStatus({
    transactionStatusRequest: {
      intent_hash_hex: transactionIntentHashHex,
    },
  })
  return response.status
}
console.log(await getTransactionStatus('266cdfe0a28a761909d04761cdbfe33555ee5fdcf1db37fcf71c9a644b53e60b'))
```

### Using generated APIs manually

You can always opt-out of using aggregated gateway client and instantiate sub-apis manually

```typescript
import { Configuration, StateApi } from '@radixdlt/babylon-gateway-api-sdk'
const config = new Configuration({ basePath: CURRENT_NETWORK?.url })
const stateApi = new StateApi(config)
const response = await stateApi.nonFungibleData({
    stateNonFungibleDataRequest: {
      resource_address: address,
      non_fungible_ids: [id]
    }
  })
console.log(response.non_fungible_ids)
```

## High Level APIs

High Level APIs will grow over time as we start encountering repeating patterns when dealing with low level APIs. They are supposed to help with most commonly performed tasks and standardize ways of working with Gateway API and Typescript.

### State

- `getEntityDetailsVaultAggregated(entities: string | string[])` - detailed information about entities

### Status

- `getCurrent()` - Gateway API version and current ledger state
- `getNetworkConfiguration()` - network identifier, network name and well-known network addresses

### Transaction

- `getStatus(txID: string)` - transaction status for given transaction id (the intent hash)
- `getCommittedDetails(txID: string)` - transaction details for given transaction id (the intent hash)

## Fetch polyfill

Behind the scenes, this library uses the fetch API. If in an environment where `fetch` is not available, a polyfill must be used (see eg [node-fetch](https://www.npmjs.com/package/node-fetch)). Pass your own `fetch` implementation to [`initialize` parameter](https://github.com/radixdlt/babylon-gateway/blob/develop/sdk/typescript/lib/generated/runtime.ts#L20).

Starting from NodeJS 16 `fetch` is available as experimental API. You can e.g. check Gateway API status by using following snippet.

```typescript
const { GatewayApiClient } = require('@radixdlt/babylon-gateway-api-sdk')

const gateway = GatewayApiClient.initialize({
  basePath: 'https://rcnet.radixdlt.com',
})
gateway.status.getCurrent().then(console.log)
```

Save this as `index.js` and use by calling `node --experimental-fetch index.js`. This will enable experimental fetch and let code work without additional configuration. In NodeJS 18.x environment fetch API is enabled by default. Users of previous node versions will need to provide their own `fetch` implementation through configuration object.