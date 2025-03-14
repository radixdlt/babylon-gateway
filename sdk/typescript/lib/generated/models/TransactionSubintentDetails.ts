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
/**
 * 
 * @export
 * @interface TransactionSubintentDetails
 */
export interface TransactionSubintentDetails {
    /**
     * Bech32m-encoded hash.
     * @type {string}
     * @memberof TransactionSubintentDetails
     */
    subintent_hash: string;
    /**
     * A text-representation of a subintent manifest.
This field will be present only for user transactions and when explicitly opted-in using the `manifest_instructions` flag.

     * @type {string}
     * @memberof TransactionSubintentDetails
     */
    manifest_instructions?: string;
    /**
     * The optional subintent message.
This type is defined in the Core API as `TransactionMessage`. See the Core API documentation for more details.

     * @type {object}
     * @memberof TransactionSubintentDetails
     */
    message?: object;
    /**
     * The subintent hash of each child of the subintent.
     * @type {Array<string>}
     * @memberof TransactionSubintentDetails
     */
    child_subintent_hashes?: Array<string>;
}

/**
 * Check if a given object implements the TransactionSubintentDetails interface.
 */
export function instanceOfTransactionSubintentDetails(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "subintent_hash" in value;

    return isInstance;
}

export function TransactionSubintentDetailsFromJSON(json: any): TransactionSubintentDetails {
    return TransactionSubintentDetailsFromJSONTyped(json, false);
}

export function TransactionSubintentDetailsFromJSONTyped(json: any, ignoreDiscriminator: boolean): TransactionSubintentDetails {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'subintent_hash': json['subintent_hash'],
        'manifest_instructions': !exists(json, 'manifest_instructions') ? undefined : json['manifest_instructions'],
        'message': !exists(json, 'message') ? undefined : json['message'],
        'child_subintent_hashes': !exists(json, 'child_subintent_hashes') ? undefined : json['child_subintent_hashes'],
    };
}

export function TransactionSubintentDetailsToJSON(value?: TransactionSubintentDetails | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'subintent_hash': value.subintent_hash,
        'manifest_instructions': value.manifest_instructions,
        'message': value.message,
        'child_subintent_hashes': value.child_subintent_hashes,
    };
}

