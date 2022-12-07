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
 * @interface NonFungibleIdsCollectionItem
 */
export interface NonFungibleIdsCollectionItem {
    /**
     * 
     * @type {string}
     * @memberof NonFungibleIdsCollectionItem
     */
    non_fungible_id: string;
}

/**
 * Check if a given object implements the NonFungibleIdsCollectionItem interface.
 */
export function instanceOfNonFungibleIdsCollectionItem(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "non_fungible_id" in value;

    return isInstance;
}

export function NonFungibleIdsCollectionItemFromJSON(json: any): NonFungibleIdsCollectionItem {
    return NonFungibleIdsCollectionItemFromJSONTyped(json, false);
}

export function NonFungibleIdsCollectionItemFromJSONTyped(json: any, ignoreDiscriminator: boolean): NonFungibleIdsCollectionItem {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'non_fungible_id': json['non_fungible_id'],
    };
}

export function NonFungibleIdsCollectionItemToJSON(value?: NonFungibleIdsCollectionItem | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'non_fungible_id': value.non_fungible_id,
    };
}

