/* tslint:disable */
/* eslint-disable */
/**
 * Radix Babylon Gateway API
 * See https://docs.radixdlt.com/main/apis/introduction.html 
 *
 * The version of the OpenAPI document: 2.0.0
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
 * @interface NonFungibleResourcesCollectionItem
 */
export interface NonFungibleResourcesCollectionItem {
    /**
     * The Bech32m-encoded human readable version of the resource (fungible, non-fungible) global address.
     * @type {string}
     * @memberof NonFungibleResourcesCollectionItem
     */
    address: string;
    /**
     * 
     * @type {number}
     * @memberof NonFungibleResourcesCollectionItem
     */
    amount: number;
}

/**
 * Check if a given object implements the NonFungibleResourcesCollectionItem interface.
 */
export function instanceOfNonFungibleResourcesCollectionItem(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "address" in value;
    isInstance = isInstance && "amount" in value;

    return isInstance;
}

export function NonFungibleResourcesCollectionItemFromJSON(json: any): NonFungibleResourcesCollectionItem {
    return NonFungibleResourcesCollectionItemFromJSONTyped(json, false);
}

export function NonFungibleResourcesCollectionItemFromJSONTyped(json: any, ignoreDiscriminator: boolean): NonFungibleResourcesCollectionItem {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'address': json['address'],
        'amount': json['amount'],
    };
}

export function NonFungibleResourcesCollectionItemToJSON(value?: NonFungibleResourcesCollectionItem | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'address': value.address,
        'amount': value.amount,
    };
}
