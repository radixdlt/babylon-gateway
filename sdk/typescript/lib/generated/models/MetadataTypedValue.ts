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

import {
    MetadataBoolArrayValue,
    instanceOfMetadataBoolArrayValue,
    MetadataBoolArrayValueFromJSON,
    MetadataBoolArrayValueFromJSONTyped,
    MetadataBoolArrayValueToJSON,
} from './MetadataBoolArrayValue';
import {
    MetadataBoolValue,
    instanceOfMetadataBoolValue,
    MetadataBoolValueFromJSON,
    MetadataBoolValueFromJSONTyped,
    MetadataBoolValueToJSON,
} from './MetadataBoolValue';
import {
    MetadataDecimalArrayValue,
    instanceOfMetadataDecimalArrayValue,
    MetadataDecimalArrayValueFromJSON,
    MetadataDecimalArrayValueFromJSONTyped,
    MetadataDecimalArrayValueToJSON,
} from './MetadataDecimalArrayValue';
import {
    MetadataDecimalValue,
    instanceOfMetadataDecimalValue,
    MetadataDecimalValueFromJSON,
    MetadataDecimalValueFromJSONTyped,
    MetadataDecimalValueToJSON,
} from './MetadataDecimalValue';
import {
    MetadataGlobalAddressArrayValue,
    instanceOfMetadataGlobalAddressArrayValue,
    MetadataGlobalAddressArrayValueFromJSON,
    MetadataGlobalAddressArrayValueFromJSONTyped,
    MetadataGlobalAddressArrayValueToJSON,
} from './MetadataGlobalAddressArrayValue';
import {
    MetadataGlobalAddressValue,
    instanceOfMetadataGlobalAddressValue,
    MetadataGlobalAddressValueFromJSON,
    MetadataGlobalAddressValueFromJSONTyped,
    MetadataGlobalAddressValueToJSON,
} from './MetadataGlobalAddressValue';
import {
    MetadataI32ArrayValue,
    instanceOfMetadataI32ArrayValue,
    MetadataI32ArrayValueFromJSON,
    MetadataI32ArrayValueFromJSONTyped,
    MetadataI32ArrayValueToJSON,
} from './MetadataI32ArrayValue';
import {
    MetadataI32Value,
    instanceOfMetadataI32Value,
    MetadataI32ValueFromJSON,
    MetadataI32ValueFromJSONTyped,
    MetadataI32ValueToJSON,
} from './MetadataI32Value';
import {
    MetadataI64ArrayValue,
    instanceOfMetadataI64ArrayValue,
    MetadataI64ArrayValueFromJSON,
    MetadataI64ArrayValueFromJSONTyped,
    MetadataI64ArrayValueToJSON,
} from './MetadataI64ArrayValue';
import {
    MetadataI64Value,
    instanceOfMetadataI64Value,
    MetadataI64ValueFromJSON,
    MetadataI64ValueFromJSONTyped,
    MetadataI64ValueToJSON,
} from './MetadataI64Value';
import {
    MetadataInstantArrayValue,
    instanceOfMetadataInstantArrayValue,
    MetadataInstantArrayValueFromJSON,
    MetadataInstantArrayValueFromJSONTyped,
    MetadataInstantArrayValueToJSON,
} from './MetadataInstantArrayValue';
import {
    MetadataInstantValue,
    instanceOfMetadataInstantValue,
    MetadataInstantValueFromJSON,
    MetadataInstantValueFromJSONTyped,
    MetadataInstantValueToJSON,
} from './MetadataInstantValue';
import {
    MetadataNonFungibleGlobalIdArrayValue,
    instanceOfMetadataNonFungibleGlobalIdArrayValue,
    MetadataNonFungibleGlobalIdArrayValueFromJSON,
    MetadataNonFungibleGlobalIdArrayValueFromJSONTyped,
    MetadataNonFungibleGlobalIdArrayValueToJSON,
} from './MetadataNonFungibleGlobalIdArrayValue';
import {
    MetadataNonFungibleGlobalIdValue,
    instanceOfMetadataNonFungibleGlobalIdValue,
    MetadataNonFungibleGlobalIdValueFromJSON,
    MetadataNonFungibleGlobalIdValueFromJSONTyped,
    MetadataNonFungibleGlobalIdValueToJSON,
} from './MetadataNonFungibleGlobalIdValue';
import {
    MetadataNonFungibleLocalIdArrayValue,
    instanceOfMetadataNonFungibleLocalIdArrayValue,
    MetadataNonFungibleLocalIdArrayValueFromJSON,
    MetadataNonFungibleLocalIdArrayValueFromJSONTyped,
    MetadataNonFungibleLocalIdArrayValueToJSON,
} from './MetadataNonFungibleLocalIdArrayValue';
import {
    MetadataNonFungibleLocalIdValue,
    instanceOfMetadataNonFungibleLocalIdValue,
    MetadataNonFungibleLocalIdValueFromJSON,
    MetadataNonFungibleLocalIdValueFromJSONTyped,
    MetadataNonFungibleLocalIdValueToJSON,
} from './MetadataNonFungibleLocalIdValue';
import {
    MetadataOriginArrayValue,
    instanceOfMetadataOriginArrayValue,
    MetadataOriginArrayValueFromJSON,
    MetadataOriginArrayValueFromJSONTyped,
    MetadataOriginArrayValueToJSON,
} from './MetadataOriginArrayValue';
import {
    MetadataOriginValue,
    instanceOfMetadataOriginValue,
    MetadataOriginValueFromJSON,
    MetadataOriginValueFromJSONTyped,
    MetadataOriginValueToJSON,
} from './MetadataOriginValue';
import {
    MetadataPublicKeyArrayValue,
    instanceOfMetadataPublicKeyArrayValue,
    MetadataPublicKeyArrayValueFromJSON,
    MetadataPublicKeyArrayValueFromJSONTyped,
    MetadataPublicKeyArrayValueToJSON,
} from './MetadataPublicKeyArrayValue';
import {
    MetadataPublicKeyHashArrayValue,
    instanceOfMetadataPublicKeyHashArrayValue,
    MetadataPublicKeyHashArrayValueFromJSON,
    MetadataPublicKeyHashArrayValueFromJSONTyped,
    MetadataPublicKeyHashArrayValueToJSON,
} from './MetadataPublicKeyHashArrayValue';
import {
    MetadataPublicKeyHashValue,
    instanceOfMetadataPublicKeyHashValue,
    MetadataPublicKeyHashValueFromJSON,
    MetadataPublicKeyHashValueFromJSONTyped,
    MetadataPublicKeyHashValueToJSON,
} from './MetadataPublicKeyHashValue';
import {
    MetadataPublicKeyValue,
    instanceOfMetadataPublicKeyValue,
    MetadataPublicKeyValueFromJSON,
    MetadataPublicKeyValueFromJSONTyped,
    MetadataPublicKeyValueToJSON,
} from './MetadataPublicKeyValue';
import {
    MetadataStringArrayValue,
    instanceOfMetadataStringArrayValue,
    MetadataStringArrayValueFromJSON,
    MetadataStringArrayValueFromJSONTyped,
    MetadataStringArrayValueToJSON,
} from './MetadataStringArrayValue';
import {
    MetadataStringValue,
    instanceOfMetadataStringValue,
    MetadataStringValueFromJSON,
    MetadataStringValueFromJSONTyped,
    MetadataStringValueToJSON,
} from './MetadataStringValue';
import {
    MetadataU32ArrayValue,
    instanceOfMetadataU32ArrayValue,
    MetadataU32ArrayValueFromJSON,
    MetadataU32ArrayValueFromJSONTyped,
    MetadataU32ArrayValueToJSON,
} from './MetadataU32ArrayValue';
import {
    MetadataU32Value,
    instanceOfMetadataU32Value,
    MetadataU32ValueFromJSON,
    MetadataU32ValueFromJSONTyped,
    MetadataU32ValueToJSON,
} from './MetadataU32Value';
import {
    MetadataU64ArrayValue,
    instanceOfMetadataU64ArrayValue,
    MetadataU64ArrayValueFromJSON,
    MetadataU64ArrayValueFromJSONTyped,
    MetadataU64ArrayValueToJSON,
} from './MetadataU64ArrayValue';
import {
    MetadataU64Value,
    instanceOfMetadataU64Value,
    MetadataU64ValueFromJSON,
    MetadataU64ValueFromJSONTyped,
    MetadataU64ValueToJSON,
} from './MetadataU64Value';
import {
    MetadataU8ArrayValue,
    instanceOfMetadataU8ArrayValue,
    MetadataU8ArrayValueFromJSON,
    MetadataU8ArrayValueFromJSONTyped,
    MetadataU8ArrayValueToJSON,
} from './MetadataU8ArrayValue';
import {
    MetadataU8Value,
    instanceOfMetadataU8Value,
    MetadataU8ValueFromJSON,
    MetadataU8ValueFromJSONTyped,
    MetadataU8ValueToJSON,
} from './MetadataU8Value';
import {
    MetadataUrlArrayValue,
    instanceOfMetadataUrlArrayValue,
    MetadataUrlArrayValueFromJSON,
    MetadataUrlArrayValueFromJSONTyped,
    MetadataUrlArrayValueToJSON,
} from './MetadataUrlArrayValue';
import {
    MetadataUrlValue,
    instanceOfMetadataUrlValue,
    MetadataUrlValueFromJSON,
    MetadataUrlValueFromJSONTyped,
    MetadataUrlValueToJSON,
} from './MetadataUrlValue';

/**
 * @type MetadataTypedValue
 * 
 * @export
 */
export type MetadataTypedValue = { type: 'Bool' } & MetadataBoolValue | { type: 'BoolArray' } & MetadataBoolArrayValue | { type: 'Decimal' } & MetadataDecimalValue | { type: 'DecimalArray' } & MetadataDecimalArrayValue | { type: 'GlobalAddress' } & MetadataGlobalAddressValue | { type: 'GlobalAddressArray' } & MetadataGlobalAddressArrayValue | { type: 'I32' } & MetadataI32Value | { type: 'I32Array' } & MetadataI32ArrayValue | { type: 'I64' } & MetadataI64Value | { type: 'I64Array' } & MetadataI64ArrayValue | { type: 'Instant' } & MetadataInstantValue | { type: 'InstantArray' } & MetadataInstantArrayValue | { type: 'NonFungibleGlobalId' } & MetadataNonFungibleGlobalIdValue | { type: 'NonFungibleGlobalIdArray' } & MetadataNonFungibleGlobalIdArrayValue | { type: 'NonFungibleLocalId' } & MetadataNonFungibleLocalIdValue | { type: 'NonFungibleLocalIdArray' } & MetadataNonFungibleLocalIdArrayValue | { type: 'Origin' } & MetadataOriginValue | { type: 'OriginArray' } & MetadataOriginArrayValue | { type: 'PublicKey' } & MetadataPublicKeyValue | { type: 'PublicKeyArray' } & MetadataPublicKeyArrayValue | { type: 'PublicKeyHash' } & MetadataPublicKeyHashValue | { type: 'PublicKeyHashArray' } & MetadataPublicKeyHashArrayValue | { type: 'String' } & MetadataStringValue | { type: 'StringArray' } & MetadataStringArrayValue | { type: 'U32' } & MetadataU32Value | { type: 'U32Array' } & MetadataU32ArrayValue | { type: 'U64' } & MetadataU64Value | { type: 'U64Array' } & MetadataU64ArrayValue | { type: 'U8' } & MetadataU8Value | { type: 'U8Array' } & MetadataU8ArrayValue | { type: 'Url' } & MetadataUrlValue | { type: 'UrlArray' } & MetadataUrlArrayValue;

export function MetadataTypedValueFromJSON(json: any): MetadataTypedValue {
    return MetadataTypedValueFromJSONTyped(json, false);
}

export function MetadataTypedValueFromJSONTyped(json: any, ignoreDiscriminator: boolean): MetadataTypedValue {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    switch (json['type']) {
        case 'Bool':
            return {...MetadataBoolValueFromJSONTyped(json, true), type: 'Bool'};
        case 'BoolArray':
            return {...MetadataBoolArrayValueFromJSONTyped(json, true), type: 'BoolArray'};
        case 'Decimal':
            return {...MetadataDecimalValueFromJSONTyped(json, true), type: 'Decimal'};
        case 'DecimalArray':
            return {...MetadataDecimalArrayValueFromJSONTyped(json, true), type: 'DecimalArray'};
        case 'GlobalAddress':
            return {...MetadataGlobalAddressValueFromJSONTyped(json, true), type: 'GlobalAddress'};
        case 'GlobalAddressArray':
            return {...MetadataGlobalAddressArrayValueFromJSONTyped(json, true), type: 'GlobalAddressArray'};
        case 'I32':
            return {...MetadataI32ValueFromJSONTyped(json, true), type: 'I32'};
        case 'I32Array':
            return {...MetadataI32ArrayValueFromJSONTyped(json, true), type: 'I32Array'};
        case 'I64':
            return {...MetadataI64ValueFromJSONTyped(json, true), type: 'I64'};
        case 'I64Array':
            return {...MetadataI64ArrayValueFromJSONTyped(json, true), type: 'I64Array'};
        case 'Instant':
            return {...MetadataInstantValueFromJSONTyped(json, true), type: 'Instant'};
        case 'InstantArray':
            return {...MetadataInstantArrayValueFromJSONTyped(json, true), type: 'InstantArray'};
        case 'NonFungibleGlobalId':
            return {...MetadataNonFungibleGlobalIdValueFromJSONTyped(json, true), type: 'NonFungibleGlobalId'};
        case 'NonFungibleGlobalIdArray':
            return {...MetadataNonFungibleGlobalIdArrayValueFromJSONTyped(json, true), type: 'NonFungibleGlobalIdArray'};
        case 'NonFungibleLocalId':
            return {...MetadataNonFungibleLocalIdValueFromJSONTyped(json, true), type: 'NonFungibleLocalId'};
        case 'NonFungibleLocalIdArray':
            return {...MetadataNonFungibleLocalIdArrayValueFromJSONTyped(json, true), type: 'NonFungibleLocalIdArray'};
        case 'Origin':
            return {...MetadataOriginValueFromJSONTyped(json, true), type: 'Origin'};
        case 'OriginArray':
            return {...MetadataOriginArrayValueFromJSONTyped(json, true), type: 'OriginArray'};
        case 'PublicKey':
            return {...MetadataPublicKeyValueFromJSONTyped(json, true), type: 'PublicKey'};
        case 'PublicKeyArray':
            return {...MetadataPublicKeyArrayValueFromJSONTyped(json, true), type: 'PublicKeyArray'};
        case 'PublicKeyHash':
            return {...MetadataPublicKeyHashValueFromJSONTyped(json, true), type: 'PublicKeyHash'};
        case 'PublicKeyHashArray':
            return {...MetadataPublicKeyHashArrayValueFromJSONTyped(json, true), type: 'PublicKeyHashArray'};
        case 'String':
            return {...MetadataStringValueFromJSONTyped(json, true), type: 'String'};
        case 'StringArray':
            return {...MetadataStringArrayValueFromJSONTyped(json, true), type: 'StringArray'};
        case 'U32':
            return {...MetadataU32ValueFromJSONTyped(json, true), type: 'U32'};
        case 'U32Array':
            return {...MetadataU32ArrayValueFromJSONTyped(json, true), type: 'U32Array'};
        case 'U64':
            return {...MetadataU64ValueFromJSONTyped(json, true), type: 'U64'};
        case 'U64Array':
            return {...MetadataU64ArrayValueFromJSONTyped(json, true), type: 'U64Array'};
        case 'U8':
            return {...MetadataU8ValueFromJSONTyped(json, true), type: 'U8'};
        case 'U8Array':
            return {...MetadataU8ArrayValueFromJSONTyped(json, true), type: 'U8Array'};
        case 'Url':
            return {...MetadataUrlValueFromJSONTyped(json, true), type: 'Url'};
        case 'UrlArray':
            return {...MetadataUrlArrayValueFromJSONTyped(json, true), type: 'UrlArray'};
        default:
            throw new Error(`No variant of MetadataTypedValue exists with 'type=${json['type']}'`);
    }
}

export function MetadataTypedValueToJSON(value?: MetadataTypedValue | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    switch (value['type']) {
        case 'Bool':
            return MetadataBoolValueToJSON(value);
        case 'BoolArray':
            return MetadataBoolArrayValueToJSON(value);
        case 'Decimal':
            return MetadataDecimalValueToJSON(value);
        case 'DecimalArray':
            return MetadataDecimalArrayValueToJSON(value);
        case 'GlobalAddress':
            return MetadataGlobalAddressValueToJSON(value);
        case 'GlobalAddressArray':
            return MetadataGlobalAddressArrayValueToJSON(value);
        case 'I32':
            return MetadataI32ValueToJSON(value);
        case 'I32Array':
            return MetadataI32ArrayValueToJSON(value);
        case 'I64':
            return MetadataI64ValueToJSON(value);
        case 'I64Array':
            return MetadataI64ArrayValueToJSON(value);
        case 'Instant':
            return MetadataInstantValueToJSON(value);
        case 'InstantArray':
            return MetadataInstantArrayValueToJSON(value);
        case 'NonFungibleGlobalId':
            return MetadataNonFungibleGlobalIdValueToJSON(value);
        case 'NonFungibleGlobalIdArray':
            return MetadataNonFungibleGlobalIdArrayValueToJSON(value);
        case 'NonFungibleLocalId':
            return MetadataNonFungibleLocalIdValueToJSON(value);
        case 'NonFungibleLocalIdArray':
            return MetadataNonFungibleLocalIdArrayValueToJSON(value);
        case 'Origin':
            return MetadataOriginValueToJSON(value);
        case 'OriginArray':
            return MetadataOriginArrayValueToJSON(value);
        case 'PublicKey':
            return MetadataPublicKeyValueToJSON(value);
        case 'PublicKeyArray':
            return MetadataPublicKeyArrayValueToJSON(value);
        case 'PublicKeyHash':
            return MetadataPublicKeyHashValueToJSON(value);
        case 'PublicKeyHashArray':
            return MetadataPublicKeyHashArrayValueToJSON(value);
        case 'String':
            return MetadataStringValueToJSON(value);
        case 'StringArray':
            return MetadataStringArrayValueToJSON(value);
        case 'U32':
            return MetadataU32ValueToJSON(value);
        case 'U32Array':
            return MetadataU32ArrayValueToJSON(value);
        case 'U64':
            return MetadataU64ValueToJSON(value);
        case 'U64Array':
            return MetadataU64ArrayValueToJSON(value);
        case 'U8':
            return MetadataU8ValueToJSON(value);
        case 'U8Array':
            return MetadataU8ArrayValueToJSON(value);
        case 'Url':
            return MetadataUrlValueToJSON(value);
        case 'UrlArray':
            return MetadataUrlArrayValueToJSON(value);
        default:
            throw new Error(`No variant of MetadataTypedValue exists with 'type=${value['type']}'`);
    }

}

