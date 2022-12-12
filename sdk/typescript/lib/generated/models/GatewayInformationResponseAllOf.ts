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

/**
 * 
 * @export
 * @interface GatewayInformationResponseAllOf
 */
export interface GatewayInformationResponseAllOf {
    /**
     * 
     * @type {GatewayInfoResponseKnownTarget}
     * @memberof GatewayInformationResponseAllOf
     */
    known_target: GatewayInfoResponseKnownTarget;
    /**
     * 
     * @type {GatewayInfoResponseReleaseInfo}
     * @memberof GatewayInformationResponseAllOf
     */
    release_info: GatewayInfoResponseReleaseInfo;
    /**
     * 
     * @type {GatewayInformationResponseAllOfWellKnownAddresses}
     * @memberof GatewayInformationResponseAllOf
     */
    well_known_addresses: GatewayInformationResponseAllOfWellKnownAddresses;
}

/**
 * Check if a given object implements the GatewayInformationResponseAllOf interface.
 */
export function instanceOfGatewayInformationResponseAllOf(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "known_target" in value;
    isInstance = isInstance && "release_info" in value;
    isInstance = isInstance && "well_known_addresses" in value;

    return isInstance;
}

export function GatewayInformationResponseAllOfFromJSON(json: any): GatewayInformationResponseAllOf {
    return GatewayInformationResponseAllOfFromJSONTyped(json, false);
}

export function GatewayInformationResponseAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): GatewayInformationResponseAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'known_target': GatewayInfoResponseKnownTargetFromJSON(json['known_target']),
        'release_info': GatewayInfoResponseReleaseInfoFromJSON(json['release_info']),
        'well_known_addresses': GatewayInformationResponseAllOfWellKnownAddressesFromJSON(json['well_known_addresses']),
    };
}

export function GatewayInformationResponseAllOfToJSON(value?: GatewayInformationResponseAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'known_target': GatewayInfoResponseKnownTargetToJSON(value.known_target),
        'release_info': GatewayInfoResponseReleaseInfoToJSON(value.release_info),
        'well_known_addresses': GatewayInformationResponseAllOfWellKnownAddressesToJSON(value.well_known_addresses),
    };
}
