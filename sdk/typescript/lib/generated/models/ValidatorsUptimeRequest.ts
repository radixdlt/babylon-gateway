/* tslint:disable */
/* eslint-disable */
/**
 * Radix Gateway API - Babylon
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers, and for light queries from front-end dApps. For exchange/asset integrations, back-end dApp integrations, or simple use cases, you should consider using the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  All responses may have additional fields added at any release, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects.  When the Radix protocol is updated, new functionality may be added, and so discriminated unions returned by the API may need to be updated to have new variants added, corresponding to the updated data. Clients may need to update in advance to be able to handle these new variants when a protocol update comes out.  On the very rare occasions we need to make breaking changes to the API, these will be warned in advance with deprecation notices on previous versions. These deprecation notices will include a safe migration path. Deprecation notes or breaking changes will be flagged clearly in release notes for new versions of the Gateway.  The Gateway DB schema is not subject to any compatibility guarantees, and may be changed at any release. DB changes will be flagged in the release notes so clients doing custom DB integrations can prepare. 
 *
 * The version of the OpenAPI document: v1.10.0
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */

import { exists, mapValues } from '../runtime';
import type { LedgerStateSelector } from './LedgerStateSelector';
import {
    LedgerStateSelectorFromJSON,
    LedgerStateSelectorFromJSONTyped,
    LedgerStateSelectorToJSON,
} from './LedgerStateSelector';

/**
 * For `at_ledger_state` and `from_ledger_state` you can use one of `state_version`, `epoch`, `epoch` and `round`, or `timestamp`, but then ongoing epoch will be selected and used for querying data.
i.e for request with `{ "from_state_version" = { "state_version" = 100 }, "at_state_version" = { "state_version" = 300} }` gateway api will check in which epoch transactions with state version 100 and 300 were and then use that as inclusive boundary for request.

 * @export
 * @interface ValidatorsUptimeRequest
 */
export interface ValidatorsUptimeRequest {
    /**
     * 
     * @type {LedgerStateSelector}
     * @memberof ValidatorsUptimeRequest
     */
    at_ledger_state?: LedgerStateSelector | null;
    /**
     * 
     * @type {LedgerStateSelector}
     * @memberof ValidatorsUptimeRequest
     */
    from_ledger_state?: LedgerStateSelector | null;
    /**
     * 
     * @type {Array<string>}
     * @memberof ValidatorsUptimeRequest
     */
    validator_addresses?: Array<string>;
}

/**
 * Check if a given object implements the ValidatorsUptimeRequest interface.
 */
export function instanceOfValidatorsUptimeRequest(value: object): boolean {
    let isInstance = true;

    return isInstance;
}

export function ValidatorsUptimeRequestFromJSON(json: any): ValidatorsUptimeRequest {
    return ValidatorsUptimeRequestFromJSONTyped(json, false);
}

export function ValidatorsUptimeRequestFromJSONTyped(json: any, ignoreDiscriminator: boolean): ValidatorsUptimeRequest {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'at_ledger_state': !exists(json, 'at_ledger_state') ? undefined : LedgerStateSelectorFromJSON(json['at_ledger_state']),
        'from_ledger_state': !exists(json, 'from_ledger_state') ? undefined : LedgerStateSelectorFromJSON(json['from_ledger_state']),
        'validator_addresses': !exists(json, 'validator_addresses') ? undefined : json['validator_addresses'],
    };
}

export function ValidatorsUptimeRequestToJSON(value?: ValidatorsUptimeRequest | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'at_ledger_state': LedgerStateSelectorToJSON(value.at_ledger_state),
        'from_ledger_state': LedgerStateSelectorToJSON(value.from_ledger_state),
        'validator_addresses': value.validator_addresses,
    };
}

