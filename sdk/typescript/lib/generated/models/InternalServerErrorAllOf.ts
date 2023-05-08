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
 * @interface InternalServerErrorAllOf
 */
export interface InternalServerErrorAllOf {
    /**
     * Gives an error type which occurred within the Gateway API when serving the request.
     * @type {string}
     * @memberof InternalServerErrorAllOf
     */
    exception: string;
    /**
     * Gives a human readable message - likely just a trace ID for reporting the error.
     * @type {string}
     * @memberof InternalServerErrorAllOf
     */
    cause: string;
    /**
     * 
     * @type {string}
     * @memberof InternalServerErrorAllOf
     */
    type?: InternalServerErrorAllOfTypeEnum;
}


/**
 * @export
 */
export const InternalServerErrorAllOfTypeEnum = {
    InternalServerError: 'InternalServerError'
} as const;
export type InternalServerErrorAllOfTypeEnum = typeof InternalServerErrorAllOfTypeEnum[keyof typeof InternalServerErrorAllOfTypeEnum];


/**
 * Check if a given object implements the InternalServerErrorAllOf interface.
 */
export function instanceOfInternalServerErrorAllOf(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "exception" in value;
    isInstance = isInstance && "cause" in value;

    return isInstance;
}

export function InternalServerErrorAllOfFromJSON(json: any): InternalServerErrorAllOf {
    return InternalServerErrorAllOfFromJSONTyped(json, false);
}

export function InternalServerErrorAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): InternalServerErrorAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'exception': json['exception'],
        'cause': json['cause'],
        'type': !exists(json, 'type') ? undefined : json['type'],
    };
}

export function InternalServerErrorAllOfToJSON(value?: InternalServerErrorAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'exception': value.exception,
        'cause': value.cause,
        'type': value.type,
    };
}

