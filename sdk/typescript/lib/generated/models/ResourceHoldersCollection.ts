/* tslint:disable */
/* eslint-disable */
/**
 * Radix Gateway API - Babylon
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers, and for light queries from front-end dApps. For exchange/asset integrations, back-end dApp integrations, or simple use cases, you should consider using the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  All responses may have additional fields added at any release, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects.  When the Radix protocol is updated, new functionality may be added, and so discriminated unions returned by the API may need to be updated to have new variants added, corresponding to the updated data. Clients may need to update in advance to be able to handle these new variants when a protocol update comes out.  On the very rare occasions we need to make breaking changes to the API, these will be warned in advance with deprecation notices on previous versions. These deprecation notices will include a safe migration path. Deprecation notes or breaking changes will be flagged clearly in release notes for new versions of the Gateway.  The Gateway DB schema is not subject to any compatibility guarantees, and may be changed at any release. DB changes will be flagged in the release notes so clients doing custom DB integrations can prepare. 
 *
 * The version of the OpenAPI document: v1.9.0
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */

import { exists, mapValues } from '../runtime';
import type { ResourceHoldersCollectionItem } from './ResourceHoldersCollectionItem';
import {
    ResourceHoldersCollectionItemFromJSON,
    ResourceHoldersCollectionItemFromJSONTyped,
    ResourceHoldersCollectionItemToJSON,
} from './ResourceHoldersCollectionItem';

/**
 * 
 * @export
 * @interface ResourceHoldersCollection
 */
export interface ResourceHoldersCollection {
    /**
     * Total number of items in underlying collection, fragment of which is available in `items` collection.
     * @type {number}
     * @memberof ResourceHoldersCollection
     */
    total_count?: number | null;
    /**
     * If specified, contains a cursor to query next page of the `items` collection.
     * @type {string}
     * @memberof ResourceHoldersCollection
     */
    next_cursor?: string | null;
    /**
     * 
     * @type {Array<ResourceHoldersCollectionItem>}
     * @memberof ResourceHoldersCollection
     */
    items: Array<ResourceHoldersCollectionItem>;
}

/**
 * Check if a given object implements the ResourceHoldersCollection interface.
 */
export function instanceOfResourceHoldersCollection(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "items" in value;

    return isInstance;
}

export function ResourceHoldersCollectionFromJSON(json: any): ResourceHoldersCollection {
    return ResourceHoldersCollectionFromJSONTyped(json, false);
}

export function ResourceHoldersCollectionFromJSONTyped(json: any, ignoreDiscriminator: boolean): ResourceHoldersCollection {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'total_count': !exists(json, 'total_count') ? undefined : json['total_count'],
        'next_cursor': !exists(json, 'next_cursor') ? undefined : json['next_cursor'],
        'items': ((json['items'] as Array<any>).map(ResourceHoldersCollectionItemFromJSON)),
    };
}

export function ResourceHoldersCollectionToJSON(value?: ResourceHoldersCollection | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'total_count': value.total_count,
        'next_cursor': value.next_cursor,
        'items': ((value.items as Array<any>).map(ResourceHoldersCollectionItemToJSON)),
    };
}

