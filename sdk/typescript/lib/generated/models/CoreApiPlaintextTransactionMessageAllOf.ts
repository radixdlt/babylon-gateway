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
import type { CoreApiPlaintextMessageContent } from './CoreApiPlaintextMessageContent';
import {
    CoreApiPlaintextMessageContentFromJSON,
    CoreApiPlaintextMessageContentFromJSONTyped,
    CoreApiPlaintextMessageContentToJSON,
} from './CoreApiPlaintextMessageContent';

/**
 * An unencrypted message.
 * @export
 * @interface CoreApiPlaintextTransactionMessageAllOf
 */
export interface CoreApiPlaintextTransactionMessageAllOf {
    /**
     * Intended to represent the RFC 2046 MIME type of the `content`.
     * A client cannot trust that this field is a valid mime type - in particular, the
     * choice between `String` or `Binary` representation of the content is not enforced by
     * this `mime_type`.
     * @type {string}
     * @memberof CoreApiPlaintextTransactionMessageAllOf
     */
    mime_type: string;
    /**
     * 
     * @type {CoreApiPlaintextMessageContent}
     * @memberof CoreApiPlaintextTransactionMessageAllOf
     */
    content: CoreApiPlaintextMessageContent;
    /**
     * 
     * @type {string}
     * @memberof CoreApiPlaintextTransactionMessageAllOf
     */
    type?: CoreApiPlaintextTransactionMessageAllOfTypeEnum;
}


/**
 * @export
 */
export const CoreApiPlaintextTransactionMessageAllOfTypeEnum = {
    Plaintext: 'Plaintext'
} as const;
export type CoreApiPlaintextTransactionMessageAllOfTypeEnum = typeof CoreApiPlaintextTransactionMessageAllOfTypeEnum[keyof typeof CoreApiPlaintextTransactionMessageAllOfTypeEnum];


/**
 * Check if a given object implements the CoreApiPlaintextTransactionMessageAllOf interface.
 */
export function instanceOfCoreApiPlaintextTransactionMessageAllOf(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "mime_type" in value;
    isInstance = isInstance && "content" in value;

    return isInstance;
}

export function CoreApiPlaintextTransactionMessageAllOfFromJSON(json: any): CoreApiPlaintextTransactionMessageAllOf {
    return CoreApiPlaintextTransactionMessageAllOfFromJSONTyped(json, false);
}

export function CoreApiPlaintextTransactionMessageAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): CoreApiPlaintextTransactionMessageAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'mime_type': json['mime_type'],
        'content': CoreApiPlaintextMessageContentFromJSON(json['content']),
        'type': !exists(json, 'type') ? undefined : json['type'],
    };
}

export function CoreApiPlaintextTransactionMessageAllOfToJSON(value?: CoreApiPlaintextTransactionMessageAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'mime_type': value.mime_type,
        'content': CoreApiPlaintextMessageContentToJSON(value.content),
        'type': value.type,
    };
}

