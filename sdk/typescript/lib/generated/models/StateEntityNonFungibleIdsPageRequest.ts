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
 * @interface StateEntityNonFungibleIdsPageRequest
 */
export interface StateEntityNonFungibleIdsPageRequest {
    /**
     * 
     * @type {LedgerStateSelector}
     * @memberof StateEntityNonFungibleIdsPageRequest
     */
    at_ledger_state?: LedgerStateSelector | null;
    /**
     * This cursor allows forward pagination, by providing the cursor from the previous request.
     * @type {string}
     * @memberof StateEntityNonFungibleIdsPageRequest
     */
    cursor?: string | null;
    /**
     * The page size requested.
     * @type {number}
     * @memberof StateEntityNonFungibleIdsPageRequest
     */
    limit_per_page?: number | null;
    /**
     * Bech32m-encoded human readable version of the entity's global address or hex-encoded id.
     * @type {string}
     * @memberof StateEntityNonFungibleIdsPageRequest
     */
    address: string;
    /**
     * Bech32m-encoded human readable version of the entity's global address or hex-encoded id.
     * @type {string}
     * @memberof StateEntityNonFungibleIdsPageRequest
     */
    vault_address: string;
    /**
     * Bech32m-encoded human readable version of the resource (fungible, non-fungible) global address or hex-encoded id.
     * @type {string}
     * @memberof StateEntityNonFungibleIdsPageRequest
     */
    resource_address: string;
}

/**
 * Check if a given object implements the StateEntityNonFungibleIdsPageRequest interface.
 */
export function instanceOfStateEntityNonFungibleIdsPageRequest(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "address" in value;
    isInstance = isInstance && "vault_address" in value;
    isInstance = isInstance && "resource_address" in value;

    return isInstance;
}

export function StateEntityNonFungibleIdsPageRequestFromJSON(json: any): StateEntityNonFungibleIdsPageRequest {
    return StateEntityNonFungibleIdsPageRequestFromJSONTyped(json, false);
}

export function StateEntityNonFungibleIdsPageRequestFromJSONTyped(json: any, ignoreDiscriminator: boolean): StateEntityNonFungibleIdsPageRequest {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'at_ledger_state': !exists(json, 'at_ledger_state') ? undefined : LedgerStateSelectorFromJSON(json['at_ledger_state']),
        'cursor': !exists(json, 'cursor') ? undefined : json['cursor'],
        'limit_per_page': !exists(json, 'limit_per_page') ? undefined : json['limit_per_page'],
        'address': json['address'],
        'vault_address': json['vault_address'],
        'resource_address': json['resource_address'],
    };
}

export function StateEntityNonFungibleIdsPageRequestToJSON(value?: StateEntityNonFungibleIdsPageRequest | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'at_ledger_state': LedgerStateSelectorToJSON(value.at_ledger_state),
        'cursor': value.cursor,
        'limit_per_page': value.limit_per_page,
        'address': value.address,
        'vault_address': value.vault_address,
        'resource_address': value.resource_address,
    };
}

