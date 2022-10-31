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
import type { ValidationErrorsAtPath } from './ValidationErrorsAtPath';
import {
    ValidationErrorsAtPathFromJSON,
    ValidationErrorsAtPathFromJSONTyped,
    ValidationErrorsAtPathToJSON,
} from './ValidationErrorsAtPath';

/**
 * 
 * @export
 * @interface InvalidRequestErrorAllOf
 */
export interface InvalidRequestErrorAllOf {
    /**
     * One or more validation errors which occurred when validating the request.
     * @type {Array<ValidationErrorsAtPath>}
     * @memberof InvalidRequestErrorAllOf
     */
    validation_errors: Array<ValidationErrorsAtPath>;
}

/**
 * Check if a given object implements the InvalidRequestErrorAllOf interface.
 */
export function instanceOfInvalidRequestErrorAllOf(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "validation_errors" in value;

    return isInstance;
}

export function InvalidRequestErrorAllOfFromJSON(json: any): InvalidRequestErrorAllOf {
    return InvalidRequestErrorAllOfFromJSONTyped(json, false);
}

export function InvalidRequestErrorAllOfFromJSONTyped(json: any, ignoreDiscriminator: boolean): InvalidRequestErrorAllOf {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'validation_errors': ((json['validation_errors'] as Array<any>).map(ValidationErrorsAtPathFromJSON)),
    };
}

export function InvalidRequestErrorAllOfToJSON(value?: InvalidRequestErrorAllOf | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'validation_errors': ((value.validation_errors as Array<any>).map(ValidationErrorsAtPathToJSON)),
    };
}

