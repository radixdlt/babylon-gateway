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
import type { FungibleResourcesCollectionItem } from './FungibleResourcesCollectionItem';
import {
    FungibleResourcesCollectionItemFromJSON,
    FungibleResourcesCollectionItemFromJSONTyped,
    FungibleResourcesCollectionItemToJSON,
} from './FungibleResourcesCollectionItem';

/**
 * 
 * @export
 * @interface FungibleResourcesCollectionAllOf
 */
export interface FungibleResourcesCollectionAllOf {
    /**
     * 
     * @type {Array<FungibleResourcesCollectionItem>}
     * @memberof FungibleResourcesCollectionAllOf
     */
    items: Array<FungibleResourcesCollectionItem>;
}

/**
 * Check if a given object implements the FungibleResourcesCollectionAllOf interface.
 */
export function instanceOfFungibleResourcesCollectionAllOf(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "items" in value;

    return isInstance;
}

export function FungibleResourcesCollectionAllOfFromJSON(json: any): FungibleResourcesCollectionAllOf {
    return FungibleResourcesCollectionAllOfFromJSONTyped(json, false);
}

export function FungibleResourcesCollectionAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): FungibleResourcesCollectionAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'items': ((json['items'] as Array<any>).map(FungibleResourcesCollectionItemFromJSON)),
    };
}

export function FungibleResourcesCollectionAllOfToJSON(value?: FungibleResourcesCollectionAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'items': ((value.items as Array<any>).map(FungibleResourcesCollectionItemToJSON)),
    };
}

