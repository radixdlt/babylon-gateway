/* tslint:disable */
/* eslint-disable */
/**
 * Radix Babylon Gateway API
 * This API is designed to enable clients to efficiently query information on the RadixDLT ledger, and allow clients to build and submit transactions to the network. It is designed for use by wallets and explorers.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/main/apis/gateway-api.html) to learn more about different usage scenarios.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network. 
 *
 * The version of the OpenAPI document: 0.1.0
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */

import { exists, mapValues } from '../runtime';
import type { CommittedTransactionInfo } from './CommittedTransactionInfo';
import {
    CommittedTransactionInfoFromJSON,
    CommittedTransactionInfoFromJSONTyped,
    CommittedTransactionInfoToJSON,
} from './CommittedTransactionInfo';
import type { LedgerState } from './LedgerState';
import {
    LedgerStateFromJSON,
    LedgerStateFromJSONTyped,
    LedgerStateToJSON,
} from './LedgerState';
import type { TransactionCommittedDetailsResponseDetails } from './TransactionCommittedDetailsResponseDetails';
import {
    TransactionCommittedDetailsResponseDetailsFromJSON,
    TransactionCommittedDetailsResponseDetailsFromJSONTyped,
    TransactionCommittedDetailsResponseDetailsToJSON,
} from './TransactionCommittedDetailsResponseDetails';

/**
 * 
 * @export
 * @interface TransactionCommittedDetailsResponse
 */
export interface TransactionCommittedDetailsResponse {
    /**
     * 
     * @type {LedgerState}
     * @memberof TransactionCommittedDetailsResponse
     */
    ledger_state: LedgerState;
    /**
     * 
     * @type {CommittedTransactionInfo}
     * @memberof TransactionCommittedDetailsResponse
     */
    transaction: CommittedTransactionInfo;
    /**
     * 
     * @type {TransactionCommittedDetailsResponseDetails}
     * @memberof TransactionCommittedDetailsResponse
     */
    details: TransactionCommittedDetailsResponseDetails;
}

/**
 * Check if a given object implements the TransactionCommittedDetailsResponse interface.
 */
export function instanceOfTransactionCommittedDetailsResponse(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "ledger_state" in value;
    isInstance = isInstance && "transaction" in value;
    isInstance = isInstance && "details" in value;

    return isInstance;
}

export function TransactionCommittedDetailsResponseFromJSON(json: any): TransactionCommittedDetailsResponse {
    return TransactionCommittedDetailsResponseFromJSONTyped(json, false);
}

export function TransactionCommittedDetailsResponseFromJSONTyped(json: any, ignoreDiscriminator: boolean): TransactionCommittedDetailsResponse {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'ledger_state': LedgerStateFromJSON(json['ledger_state']),
        'transaction': CommittedTransactionInfoFromJSON(json['transaction']),
        'details': TransactionCommittedDetailsResponseDetailsFromJSON(json['details']),
    };
}

export function TransactionCommittedDetailsResponseToJSON(value?: TransactionCommittedDetailsResponse | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'ledger_state': LedgerStateToJSON(value.ledger_state),
        'transaction': CommittedTransactionInfoToJSON(value.transaction),
        'details': TransactionCommittedDetailsResponseDetailsToJSON(value.details),
    };
}

