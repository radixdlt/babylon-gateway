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
 * Represents a decimal amount of a given resource.
 * @export
 * @interface TokenAmount
 */
export interface TokenAmount {
    /**
     * String-encoded decimal representing the amount of a related fungible resource.
     * @type {string}
     * @memberof TokenAmount
     */
    value: string;
    /**
     * Bech32m-encoded human readable version of the resource (fungible, non-fungible) global address or hex-encoded id.
     * @type {string}
     * @memberof TokenAmount
     */
    address?: string;
}

/**
 * Check if a given object implements the TokenAmount interface.
 */
export function instanceOfTokenAmount(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "value" in value;

    return isInstance;
}

export function TokenAmountFromJSON(json: any): TokenAmount {
    return TokenAmountFromJSONTyped(json, false);
}

export function TokenAmountFromJSONTyped(json: any, ignoreDiscriminator: boolean): TokenAmount {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'value': json['value'],
        'address': !exists(json, 'address') ? undefined : json['address'],
    };
}

export function TokenAmountToJSON(value?: TokenAmount | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'value': value.value,
        'address': value.address,
    };
}

