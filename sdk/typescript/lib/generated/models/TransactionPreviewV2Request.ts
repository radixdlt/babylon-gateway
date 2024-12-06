/* tslint:disable */
/* eslint-disable */
/**
 * Radix Gateway API - Babylon
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers, and for light queries from front-end dApps. For exchange/asset integrations, back-end dApp integrations, or simple use cases, you should consider using the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  All responses may have additional fields added at any release, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects.  When the Radix protocol is updated, new functionality may be added, and so discriminated unions returned by the API may need to be updated to have new variants added, corresponding to the updated data. Clients may need to update in advance to be able to handle these new variants when a protocol update comes out.  On the very rare occasions we need to make breaking changes to the API, these will be warned in advance with deprecation notices on previous versions. These deprecation notices will include a safe migration path. Deprecation notes or breaking changes will be flagged clearly in release notes for new versions of the Gateway.  The Gateway DB schema is not subject to any compatibility guarantees, and may be changed at any release. DB changes will be flagged in the release notes so clients doing custom DB integrations can prepare. 
 *
 * The version of the OpenAPI document: v1.9.2
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */

import { exists, mapValues } from '../runtime';
import type { PreviewFlags } from './PreviewFlags';
import {
    PreviewFlagsFromJSON,
    PreviewFlagsFromJSONTyped,
    PreviewFlagsToJSON,
} from './PreviewFlags';
import type { PreviewTransaction } from './PreviewTransaction';
import {
    PreviewTransactionFromJSON,
    PreviewTransactionFromJSONTyped,
    PreviewTransactionToJSON,
} from './PreviewTransaction';
import type { TransactionPreviewV2OptIns } from './TransactionPreviewV2OptIns';
import {
    TransactionPreviewV2OptInsFromJSON,
    TransactionPreviewV2OptInsFromJSONTyped,
    TransactionPreviewV2OptInsToJSON,
} from './TransactionPreviewV2OptIns';

/**
 * 
 * @export
 * @interface TransactionPreviewV2Request
 */
export interface TransactionPreviewV2Request {
    /**
     * 
     * @type {PreviewTransaction}
     * @memberof TransactionPreviewV2Request
     */
    preview_transaction: PreviewTransaction;
    /**
     * 
     * @type {PreviewFlags}
     * @memberof TransactionPreviewV2Request
     */
    flags?: PreviewFlags;
    /**
     * 
     * @type {TransactionPreviewV2OptIns}
     * @memberof TransactionPreviewV2Request
     */
    opt_ins?: TransactionPreviewV2OptIns;
}

/**
 * Check if a given object implements the TransactionPreviewV2Request interface.
 */
export function instanceOfTransactionPreviewV2Request(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "preview_transaction" in value;

    return isInstance;
}

export function TransactionPreviewV2RequestFromJSON(json: any): TransactionPreviewV2Request {
    return TransactionPreviewV2RequestFromJSONTyped(json, false);
}

export function TransactionPreviewV2RequestFromJSONTyped(json: any, ignoreDiscriminator: boolean): TransactionPreviewV2Request {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'preview_transaction': PreviewTransactionFromJSON(json['preview_transaction']),
        'flags': !exists(json, 'flags') ? undefined : PreviewFlagsFromJSON(json['flags']),
        'opt_ins': !exists(json, 'opt_ins') ? undefined : TransactionPreviewV2OptInsFromJSON(json['opt_ins']),
    };
}

export function TransactionPreviewV2RequestToJSON(value?: TransactionPreviewV2Request | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'preview_transaction': PreviewTransactionToJSON(value.preview_transaction),
        'flags': PreviewFlagsToJSON(value.flags),
        'opt_ins': TransactionPreviewV2OptInsToJSON(value.opt_ins),
    };
}

