/* tslint:disable */
/* eslint-disable */
/**
 * Radix Gateway API - Babylon
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers, and for light queries from front-end dApps. For exchange/asset integrations, back-end dApp integrations, or simple use cases, you should consider using the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  All responses may have additional fields added at any release, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects.  When the Radix protocol is updated, new functionality may be added, and so discriminated unions returned by the API may need to be updated to have new variants added, corresponding to the updated data. Clients may need to update in advance to be able to handle these new variants when a protocol update comes out.  On the very rare occasions we need to make breaking changes to the API, these will be warned in advance with deprecation notices on previous versions. These deprecation notices will include a safe migration path. Deprecation notes or breaking changes will be flagged clearly in release notes for new versions of the Gateway.  The Gateway DB schema is not subject to any compatibility guarantees, and may be changed at any release. DB changes will be flagged in the release notes so clients doing custom DB integrations can prepare. 
 *
 * The version of the OpenAPI document: v1.10.0
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
import type { NonFungibleResourcesCollectionItemVaultAggregatedVault } from './NonFungibleResourcesCollectionItemVaultAggregatedVault';
import {
    NonFungibleResourcesCollectionItemVaultAggregatedVaultFromJSON,
    NonFungibleResourcesCollectionItemVaultAggregatedVaultFromJSONTyped,
    NonFungibleResourcesCollectionItemVaultAggregatedVaultToJSON,
} from './NonFungibleResourcesCollectionItemVaultAggregatedVault';

/**
 * 
 * @export
 * @interface NonFungibleResourcesCollectionItemVaultAggregated
 */
export interface NonFungibleResourcesCollectionItemVaultAggregated {
    /**
     * 
     * @type {string}
     * @memberof NonFungibleResourcesCollectionItemVaultAggregated
     */
    aggregation_level: NonFungibleResourcesCollectionItemVaultAggregatedAggregationLevelEnum;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NonFungibleResourcesCollectionItemVaultAggregated
     */
    resource_address: string;
    /**
     * 
     * @type {EntityMetadataCollection}
     * @memberof NonFungibleResourcesCollectionItemVaultAggregated
     */
    explicit_metadata?: EntityMetadataCollection;
    /**
     * 
     * @type {NonFungibleResourcesCollectionItemVaultAggregatedVault}
     * @memberof NonFungibleResourcesCollectionItemVaultAggregated
     */
    vaults: NonFungibleResourcesCollectionItemVaultAggregatedVault;
}


/**
 * @export
 */
export const NonFungibleResourcesCollectionItemVaultAggregatedAggregationLevelEnum = {
    Vault: 'Vault'
} as const;
export type NonFungibleResourcesCollectionItemVaultAggregatedAggregationLevelEnum = typeof NonFungibleResourcesCollectionItemVaultAggregatedAggregationLevelEnum[keyof typeof NonFungibleResourcesCollectionItemVaultAggregatedAggregationLevelEnum];


/**
 * Check if a given object implements the NonFungibleResourcesCollectionItemVaultAggregated interface.
 */
export function instanceOfNonFungibleResourcesCollectionItemVaultAggregated(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "aggregation_level" in value;
    isInstance = isInstance && "resource_address" in value;
    isInstance = isInstance && "vaults" in value;

    return isInstance;
}

export function NonFungibleResourcesCollectionItemVaultAggregatedFromJSON(json: any): NonFungibleResourcesCollectionItemVaultAggregated {
    return NonFungibleResourcesCollectionItemVaultAggregatedFromJSONTyped(json, false);
}

export function NonFungibleResourcesCollectionItemVaultAggregatedFromJSONTyped(json: any, ignoreDiscriminator: boolean): NonFungibleResourcesCollectionItemVaultAggregated {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'aggregation_level': json['aggregation_level'],
        'resource_address': json['resource_address'],
        'explicit_metadata': !exists(json, 'explicit_metadata') ? undefined : EntityMetadataCollectionFromJSON(json['explicit_metadata']),
        'vaults': NonFungibleResourcesCollectionItemVaultAggregatedVaultFromJSON(json['vaults']),
    };
}

export function NonFungibleResourcesCollectionItemVaultAggregatedToJSON(value?: NonFungibleResourcesCollectionItemVaultAggregated | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'aggregation_level': value.aggregation_level,
        'resource_address': value.resource_address,
        'explicit_metadata': EntityMetadataCollectionToJSON(value.explicit_metadata),
        'vaults': NonFungibleResourcesCollectionItemVaultAggregatedVaultToJSON(value.vaults),
    };
}

