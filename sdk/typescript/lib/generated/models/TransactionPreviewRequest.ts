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
import type { PreviewFlags } from './PreviewFlags';
import {
    PreviewFlagsFromJSON,
    PreviewFlagsFromJSONTyped,
    PreviewFlagsToJSON,
} from './PreviewFlags';
import type { PublicKey } from './PublicKey';
import {
    PublicKeyFromJSON,
    PublicKeyFromJSONTyped,
    PublicKeyToJSON,
} from './PublicKey';
import type { TransactionPreviewOptIns } from './TransactionPreviewOptIns';
import {
    TransactionPreviewOptInsFromJSON,
    TransactionPreviewOptInsFromJSONTyped,
    TransactionPreviewOptInsToJSON,
} from './TransactionPreviewOptIns';

/**
 * 
 * @export
 * @interface TransactionPreviewRequest
 */
export interface TransactionPreviewRequest {
    /**
     * 
     * @type {TransactionPreviewOptIns}
     * @memberof TransactionPreviewRequest
     */
    opt_ins?: TransactionPreviewOptIns;
    /**
     * A text-representation of a transaction manifest
     * @type {string}
     * @memberof TransactionPreviewRequest
     */
    manifest: string;
    /**
     * An array of hex-encoded blob data, if referenced by the manifest.
     * @type {Array<string>}
     * @memberof TransactionPreviewRequest
     */
    blobs_hex?: Array<string>;
    /**
     * An integer between `0` and `10^10`, marking the epoch at which the transaction startsbeing valid.
If omitted, the current epoch will be used.

     * @type {number}
     * @memberof TransactionPreviewRequest
     */
    start_epoch_inclusive?: number;
    /**
     * An integer between `0` and `10^10`, marking the epoch at which the transaction is no
longer valid. If omitted, a maximum epoch (relative to the `start_epoch_inclusive`) will be used.

     * @type {number}
     * @memberof TransactionPreviewRequest
     */
    end_epoch_exclusive?: number;
    /**
     * 
     * @type {PublicKey}
     * @memberof TransactionPreviewRequest
     */
    notary_public_key?: PublicKey;
    /**
     * Whether the notary should be used as a signer (optional).
If not provided, this defaults to false.

     * @type {boolean}
     * @memberof TransactionPreviewRequest
     */
    notary_is_signatory?: boolean;
    /**
     * An integer between `0` and `65535`, giving the validator tip as a percentage amount.
A value of `1` corresponds to a 1% fee.
If not provided, this defaults to 0.

     * @type {number}
     * @memberof TransactionPreviewRequest
     */
    tip_percentage?: number;
    /**
     * An integer between `0` and `2^32 - 1`, chosen to allow a unique intent to be created
(to enable submitting an otherwise identical/duplicate intent).
If not provided, this defaults to 0.

     * @type {number}
     * @memberof TransactionPreviewRequest
     */
    nonce?: number;
    /**
     * A list of public keys to be used as transaction signers.
If not provided, this defaults to an empty array.

     * @type {Array<PublicKey>}
     * @memberof TransactionPreviewRequest
     */
    signer_public_keys?: Array<PublicKey>;
    /**
     * An optional transaction message. Only affects the costing.
This type is defined in the Core API as `TransactionMessage`. See the Core API documentation for more details.

     * @type {object}
     * @memberof TransactionPreviewRequest
     */
    message?: object;
    /**
     * 
     * @type {PreviewFlags}
     * @memberof TransactionPreviewRequest
     */
    flags?: PreviewFlags;
}

/**
 * Check if a given object implements the TransactionPreviewRequest interface.
 */
export function instanceOfTransactionPreviewRequest(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "manifest" in value;

    return isInstance;
}

export function TransactionPreviewRequestFromJSON(json: any): TransactionPreviewRequest {
    return TransactionPreviewRequestFromJSONTyped(json, false);
}

export function TransactionPreviewRequestFromJSONTyped(json: any, ignoreDiscriminator: boolean): TransactionPreviewRequest {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'opt_ins': !exists(json, 'opt_ins') ? undefined : TransactionPreviewOptInsFromJSON(json['opt_ins']),
        'manifest': json['manifest'],
        'blobs_hex': !exists(json, 'blobs_hex') ? undefined : json['blobs_hex'],
        'start_epoch_inclusive': !exists(json, 'start_epoch_inclusive') ? undefined : json['start_epoch_inclusive'],
        'end_epoch_exclusive': !exists(json, 'end_epoch_exclusive') ? undefined : json['end_epoch_exclusive'],
        'notary_public_key': !exists(json, 'notary_public_key') ? undefined : PublicKeyFromJSON(json['notary_public_key']),
        'notary_is_signatory': !exists(json, 'notary_is_signatory') ? undefined : json['notary_is_signatory'],
        'tip_percentage': !exists(json, 'tip_percentage') ? undefined : json['tip_percentage'],
        'nonce': !exists(json, 'nonce') ? undefined : json['nonce'],
        'signer_public_keys': !exists(json, 'signer_public_keys') ? undefined : ((json['signer_public_keys'] as Array<any>).map(PublicKeyFromJSON)),
        'message': !exists(json, 'message') ? undefined : json['message'],
        'flags': !exists(json, 'flags') ? undefined : PreviewFlagsFromJSON(json['flags']),
    };
}

export function TransactionPreviewRequestToJSON(value?: TransactionPreviewRequest | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'opt_ins': TransactionPreviewOptInsToJSON(value.opt_ins),
        'manifest': value.manifest,
        'blobs_hex': value.blobs_hex,
        'start_epoch_inclusive': value.start_epoch_inclusive,
        'end_epoch_exclusive': value.end_epoch_exclusive,
        'notary_public_key': PublicKeyToJSON(value.notary_public_key),
        'notary_is_signatory': value.notary_is_signatory,
        'tip_percentage': value.tip_percentage,
        'nonce': value.nonce,
        'signer_public_keys': value.signer_public_keys === undefined ? undefined : ((value.signer_public_keys as Array<any>).map(PublicKeyToJSON)),
        'message': value.message,
        'flags': PreviewFlagsToJSON(value.flags),
    };
}

