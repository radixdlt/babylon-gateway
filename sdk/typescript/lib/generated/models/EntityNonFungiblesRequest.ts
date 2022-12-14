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
     * Bech32m-encoded human readable version of the entity's global address.
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

