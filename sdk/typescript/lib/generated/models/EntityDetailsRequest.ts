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
import type { PartialLedgerStateIdentifier } from './PartialLedgerStateIdentifier';
import {
    PartialLedgerStateIdentifierFromJSON,
    PartialLedgerStateIdentifierFromJSONTyped,
    PartialLedgerStateIdentifierToJSON,
} from './PartialLedgerStateIdentifier';

/**
 * 
 * @export
 * @interface EntityDetailsRequest
 */
export interface EntityDetailsRequest {
    /**
     * The Bech32m-encoded human readable version of the entity's global address
     * @type {string}
     * @memberof EntityDetailsRequest
     */
    address: string;
    /**
     * 
     * @type {PartialLedgerStateIdentifier}
     * @memberof EntityDetailsRequest
     */
    at_state_identifier?: PartialLedgerStateIdentifier | null;
}

/**
 * Check if a given object implements the EntityDetailsRequest interface.
 */
export function instanceOfEntityDetailsRequest(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "address" in value;

    return isInstance;
}

export function EntityDetailsRequestFromJSON(json: any): EntityDetailsRequest {
    return EntityDetailsRequestFromJSONTyped(json, false);
}

export function EntityDetailsRequestFromJSONTyped(json: any, ignoreDiscriminator: boolean): EntityDetailsRequest {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'address': json['address'],
        'at_state_identifier': !exists(json, 'at_state_identifier') ? undefined : PartialLedgerStateIdentifierFromJSON(json['at_state_identifier']),
    };
}

export function EntityDetailsRequestToJSON(value?: EntityDetailsRequest | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'address': value.address,
        'at_state_identifier': PartialLedgerStateIdentifierToJSON(value.at_state_identifier),
    };
}

