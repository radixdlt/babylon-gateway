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
import type { EntityOverviewResponseEntityItemMetadata } from './EntityOverviewResponseEntityItemMetadata';
import {
    EntityOverviewResponseEntityItemMetadataFromJSON,
    EntityOverviewResponseEntityItemMetadataFromJSONTyped,
    EntityOverviewResponseEntityItemMetadataToJSON,
} from './EntityOverviewResponseEntityItemMetadata';

/**
 * 
 * @export
 * @interface EntityOverviewResponseEntityItem
 */
export interface EntityOverviewResponseEntityItem {
    /**
     * The Bech32m-encoded human readable version of the entity's global address
     * @type {string}
     * @memberof EntityOverviewResponseEntityItem
     */
    address: string;
    /**
     * 
     * @type {EntityOverviewResponseEntityItemMetadata}
     * @memberof EntityOverviewResponseEntityItem
     */
    metadata: EntityOverviewResponseEntityItemMetadata;
}

/**
 * Check if a given object implements the EntityOverviewResponseEntityItem interface.
 */
export function instanceOfEntityOverviewResponseEntityItem(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "address" in value;
    isInstance = isInstance && "metadata" in value;

    return isInstance;
}

export function EntityOverviewResponseEntityItemFromJSON(json: any): EntityOverviewResponseEntityItem {
    return EntityOverviewResponseEntityItemFromJSONTyped(json, false);
}

export function EntityOverviewResponseEntityItemFromJSONTyped(json: any, ignoreDiscriminator: boolean): EntityOverviewResponseEntityItem {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'address': json['address'],
        'metadata': EntityOverviewResponseEntityItemMetadataFromJSON(json['metadata']),
    };
}

export function EntityOverviewResponseEntityItemToJSON(value?: EntityOverviewResponseEntityItem | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'address': value.address,
        'metadata': EntityOverviewResponseEntityItemMetadataToJSON(value.metadata),
    };
}

