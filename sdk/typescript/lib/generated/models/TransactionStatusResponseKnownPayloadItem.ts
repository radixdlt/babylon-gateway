/* tslint:disable */
/* eslint-disable */
/**
 * Radix Gateway API - Babylon
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers, and for light queries from front-end dApps. For exchange/asset integrations, back-end dApp integrations, or simple use cases, you should consider using the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  All responses may have additional fields added at any release, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects.  When the Radix protocol is updated, new functionality may be added, and so discriminated unions returned by the API may need to be updated to have new variants added, corresponding to the updated data. Clients may need to update in advance to be able to handle these new variants when a protocol update comes out.  On the very rare occasions we need to make breaking changes to the API, these will be warned in advance with deprecation notices on previous versions. These deprecation notices will include a safe migration path. Deprecation notes or breaking changes will be flagged clearly in release notes for new versions of the Gateway.  The Gateway DB schema is not subject to any compatibility guarantees, and may be changed at any release. DB changes will be flagged in the release notes so clients doing custom DB integrations can prepare. 
 *
 * The version of the OpenAPI document: v1.5.0
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */

import { exists, mapValues } from '../runtime';
import type { TransactionPayloadGatewayHandlingStatus } from './TransactionPayloadGatewayHandlingStatus';
import {
    TransactionPayloadGatewayHandlingStatusFromJSON,
    TransactionPayloadGatewayHandlingStatusFromJSONTyped,
    TransactionPayloadGatewayHandlingStatusToJSON,
} from './TransactionPayloadGatewayHandlingStatus';
import type { TransactionPayloadStatus } from './TransactionPayloadStatus';
import {
    TransactionPayloadStatusFromJSON,
    TransactionPayloadStatusFromJSONTyped,
    TransactionPayloadStatusToJSON,
} from './TransactionPayloadStatus';
import type { TransactionStatus } from './TransactionStatus';
import {
    TransactionStatusFromJSON,
    TransactionStatusFromJSONTyped,
    TransactionStatusToJSON,
} from './TransactionStatus';

/**
 * 
 * @export
 * @interface TransactionStatusResponseKnownPayloadItem
 */
export interface TransactionStatusResponseKnownPayloadItem {
    /**
     * Bech32m-encoded hash.
     * @type {string}
     * @memberof TransactionStatusResponseKnownPayloadItem
     */
    payload_hash: string;
    /**
     * 
     * @type {TransactionStatus}
     * @memberof TransactionStatusResponseKnownPayloadItem
     */
    status: TransactionStatus;
    /**
     * 
     * @type {TransactionPayloadStatus}
     * @memberof TransactionStatusResponseKnownPayloadItem
     */
    payload_status?: TransactionPayloadStatus;
    /**
     * An additional description to clarify the payload status.

     * @type {string}
     * @memberof TransactionStatusResponseKnownPayloadItem
     */
    payload_status_description?: string;
    /**
     * The initial error message received for a rejection or failure during transaction execution.
This will typically be the useful error message, explaining the root cause of the issue.
Please note that presence of an error message doesn't imply that this payload
will definitely reject or fail. This could represent an error during a temporary
rejection (such as out of fees) which then gets resolved (e.g. by depositing money
to pay the fee), allowing the transaction to be committed.

     * @type {string}
     * @memberof TransactionStatusResponseKnownPayloadItem
     */
    error_message?: string | null;
    /**
     * The latest error message received for a rejection or failure during transaction execution,
this is only returned if it is different from the initial error message.
This is more current than the initial error message, but may be less useful, as it could
be a message regarding the expiry of the transaction at the end of its epoch validity window.
Please note that presence of an error message doesn't imply that this payload
will definitely reject or fail. This could represent an error during a temporary
rejection (such as out of fees) which then gets resolved (e.g. by depositing money
to pay the fee), allowing the transaction to be committed.

     * @type {string}
     * @memberof TransactionStatusResponseKnownPayloadItem
     */
    latest_error_message?: string | null;
    /**
     * 
     * @type {TransactionPayloadGatewayHandlingStatus}
     * @memberof TransactionStatusResponseKnownPayloadItem
     */
    handling_status?: TransactionPayloadGatewayHandlingStatus;
    /**
     * Additional reason for why the Gateway has its current handling status.

     * @type {string}
     * @memberof TransactionStatusResponseKnownPayloadItem
     */
    handling_status_reason?: string | null;
    /**
     * The most recent error message received when submitting this transaction to the network.
Please note that the presence of an error message doesn't imply that this transaction
payload will definitely reject or fail. This could be a transient error.

     * @type {string}
     * @memberof TransactionStatusResponseKnownPayloadItem
     */
    submission_error?: string | null;
}

/**
 * Check if a given object implements the TransactionStatusResponseKnownPayloadItem interface.
 */
export function instanceOfTransactionStatusResponseKnownPayloadItem(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "payload_hash" in value;
    isInstance = isInstance && "status" in value;

    return isInstance;
}

export function TransactionStatusResponseKnownPayloadItemFromJSON(json: any): TransactionStatusResponseKnownPayloadItem {
    return TransactionStatusResponseKnownPayloadItemFromJSONTyped(json, false);
}

export function TransactionStatusResponseKnownPayloadItemFromJSONTyped(json: any, ignoreDiscriminator: boolean): TransactionStatusResponseKnownPayloadItem {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'payload_hash': json['payload_hash'],
        'status': TransactionStatusFromJSON(json['status']),
        'payload_status': !exists(json, 'payload_status') ? undefined : TransactionPayloadStatusFromJSON(json['payload_status']),
        'payload_status_description': !exists(json, 'payload_status_description') ? undefined : json['payload_status_description'],
        'error_message': !exists(json, 'error_message') ? undefined : json['error_message'],
        'latest_error_message': !exists(json, 'latest_error_message') ? undefined : json['latest_error_message'],
        'handling_status': !exists(json, 'handling_status') ? undefined : TransactionPayloadGatewayHandlingStatusFromJSON(json['handling_status']),
        'handling_status_reason': !exists(json, 'handling_status_reason') ? undefined : json['handling_status_reason'],
        'submission_error': !exists(json, 'submission_error') ? undefined : json['submission_error'],
    };
}

export function TransactionStatusResponseKnownPayloadItemToJSON(value?: TransactionStatusResponseKnownPayloadItem | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'payload_hash': value.payload_hash,
        'status': TransactionStatusToJSON(value.status),
        'payload_status': TransactionPayloadStatusToJSON(value.payload_status),
        'payload_status_description': value.payload_status_description,
        'error_message': value.error_message,
        'latest_error_message': value.latest_error_message,
        'handling_status': TransactionPayloadGatewayHandlingStatusToJSON(value.handling_status),
        'handling_status_reason': value.handling_status_reason,
        'submission_error': value.submission_error,
    };
}

