/* tslint:disable */
/* eslint-disable */
/**
 * Radix Gateway API - Babylon
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers, and for light queries from front-end dApps. For exchange/asset integrations, back-end dApp integrations, or simple use cases, you should consider using the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  All responses may have additional fields added at any release, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects.  When the Radix protocol is updated, new functionality may be added, and so discriminated unions returned by the API may need to be updated to have new variants added, corresponding to the updated data. Clients may need to update in advance to be able to handle these new variants when a protocol update comes out.  On the very rare occasions we need to make breaking changes to the API, these will be warned in advance with deprecation notices on previous versions. These deprecation notices will include a safe migration path. Deprecation notes or breaking changes will be flagged clearly in release notes for new versions of the Gateway.  The Gateway DB schema is not subject to any compatibility guarantees, and may be changed at any release. DB changes will be flagged in the release notes so clients doing custom DB integrations can prepare. 
 *
 * The version of the OpenAPI document: v1.2.0
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

/**
 * 
 * @export
 * @interface StateEntityDetailsResponseComponentDetails
 */
export interface StateEntityDetailsResponseComponentDetails {
    /**
     * 
     * @type {string}
     * @memberof StateEntityDetailsResponseComponentDetails
     */
    type: StateEntityDetailsResponseComponentDetailsTypeEnum;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof StateEntityDetailsResponseComponentDetails
     */
    package_address?: string;
    /**
     * 
     * @type {string}
     * @memberof StateEntityDetailsResponseComponentDetails
     */
    blueprint_name: string;
    /**
     * 
     * @type {string}
     * @memberof StateEntityDetailsResponseComponentDetails
     */
    blueprint_version: string;
    /**
     * A representation of a component's inner state. If this entity is a `GenericComponent`, this field will be in a programmatic JSON
     * structure (you can deserialize it as a `ProgrammaticScryptoSborValue`). Otherwise, for "native" components such as `Account`,
     * `Validator`, `AccessController`, `OneResourcePool`, `TwoResourcePool`, and `MultiResourcePool`, this field will be a 
     * custom JSON model defined in the Core API schema.
     * @type {object}
     * @memberof StateEntityDetailsResponseComponentDetails
     */
    state?: object;
    /**
     * 
     * @type {ComponentEntityRoleAssignments}
     * @memberof StateEntityDetailsResponseComponentDetails
     */
    role_assignments?: ComponentEntityRoleAssignments;
    /**
     * String-encoded decimal representing the amount of a related fungible resource.
     * @type {string}
     * @memberof StateEntityDetailsResponseComponentDetails
     */
    royalty_vault_balance?: string;
}


/**
 * @export
 */
export const StateEntityDetailsResponseComponentDetailsTypeEnum = {
    Component: 'Component'
} as const;
export type StateEntityDetailsResponseComponentDetailsTypeEnum = typeof StateEntityDetailsResponseComponentDetailsTypeEnum[keyof typeof StateEntityDetailsResponseComponentDetailsTypeEnum];


/**
 * Check if a given object implements the StateEntityDetailsResponseComponentDetails interface.
 */
export function instanceOfStateEntityDetailsResponseComponentDetails(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "type" in value;
    isInstance = isInstance && "blueprint_name" in value;
    isInstance = isInstance && "blueprint_version" in value;

    return isInstance;
}

export function StateEntityDetailsResponseComponentDetailsFromJSON(json: any): StateEntityDetailsResponseComponentDetails {
    return StateEntityDetailsResponseComponentDetailsFromJSONTyped(json, false);
}

export function StateEntityDetailsResponseComponentDetailsFromJSONTyped(json: any, ignoreDiscriminator: boolean): StateEntityDetailsResponseComponentDetails {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'type': json['type'],
        'package_address': !exists(json, 'package_address') ? undefined : json['package_address'],
        'blueprint_name': json['blueprint_name'],
        'blueprint_version': json['blueprint_version'],
        'state': !exists(json, 'state') ? undefined : json['state'],
        'role_assignments': !exists(json, 'role_assignments') ? undefined : ComponentEntityRoleAssignmentsFromJSON(json['role_assignments']),
        'royalty_vault_balance': !exists(json, 'royalty_vault_balance') ? undefined : json['royalty_vault_balance'],
    };
}

export function StateEntityDetailsResponseComponentDetailsToJSON(value?: StateEntityDetailsResponseComponentDetails | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'type': value.type,
        'package_address': value.package_address,
        'blueprint_name': value.blueprint_name,
        'blueprint_version': value.blueprint_version,
        'state': value.state,
        'role_assignments': ComponentEntityRoleAssignmentsToJSON(value.role_assignments),
        'royalty_vault_balance': value.royalty_vault_balance,
    };
}

