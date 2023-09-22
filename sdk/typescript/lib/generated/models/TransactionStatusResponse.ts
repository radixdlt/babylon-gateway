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
import type { LedgerState } from './LedgerState';
import {
    LedgerStateFromJSON,
    LedgerStateFromJSONTyped,
    LedgerStateToJSON,
} from './LedgerState';
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
 * @interface TransactionStatusResponse
 */
export interface TransactionStatusResponse {
    /**
     * 
     * @type {LedgerState}
     * @memberof TransactionStatusResponse
     */
    ledger_state: LedgerState;
    /**
     * 
     * @type {TransactionStatus}
     * @memberof TransactionStatusResponse
     */
    status: TransactionStatus;
    /**
     * 
     * @type {TransactionIntentStatus}
     * @memberof TransactionStatusResponse
     */
    intent_status: TransactionIntentStatus;
    /**
     * An additional description to clarify the intent status.
     * @type {string}
     * @memberof TransactionStatusResponse
     */
    intent_status_description: string;
    /**
     * 
     * @type {Array<TransactionStatusResponseKnownPayloadItem>}
     * @memberof TransactionStatusResponse
     */
    known_payloads: Array<TransactionStatusResponseKnownPayloadItem>;
    /**
     * If the intent was committed, this gives the state version when this intent was committed.
     * @type {number}
     * @memberof TransactionStatusResponse
     */
    committed_state_version?: number | null;
    /**
     * The most relevant error message received, due to a rejection or commit as failure.
     * Please note that presence of an error message doesn't imply that the intent
     * will definitely reject or fail. This could represent a temporary error (such as out
     * of fees), or an error with a payload which doesn't end up being committed.
     * @type {string}
     * @memberof TransactionStatusResponse
     */
    error_message?: string | null;
}

/**
 * Check if a given object implements the TransactionStatusResponse interface.
 */
export function instanceOfTransactionStatusResponse(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "ledger_state" in value;
    isInstance = isInstance && "status" in value;
    isInstance = isInstance && "intent_status" in value;
    isInstance = isInstance && "intent_status_description" in value;
    isInstance = isInstance && "known_payloads" in value;

    return isInstance;
}

export function TransactionStatusResponseFromJSON(json: any): TransactionStatusResponse {
    return TransactionStatusResponseFromJSONTyped(json, false);
}

export function TransactionStatusResponseFromJSONTyped(json: any, ignoreDiscriminator: boolean): TransactionStatusResponse {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'ledger_state': LedgerStateFromJSON(json['ledger_state']),
        'status': TransactionStatusFromJSON(json['status']),
        'intent_status': TransactionIntentStatusFromJSON(json['intent_status']),
        'intent_status_description': json['intent_status_description'],
        'known_payloads': ((json['known_payloads'] as Array<any>).map(TransactionStatusResponseKnownPayloadItemFromJSON)),
        'committed_state_version': !exists(json, 'committed_state_version') ? undefined : json['committed_state_version'],
        'error_message': !exists(json, 'error_message') ? undefined : json['error_message'],
    };
}

export function TransactionStatusResponseToJSON(value?: TransactionStatusResponse | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'ledger_state': LedgerStateToJSON(value.ledger_state),
        'status': TransactionStatusToJSON(value.status),
        'intent_status': TransactionIntentStatusToJSON(value.intent_status),
        'intent_status_description': value.intent_status_description,
        'known_payloads': ((value.known_payloads as Array<any>).map(TransactionStatusResponseKnownPayloadItemToJSON)),
        'committed_state_version': value.committed_state_version,
        'error_message': value.error_message,
    };
}

