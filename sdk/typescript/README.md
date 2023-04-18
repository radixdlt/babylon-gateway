# Gateway API SDK

This SDK is a thin wrapper around the [Babylon Gateway API](https://docs-babylon.radixdlt.com/main/apis/api-specification.html).

## Usage

Behind the scenes, this library uses the fetch API.

If in an environment where `fetch` is not available, a polyfill must be used (see eg [node-fetch](https://www.npmjs.com/package/node-fetch)).

```typescript
import "./node-fetch-polyfill" // Polyfill for fetch required if running in node-js
import { StateApi, StatusApi, TransactionApi, StreamApi } from "@radixdlt/babylon-gateway-api-sdk";

const transactionApi = new TransactionApi();

async function getTransactionStatus(transactionIntentHashHex: string) {
    let response = await transactionApi.transactionStatus({
        transactionStatusRequest: {
            intent_hash_hex: transactionIntentHashHex
        }
    });
    return response.status;
}
```
