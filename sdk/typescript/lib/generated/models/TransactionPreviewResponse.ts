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
 * 
 * @export
 * @interface TransactionPreviewResponse
 */
export interface TransactionPreviewResponse {
    /**
     * 
     * @type {object}
     * @memberof TransactionPreviewResponse
     */
    core_api_response: object;
}

/**
 * Check if a given object implements the TransactionPreviewResponse interface.
 */
export function instanceOfTransactionPreviewResponse(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "core_api_response" in value;

    return isInstance;
}

export function TransactionPreviewResponseFromJSON(json: any): TransactionPreviewResponse {
    return TransactionPreviewResponseFromJSONTyped(json, false);
}

export function TransactionPreviewResponseFromJSONTyped(json: any, ignoreDiscriminator: boolean): TransactionPreviewResponse {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'core_api_response': json['core_api_response'],
    };
}

export function TransactionPreviewResponseToJSON(value?: TransactionPreviewResponse | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'core_api_response': value.core_api_response,
    };
}

