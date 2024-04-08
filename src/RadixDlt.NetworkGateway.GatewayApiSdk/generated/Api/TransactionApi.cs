/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

/*
 * Radix Gateway API - Babylon
 *
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers, and for light queries from front-end dApps. For exchange/asset integrations, back-end dApp integrations, or simple use cases, you should consider using the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  All responses may have additional fields added at any release, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects.  When the Radix protocol is updated, new functionality may be added, and so discriminated unions returned by the API may need to be updated to have new variants added, corresponding to the updated data. Clients may need to update in advance to be able to handle these new variants when a protocol update comes out.  On the very rare occasions we need to make breaking changes to the API, these will be warned in advance with deprecation notices on previous versions. These deprecation notices will include a safe migration path. Deprecation notes or breaking changes will be flagged clearly in release notes for new versions of the Gateway.  The Gateway DB schema is not subject to any compatibility guarantees, and may be changed at any release. DB changes will be flagged in the release notes so clients doing custom DB integrations can prepare. 
 *
 * The version of the OpenAPI document: v1.5.1
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using RadixDlt.NetworkGateway.GatewayApiSdk.Client;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Api
{

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ITransactionApiSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// PreValidate deposit of resources to an account
        /// </summary>
        /// <remarks>
        /// Helper endpoint that allows pre-validation if a deposit of certain resources to a given account can succeed or not. It is only meant for pre-validation usage, it does not guarantee that execution will succeed. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="accountDepositPreValidationRequest"></param>
        /// <returns>AccountDepositPreValidationResponse</returns>
        AccountDepositPreValidationResponse AccountDepositPreValidation(AccountDepositPreValidationRequest accountDepositPreValidationRequest);

        /// <summary>
        /// PreValidate deposit of resources to an account
        /// </summary>
        /// <remarks>
        /// Helper endpoint that allows pre-validation if a deposit of certain resources to a given account can succeed or not. It is only meant for pre-validation usage, it does not guarantee that execution will succeed. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="accountDepositPreValidationRequest"></param>
        /// <returns>ApiResponse of AccountDepositPreValidationResponse</returns>
        ApiResponse<AccountDepositPreValidationResponse> AccountDepositPreValidationWithHttpInfo(AccountDepositPreValidationRequest accountDepositPreValidationRequest);
        /// <summary>
        /// Get Committed Transaction Details
        /// </summary>
        /// <remarks>
        /// Returns the committed details and receipt of the transaction for a given transaction identifier. Transaction identifiers which don&#39;t correspond to a committed transaction will return a &#x60;TransactionNotFoundError&#x60;. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCommittedDetailsRequest"></param>
        /// <returns>TransactionCommittedDetailsResponse</returns>
        TransactionCommittedDetailsResponse TransactionCommittedDetails(TransactionCommittedDetailsRequest transactionCommittedDetailsRequest);

        /// <summary>
        /// Get Committed Transaction Details
        /// </summary>
        /// <remarks>
        /// Returns the committed details and receipt of the transaction for a given transaction identifier. Transaction identifiers which don&#39;t correspond to a committed transaction will return a &#x60;TransactionNotFoundError&#x60;. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCommittedDetailsRequest"></param>
        /// <returns>ApiResponse of TransactionCommittedDetailsResponse</returns>
        ApiResponse<TransactionCommittedDetailsResponse> TransactionCommittedDetailsWithHttpInfo(TransactionCommittedDetailsRequest transactionCommittedDetailsRequest);
        /// <summary>
        /// Get Construction Metadata
        /// </summary>
        /// <remarks>
        /// Returns information needed to construct a new transaction including current &#x60;epoch&#x60; number. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>TransactionConstructionResponse</returns>
        TransactionConstructionResponse TransactionConstruction();

        /// <summary>
        /// Get Construction Metadata
        /// </summary>
        /// <remarks>
        /// Returns information needed to construct a new transaction including current &#x60;epoch&#x60; number. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of TransactionConstructionResponse</returns>
        ApiResponse<TransactionConstructionResponse> TransactionConstructionWithHttpInfo();
        /// <summary>
        /// Preview Transaction
        /// </summary>
        /// <remarks>
        /// Previews transaction against the network. This endpoint is effectively a proxy towards the Core API &#x60;/v0/transaction/preview&#x60; endpoint. See the Core API documentation for more details. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <returns>TransactionPreviewResponse</returns>
        TransactionPreviewResponse TransactionPreview(TransactionPreviewRequest transactionPreviewRequest);

        /// <summary>
        /// Preview Transaction
        /// </summary>
        /// <remarks>
        /// Previews transaction against the network. This endpoint is effectively a proxy towards the Core API &#x60;/v0/transaction/preview&#x60; endpoint. See the Core API documentation for more details. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <returns>ApiResponse of TransactionPreviewResponse</returns>
        ApiResponse<TransactionPreviewResponse> TransactionPreviewWithHttpInfo(TransactionPreviewRequest transactionPreviewRequest);
        /// <summary>
        /// Get Transaction Status
        /// </summary>
        /// <remarks>
        /// Returns overall transaction status and all of its known payloads based on supplied intent hash. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <returns>TransactionStatusResponse</returns>
        TransactionStatusResponse TransactionStatus(TransactionStatusRequest transactionStatusRequest);

        /// <summary>
        /// Get Transaction Status
        /// </summary>
        /// <remarks>
        /// Returns overall transaction status and all of its known payloads based on supplied intent hash. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <returns>ApiResponse of TransactionStatusResponse</returns>
        ApiResponse<TransactionStatusResponse> TransactionStatusWithHttpInfo(TransactionStatusRequest transactionStatusRequest);
        /// <summary>
        /// Submit Transaction
        /// </summary>
        /// <remarks>
        /// Submits a signed transaction payload to the network. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <returns>TransactionSubmitResponse</returns>
        TransactionSubmitResponse TransactionSubmit(TransactionSubmitRequest transactionSubmitRequest);

        /// <summary>
        /// Submit Transaction
        /// </summary>
        /// <remarks>
        /// Submits a signed transaction payload to the network. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <returns>ApiResponse of TransactionSubmitResponse</returns>
        ApiResponse<TransactionSubmitResponse> TransactionSubmitWithHttpInfo(TransactionSubmitRequest transactionSubmitRequest);
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ITransactionApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// PreValidate deposit of resources to an account
        /// </summary>
        /// <remarks>
        /// Helper endpoint that allows pre-validation if a deposit of certain resources to a given account can succeed or not. It is only meant for pre-validation usage, it does not guarantee that execution will succeed. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="accountDepositPreValidationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of AccountDepositPreValidationResponse</returns>
        System.Threading.Tasks.Task<AccountDepositPreValidationResponse> AccountDepositPreValidationAsync(AccountDepositPreValidationRequest accountDepositPreValidationRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// PreValidate deposit of resources to an account
        /// </summary>
        /// <remarks>
        /// Helper endpoint that allows pre-validation if a deposit of certain resources to a given account can succeed or not. It is only meant for pre-validation usage, it does not guarantee that execution will succeed. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="accountDepositPreValidationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (AccountDepositPreValidationResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<AccountDepositPreValidationResponse>> AccountDepositPreValidationWithHttpInfoAsync(AccountDepositPreValidationRequest accountDepositPreValidationRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Committed Transaction Details
        /// </summary>
        /// <remarks>
        /// Returns the committed details and receipt of the transaction for a given transaction identifier. Transaction identifiers which don&#39;t correspond to a committed transaction will return a &#x60;TransactionNotFoundError&#x60;. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCommittedDetailsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionCommittedDetailsResponse</returns>
        System.Threading.Tasks.Task<TransactionCommittedDetailsResponse> TransactionCommittedDetailsAsync(TransactionCommittedDetailsRequest transactionCommittedDetailsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Committed Transaction Details
        /// </summary>
        /// <remarks>
        /// Returns the committed details and receipt of the transaction for a given transaction identifier. Transaction identifiers which don&#39;t correspond to a committed transaction will return a &#x60;TransactionNotFoundError&#x60;. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCommittedDetailsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionCommittedDetailsResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<TransactionCommittedDetailsResponse>> TransactionCommittedDetailsWithHttpInfoAsync(TransactionCommittedDetailsRequest transactionCommittedDetailsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Construction Metadata
        /// </summary>
        /// <remarks>
        /// Returns information needed to construct a new transaction including current &#x60;epoch&#x60; number. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionConstructionResponse</returns>
        System.Threading.Tasks.Task<TransactionConstructionResponse> TransactionConstructionAsync(System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Construction Metadata
        /// </summary>
        /// <remarks>
        /// Returns information needed to construct a new transaction including current &#x60;epoch&#x60; number. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionConstructionResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<TransactionConstructionResponse>> TransactionConstructionWithHttpInfoAsync(System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Preview Transaction
        /// </summary>
        /// <remarks>
        /// Previews transaction against the network. This endpoint is effectively a proxy towards the Core API &#x60;/v0/transaction/preview&#x60; endpoint. See the Core API documentation for more details. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionPreviewResponse</returns>
        System.Threading.Tasks.Task<TransactionPreviewResponse> TransactionPreviewAsync(TransactionPreviewRequest transactionPreviewRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Preview Transaction
        /// </summary>
        /// <remarks>
        /// Previews transaction against the network. This endpoint is effectively a proxy towards the Core API &#x60;/v0/transaction/preview&#x60; endpoint. See the Core API documentation for more details. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionPreviewResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<TransactionPreviewResponse>> TransactionPreviewWithHttpInfoAsync(TransactionPreviewRequest transactionPreviewRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Transaction Status
        /// </summary>
        /// <remarks>
        /// Returns overall transaction status and all of its known payloads based on supplied intent hash. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionStatusResponse</returns>
        System.Threading.Tasks.Task<TransactionStatusResponse> TransactionStatusAsync(TransactionStatusRequest transactionStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Transaction Status
        /// </summary>
        /// <remarks>
        /// Returns overall transaction status and all of its known payloads based on supplied intent hash. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionStatusResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<TransactionStatusResponse>> TransactionStatusWithHttpInfoAsync(TransactionStatusRequest transactionStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Submit Transaction
        /// </summary>
        /// <remarks>
        /// Submits a signed transaction payload to the network. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionSubmitResponse</returns>
        System.Threading.Tasks.Task<TransactionSubmitResponse> TransactionSubmitAsync(TransactionSubmitRequest transactionSubmitRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Submit Transaction
        /// </summary>
        /// <remarks>
        /// Submits a signed transaction payload to the network. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionSubmitResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<TransactionSubmitResponse>> TransactionSubmitWithHttpInfoAsync(TransactionSubmitRequest transactionSubmitRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ITransactionApi : ITransactionApiSync, ITransactionApiAsync
    {

    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class TransactionApi : IDisposable, ITransactionApi
    {
        private RadixDlt.NetworkGateway.GatewayApiSdk.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <returns></returns>
        public TransactionApi() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="basePath">The target service's base path in URL format.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public TransactionApi(string basePath)
        {
            this.Configuration = RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration.MergeConfigurations(
                RadixDlt.NetworkGateway.GatewayApiSdk.Client.GlobalConfiguration.Instance,
                new RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration { BasePath = basePath }
            );
            this.ApiClient = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiClient(this.Configuration.BasePath);
            this.Client =  this.ApiClient;
            this.AsynchronousClient = this.ApiClient;
            this.ExceptionFactory = RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionApi"/> class using Configuration object.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="configuration">An instance of Configuration.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public TransactionApi(RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            this.Configuration = RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration.MergeConfigurations(
                RadixDlt.NetworkGateway.GatewayApiSdk.Client.GlobalConfiguration.Instance,
                configuration
            );
            this.ApiClient = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiClient(this.Configuration.BasePath);
            this.Client = this.ApiClient;
            this.AsynchronousClient = this.ApiClient;
            ExceptionFactory = RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionApi"/> class.
        /// </summary>
        /// <param name="client">An instance of HttpClient.</param>
        /// <param name="handler">An optional instance of HttpClientHandler that is used by HttpClient.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        /// <remarks>
        /// Some configuration settings will not be applied without passing an HttpClientHandler.
        /// The features affected are: Setting and Retrieving Cookies, Client Certificates, Proxy settings.
        /// </remarks>
        public TransactionApi(HttpClient client, HttpClientHandler handler = null) : this(client, (string)null, handler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionApi"/> class.
        /// </summary>
        /// <param name="client">An instance of HttpClient.</param>
        /// <param name="basePath">The target service's base path in URL format.</param>
        /// <param name="handler">An optional instance of HttpClientHandler that is used by HttpClient.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        /// <remarks>
        /// Some configuration settings will not be applied without passing an HttpClientHandler.
        /// The features affected are: Setting and Retrieving Cookies, Client Certificates, Proxy settings.
        /// </remarks>
        public TransactionApi(HttpClient client, string basePath, HttpClientHandler handler = null)
        {
            if (client == null) throw new ArgumentNullException("client");

            this.Configuration = RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration.MergeConfigurations(
                RadixDlt.NetworkGateway.GatewayApiSdk.Client.GlobalConfiguration.Instance,
                new RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration { BasePath = basePath }
            );
            this.ApiClient = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiClient(client, this.Configuration.BasePath, handler);
            this.Client =  this.ApiClient;
            this.AsynchronousClient = this.ApiClient;
            this.ExceptionFactory = RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionApi"/> class using Configuration object.
        /// </summary>
        /// <param name="client">An instance of HttpClient.</param>
        /// <param name="configuration">An instance of Configuration.</param>
        /// <param name="handler">An optional instance of HttpClientHandler that is used by HttpClient.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        /// <remarks>
        /// Some configuration settings will not be applied without passing an HttpClientHandler.
        /// The features affected are: Setting and Retrieving Cookies, Client Certificates, Proxy settings.
        /// </remarks>
        public TransactionApi(HttpClient client, RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration configuration, HttpClientHandler handler = null)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (client == null) throw new ArgumentNullException("client");

            this.Configuration = RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration.MergeConfigurations(
                RadixDlt.NetworkGateway.GatewayApiSdk.Client.GlobalConfiguration.Instance,
                configuration
            );
            this.ApiClient = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiClient(client, this.Configuration.BasePath, handler);
            this.Client = this.ApiClient;
            this.AsynchronousClient = this.ApiClient;
            ExceptionFactory = RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TransactionApi(RadixDlt.NetworkGateway.GatewayApiSdk.Client.ISynchronousClient client, RadixDlt.NetworkGateway.GatewayApiSdk.Client.IAsynchronousClient asyncClient, RadixDlt.NetworkGateway.GatewayApiSdk.Client.IReadableConfiguration configuration)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (asyncClient == null) throw new ArgumentNullException("asyncClient");
            if (configuration == null) throw new ArgumentNullException("configuration");

            this.Client = client;
            this.AsynchronousClient = asyncClient;
            this.Configuration = configuration;
            this.ExceptionFactory = RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Disposes resources if they were created by us
        /// </summary>
        public void Dispose()
        {
            this.ApiClient?.Dispose();
        }

        /// <summary>
        /// Holds the ApiClient if created
        /// </summary>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiClient ApiClient { get; set; } = null;

        /// <summary>
        /// The client for accessing this underlying API asynchronously.
        /// </summary>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.IAsynchronousClient AsynchronousClient { get; set; }

        /// <summary>
        /// The client for accessing this underlying API synchronously.
        /// </summary>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ISynchronousClient Client { get; set; }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public string GetBasePath()
        {
            return this.Configuration.BasePath;
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.IReadableConfiguration Configuration { get; set; }

        /// <summary>
        /// Provides a factory method hook for the creation of exceptions.
        /// </summary>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ExceptionFactory ExceptionFactory
        {
            get
            {
                if (_exceptionFactory != null && _exceptionFactory.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("Multicast delegate for ExceptionFactory is unsupported.");
                }
                return _exceptionFactory;
            }
            set { _exceptionFactory = value; }
        }

        /// <summary>
        /// PreValidate deposit of resources to an account Helper endpoint that allows pre-validation if a deposit of certain resources to a given account can succeed or not. It is only meant for pre-validation usage, it does not guarantee that execution will succeed. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="accountDepositPreValidationRequest"></param>
        /// <returns>AccountDepositPreValidationResponse</returns>
        public AccountDepositPreValidationResponse AccountDepositPreValidation(AccountDepositPreValidationRequest accountDepositPreValidationRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<AccountDepositPreValidationResponse> localVarResponse = AccountDepositPreValidationWithHttpInfo(accountDepositPreValidationRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// PreValidate deposit of resources to an account Helper endpoint that allows pre-validation if a deposit of certain resources to a given account can succeed or not. It is only meant for pre-validation usage, it does not guarantee that execution will succeed. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="accountDepositPreValidationRequest"></param>
        /// <returns>ApiResponse of AccountDepositPreValidationResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<AccountDepositPreValidationResponse> AccountDepositPreValidationWithHttpInfo(AccountDepositPreValidationRequest accountDepositPreValidationRequest)
        {
            // verify the required parameter 'accountDepositPreValidationRequest' is set
            if (accountDepositPreValidationRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'accountDepositPreValidationRequest' when calling TransactionApi->AccountDepositPreValidation");

            RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = accountDepositPreValidationRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<AccountDepositPreValidationResponse>("/transaction/account-deposit-pre-validation", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("AccountDepositPreValidation", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// PreValidate deposit of resources to an account Helper endpoint that allows pre-validation if a deposit of certain resources to a given account can succeed or not. It is only meant for pre-validation usage, it does not guarantee that execution will succeed. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="accountDepositPreValidationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of AccountDepositPreValidationResponse</returns>
        public async System.Threading.Tasks.Task<AccountDepositPreValidationResponse> AccountDepositPreValidationAsync(AccountDepositPreValidationRequest accountDepositPreValidationRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<AccountDepositPreValidationResponse> localVarResponse = await AccountDepositPreValidationWithHttpInfoAsync(accountDepositPreValidationRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// PreValidate deposit of resources to an account Helper endpoint that allows pre-validation if a deposit of certain resources to a given account can succeed or not. It is only meant for pre-validation usage, it does not guarantee that execution will succeed. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="accountDepositPreValidationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (AccountDepositPreValidationResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<AccountDepositPreValidationResponse>> AccountDepositPreValidationWithHttpInfoAsync(AccountDepositPreValidationRequest accountDepositPreValidationRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'accountDepositPreValidationRequest' is set
            if (accountDepositPreValidationRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'accountDepositPreValidationRequest' when calling TransactionApi->AccountDepositPreValidation");


            RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };


            var localVarContentType = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = accountDepositPreValidationRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<AccountDepositPreValidationResponse>("/transaction/account-deposit-pre-validation", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("AccountDepositPreValidation", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Committed Transaction Details Returns the committed details and receipt of the transaction for a given transaction identifier. Transaction identifiers which don&#39;t correspond to a committed transaction will return a &#x60;TransactionNotFoundError&#x60;. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCommittedDetailsRequest"></param>
        /// <returns>TransactionCommittedDetailsResponse</returns>
        public TransactionCommittedDetailsResponse TransactionCommittedDetails(TransactionCommittedDetailsRequest transactionCommittedDetailsRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionCommittedDetailsResponse> localVarResponse = TransactionCommittedDetailsWithHttpInfo(transactionCommittedDetailsRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Committed Transaction Details Returns the committed details and receipt of the transaction for a given transaction identifier. Transaction identifiers which don&#39;t correspond to a committed transaction will return a &#x60;TransactionNotFoundError&#x60;. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCommittedDetailsRequest"></param>
        /// <returns>ApiResponse of TransactionCommittedDetailsResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionCommittedDetailsResponse> TransactionCommittedDetailsWithHttpInfo(TransactionCommittedDetailsRequest transactionCommittedDetailsRequest)
        {
            // verify the required parameter 'transactionCommittedDetailsRequest' is set
            if (transactionCommittedDetailsRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'transactionCommittedDetailsRequest' when calling TransactionApi->TransactionCommittedDetails");

            RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionCommittedDetailsRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<TransactionCommittedDetailsResponse>("/transaction/committed-details", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionCommittedDetails", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Committed Transaction Details Returns the committed details and receipt of the transaction for a given transaction identifier. Transaction identifiers which don&#39;t correspond to a committed transaction will return a &#x60;TransactionNotFoundError&#x60;. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCommittedDetailsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionCommittedDetailsResponse</returns>
        public async System.Threading.Tasks.Task<TransactionCommittedDetailsResponse> TransactionCommittedDetailsAsync(TransactionCommittedDetailsRequest transactionCommittedDetailsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionCommittedDetailsResponse> localVarResponse = await TransactionCommittedDetailsWithHttpInfoAsync(transactionCommittedDetailsRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Committed Transaction Details Returns the committed details and receipt of the transaction for a given transaction identifier. Transaction identifiers which don&#39;t correspond to a committed transaction will return a &#x60;TransactionNotFoundError&#x60;. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCommittedDetailsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionCommittedDetailsResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionCommittedDetailsResponse>> TransactionCommittedDetailsWithHttpInfoAsync(TransactionCommittedDetailsRequest transactionCommittedDetailsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'transactionCommittedDetailsRequest' is set
            if (transactionCommittedDetailsRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'transactionCommittedDetailsRequest' when calling TransactionApi->TransactionCommittedDetails");


            RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };


            var localVarContentType = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionCommittedDetailsRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<TransactionCommittedDetailsResponse>("/transaction/committed-details", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionCommittedDetails", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Construction Metadata Returns information needed to construct a new transaction including current &#x60;epoch&#x60; number. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>TransactionConstructionResponse</returns>
        public TransactionConstructionResponse TransactionConstruction()
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionConstructionResponse> localVarResponse = TransactionConstructionWithHttpInfo();
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Construction Metadata Returns information needed to construct a new transaction including current &#x60;epoch&#x60; number. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of TransactionConstructionResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionConstructionResponse> TransactionConstructionWithHttpInfo()
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);



            // make the HTTP request
            var localVarResponse = this.Client.Post<TransactionConstructionResponse>("/transaction/construction", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionConstruction", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Construction Metadata Returns information needed to construct a new transaction including current &#x60;epoch&#x60; number. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionConstructionResponse</returns>
        public async System.Threading.Tasks.Task<TransactionConstructionResponse> TransactionConstructionAsync(System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionConstructionResponse> localVarResponse = await TransactionConstructionWithHttpInfoAsync(cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Construction Metadata Returns information needed to construct a new transaction including current &#x60;epoch&#x60; number. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionConstructionResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionConstructionResponse>> TransactionConstructionWithHttpInfoAsync(System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {

            RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };


            var localVarContentType = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);



            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<TransactionConstructionResponse>("/transaction/construction", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionConstruction", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Preview Transaction Previews transaction against the network. This endpoint is effectively a proxy towards the Core API &#x60;/v0/transaction/preview&#x60; endpoint. See the Core API documentation for more details. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <returns>TransactionPreviewResponse</returns>
        public TransactionPreviewResponse TransactionPreview(TransactionPreviewRequest transactionPreviewRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionPreviewResponse> localVarResponse = TransactionPreviewWithHttpInfo(transactionPreviewRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Preview Transaction Previews transaction against the network. This endpoint is effectively a proxy towards the Core API &#x60;/v0/transaction/preview&#x60; endpoint. See the Core API documentation for more details. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <returns>ApiResponse of TransactionPreviewResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionPreviewResponse> TransactionPreviewWithHttpInfo(TransactionPreviewRequest transactionPreviewRequest)
        {
            // verify the required parameter 'transactionPreviewRequest' is set
            if (transactionPreviewRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'transactionPreviewRequest' when calling TransactionApi->TransactionPreview");

            RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionPreviewRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<TransactionPreviewResponse>("/transaction/preview", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionPreview", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Preview Transaction Previews transaction against the network. This endpoint is effectively a proxy towards the Core API &#x60;/v0/transaction/preview&#x60; endpoint. See the Core API documentation for more details. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionPreviewResponse</returns>
        public async System.Threading.Tasks.Task<TransactionPreviewResponse> TransactionPreviewAsync(TransactionPreviewRequest transactionPreviewRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionPreviewResponse> localVarResponse = await TransactionPreviewWithHttpInfoAsync(transactionPreviewRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Preview Transaction Previews transaction against the network. This endpoint is effectively a proxy towards the Core API &#x60;/v0/transaction/preview&#x60; endpoint. See the Core API documentation for more details. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionPreviewResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionPreviewResponse>> TransactionPreviewWithHttpInfoAsync(TransactionPreviewRequest transactionPreviewRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'transactionPreviewRequest' is set
            if (transactionPreviewRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'transactionPreviewRequest' when calling TransactionApi->TransactionPreview");


            RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };


            var localVarContentType = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionPreviewRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<TransactionPreviewResponse>("/transaction/preview", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionPreview", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Transaction Status Returns overall transaction status and all of its known payloads based on supplied intent hash. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <returns>TransactionStatusResponse</returns>
        public TransactionStatusResponse TransactionStatus(TransactionStatusRequest transactionStatusRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionStatusResponse> localVarResponse = TransactionStatusWithHttpInfo(transactionStatusRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Transaction Status Returns overall transaction status and all of its known payloads based on supplied intent hash. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <returns>ApiResponse of TransactionStatusResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionStatusResponse> TransactionStatusWithHttpInfo(TransactionStatusRequest transactionStatusRequest)
        {
            // verify the required parameter 'transactionStatusRequest' is set
            if (transactionStatusRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'transactionStatusRequest' when calling TransactionApi->TransactionStatus");

            RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionStatusRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<TransactionStatusResponse>("/transaction/status", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionStatus", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Transaction Status Returns overall transaction status and all of its known payloads based on supplied intent hash. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionStatusResponse</returns>
        public async System.Threading.Tasks.Task<TransactionStatusResponse> TransactionStatusAsync(TransactionStatusRequest transactionStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionStatusResponse> localVarResponse = await TransactionStatusWithHttpInfoAsync(transactionStatusRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Transaction Status Returns overall transaction status and all of its known payloads based on supplied intent hash. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionStatusResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionStatusResponse>> TransactionStatusWithHttpInfoAsync(TransactionStatusRequest transactionStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'transactionStatusRequest' is set
            if (transactionStatusRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'transactionStatusRequest' when calling TransactionApi->TransactionStatus");


            RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };


            var localVarContentType = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionStatusRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<TransactionStatusResponse>("/transaction/status", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionStatus", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Submit Transaction Submits a signed transaction payload to the network. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <returns>TransactionSubmitResponse</returns>
        public TransactionSubmitResponse TransactionSubmit(TransactionSubmitRequest transactionSubmitRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionSubmitResponse> localVarResponse = TransactionSubmitWithHttpInfo(transactionSubmitRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Submit Transaction Submits a signed transaction payload to the network. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <returns>ApiResponse of TransactionSubmitResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionSubmitResponse> TransactionSubmitWithHttpInfo(TransactionSubmitRequest transactionSubmitRequest)
        {
            // verify the required parameter 'transactionSubmitRequest' is set
            if (transactionSubmitRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'transactionSubmitRequest' when calling TransactionApi->TransactionSubmit");

            RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionSubmitRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<TransactionSubmitResponse>("/transaction/submit", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionSubmit", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Submit Transaction Submits a signed transaction payload to the network. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionSubmitResponse</returns>
        public async System.Threading.Tasks.Task<TransactionSubmitResponse> TransactionSubmitAsync(TransactionSubmitRequest transactionSubmitRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionSubmitResponse> localVarResponse = await TransactionSubmitWithHttpInfoAsync(transactionSubmitRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Submit Transaction Submits a signed transaction payload to the network. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionSubmitResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<TransactionSubmitResponse>> TransactionSubmitWithHttpInfoAsync(TransactionSubmitRequest transactionSubmitRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'transactionSubmitRequest' is set
            if (transactionSubmitRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'transactionSubmitRequest' when calling TransactionApi->TransactionSubmit");


            RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.NetworkGateway.GatewayApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };


            var localVarContentType = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.NetworkGateway.GatewayApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionSubmitRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<TransactionSubmitResponse>("/transaction/submit", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionSubmit", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

    }
}
