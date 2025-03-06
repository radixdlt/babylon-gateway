/* tslint:disable */
/* eslint-disable */
/**
 * Radix Gateway API - Babylon
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers, and for light queries from front-end dApps. For exchange/asset integrations, back-end dApp integrations, or simple use cases, you should consider using the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  All responses may have additional fields added at any release, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects.  When the Radix protocol is updated, new functionality may be added, and so discriminated unions returned by the API may need to be updated to have new variants added, corresponding to the updated data. Clients may need to update in advance to be able to handle these new variants when a protocol update comes out.  On the very rare occasions we need to make breaking changes to the API, these will be warned in advance with deprecation notices on previous versions. These deprecation notices will include a safe migration path. Deprecation notes or breaking changes will be flagged clearly in release notes for new versions of the Gateway.  The Gateway DB schema is not subject to any compatibility guarantees, and may be changed at any release. DB changes will be flagged in the release notes so clients doing custom DB integrations can prepare. 
 *
 * The version of the OpenAPI document: v1.10.1
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */

import { exists, mapValues } from '../runtime';
import type { ManifestClass } from './ManifestClass';
import {
    ManifestClassFromJSON,
    ManifestClassFromJSONTyped,
    ManifestClassToJSON,
} from './ManifestClass';
import type { TransactionBalanceChanges } from './TransactionBalanceChanges';
import {
    TransactionBalanceChangesFromJSON,
    TransactionBalanceChangesFromJSONTyped,
    TransactionBalanceChangesToJSON,
} from './TransactionBalanceChanges';
import type { TransactionReceipt } from './TransactionReceipt';
import {
    TransactionReceiptFromJSON,
    TransactionReceiptFromJSONTyped,
    TransactionReceiptToJSON,
} from './TransactionReceipt';
import type { TransactionStatus } from './TransactionStatus';
import {
    TransactionStatusFromJSON,
    TransactionStatusFromJSONTyped,
    TransactionStatusToJSON,
} from './TransactionStatus';
import type { TransactionSubintentDetails } from './TransactionSubintentDetails';
import {
    TransactionSubintentDetailsFromJSON,
    TransactionSubintentDetailsFromJSONTyped,
    TransactionSubintentDetailsToJSON,
} from './TransactionSubintentDetails';

/**
 * 
 * @export
 * @interface CommittedTransactionInfo
 */
export interface CommittedTransactionInfo {
    /**
     * 
     * @type {number}
     * @memberof CommittedTransactionInfo
     */
    state_version: number;
    /**
     * 
     * @type {number}
     * @memberof CommittedTransactionInfo
     */
    epoch: number;
    /**
     * 
     * @type {number}
     * @memberof CommittedTransactionInfo
     */
    round: number;
    /**
     * 
     * @type {string}
     * @memberof CommittedTransactionInfo
     */
    round_timestamp: string;
    /**
     * 
     * @type {TransactionStatus}
     * @memberof CommittedTransactionInfo
     */
    transaction_status: TransactionStatus;
    /**
     * Bech32m-encoded hash.
     * @type {string}
     * @memberof CommittedTransactionInfo
     */
    payload_hash?: string;
    /**
     * Bech32m-encoded hash.
     * @type {string}
     * @memberof CommittedTransactionInfo
     */
    intent_hash?: string;
    /**
     * String-encoded decimal representing the amount of a related fungible resource.
     * @type {string}
     * @memberof CommittedTransactionInfo
     */
    fee_paid?: string;
    /**
     * 
     * @type {Array<string>}
     * @memberof CommittedTransactionInfo
     */
    affected_global_entities?: Array<string>;
    /**
     * 
     * @type {Date}
     * @memberof CommittedTransactionInfo
     */
    confirmed_at?: Date | null;
    /**
     * 
     * @type {string}
     * @memberof CommittedTransactionInfo
     */
    error_message?: string | null;
    /**
     * Hex-encoded binary blob.
     * @type {string}
     * @memberof CommittedTransactionInfo
     */
    raw_hex?: string;
    /**
     * 
     * @type {TransactionReceipt}
     * @memberof CommittedTransactionInfo
     */
    receipt?: TransactionReceipt;
    /**
     * A text-representation of a transaction manifest.
This field will be present only for user transactions and when explicitly opted-in using the `manifest_instructions` flag.

     * @type {string}
     * @memberof CommittedTransactionInfo
     */
    manifest_instructions?: string;
    /**
     * A collection of zero or more manifest classes ordered from the most specific class to the least specific one.
This field will be present only for user transactions.
For user transactions with subintents only the root transaction intent is currently used to determine the manifest classes.

     * @type {Array<ManifestClass>}
     * @memberof CommittedTransactionInfo
     */
    manifest_classes?: Array<ManifestClass>;
    /**
     * The optional transaction message.
This type is defined in the Core API as `TransactionMessage`. See the Core API documentation for more details.

     * @type {object}
     * @memberof CommittedTransactionInfo
     */
    message?: object;
    /**
     * 
     * @type {TransactionBalanceChanges}
     * @memberof CommittedTransactionInfo
     */
    balance_changes?: TransactionBalanceChanges | null;
    /**
     * Subintent details.
Please note that it is returned regardless of whether the transaction was committed successfully or failed, and it can be returned in multiple transactions.

     * @type {Array<TransactionSubintentDetails>}
     * @memberof CommittedTransactionInfo
     */
    subintent_details?: Array<TransactionSubintentDetails>;
    /**
     * The child subintent hashes of the root transaction intent.
Please note that it is returned regardless of whether the transaction was committed successfully or failed, and it can be returned in multiple transactions.

     * @type {Array<string>}
     * @memberof CommittedTransactionInfo
     */
    child_subintent_hashes?: Array<string>;
}

/**
 * Check if a given object implements the CommittedTransactionInfo interface.
 */
export function instanceOfCommittedTransactionInfo(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "state_version" in value;
    isInstance = isInstance && "epoch" in value;
    isInstance = isInstance && "round" in value;
    isInstance = isInstance && "round_timestamp" in value;
    isInstance = isInstance && "transaction_status" in value;

    return isInstance;
}

export function CommittedTransactionInfoFromJSON(json: any): CommittedTransactionInfo {
    return CommittedTransactionInfoFromJSONTyped(json, false);
}

export function CommittedTransactionInfoFromJSONTyped(json: any, ignoreDiscriminator: boolean): CommittedTransactionInfo {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'state_version': json['state_version'],
        'epoch': json['epoch'],
        'round': json['round'],
        'round_timestamp': json['round_timestamp'],
        'transaction_status': TransactionStatusFromJSON(json['transaction_status']),
        'payload_hash': !exists(json, 'payload_hash') ? undefined : json['payload_hash'],
        'intent_hash': !exists(json, 'intent_hash') ? undefined : json['intent_hash'],
        'fee_paid': !exists(json, 'fee_paid') ? undefined : json['fee_paid'],
        'affected_global_entities': !exists(json, 'affected_global_entities') ? undefined : json['affected_global_entities'],
        'confirmed_at': !exists(json, 'confirmed_at') ? undefined : (json['confirmed_at'] === null ? null : new Date(json['confirmed_at'])),
        'error_message': !exists(json, 'error_message') ? undefined : json['error_message'],
        'raw_hex': !exists(json, 'raw_hex') ? undefined : json['raw_hex'],
        'receipt': !exists(json, 'receipt') ? undefined : TransactionReceiptFromJSON(json['receipt']),
        'manifest_instructions': !exists(json, 'manifest_instructions') ? undefined : json['manifest_instructions'],
        'manifest_classes': !exists(json, 'manifest_classes') ? undefined : ((json['manifest_classes'] as Array<any>).map(ManifestClassFromJSON)),
        'message': !exists(json, 'message') ? undefined : json['message'],
        'balance_changes': !exists(json, 'balance_changes') ? undefined : TransactionBalanceChangesFromJSON(json['balance_changes']),
        'subintent_details': !exists(json, 'subintent_details') ? undefined : ((json['subintent_details'] as Array<any>).map(TransactionSubintentDetailsFromJSON)),
        'child_subintent_hashes': !exists(json, 'child_subintent_hashes') ? undefined : json['child_subintent_hashes'],
    };
}

export function CommittedTransactionInfoToJSON(value?: CommittedTransactionInfo | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'state_version': value.state_version,
        'epoch': value.epoch,
        'round': value.round,
        'round_timestamp': value.round_timestamp,
        'transaction_status': TransactionStatusToJSON(value.transaction_status),
        'payload_hash': value.payload_hash,
        'intent_hash': value.intent_hash,
        'fee_paid': value.fee_paid,
        'affected_global_entities': value.affected_global_entities,
        'confirmed_at': value.confirmed_at === undefined ? undefined : (value.confirmed_at === null ? null : value.confirmed_at.toISOString()),
        'error_message': value.error_message,
        'raw_hex': value.raw_hex,
        'receipt': TransactionReceiptToJSON(value.receipt),
        'manifest_instructions': value.manifest_instructions,
        'manifest_classes': value.manifest_classes === undefined ? undefined : ((value.manifest_classes as Array<any>).map(ManifestClassToJSON)),
        'message': value.message,
        'balance_changes': TransactionBalanceChangesToJSON(value.balance_changes),
        'subintent_details': value.subintent_details === undefined ? undefined : ((value.subintent_details as Array<any>).map(TransactionSubintentDetailsToJSON)),
        'child_subintent_hashes': value.child_subintent_hashes,
    };
}

