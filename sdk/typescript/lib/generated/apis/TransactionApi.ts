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


import * as runtime from '../runtime';
import type {
  RecentTransactionsRequest,
  RecentTransactionsResponse,
  TransactionDetailsRequest,
  TransactionDetailsResponse,
  TransactionPreviewRequest,
  TransactionPreviewResponse,
  TransactionStatusRequest,
  TransactionStatusResponse,
  TransactionSubmitRequest,
  TransactionSubmitResponse,
} from '../models';
import {
    RecentTransactionsRequestFromJSON,
    RecentTransactionsRequestToJSON,
    RecentTransactionsResponseFromJSON,
    RecentTransactionsResponseToJSON,
    TransactionDetailsRequestFromJSON,
    TransactionDetailsRequestToJSON,
    TransactionDetailsResponseFromJSON,
    TransactionDetailsResponseToJSON,
    TransactionPreviewRequestFromJSON,
    TransactionPreviewRequestToJSON,
    TransactionPreviewResponseFromJSON,
    TransactionPreviewResponseToJSON,
    TransactionStatusRequestFromJSON,
    TransactionStatusRequestToJSON,
    TransactionStatusResponseFromJSON,
    TransactionStatusResponseToJSON,
    TransactionSubmitRequestFromJSON,
    TransactionSubmitRequestToJSON,
    TransactionSubmitResponseFromJSON,
    TransactionSubmitResponseToJSON,
} from '../models';

export interface PreviewTransactionRequest {
    transactionPreviewRequest: TransactionPreviewRequest;
}

export interface RecentTransactionsOperationRequest {
    recentTransactionsRequest: RecentTransactionsRequest;
}

export interface SubmitTransactionRequest {
    transactionSubmitRequest: TransactionSubmitRequest;
}

export interface TransactionDetailsOperationRequest {
    transactionDetailsRequest: TransactionDetailsRequest;
}

export interface TransactionStatusOperationRequest {
    transactionStatusRequest: TransactionStatusRequest;
}

/**
 * 
 */
export class TransactionApi extends runtime.BaseAPI {

    /**
     * Previews transaction against the network. 
     * Preview Transaction
     */
    async previewTransactionRaw(requestParameters: PreviewTransactionRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<TransactionPreviewResponse>> {
        if (requestParameters.transactionPreviewRequest === null || requestParameters.transactionPreviewRequest === undefined) {
            throw new runtime.RequiredError('transactionPreviewRequest','Required parameter requestParameters.transactionPreviewRequest was null or undefined when calling previewTransaction.');
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/transaction/preview`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: TransactionPreviewRequestToJSON(requestParameters.transactionPreviewRequest),
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => TransactionPreviewResponseFromJSON(jsonValue));
    }

    /**
     * Previews transaction against the network. 
     * Preview Transaction
     */
    async previewTransaction(requestParameters: PreviewTransactionRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<TransactionPreviewResponse> {
        const response = await this.previewTransactionRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     * Returns user-initiated transactions which have been succesfully committed to the ledger. The transactions are returned in a paginated format, ordered by most recent. 
     * Get Recent Transactions
     */
    async recentTransactionsRaw(requestParameters: RecentTransactionsOperationRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<RecentTransactionsResponse>> {
        if (requestParameters.recentTransactionsRequest === null || requestParameters.recentTransactionsRequest === undefined) {
            throw new runtime.RequiredError('recentTransactionsRequest','Required parameter requestParameters.recentTransactionsRequest was null or undefined when calling recentTransactions.');
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/transaction/recent`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: RecentTransactionsRequestToJSON(requestParameters.recentTransactionsRequest),
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => RecentTransactionsResponseFromJSON(jsonValue));
    }

    /**
     * Returns user-initiated transactions which have been succesfully committed to the ledger. The transactions are returned in a paginated format, ordered by most recent. 
     * Get Recent Transactions
     */
    async recentTransactions(requestParameters: RecentTransactionsOperationRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<RecentTransactionsResponse> {
        const response = await this.recentTransactionsRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     * Submits a signed transaction payload to the network. The transaction identifier from finalize or submit can then be used to track the transaction status. 
     * Submit Transaction
     */
    async submitTransactionRaw(requestParameters: SubmitTransactionRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<TransactionSubmitResponse>> {
        if (requestParameters.transactionSubmitRequest === null || requestParameters.transactionSubmitRequest === undefined) {
            throw new runtime.RequiredError('transactionSubmitRequest','Required parameter requestParameters.transactionSubmitRequest was null or undefined when calling submitTransaction.');
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/transaction/submit`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: TransactionSubmitRequestToJSON(requestParameters.transactionSubmitRequest),
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => TransactionSubmitResponseFromJSON(jsonValue));
    }

    /**
     * Submits a signed transaction payload to the network. The transaction identifier from finalize or submit can then be used to track the transaction status. 
     * Submit Transaction
     */
    async submitTransaction(requestParameters: SubmitTransactionRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<TransactionSubmitResponse> {
        const response = await this.submitTransactionRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     * Returns the status and contents of the transaction with the given transaction identifier. Transaction identifiers which aren\'t recognised as either belonging to a committed transaction or a transaction submitted through this Network Gateway may return a `TransactionNotFoundError`. Transaction identifiers relating to failed transactions will, after a delay, also be reported as a `TransactionNotFoundError`. 
     * Transaction Details
     */
    async transactionDetailsRaw(requestParameters: TransactionDetailsOperationRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<TransactionDetailsResponse>> {
        if (requestParameters.transactionDetailsRequest === null || requestParameters.transactionDetailsRequest === undefined) {
            throw new runtime.RequiredError('transactionDetailsRequest','Required parameter requestParameters.transactionDetailsRequest was null or undefined when calling transactionDetails.');
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/transaction/details`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: TransactionDetailsRequestToJSON(requestParameters.transactionDetailsRequest),
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => TransactionDetailsResponseFromJSON(jsonValue));
    }

    /**
     * Returns the status and contents of the transaction with the given transaction identifier. Transaction identifiers which aren\'t recognised as either belonging to a committed transaction or a transaction submitted through this Network Gateway may return a `TransactionNotFoundError`. Transaction identifiers relating to failed transactions will, after a delay, also be reported as a `TransactionNotFoundError`. 
     * Transaction Details
     */
    async transactionDetails(requestParameters: TransactionDetailsOperationRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<TransactionDetailsResponse> {
        const response = await this.transactionDetailsRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     * Returns the status and contents of the transaction with the given transaction identifier. Transaction identifiers which aren\'t recognised as either belonging to a committed transaction or a transaction submitted through this Network Gateway may return a `TransactionNotFoundError`. Transaction identifiers relating to failed transactions will, after a delay, also be reported as a `TransactionNotFoundError`. 
     * Transaction Status
     */
    async transactionStatusRaw(requestParameters: TransactionStatusOperationRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<TransactionStatusResponse>> {
        if (requestParameters.transactionStatusRequest === null || requestParameters.transactionStatusRequest === undefined) {
            throw new runtime.RequiredError('transactionStatusRequest','Required parameter requestParameters.transactionStatusRequest was null or undefined when calling transactionStatus.');
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/transaction/status`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: TransactionStatusRequestToJSON(requestParameters.transactionStatusRequest),
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => TransactionStatusResponseFromJSON(jsonValue));
    }

    /**
     * Returns the status and contents of the transaction with the given transaction identifier. Transaction identifiers which aren\'t recognised as either belonging to a committed transaction or a transaction submitted through this Network Gateway may return a `TransactionNotFoundError`. Transaction identifiers relating to failed transactions will, after a delay, also be reported as a `TransactionNotFoundError`. 
     * Transaction Status
     */
    async transactionStatus(requestParameters: TransactionStatusOperationRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<TransactionStatusResponse> {
        const response = await this.transactionStatusRaw(requestParameters, initOverrides);
        return await response.value();
    }

}
