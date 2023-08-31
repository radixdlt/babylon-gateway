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
/**
 * 
 * @export
 * @interface MetadataU64ArrayValueAllOf
 */
export interface MetadataU64ArrayValueAllOf {
    /**
     * 
     * @type {Array<string>}
     * @memberof MetadataU64ArrayValueAllOf
     */
    values: Array<string>;
    /**
     * 
     * @type {string}
     * @memberof MetadataU64ArrayValueAllOf
     */
    type?: MetadataU64ArrayValueAllOfTypeEnum;
}


/**
 * @export
 */
export const MetadataU64ArrayValueAllOfTypeEnum = {
    U64Array: 'U64Array'
} as const;
export type MetadataU64ArrayValueAllOfTypeEnum = typeof MetadataU64ArrayValueAllOfTypeEnum[keyof typeof MetadataU64ArrayValueAllOfTypeEnum];


/**
 * Check if a given object implements the MetadataU64ArrayValueAllOf interface.
 */
export function instanceOfMetadataU64ArrayValueAllOf(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "values" in value;

    return isInstance;
}

export function MetadataU64ArrayValueAllOfFromJSON(json: any): MetadataU64ArrayValueAllOf {
    return MetadataU64ArrayValueAllOfFromJSONTyped(json, false);
}

export function MetadataU64ArrayValueAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): MetadataU64ArrayValueAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'values': json['values'],
        'type': !exists(json, 'type') ? undefined : json['type'],
    };
}

export function MetadataU64ArrayValueAllOfToJSON(value?: MetadataU64ArrayValueAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'values': value.values,
        'type': value.type,
    };
}

