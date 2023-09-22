/* tslint:disable */
/* eslint-disable */
/**
 * Babylon Gateway API - RCnet V3
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers. For simple use cases, you can typically use the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs-babylon.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  We give no guarantees that other endpoints will not change before Babylon mainnet launch, although changes are expected to be minimal. 
 *
 * The version of the OpenAPI document: 0.5.0
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */

import { exists, mapValues } from '../runtime';
import type { TransactionIntentStatus } from './TransactionIntentStatus';
import {
    TransactionIntentStatusFromJSON,
    TransactionIntentStatusFromJSONTyped,
    TransactionIntentStatusToJSON,
} from './TransactionIntentStatus';
import type { TransactionStatus } from './TransactionStatus';
import {
    TransactionStatusFromJSON,
    TransactionStatusFromJSONTyped,
    TransactionStatusToJSON,
} from './TransactionStatus';
import type { TransactionStatusResponseKnownPayloadItem } from './TransactionStatusResponseKnownPayloadItem';
import {
    TransactionStatusResponseKnownPayloadItemFromJSON,
    TransactionStatusResponseKnownPayloadItemFromJSONTyped,
    TransactionStatusResponseKnownPayloadItemToJSON,
} from './TransactionStatusResponseKnownPayloadItem';

/**
 * 
 * @export
 * @interface TransactionStatusResponseAllOf
 */
export interface TransactionStatusResponseAllOf {
    /**
     * 
     * @type {TransactionStatus}
     * @memberof TransactionStatusResponseAllOf
     */
    status: TransactionStatus;
    /**
     * 
     * @type {TransactionIntentStatus}
     * @memberof TransactionStatusResponseAllOf
     */
    intent_status: TransactionIntentStatus;
    /**
     * An additional description to clarify the intent status.
     * @type {string}
     * @memberof TransactionStatusResponseAllOf
     */
    intent_status_description: string;
    /**
     * 
     * @type {Array<TransactionStatusResponseKnownPayloadItem>}
     * @memberof TransactionStatusResponseAllOf
     */
    known_payloads: Array<TransactionStatusResponseKnownPayloadItem>;
    /**
     * If the intent was committed, this gives the state version when this intent was committed.
     * @type {number}
     * @memberof TransactionStatusResponseAllOf
     */
    committed_state_version?: number | null;
    /**
     * The most relevant error message received, due to a rejection or commit as failure.
     * Please note that presence of an error message doesn't imply that the intent
     * will definitely reject or fail. This could represent a temporary error (such as out
     * of fees), or an error with a payload which doesn't end up being committed.
     * @type {string}
     * @memberof TransactionStatusResponseAllOf
     */
    error_message?: string | null;
}

/**
 * Check if a given object implements the TransactionStatusResponseAllOf interface.
 */
export function instanceOfTransactionStatusResponseAllOf(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "status" in value;
    isInstance = isInstance && "intent_status" in value;
    isInstance = isInstance && "intent_status_description" in value;
    isInstance = isInstance && "known_payloads" in value;

    return isInstance;
}

export function TransactionStatusResponseAllOfFromJSON(json: any): TransactionStatusResponseAllOf {
    return TransactionStatusResponseAllOfFromJSONTyped(json, false);
}

export function TransactionStatusResponseAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): TransactionStatusResponseAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'status': TransactionStatusFromJSON(json['status']),
        'intent_status': TransactionIntentStatusFromJSON(json['intent_status']),
        'intent_status_description': json['intent_status_description'],
        'known_payloads': ((json['known_payloads'] as Array<any>).map(TransactionStatusResponseKnownPayloadItemFromJSON)),
        'committed_state_version': !exists(json, 'committed_state_version') ? undefined : json['committed_state_version'],
        'error_message': !exists(json, 'error_message') ? undefined : json['error_message'],
    };
}

export function TransactionStatusResponseAllOfToJSON(value?: TransactionStatusResponseAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'status': TransactionStatusToJSON(value.status),
        'intent_status': TransactionIntentStatusToJSON(value.intent_status),
        'intent_status_description': value.intent_status_description,
        'known_payloads': ((value.known_payloads as Array<any>).map(TransactionStatusResponseKnownPayloadItemToJSON)),
        'committed_state_version': value.committed_state_version,
        'error_message': value.error_message,
    };
}

