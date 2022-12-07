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
/**
 * The ledger state against which the response was generated. Can be used to detect if the Network Gateway is returning up-to-date information.
 * @export
 * @interface LedgerState
 */
export interface LedgerState {
    /**
     * The name of the network against which the request is made.
     * @type {string}
     * @memberof LedgerState
     */
    network: string;
    /**
     * The state version of the ledger. Each transaction increments the state version by 1.
     * @type {number}
     * @memberof LedgerState
     */
    state_version: number;
    /**
     * The round timestamp of the consensus round when this transaction was committed to ledger. This is not guaranteed to be strictly increasing, as it is computed as an average across the validator set. If this is significantly behind the current timestamp, the Network Gateway is likely reporting out-dated information, or the network has stalled.
     * @type {string}
     * @memberof LedgerState
     */
    proposer_round_timestamp: string;
    /**
     * The epoch number of the ledger at this state version.
     * @type {number}
     * @memberof LedgerState
     */
    epoch: number;
    /**
     * The consensus round in the epoch that this state version was committed in.
     * @type {number}
     * @memberof LedgerState
     */
    round: number;
}

/**
 * Check if a given object implements the LedgerState interface.
 */
export function instanceOfLedgerState(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "network" in value;
    isInstance = isInstance && "state_version" in value;
    isInstance = isInstance && "proposer_round_timestamp" in value;
    isInstance = isInstance && "epoch" in value;
    isInstance = isInstance && "round" in value;

    return isInstance;
}

export function LedgerStateFromJSON(json: any): LedgerState {
    return LedgerStateFromJSONTyped(json, false);
}

export function LedgerStateFromJSONTyped(json: any, ignoreDiscriminator: boolean): LedgerState {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'network': json['network'],
        'state_version': json['state_version'],
        'proposer_round_timestamp': json['proposer_round_timestamp'],
        'epoch': json['epoch'],
        'round': json['round'],
    };
}

export function LedgerStateToJSON(value?: LedgerState | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'network': value.network,
        'state_version': value.state_version,
        'proposer_round_timestamp': value.proposer_round_timestamp,
        'epoch': value.epoch,
        'round': value.round,
    };
}

