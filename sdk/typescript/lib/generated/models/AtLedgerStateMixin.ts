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
 * @interface AtLedgerStateMixin
 */
export interface AtLedgerStateMixin {
    /**
     * 
     * @type {LedgerStateSelector}
     * @memberof AtLedgerStateMixin
     */
    at_ledger_state?: LedgerStateSelector | null;
}

/**
 * Check if a given object implements the AtLedgerStateMixin interface.
 */
export function instanceOfAtLedgerStateMixin(value: object): boolean {
    let isInstance = true;

    return isInstance;
}

export function AtLedgerStateMixinFromJSON(json: any): AtLedgerStateMixin {
    return AtLedgerStateMixinFromJSONTyped(json, false);
}

export function AtLedgerStateMixinFromJSONTyped(json: any, ignoreDiscriminator: boolean): AtLedgerStateMixin {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'at_ledger_state': !exists(json, 'at_ledger_state') ? undefined : LedgerStateSelectorFromJSON(json['at_ledger_state']),
    };
}

export function AtLedgerStateMixinToJSON(value?: AtLedgerStateMixin | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'at_ledger_state': LedgerStateSelectorToJSON(value.at_ledger_state),
    };
}
