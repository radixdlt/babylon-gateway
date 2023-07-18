/* tslint:disable */
/* eslint-disable */
/**
 * Babylon Gateway API - RCnet V2
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers. For simple use cases, you can typically use the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs-babylon.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Integration and forward compatibility guarantees  We give no guarantees that other endpoints will not change before Babylon mainnet launch, although changes are expected to be minimal. 
 *
 * The version of the OpenAPI document: 0.4.0
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
 * @interface MetadataU8Value
 */
export interface MetadataU8Value {
    /**
     * 
     * @type {string}
     * @memberof MetadataU8Value
     */
    type: MetadataU8ValueTypeEnum;
    /**
     * 
     * @type {string}
     * @memberof MetadataU8Value
     */
    value: string;
}


/**
 * @export
 */
export const MetadataU8ValueTypeEnum = {
    U8: 'U8'
} as const;
export type MetadataU8ValueTypeEnum = typeof MetadataU8ValueTypeEnum[keyof typeof MetadataU8ValueTypeEnum];


/**
 * Check if a given object implements the MetadataU8Value interface.
 */
export function instanceOfMetadataU8Value(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "type" in value;
    isInstance = isInstance && "value" in value;

    return isInstance;
}

export function MetadataU8ValueFromJSON(json: any): MetadataU8Value {
    return MetadataU8ValueFromJSONTyped(json, false);
}

export function MetadataU8ValueFromJSONTyped(json: any, ignoreDiscriminator: boolean): MetadataU8Value {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'type': json['type'],
        'value': json['value'],
    };
}

export function MetadataU8ValueToJSON(value?: MetadataU8Value | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'type': value.type,
        'value': value.value,
    };
}
