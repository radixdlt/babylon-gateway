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
import type { GatewayInfoResponseKnownTarget } from './GatewayInfoResponseKnownTarget';
import {
    GatewayInfoResponseKnownTargetFromJSON,
    GatewayInfoResponseKnownTargetFromJSONTyped,
    GatewayInfoResponseKnownTargetToJSON,
} from './GatewayInfoResponseKnownTarget';
import type { GatewayInfoResponseReleaseInfo } from './GatewayInfoResponseReleaseInfo';
import {
    GatewayInfoResponseReleaseInfoFromJSON,
    GatewayInfoResponseReleaseInfoFromJSONTyped,
    GatewayInfoResponseReleaseInfoToJSON,
} from './GatewayInfoResponseReleaseInfo';
import type { GatewayInformationResponseAllOfWellKnownAddresses } from './GatewayInformationResponseAllOfWellKnownAddresses';
import {
    GatewayInformationResponseAllOfWellKnownAddressesFromJSON,
    GatewayInformationResponseAllOfWellKnownAddressesFromJSONTyped,
    GatewayInformationResponseAllOfWellKnownAddressesToJSON,
} from './GatewayInformationResponseAllOfWellKnownAddresses';
import type { LedgerState } from './LedgerState';
import {
    LedgerStateFromJSON,
    LedgerStateFromJSONTyped,
    LedgerStateToJSON,
} from './LedgerState';

/**
 * 
 * @export
 * @interface GatewayInformationResponse
 */
export interface GatewayInformationResponse {
    /**
     * 
     * @type {LedgerState}
     * @memberof GatewayInformationResponse
     */
    ledger_state: LedgerState;
    /**
     * 
     * @type {GatewayInfoResponseKnownTarget}
     * @memberof GatewayInformationResponse
     */
    known_target: GatewayInfoResponseKnownTarget;
    /**
     * 
     * @type {GatewayInfoResponseReleaseInfo}
     * @memberof GatewayInformationResponse
     */
    release_info: GatewayInfoResponseReleaseInfo;
    /**
     * 
     * @type {GatewayInformationResponseAllOfWellKnownAddresses}
     * @memberof GatewayInformationResponse
     */
    well_known_addresses: GatewayInformationResponseAllOfWellKnownAddresses;
}

/**
 * Check if a given object implements the GatewayInformationResponse interface.
 */
export function instanceOfGatewayInformationResponse(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "ledger_state" in value;
    isInstance = isInstance && "known_target" in value;
    isInstance = isInstance && "release_info" in value;
    isInstance = isInstance && "well_known_addresses" in value;

    return isInstance;
}

export function GatewayInformationResponseFromJSON(json: any): GatewayInformationResponse {
    return GatewayInformationResponseFromJSONTyped(json, false);
}

export function GatewayInformationResponseFromJSONTyped(json: any, ignoreDiscriminator: boolean): GatewayInformationResponse {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'ledger_state': LedgerStateFromJSON(json['ledger_state']),
        'known_target': GatewayInfoResponseKnownTargetFromJSON(json['known_target']),
        'release_info': GatewayInfoResponseReleaseInfoFromJSON(json['release_info']),
        'well_known_addresses': GatewayInformationResponseAllOfWellKnownAddressesFromJSON(json['well_known_addresses']),
    };
}

export function GatewayInformationResponseToJSON(value?: GatewayInformationResponse | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'ledger_state': LedgerStateToJSON(value.ledger_state),
        'known_target': GatewayInfoResponseKnownTargetToJSON(value.known_target),
        'release_info': GatewayInfoResponseReleaseInfoToJSON(value.release_info),
        'well_known_addresses': GatewayInformationResponseAllOfWellKnownAddressesToJSON(value.well_known_addresses),
    };
}

