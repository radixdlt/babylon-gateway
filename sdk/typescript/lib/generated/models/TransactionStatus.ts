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


/**
 * 
 * @export
 */
export const TransactionStatus = {
    Unknown: 'unknown',
    CommittedSuccess: 'committed_success',
    CommittedFailure: 'committed_failure',
    Pending: 'pending',
    Rejected: 'rejected'
} as const;
export type TransactionStatus = typeof TransactionStatus[keyof typeof TransactionStatus];


export function TransactionStatusFromJSON(json: any): TransactionStatus {
    return TransactionStatusFromJSONTyped(json, false);
}

export function TransactionStatusFromJSONTyped(json: any, ignoreDiscriminator: boolean): TransactionStatus {
    return json as TransactionStatus;
}

export function TransactionStatusToJSON(value?: TransactionStatus | null): any {
    return value as any;
}

