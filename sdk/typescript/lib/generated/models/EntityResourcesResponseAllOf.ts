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
import type { FungibleResourcesCollection } from './FungibleResourcesCollection';
import {
    FungibleResourcesCollectionFromJSON,
    FungibleResourcesCollectionFromJSONTyped,
    FungibleResourcesCollectionToJSON,
} from './FungibleResourcesCollection';
import type { NonFungibleResourcesCollection } from './NonFungibleResourcesCollection';
import {
    NonFungibleResourcesCollectionFromJSON,
    NonFungibleResourcesCollectionFromJSONTyped,
    NonFungibleResourcesCollectionToJSON,
} from './NonFungibleResourcesCollection';

/**
 * 
 * @export
 * @interface EntityResourcesResponseAllOf
 */
export interface EntityResourcesResponseAllOf {
    /**
     * Bech32m-encoded human readable version of the entity's global address.
     * @type {string}
     * @memberof EntityResourcesResponseAllOf
     */
    address: string;
    /**
     * 
     * @type {FungibleResourcesCollection}
     * @memberof EntityResourcesResponseAllOf
     */
    fungible_resources: FungibleResourcesCollection;
    /**
     * 
     * @type {NonFungibleResourcesCollection}
     * @memberof EntityResourcesResponseAllOf
     */
    non_fungible_resources: NonFungibleResourcesCollection;
}

/**
 * Check if a given object implements the EntityResourcesResponseAllOf interface.
 */
export function instanceOfEntityResourcesResponseAllOf(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "address" in value;
    isInstance = isInstance && "fungible_resources" in value;
    isInstance = isInstance && "non_fungible_resources" in value;

    return isInstance;
}

export function EntityResourcesResponseAllOfFromJSON(json: any): EntityResourcesResponseAllOf {
    return EntityResourcesResponseAllOfFromJSONTyped(json, false);
}

export function EntityResourcesResponseAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): EntityResourcesResponseAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'address': json['address'],
        'fungible_resources': FungibleResourcesCollectionFromJSON(json['fungible_resources']),
        'non_fungible_resources': NonFungibleResourcesCollectionFromJSON(json['non_fungible_resources']),
    };
}

export function EntityResourcesResponseAllOfToJSON(value?: EntityResourcesResponseAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'address': value.address,
        'fungible_resources': FungibleResourcesCollectionToJSON(value.fungible_resources),
        'non_fungible_resources': NonFungibleResourcesCollectionToJSON(value.non_fungible_resources),
    };
}

