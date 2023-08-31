/* tslint:disable */
/* eslint-disable */
/**
 * Babylon Gateway API - RCnet V3
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers. For simple use cases, you can typically use the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs-babylon.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  We give no guarantees that other endpoints will not change before Babylon mainnet launch, although changes are expected to be minimal. 
 *
 * The version of the OpenAPI document: 0.5.0
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
 * @interface NonFungibleResourcesCollectionItemVaultAggregatedVaultItem
 */
export interface NonFungibleResourcesCollectionItemVaultAggregatedVaultItem {
    /**
     * 
     * @type {number}
     * @memberof NonFungibleResourcesCollectionItemVaultAggregatedVaultItem
     */
    total_count: number;
    /**
     * If specified, contains a cursor to query next page of the `items` collection.
     * @type {string}
     * @memberof NonFungibleResourcesCollectionItemVaultAggregatedVaultItem
     */
    next_cursor?: string | null;
    /**
     * 
     * @type {Array<string>}
     * @memberof NonFungibleResourcesCollectionItemVaultAggregatedVaultItem
     */
    items?: Array<string>;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NonFungibleResourcesCollectionItemVaultAggregatedVaultItem
     */
    vault_address: string;
    /**
     * TBD
     * @type {number}
     * @memberof NonFungibleResourcesCollectionItemVaultAggregatedVaultItem
     */
    last_updated_at_state_version: number;
}

/**
 * Check if a given object implements the NonFungibleResourcesCollectionItemVaultAggregatedVaultItem interface.
 */
export function instanceOfNonFungibleResourcesCollectionItemVaultAggregatedVaultItem(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "total_count" in value;
    isInstance = isInstance && "vault_address" in value;
    isInstance = isInstance && "last_updated_at_state_version" in value;

    return isInstance;
}

export function NonFungibleResourcesCollectionItemVaultAggregatedVaultItemFromJSON(json: any): NonFungibleResourcesCollectionItemVaultAggregatedVaultItem {
    return NonFungibleResourcesCollectionItemVaultAggregatedVaultItemFromJSONTyped(json, false);
}

export function NonFungibleResourcesCollectionItemVaultAggregatedVaultItemFromJSONTyped(json: any, ignoreDiscriminator: boolean): NonFungibleResourcesCollectionItemVaultAggregatedVaultItem {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'total_count': json['total_count'],
        'next_cursor': !exists(json, 'next_cursor') ? undefined : json['next_cursor'],
        'items': !exists(json, 'items') ? undefined : json['items'],
        'vault_address': json['vault_address'],
        'last_updated_at_state_version': json['last_updated_at_state_version'],
    };
}

export function NonFungibleResourcesCollectionItemVaultAggregatedVaultItemToJSON(value?: NonFungibleResourcesCollectionItemVaultAggregatedVaultItem | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'total_count': value.total_count,
        'next_cursor': value.next_cursor,
        'items': value.items,
        'vault_address': value.vault_address,
        'last_updated_at_state_version': value.last_updated_at_state_version,
    };
}

