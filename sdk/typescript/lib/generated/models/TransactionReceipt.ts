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
import type { CoreApiCostingParameters } from './CoreApiCostingParameters';
import {
    CoreApiCostingParametersFromJSON,
    CoreApiCostingParametersFromJSONTyped,
    CoreApiCostingParametersToJSON,
} from './CoreApiCostingParameters';
import type { CoreApiFeeDestination } from './CoreApiFeeDestination';
import {
    CoreApiFeeDestinationFromJSON,
    CoreApiFeeDestinationFromJSONTyped,
    CoreApiFeeDestinationToJSON,
} from './CoreApiFeeDestination';
import type { CoreApiFeeSource } from './CoreApiFeeSource';
import {
    CoreApiFeeSourceFromJSON,
    CoreApiFeeSourceFromJSONTyped,
    CoreApiFeeSourceToJSON,
} from './CoreApiFeeSource';
import type { CoreApiFeeSummary } from './CoreApiFeeSummary';
import {
    CoreApiFeeSummaryFromJSON,
    CoreApiFeeSummaryFromJSONTyped,
    CoreApiFeeSummaryToJSON,
} from './CoreApiFeeSummary';
import type { CoreApiNextEpoch } from './CoreApiNextEpoch';
import {
    CoreApiNextEpochFromJSON,
    CoreApiNextEpochFromJSONTyped,
    CoreApiNextEpochToJSON,
} from './CoreApiNextEpoch';
import type { CoreApiSborData } from './CoreApiSborData';
import {
    CoreApiSborDataFromJSON,
    CoreApiSborDataFromJSONTyped,
    CoreApiSborDataToJSON,
} from './CoreApiSborData';
import type { CoreApiStateUpdates } from './CoreApiStateUpdates';
import {
    CoreApiStateUpdatesFromJSON,
    CoreApiStateUpdatesFromJSONTyped,
    CoreApiStateUpdatesToJSON,
} from './CoreApiStateUpdates';
import type { EventsItem } from './EventsItem';
import {
    EventsItemFromJSON,
    EventsItemFromJSONTyped,
    EventsItemToJSON,
} from './EventsItem';
import type { TransactionStatus } from './TransactionStatus';
import {
    TransactionStatusFromJSON,
    TransactionStatusFromJSONTyped,
    TransactionStatusToJSON,
} from './TransactionStatus';

/**
 * 
 * @export
 * @interface TransactionReceipt
 */
export interface TransactionReceipt {
    /**
     * 
     * @type {TransactionStatus}
     * @memberof TransactionReceipt
     */
    status: TransactionStatus;
    /**
     * 
     * @type {CoreApiFeeSummary}
     * @memberof TransactionReceipt
     */
    fee_summary?: CoreApiFeeSummary;
    /**
     * 
     * @type {CoreApiCostingParameters}
     * @memberof TransactionReceipt
     */
    costing_parameters?: CoreApiCostingParameters;
    /**
     * 
     * @type {CoreApiFeeSource}
     * @memberof TransactionReceipt
     */
    fee_source?: CoreApiFeeSource;
    /**
     * 
     * @type {CoreApiFeeDestination}
     * @memberof TransactionReceipt
     */
    fee_destination?: CoreApiFeeDestination;
    /**
     * 
     * @type {CoreApiStateUpdates}
     * @memberof TransactionReceipt
     */
    state_updates?: CoreApiStateUpdates;
    /**
     * 
     * @type {CoreApiNextEpoch}
     * @memberof TransactionReceipt
     */
    next_epoch?: CoreApiNextEpoch;
    /**
     * The manifest line-by-line engine return data (only present if `status` is `Succeeded`)
     * @type {Array<CoreApiSborData>}
     * @memberof TransactionReceipt
     */
    output?: Array<CoreApiSborData>;
    /**
     * Events emitted by a transaction.
     * @type {Array<EventsItem>}
     * @memberof TransactionReceipt
     */
    events?: Array<EventsItem>;
    /**
     * Error message (only present if status is `Failed` or `Rejected`)
     * @type {string}
     * @memberof TransactionReceipt
     */
    error_message?: string | null;
}

/**
 * Check if a given object implements the TransactionReceipt interface.
 */
export function instanceOfTransactionReceipt(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "status" in value;

    return isInstance;
}

export function TransactionReceiptFromJSON(json: any): TransactionReceipt {
    return TransactionReceiptFromJSONTyped(json, false);
}

export function TransactionReceiptFromJSONTyped(json: any, ignoreDiscriminator: boolean): TransactionReceipt {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'status': TransactionStatusFromJSON(json['status']),
        'fee_summary': !exists(json, 'fee_summary') ? undefined : CoreApiFeeSummaryFromJSON(json['fee_summary']),
        'costing_parameters': !exists(json, 'costing_parameters') ? undefined : CoreApiCostingParametersFromJSON(json['costing_parameters']),
        'fee_source': !exists(json, 'fee_source') ? undefined : CoreApiFeeSourceFromJSON(json['fee_source']),
        'fee_destination': !exists(json, 'fee_destination') ? undefined : CoreApiFeeDestinationFromJSON(json['fee_destination']),
        'state_updates': !exists(json, 'state_updates') ? undefined : CoreApiStateUpdatesFromJSON(json['state_updates']),
        'next_epoch': !exists(json, 'next_epoch') ? undefined : CoreApiNextEpochFromJSON(json['next_epoch']),
        'output': !exists(json, 'output') ? undefined : ((json['output'] as Array<any>).map(CoreApiSborDataFromJSON)),
        'events': !exists(json, 'events') ? undefined : ((json['events'] as Array<any>).map(EventsItemFromJSON)),
        'error_message': !exists(json, 'error_message') ? undefined : json['error_message'],
    };
}

export function TransactionReceiptToJSON(value?: TransactionReceipt | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'status': TransactionStatusToJSON(value.status),
        'fee_summary': CoreApiFeeSummaryToJSON(value.fee_summary),
        'costing_parameters': CoreApiCostingParametersToJSON(value.costing_parameters),
        'fee_source': CoreApiFeeSourceToJSON(value.fee_source),
        'fee_destination': CoreApiFeeDestinationToJSON(value.fee_destination),
        'state_updates': CoreApiStateUpdatesToJSON(value.state_updates),
        'next_epoch': CoreApiNextEpochToJSON(value.next_epoch),
        'output': value.output === undefined ? undefined : ((value.output as Array<any>).map(CoreApiSborDataToJSON)),
        'events': value.events === undefined ? undefined : ((value.events as Array<any>).map(EventsItemToJSON)),
        'error_message': value.error_message,
    };
}

