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
    AccountLockerNotFoundError,
    instanceOfAccountLockerNotFoundError,
    AccountLockerNotFoundErrorFromJSON,
    AccountLockerNotFoundErrorFromJSONTyped,
    AccountLockerNotFoundErrorToJSON,
} from './AccountLockerNotFoundError';
import {
    EntityNotFoundError,
    instanceOfEntityNotFoundError,
    EntityNotFoundErrorFromJSON,
    EntityNotFoundErrorFromJSONTyped,
    EntityNotFoundErrorToJSON,
} from './EntityNotFoundError';
import {
    InternalServerError,
    instanceOfInternalServerError,
    InternalServerErrorFromJSON,
    InternalServerErrorFromJSONTyped,
    InternalServerErrorToJSON,
} from './InternalServerError';
import {
    InvalidEntityError,
    instanceOfInvalidEntityError,
    InvalidEntityErrorFromJSON,
    InvalidEntityErrorFromJSONTyped,
    InvalidEntityErrorToJSON,
} from './InvalidEntityError';
import {
    InvalidRequestError,
    instanceOfInvalidRequestError,
    InvalidRequestErrorFromJSON,
    InvalidRequestErrorFromJSONTyped,
    InvalidRequestErrorToJSON,
} from './InvalidRequestError';
import {
    InvalidTransactionError,
    instanceOfInvalidTransactionError,
    InvalidTransactionErrorFromJSON,
    InvalidTransactionErrorFromJSONTyped,
    InvalidTransactionErrorToJSON,
} from './InvalidTransactionError';
import {
    NotSyncedUpError,
    instanceOfNotSyncedUpError,
    NotSyncedUpErrorFromJSON,
    NotSyncedUpErrorFromJSONTyped,
    NotSyncedUpErrorToJSON,
} from './NotSyncedUpError';
import {
    TransactionNotFoundError,
    instanceOfTransactionNotFoundError,
    TransactionNotFoundErrorFromJSON,
    TransactionNotFoundErrorFromJSONTyped,
    TransactionNotFoundErrorToJSON,
} from './TransactionNotFoundError';

/**
 * @type GatewayError
 * 
 * @export
 */
export type GatewayError = { type: 'AccountLockerNotFoundError' } & AccountLockerNotFoundError | { type: 'EntityNotFoundError' } & EntityNotFoundError | { type: 'InternalServerError' } & InternalServerError | { type: 'InvalidEntityError' } & InvalidEntityError | { type: 'InvalidRequestError' } & InvalidRequestError | { type: 'InvalidTransactionError' } & InvalidTransactionError | { type: 'NotSyncedUpError' } & NotSyncedUpError | { type: 'TransactionNotFoundError' } & TransactionNotFoundError;

export function GatewayErrorFromJSON(json: any): GatewayError {
    return GatewayErrorFromJSONTyped(json, false);
}

export function GatewayErrorFromJSONTyped(json: any, ignoreDiscriminator: boolean): GatewayError {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    switch (json['type']) {
        case 'AccountLockerNotFoundError':
            return {...AccountLockerNotFoundErrorFromJSONTyped(json, true), type: 'AccountLockerNotFoundError'};
        case 'EntityNotFoundError':
            return {...EntityNotFoundErrorFromJSONTyped(json, true), type: 'EntityNotFoundError'};
        case 'InternalServerError':
            return {...InternalServerErrorFromJSONTyped(json, true), type: 'InternalServerError'};
        case 'InvalidEntityError':
            return {...InvalidEntityErrorFromJSONTyped(json, true), type: 'InvalidEntityError'};
        case 'InvalidRequestError':
            return {...InvalidRequestErrorFromJSONTyped(json, true), type: 'InvalidRequestError'};
        case 'InvalidTransactionError':
            return {...InvalidTransactionErrorFromJSONTyped(json, true), type: 'InvalidTransactionError'};
        case 'NotSyncedUpError':
            return {...NotSyncedUpErrorFromJSONTyped(json, true), type: 'NotSyncedUpError'};
        case 'TransactionNotFoundError':
            return {...TransactionNotFoundErrorFromJSONTyped(json, true), type: 'TransactionNotFoundError'};
        default:
            throw new Error(`No variant of GatewayError exists with 'type=${json['type']}'`);
    }
}

export function GatewayErrorToJSON(value?: GatewayError | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    switch (value['type']) {
        case 'AccountLockerNotFoundError':
            return AccountLockerNotFoundErrorToJSON(value);
        case 'EntityNotFoundError':
            return EntityNotFoundErrorToJSON(value);
        case 'InternalServerError':
            return InternalServerErrorToJSON(value);
        case 'InvalidEntityError':
            return InvalidEntityErrorToJSON(value);
        case 'InvalidRequestError':
            return InvalidRequestErrorToJSON(value);
        case 'InvalidTransactionError':
            return InvalidTransactionErrorToJSON(value);
        case 'NotSyncedUpError':
            return NotSyncedUpErrorToJSON(value);
        case 'TransactionNotFoundError':
            return TransactionNotFoundErrorToJSON(value);
        default:
            throw new Error(`No variant of GatewayError exists with 'type=${value['type']}'`);
    }

}

