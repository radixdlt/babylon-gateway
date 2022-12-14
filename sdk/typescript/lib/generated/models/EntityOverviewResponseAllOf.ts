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
import type { EntityOverviewResponseEntityItem } from './EntityOverviewResponseEntityItem';
import {
    EntityOverviewResponseEntityItemFromJSON,
    EntityOverviewResponseEntityItemFromJSONTyped,
    EntityOverviewResponseEntityItemToJSON,
} from './EntityOverviewResponseEntityItem';

/**
 * 
 * @export
 * @interface EntityOverviewResponseAllOf
 */
export interface EntityOverviewResponseAllOf {
    /**
     * 
     * @type {Array<EntityOverviewResponseEntityItem>}
     * @memberof EntityOverviewResponseAllOf
     */
    entities: Array<EntityOverviewResponseEntityItem>;
}

/**
 * Check if a given object implements the EntityOverviewResponseAllOf interface.
 */
export function instanceOfEntityOverviewResponseAllOf(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "entities" in value;

    return isInstance;
}

export function EntityOverviewResponseAllOfFromJSON(json: any): EntityOverviewResponseAllOf {
    return EntityOverviewResponseAllOfFromJSONTyped(json, false);
}

export function EntityOverviewResponseAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): EntityOverviewResponseAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'entities': ((json['entities'] as Array<any>).map(EntityOverviewResponseEntityItemFromJSON)),
    };
}

export function EntityOverviewResponseAllOfToJSON(value?: EntityOverviewResponseAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'entities': ((value.entities as Array<any>).map(EntityOverviewResponseEntityItemToJSON)),
    };
}

