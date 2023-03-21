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
 * @interface StateNonFungibleDetailsRequestAllOf
 */
export interface StateNonFungibleDetailsRequestAllOf {
    /**
     * Bech32m-encoded human readable version of the resource (fungible, non-fungible) global address or hex-encoded id.
     * @type {string}
     * @memberof StateNonFungibleDetailsRequestAllOf
     */
    resource_address: string;
    /**
     * 
     * @type {Array<string>}
     * @memberof StateNonFungibleDetailsRequestAllOf
     */
    non_fungible_ids: Array<string>;
}

/**
 * Check if a given object implements the StateNonFungibleDetailsRequestAllOf interface.
 */
export function instanceOfStateNonFungibleDetailsRequestAllOf(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "resource_address" in value;
    isInstance = isInstance && "non_fungible_ids" in value;

    return isInstance;
}

export function StateNonFungibleDetailsRequestAllOfFromJSON(json: any): StateNonFungibleDetailsRequestAllOf {
    return StateNonFungibleDetailsRequestAllOfFromJSONTyped(json, false);
}

export function StateNonFungibleDetailsRequestAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): StateNonFungibleDetailsRequestAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'resource_address': json['resource_address'],
        'non_fungible_ids': json['non_fungible_ids'],
    };
}

export function StateNonFungibleDetailsRequestAllOfToJSON(value?: StateNonFungibleDetailsRequestAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'resource_address': value.resource_address,
        'non_fungible_ids': value.non_fungible_ids,
    };
}

