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
import type { EntityMetadataCollection } from './EntityMetadataCollection';
import {
    EntityMetadataCollectionFromJSON,
    EntityMetadataCollectionFromJSONTyped,
    EntityMetadataCollectionToJSON,
} from './EntityMetadataCollection';
import type { LedgerState } from './LedgerState';
import {
    LedgerStateFromJSON,
    LedgerStateFromJSONTyped,
    LedgerStateToJSON,
} from './LedgerState';

/**
 * 
 * @export
 * @interface EntityMetadataResponse
 */
export interface EntityMetadataResponse {
    /**
     * 
     * @type {LedgerState}
     * @memberof EntityMetadataResponse
     */
    ledger_state: LedgerState;
    /**
     * The Bech32m-encoded human readable version of the entity's global address.
     * @type {string}
     * @memberof EntityMetadataResponse
     */
    address: string;
    /**
     * 
     * @type {EntityMetadataCollection}
     * @memberof EntityMetadataResponse
     */
    metadata: EntityMetadataCollection;
}

/**
 * Check if a given object implements the EntityMetadataResponse interface.
 */
export function instanceOfEntityMetadataResponse(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "ledger_state" in value;
    isInstance = isInstance && "address" in value;
    isInstance = isInstance && "metadata" in value;

    return isInstance;
}

export function EntityMetadataResponseFromJSON(json: any): EntityMetadataResponse {
    return EntityMetadataResponseFromJSONTyped(json, false);
}

export function EntityMetadataResponseFromJSONTyped(json: any, ignoreDiscriminator: boolean): EntityMetadataResponse {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'ledger_state': LedgerStateFromJSON(json['ledger_state']),
        'address': json['address'],
        'metadata': EntityMetadataCollectionFromJSON(json['metadata']),
    };
}

export function EntityMetadataResponseToJSON(value?: EntityMetadataResponse | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'ledger_state': LedgerStateToJSON(value.ledger_state),
        'address': value.address,
        'metadata': EntityMetadataCollectionToJSON(value.metadata),
    };
}
