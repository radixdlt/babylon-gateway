/* tslint:disable */
/* eslint-disable */
/**
 * Radix Gateway API - Babylon
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers, and for light queries from front-end dApps. For exchange/asset integrations, back-end dApp integrations, or simple use cases, you should consider using the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  All responses may have additional fields added at any release, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects.  When the Radix protocol is updated, new functionality may be added, and so discriminated unions returned by the API may need to be updated to have new variants added, corresponding to the updated data. Clients may need to update in advance to be able to handle these new variants when a protocol update comes out.  On the very rare occasions we need to make breaking changes to the API, these will be warned in advance with deprecation notices on previous versions. These deprecation notices will include a safe migration path. Deprecation notes or breaking changes will be flagged clearly in release notes for new versions of the Gateway.  The Gateway DB schema is not subject to any compatibility guarantees, and may be changed at any release. DB changes will be flagged in the release notes so clients doing custom DB integrations can prepare. 
 *
 * The version of the OpenAPI document: v1.10.1
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */

import { exists, mapValues } from '../runtime';
import type { ComponentEntityRoleAssignments } from './ComponentEntityRoleAssignments';
import {
    ComponentEntityRoleAssignmentsFromJSON,
    ComponentEntityRoleAssignmentsFromJSONTyped,
    ComponentEntityRoleAssignmentsToJSON,
} from './ComponentEntityRoleAssignments';
import type { NativeResourceDetails } from './NativeResourceDetails';
import {
    NativeResourceDetailsFromJSON,
    NativeResourceDetailsFromJSONTyped,
    NativeResourceDetailsToJSON,
} from './NativeResourceDetails';
import type { TwoWayLinkedDappsCollection } from './TwoWayLinkedDappsCollection';
import {
    TwoWayLinkedDappsCollectionFromJSON,
    TwoWayLinkedDappsCollectionFromJSONTyped,
    TwoWayLinkedDappsCollectionToJSON,
} from './TwoWayLinkedDappsCollection';

/**
 * 
 * @export
 * @interface StateEntityDetailsResponseFungibleResourceDetails
 */
export interface StateEntityDetailsResponseFungibleResourceDetails {
    /**
     * 
     * @type {string}
     * @memberof StateEntityDetailsResponseFungibleResourceDetails
     */
    type: StateEntityDetailsResponseFungibleResourceDetailsTypeEnum;
    /**
     * 
     * @type {ComponentEntityRoleAssignments}
     * @memberof StateEntityDetailsResponseFungibleResourceDetails
     */
    role_assignments: ComponentEntityRoleAssignments;
    /**
     * 
     * @type {number}
     * @memberof StateEntityDetailsResponseFungibleResourceDetails
     */
    divisibility: number;
    /**
     * String-encoded decimal representing the amount of a related fungible resource.
     * @type {string}
     * @memberof StateEntityDetailsResponseFungibleResourceDetails
     */
    total_supply: string;
    /**
     * String-encoded decimal representing the amount of a related fungible resource.
     * @type {string}
     * @memberof StateEntityDetailsResponseFungibleResourceDetails
     */
    total_minted: string;
    /**
     * String-encoded decimal representing the amount of a related fungible resource.
     * @type {string}
     * @memberof StateEntityDetailsResponseFungibleResourceDetails
     */
    total_burned: string;
    /**
     * 
     * @type {TwoWayLinkedDappsCollection}
     * @memberof StateEntityDetailsResponseFungibleResourceDetails
     */
    two_way_linked_dapps?: TwoWayLinkedDappsCollection;
    /**
     * 
     * @type {NativeResourceDetails}
     * @memberof StateEntityDetailsResponseFungibleResourceDetails
     */
    native_resource_details?: NativeResourceDetails;
}


/**
 * @export
 */
export const StateEntityDetailsResponseFungibleResourceDetailsTypeEnum = {
    FungibleResource: 'FungibleResource'
} as const;
export type StateEntityDetailsResponseFungibleResourceDetailsTypeEnum = typeof StateEntityDetailsResponseFungibleResourceDetailsTypeEnum[keyof typeof StateEntityDetailsResponseFungibleResourceDetailsTypeEnum];


/**
 * Check if a given object implements the StateEntityDetailsResponseFungibleResourceDetails interface.
 */
export function instanceOfStateEntityDetailsResponseFungibleResourceDetails(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "type" in value;
    isInstance = isInstance && "role_assignments" in value;
    isInstance = isInstance && "divisibility" in value;
    isInstance = isInstance && "total_supply" in value;
    isInstance = isInstance && "total_minted" in value;
    isInstance = isInstance && "total_burned" in value;

    return isInstance;
}

export function StateEntityDetailsResponseFungibleResourceDetailsFromJSON(json: any): StateEntityDetailsResponseFungibleResourceDetails {
    return StateEntityDetailsResponseFungibleResourceDetailsFromJSONTyped(json, false);
}

export function StateEntityDetailsResponseFungibleResourceDetailsFromJSONTyped(json: any, ignoreDiscriminator: boolean): StateEntityDetailsResponseFungibleResourceDetails {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'type': json['type'],
        'role_assignments': ComponentEntityRoleAssignmentsFromJSON(json['role_assignments']),
        'divisibility': json['divisibility'],
        'total_supply': json['total_supply'],
        'total_minted': json['total_minted'],
        'total_burned': json['total_burned'],
        'two_way_linked_dapps': !exists(json, 'two_way_linked_dapps') ? undefined : TwoWayLinkedDappsCollectionFromJSON(json['two_way_linked_dapps']),
        'native_resource_details': !exists(json, 'native_resource_details') ? undefined : NativeResourceDetailsFromJSON(json['native_resource_details']),
    };
}

export function StateEntityDetailsResponseFungibleResourceDetailsToJSON(value?: StateEntityDetailsResponseFungibleResourceDetails | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'type': value.type,
        'role_assignments': ComponentEntityRoleAssignmentsToJSON(value.role_assignments),
        'divisibility': value.divisibility,
        'total_supply': value.total_supply,
        'total_minted': value.total_minted,
        'total_burned': value.total_burned,
        'two_way_linked_dapps': TwoWayLinkedDappsCollectionToJSON(value.two_way_linked_dapps),
        'native_resource_details': NativeResourceDetailsToJSON(value.native_resource_details),
    };
}

