/* tslint:disable */
/* eslint-disable */
/**
 * Radix Gateway API - Babylon
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers, and for light queries from front-end dApps. For exchange/asset integrations, back-end dApp integrations, or simple use cases, you should consider using the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  All responses may have additional fields added at any release, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects.  When the Radix protocol is updated, new functionality may be added, and so discriminated unions returned by the API may need to be updated to have new variants added, corresponding to the updated data. Clients may need to update in advance to be able to handle these new variants when a protocol update comes out.  On the very rare occasions we need to make breaking changes to the API, these will be warned in advance with deprecation notices on previous versions. These deprecation notices will include a safe migration path. Deprecation notes or breaking changes will be flagged clearly in release notes for new versions of the Gateway.  The Gateway DB schema is not subject to any compatibility guarantees, and may be changed at any release. DB changes will be flagged in the release notes so clients doing custom DB integrations can prepare. 
 *
 * The version of the OpenAPI document: v1.6.1
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */

import { exists, mapValues } from '../runtime';
import type { CoreApiSubstateKeyType } from './CoreApiSubstateKeyType';
import {
    CoreApiSubstateKeyTypeFromJSON,
    CoreApiSubstateKeyTypeFromJSONTyped,
    CoreApiSubstateKeyTypeToJSON,
} from './CoreApiSubstateKeyType';

/**
 * 
 * @export
 * @interface CoreApiSubstateKeyBase
 */
export interface CoreApiSubstateKeyBase {
    /**
     * 
     * @type {CoreApiSubstateKeyType}
     * @memberof CoreApiSubstateKeyBase
     */
    key_type: CoreApiSubstateKeyType;
    /**
     * The hex-encoded bytes of the partially-hashed DB sort key, under the given entity partition
     * @type {string}
     * @memberof CoreApiSubstateKeyBase
     */
    db_sort_key_hex: string;
}

/**
 * Check if a given object implements the CoreApiSubstateKeyBase interface.
 */
export function instanceOfCoreApiSubstateKeyBase(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "key_type" in value;
    isInstance = isInstance && "db_sort_key_hex" in value;

    return isInstance;
}

export function CoreApiSubstateKeyBaseFromJSON(json: any): CoreApiSubstateKeyBase {
    return CoreApiSubstateKeyBaseFromJSONTyped(json, false);
}

export function CoreApiSubstateKeyBaseFromJSONTyped(json: any, ignoreDiscriminator: boolean): CoreApiSubstateKeyBase {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'key_type': CoreApiSubstateKeyTypeFromJSON(json['key_type']),
        'db_sort_key_hex': json['db_sort_key_hex'],
    };
}

export function CoreApiSubstateKeyBaseToJSON(value?: CoreApiSubstateKeyBase | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'key_type': CoreApiSubstateKeyTypeToJSON(value.key_type),
        'db_sort_key_hex': value.db_sort_key_hex,
    };
}

