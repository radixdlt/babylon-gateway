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
import type { NonFungibleIdType } from './NonFungibleIdType';
import {
    NonFungibleIdTypeFromJSON,
    NonFungibleIdTypeFromJSONTyped,
    NonFungibleIdTypeToJSON,
} from './NonFungibleIdType';

/**
 * 
 * @export
 * @interface NonFungibleDataResponseAllOf
 */
export interface NonFungibleDataResponseAllOf {
    /**
     * Bech32m-encoded human readable version of the resource (fungible, non-fungible) global address.
     * @type {string}
     * @memberof NonFungibleDataResponseAllOf
     */
    address: string;
    /**
     * 
     * @type {NonFungibleIdType}
     * @memberof NonFungibleDataResponseAllOf
     */
    non_fungible_id_type: NonFungibleIdType;
    /**
     * String-encoded non-fungible ID.
     * @type {string}
     * @memberof NonFungibleDataResponseAllOf
     */
    non_fungible_id: string;
    /**
     * Hex-encoded binary blob.
     * @type {string}
     * @memberof NonFungibleDataResponseAllOf
     */
    mutable_data_hex: string;
    /**
     * Hex-encoded binary blob.
     * @type {string}
     * @memberof NonFungibleDataResponseAllOf
     */
    immutable_data_hex: string;
}

/**
 * Check if a given object implements the NonFungibleDataResponseAllOf interface.
 */
export function instanceOfNonFungibleDataResponseAllOf(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "address" in value;
    isInstance = isInstance && "non_fungible_id_type" in value;
    isInstance = isInstance && "non_fungible_id" in value;
    isInstance = isInstance && "mutable_data_hex" in value;
    isInstance = isInstance && "immutable_data_hex" in value;

    return isInstance;
}

export function NonFungibleDataResponseAllOfFromJSON(json: any): NonFungibleDataResponseAllOf {
    return NonFungibleDataResponseAllOfFromJSONTyped(json, false);
}

export function NonFungibleDataResponseAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): NonFungibleDataResponseAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'address': json['address'],
        'non_fungible_id_type': NonFungibleIdTypeFromJSON(json['non_fungible_id_type']),
        'non_fungible_id': json['non_fungible_id'],
        'mutable_data_hex': json['mutable_data_hex'],
        'immutable_data_hex': json['immutable_data_hex'],
    };
}

export function NonFungibleDataResponseAllOfToJSON(value?: NonFungibleDataResponseAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'address': value.address,
        'non_fungible_id_type': NonFungibleIdTypeToJSON(value.non_fungible_id_type),
        'non_fungible_id': value.non_fungible_id,
        'mutable_data_hex': value.mutable_data_hex,
        'immutable_data_hex': value.immutable_data_hex,
    };
}

