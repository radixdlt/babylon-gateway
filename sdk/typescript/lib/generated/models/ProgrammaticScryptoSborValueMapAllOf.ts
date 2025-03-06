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
import type { ProgrammaticScryptoSborValueKind } from './ProgrammaticScryptoSborValueKind';
import {
    ProgrammaticScryptoSborValueKindFromJSON,
    ProgrammaticScryptoSborValueKindFromJSONTyped,
    ProgrammaticScryptoSborValueKindToJSON,
} from './ProgrammaticScryptoSborValueKind';
import type { ProgrammaticScryptoSborValueMapEntry } from './ProgrammaticScryptoSborValueMapEntry';
import {
    ProgrammaticScryptoSborValueMapEntryFromJSON,
    ProgrammaticScryptoSborValueMapEntryFromJSONTyped,
    ProgrammaticScryptoSborValueMapEntryToJSON,
} from './ProgrammaticScryptoSborValueMapEntry';

/**
 * 
 * @export
 * @interface ProgrammaticScryptoSborValueMapAllOf
 */
export interface ProgrammaticScryptoSborValueMapAllOf {
    /**
     * 
     * @type {ProgrammaticScryptoSborValueKind}
     * @memberof ProgrammaticScryptoSborValueMapAllOf
     */
    key_kind: ProgrammaticScryptoSborValueKind;
    /**
     * 
     * @type {string}
     * @memberof ProgrammaticScryptoSborValueMapAllOf
     */
    key_type_name?: string;
    /**
     * 
     * @type {ProgrammaticScryptoSborValueKind}
     * @memberof ProgrammaticScryptoSborValueMapAllOf
     */
    value_kind: ProgrammaticScryptoSborValueKind;
    /**
     * 
     * @type {string}
     * @memberof ProgrammaticScryptoSborValueMapAllOf
     */
    value_type_name?: string;
    /**
     * 
     * @type {Array<ProgrammaticScryptoSborValueMapEntry>}
     * @memberof ProgrammaticScryptoSborValueMapAllOf
     */
    entries: Array<ProgrammaticScryptoSborValueMapEntry>;
    /**
     * 
     * @type {string}
     * @memberof ProgrammaticScryptoSborValueMapAllOf
     */
    kind?: ProgrammaticScryptoSborValueMapAllOfKindEnum;
}


/**
 * @export
 */
export const ProgrammaticScryptoSborValueMapAllOfKindEnum = {
    Map: 'Map'
} as const;
export type ProgrammaticScryptoSborValueMapAllOfKindEnum = typeof ProgrammaticScryptoSborValueMapAllOfKindEnum[keyof typeof ProgrammaticScryptoSborValueMapAllOfKindEnum];


/**
 * Check if a given object implements the ProgrammaticScryptoSborValueMapAllOf interface.
 */
export function instanceOfProgrammaticScryptoSborValueMapAllOf(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "key_kind" in value;
    isInstance = isInstance && "value_kind" in value;
    isInstance = isInstance && "entries" in value;

    return isInstance;
}

export function ProgrammaticScryptoSborValueMapAllOfFromJSON(json: any): ProgrammaticScryptoSborValueMapAllOf {
    return ProgrammaticScryptoSborValueMapAllOfFromJSONTyped(json, false);
}

export function ProgrammaticScryptoSborValueMapAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): ProgrammaticScryptoSborValueMapAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'key_kind': ProgrammaticScryptoSborValueKindFromJSON(json['key_kind']),
        'key_type_name': !exists(json, 'key_type_name') ? undefined : json['key_type_name'],
        'value_kind': ProgrammaticScryptoSborValueKindFromJSON(json['value_kind']),
        'value_type_name': !exists(json, 'value_type_name') ? undefined : json['value_type_name'],
        'entries': ((json['entries'] as Array<any>).map(ProgrammaticScryptoSborValueMapEntryFromJSON)),
        'kind': !exists(json, 'kind') ? undefined : json['kind'],
    };
}

export function ProgrammaticScryptoSborValueMapAllOfToJSON(value?: ProgrammaticScryptoSborValueMapAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'key_kind': ProgrammaticScryptoSborValueKindToJSON(value.key_kind),
        'key_type_name': value.key_type_name,
        'value_kind': ProgrammaticScryptoSborValueKindToJSON(value.value_kind),
        'value_type_name': value.value_type_name,
        'entries': ((value.entries as Array<any>).map(ProgrammaticScryptoSborValueMapEntryToJSON)),
        'kind': value.kind,
    };
}

