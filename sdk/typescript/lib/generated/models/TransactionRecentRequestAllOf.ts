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
import type { LedgerStateSelector } from './LedgerStateSelector';
import {
    LedgerStateSelectorFromJSON,
    LedgerStateSelectorFromJSONTyped,
    LedgerStateSelectorToJSON,
} from './LedgerStateSelector';

/**
 * 
 * @export
 * @interface TransactionRecentRequestAllOf
 */
export interface TransactionRecentRequestAllOf {
    /**
     * 
     * @type {LedgerStateSelector}
     * @memberof TransactionRecentRequestAllOf
     */
    from_ledger_state?: LedgerStateSelector | null;
    /**
     * This cursor allows forward pagination, by providing the cursor from the previous request.
     * @type {string}
     * @memberof TransactionRecentRequestAllOf
     */
    cursor?: string | null;
    /**
     * The page size requested.
     * @type {number}
     * @memberof TransactionRecentRequestAllOf
     */
    limit?: number | null;
}

/**
 * Check if a given object implements the TransactionRecentRequestAllOf interface.
 */
export function instanceOfTransactionRecentRequestAllOf(value: object): boolean {
    let isInstance = true;

    return isInstance;
}

export function TransactionRecentRequestAllOfFromJSON(json: any): TransactionRecentRequestAllOf {
    return TransactionRecentRequestAllOfFromJSONTyped(json, false);
}

export function TransactionRecentRequestAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): TransactionRecentRequestAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'from_ledger_state': !exists(json, 'from_ledger_state') ? undefined : LedgerStateSelectorFromJSON(json['from_ledger_state']),
        'cursor': !exists(json, 'cursor') ? undefined : json['cursor'],
        'limit': !exists(json, 'limit') ? undefined : json['limit'],
    };
}

export function TransactionRecentRequestAllOfToJSON(value?: TransactionRecentRequestAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'from_ledger_state': LedgerStateSelectorToJSON(value.from_ledger_state),
        'cursor': value.cursor,
        'limit': value.limit,
    };
}

