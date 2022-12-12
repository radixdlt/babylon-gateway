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
import type { LedgerStateSelector } from './LedgerStateSelector';
import {
    LedgerStateSelectorFromJSON,
    LedgerStateSelectorFromJSONTyped,
    LedgerStateSelectorToJSON,
} from './LedgerStateSelector';

/**
 * 
 * @export
 * @interface EntityNonFungiblesRequest
 */
export interface EntityNonFungiblesRequest {
    /**
     * 
     * @type {LedgerStateSelector}
     * @memberof EntityNonFungiblesRequest
     */
    at_ledger_state?: LedgerStateSelector | null;
    /**
     * The Bech32m-encoded human readable version of the entity's global address.
     * @type {string}
     * @memberof EntityNonFungiblesRequest
     */
    address: string;
    /**
     * This cursor allows forward pagination, by providing the cursor from the previous request.
     * @type {string}
     * @memberof EntityNonFungiblesRequest
     */
    cursor?: string | null;
    /**
     * The page size requested.
     * @type {number}
     * @memberof EntityNonFungiblesRequest
     */
    limit?: number | null;
}

/**
 * Check if a given object implements the EntityNonFungiblesRequest interface.
 */
export function instanceOfEntityNonFungiblesRequest(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "address" in value;

    return isInstance;
}

export function EntityNonFungiblesRequestFromJSON(json: any): EntityNonFungiblesRequest {
    return EntityNonFungiblesRequestFromJSONTyped(json, false);
}

export function EntityNonFungiblesRequestFromJSONTyped(json: any, ignoreDiscriminator: boolean): EntityNonFungiblesRequest {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'at_ledger_state': !exists(json, 'at_ledger_state') ? undefined : LedgerStateSelectorFromJSON(json['at_ledger_state']),
        'address': json['address'],
        'cursor': !exists(json, 'cursor') ? undefined : json['cursor'],
        'limit': !exists(json, 'limit') ? undefined : json['limit'],
    };
}

export function EntityNonFungiblesRequestToJSON(value?: EntityNonFungiblesRequest | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'at_ledger_state': LedgerStateSelectorToJSON(value.at_ledger_state),
        'address': value.address,
        'cursor': value.cursor,
        'limit': value.limit,
    };
}
