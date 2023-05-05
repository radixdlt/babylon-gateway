/* tslint:disable */
/* eslint-disable */
/**
 * Babylon Gateway API - RCnet V2
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers. For simple use cases, you can typically use the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs-babylon.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Integration and forward compatibility guarantees  We give no guarantees that other endpoints will not change before Babylon mainnet launch, although changes are expected to be minimal. 
 *
 * The version of the OpenAPI document: 0.4.0
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
 * @interface InvalidTransactionErrorAllOf
 */
export interface InvalidTransactionErrorAllOf {
    /**
     * 
     * @type {string}
     * @memberof InvalidTransactionErrorAllOf
     */
    type?: InvalidTransactionErrorAllOfTypeEnum;
}


/**
 * @export
 */
export const InvalidTransactionErrorAllOfTypeEnum = {
    InvalidTransactionError: 'InvalidTransactionError'
} as const;
export type InvalidTransactionErrorAllOfTypeEnum = typeof InvalidTransactionErrorAllOfTypeEnum[keyof typeof InvalidTransactionErrorAllOfTypeEnum];


/**
 * Check if a given object implements the InvalidTransactionErrorAllOf interface.
 */
export function instanceOfInvalidTransactionErrorAllOf(value: object): boolean {
    let isInstance = true;

    return isInstance;
}

export function InvalidTransactionErrorAllOfFromJSON(json: any): InvalidTransactionErrorAllOf {
    return InvalidTransactionErrorAllOfFromJSONTyped(json, false);
}

export function InvalidTransactionErrorAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): InvalidTransactionErrorAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'type': !exists(json, 'type') ? undefined : json['type'],
    };
}

export function InvalidTransactionErrorAllOfToJSON(value?: InvalidTransactionErrorAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'type': value.type,
    };
}
