/* tslint:disable */
/* eslint-disable */
/**
 * Radix Gateway API - Babylon
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers, and for light queries from front-end dApps. For exchange/asset integrations, back-end dApp integrations, or simple use cases, you should consider using the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  All responses may have additional fields added at any release, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects.  When the Radix protocol is updated, new functionality may be added, and so discriminated unions returned by the API may need to be updated to have new variants added, corresponding to the updated data. Clients may need to update in advance to be able to handle these new variants when a protocol update comes out.  On the very rare occasions we need to make breaking changes to the API, these will be warned in advance with deprecation notices on previous versions. These deprecation notices will include a safe migration path. Deprecation notes or breaking changes will be flagged clearly in release notes for new versions of the Gateway.  The Gateway DB schema is not subject to any compatibility guarantees, and may be changed at any release. DB changes will be flagged in the release notes so clients doing custom DB integrations can prepare. 
 *
 * The version of the OpenAPI document: v1.9.0
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */

import { exists, mapValues } from '../runtime';
import type { TransactionPreviewResponseLogsInner } from './TransactionPreviewResponseLogsInner';
import {
    TransactionPreviewResponseLogsInnerFromJSON,
    TransactionPreviewResponseLogsInnerFromJSONTyped,
    TransactionPreviewResponseLogsInnerToJSON,
} from './TransactionPreviewResponseLogsInner';

/**
 * 
 * @export
 * @interface TransactionPreviewResponse
 */
export interface TransactionPreviewResponse {
    /**
     * Hex-encoded binary blob.
     * @type {string}
     * @memberof TransactionPreviewResponse
     */
    encoded_receipt: string;
    /**
     * An optional field which is only provided if the `radix_engine_toolkit_receipt`
flag is set to true when requesting a transaction preview from the API.

This receipt is primarily intended for use with the toolkit and may contain information
that is already available in the receipt provided in the `receipt` field of this response.

A typical client of this API is not expected to use this receipt. The primary clients
this receipt is intended for is the Radix wallet or any client that needs to perform
execution summaries on their transactions.

     * @type {object}
     * @memberof TransactionPreviewResponse
     */
    radix_engine_toolkit_receipt?: object;
    /**
     * This type is defined in the Core API as `TransactionReceipt`. See the Core API documentation for more details.

     * @type {object}
     * @memberof TransactionPreviewResponse
     */
    receipt: object;
    /**
     * 
     * @type {Array<object>}
     * @memberof TransactionPreviewResponse
     */
    resource_changes: Array<object>;
    /**
     * 
     * @type {Array<TransactionPreviewResponseLogsInner>}
     * @memberof TransactionPreviewResponse
     */
    logs: Array<TransactionPreviewResponseLogsInner>;
}

/**
 * Check if a given object implements the TransactionPreviewResponse interface.
 */
export function instanceOfTransactionPreviewResponse(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "encoded_receipt" in value;
    isInstance = isInstance && "receipt" in value;
    isInstance = isInstance && "resource_changes" in value;
    isInstance = isInstance && "logs" in value;

    return isInstance;
}

export function TransactionPreviewResponseFromJSON(json: any): TransactionPreviewResponse {
    return TransactionPreviewResponseFromJSONTyped(json, false);
}

export function TransactionPreviewResponseFromJSONTyped(json: any, ignoreDiscriminator: boolean): TransactionPreviewResponse {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'encoded_receipt': json['encoded_receipt'],
        'radix_engine_toolkit_receipt': !exists(json, 'radix_engine_toolkit_receipt') ? undefined : json['radix_engine_toolkit_receipt'],
        'receipt': json['receipt'],
        'resource_changes': json['resource_changes'],
        'logs': ((json['logs'] as Array<any>).map(TransactionPreviewResponseLogsInnerFromJSON)),
    };
}

export function TransactionPreviewResponseToJSON(value?: TransactionPreviewResponse | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'encoded_receipt': value.encoded_receipt,
        'radix_engine_toolkit_receipt': value.radix_engine_toolkit_receipt,
        'receipt': value.receipt,
        'resource_changes': value.resource_changes,
        'logs': ((value.logs as Array<any>).map(TransactionPreviewResponseLogsInnerToJSON)),
    };
}

