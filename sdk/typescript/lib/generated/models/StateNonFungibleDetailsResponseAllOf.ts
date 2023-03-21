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
import type { StateNonFungibleDetailsResponseItem } from './StateNonFungibleDetailsResponseItem';
import {
    StateNonFungibleDetailsResponseItemFromJSON,
    StateNonFungibleDetailsResponseItemFromJSONTyped,
    StateNonFungibleDetailsResponseItemToJSON,
} from './StateNonFungibleDetailsResponseItem';

/**
 * 
 * @export
 * @interface StateNonFungibleDetailsResponseAllOf
 */
export interface StateNonFungibleDetailsResponseAllOf {
    /**
     * Bech32m-encoded human readable version of the resource (fungible, non-fungible) global address or hex-encoded id.
     * @type {string}
     * @memberof StateNonFungibleDetailsResponseAllOf
     */
    resource_address: string;
    /**
     * 
     * @type {NonFungibleIdType}
     * @memberof StateNonFungibleDetailsResponseAllOf
     */
    non_fungible_id_type: NonFungibleIdType;
    /**
     * 
     * @type {Array<StateNonFungibleDetailsResponseItem>}
     * @memberof StateNonFungibleDetailsResponseAllOf
     */
    non_fungible_ids: Array<StateNonFungibleDetailsResponseItem>;
}

/**
 * Check if a given object implements the StateNonFungibleDetailsResponseAllOf interface.
 */
export function instanceOfStateNonFungibleDetailsResponseAllOf(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "resource_address" in value;
    isInstance = isInstance && "non_fungible_id_type" in value;
    isInstance = isInstance && "non_fungible_ids" in value;

    return isInstance;
}

export function StateNonFungibleDetailsResponseAllOfFromJSON(json: any): StateNonFungibleDetailsResponseAllOf {
    return StateNonFungibleDetailsResponseAllOfFromJSONTyped(json, false);
}

export function StateNonFungibleDetailsResponseAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): StateNonFungibleDetailsResponseAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'resource_address': json['resource_address'],
        'non_fungible_id_type': NonFungibleIdTypeFromJSON(json['non_fungible_id_type']),
        'non_fungible_ids': ((json['non_fungible_ids'] as Array<any>).map(StateNonFungibleDetailsResponseItemFromJSON)),
    };
}

export function StateNonFungibleDetailsResponseAllOfToJSON(value?: StateNonFungibleDetailsResponseAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'resource_address': value.resource_address,
        'non_fungible_id_type': NonFungibleIdTypeToJSON(value.non_fungible_id_type),
        'non_fungible_ids': ((value.non_fungible_ids as Array<any>).map(StateNonFungibleDetailsResponseItemToJSON)),
    };
}
