/* tslint:disable */
/* eslint-disable */
/**
 * Radix Babylon Gateway API
 * This API is designed to enable clients to efficiently query information on the RadixDLT ledger, and allow clients to build and submit transactions to the network. It is designed for use by wallets and explorers.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/main/apis/gateway-api.html) to learn more about different usage scenarios.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network. 
 *
 * The version of the OpenAPI document: 0.1.0
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
 * @interface EntityMetadataItemValueAllOf
 */
export interface EntityMetadataItemValueAllOf {
    /**
     * 
     * @type {string}
     * @memberof EntityMetadataItemValueAllOf
     */
    as_string?: string;
    /**
     * 
     * @type {Array<string>}
     * @memberof EntityMetadataItemValueAllOf
     */
    as_string_collection?: Array<string>;
}

/**
 * Check if a given object implements the EntityMetadataItemValueAllOf interface.
 */
export function instanceOfEntityMetadataItemValueAllOf(value: object): boolean {
    let isInstance = true;

    return isInstance;
}

export function EntityMetadataItemValueAllOfFromJSON(json: any): EntityMetadataItemValueAllOf {
    return EntityMetadataItemValueAllOfFromJSONTyped(json, false);
}

export function EntityMetadataItemValueAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): EntityMetadataItemValueAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'as_string': !exists(json, 'as_string') ? undefined : json['as_string'],
        'as_string_collection': !exists(json, 'as_string_collection') ? undefined : json['as_string_collection'],
    };
}

export function EntityMetadataItemValueAllOfToJSON(value?: EntityMetadataItemValueAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'as_string': value.as_string,
        'as_string_collection': value.as_string_collection,
    };
}

