# Gateway API SDK

This SDK is a thin wrapper around the Babylon Gateway API, detailed in [the Open API schema](https://redocly.github.io/redoc/?url=https://raw.githubusercontent.com/radixdlt/babylon-gateway/main/src/RadixDlt.NetworkGateway.GatewayApi/gateway-api-spec.yaml).

## Usage

Behind the scenes, this library uses the fetch API.

If in an environment where this is not available, a polyfill must be used.

```typescript
import "./node-fetch-polyfill" // Polyfill for fetch required if running in node-js
import { TransactionApi } from "@radixdlt/babylon-gateway-api-sdk";

const transactionApi = new TransactionApi();

async function getTransactionStatus(transactionIntentHashHex: string) {
    let response = await transactionApi.transactionStatus({
        transaction_identifier: {
            origin: TransactionLookupOrigin.Intent,
            value_hex: transactionIntentHashHex
        }
    });
    return response.transaction.transaction_status;
}
```
