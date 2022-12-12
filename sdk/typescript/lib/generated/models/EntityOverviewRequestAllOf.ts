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
 * @interface EntityOverviewRequestAllOf
 */
export interface EntityOverviewRequestAllOf {
    /**
     * 
     * @type {Array<string>}
     * @memberof EntityOverviewRequestAllOf
     */
    addresses: Array<string>;
}

/**
 * Check if a given object implements the EntityOverviewRequestAllOf interface.
 */
export function instanceOfEntityOverviewRequestAllOf(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "addresses" in value;

    return isInstance;
}

export function EntityOverviewRequestAllOfFromJSON(json: any): EntityOverviewRequestAllOf {
    return EntityOverviewRequestAllOfFromJSONTyped(json, false);
}

export function EntityOverviewRequestAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): EntityOverviewRequestAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'addresses': json['addresses'],
    };
}

export function EntityOverviewRequestAllOfToJSON(value?: EntityOverviewRequestAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'addresses': value.addresses,
    };
}
