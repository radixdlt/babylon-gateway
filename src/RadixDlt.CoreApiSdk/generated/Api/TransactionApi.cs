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
 * Radix Core API - Babylon (v1.0.0)
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  The default configuration is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function. The node exposes a configuration flag which allows disabling certain endpoints which may be problematic, but monitoring is advised. This configuration parameter is `api.core.flags.enable_unbounded_endpoints` / `RADIXDLT_CORE_API_FLAGS_ENABLE_UNBOUNDED_ENDPOINTS`.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` have high guarantees of forward compatibility in future node versions. We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  Other endpoints may be changed with new node versions carrying protocol-updates, although any breaking changes will be flagged clearly in the corresponding release notes.  All responses may have additional fields added, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects. 
 *
 * The version of the OpenAPI document: 1.0.0
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using RadixDlt.CoreApiSdk.Client;
using RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.CoreApiSdk.Api
{

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ITransactionApiSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Scrypto Call Preview
        /// </summary>
        /// <remarks>
        /// Preview a scrypto function or method call against the latest network state. Returns the result of the scrypto function or method call. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCallPreviewRequest"></param>
        /// <returns>TransactionCallPreviewResponse</returns>
        TransactionCallPreviewResponse TransactionCallPreviewPost(TransactionCallPreviewRequest transactionCallPreviewRequest);

        /// <summary>
        /// Scrypto Call Preview
        /// </summary>
        /// <remarks>
        /// Preview a scrypto function or method call against the latest network state. Returns the result of the scrypto function or method call. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCallPreviewRequest"></param>
        /// <returns>ApiResponse of TransactionCallPreviewResponse</returns>
        ApiResponse<TransactionCallPreviewResponse> TransactionCallPreviewPostWithHttpInfo(TransactionCallPreviewRequest transactionCallPreviewRequest);
        /// <summary>
        /// Parse Transaction Payload
        /// </summary>
        /// <remarks>
        /// Extracts the contents and hashes of various types of transaction payloads, or sub-payloads.
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionParseRequest"></param>
        /// <returns>TransactionParseResponse</returns>
        TransactionParseResponse TransactionParsePost(TransactionParseRequest transactionParseRequest);

        /// <summary>
        /// Parse Transaction Payload
        /// </summary>
        /// <remarks>
        /// Extracts the contents and hashes of various types of transaction payloads, or sub-payloads.
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionParseRequest"></param>
        /// <returns>ApiResponse of TransactionParseResponse</returns>
        ApiResponse<TransactionParseResponse> TransactionParsePostWithHttpInfo(TransactionParseRequest transactionParseRequest);
        /// <summary>
        /// Transaction Preview
        /// </summary>
        /// <remarks>
        /// Preview a transaction against the latest network state, and returns the preview receipt. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <returns>TransactionPreviewResponse</returns>
        TransactionPreviewResponse TransactionPreviewPost(TransactionPreviewRequest transactionPreviewRequest);

        /// <summary>
        /// Transaction Preview
        /// </summary>
        /// <remarks>
        /// Preview a transaction against the latest network state, and returns the preview receipt. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <returns>ApiResponse of TransactionPreviewResponse</returns>
        ApiResponse<TransactionPreviewResponse> TransactionPreviewPostWithHttpInfo(TransactionPreviewRequest transactionPreviewRequest);
        /// <summary>
        /// Get Transaction Receipt
        /// </summary>
        /// <remarks>
        /// Gets the transaction receipt for a committed transaction. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionReceiptRequest"></param>
        /// <returns>TransactionReceiptResponse</returns>
        TransactionReceiptResponse TransactionReceiptPost(TransactionReceiptRequest transactionReceiptRequest);

        /// <summary>
        /// Get Transaction Receipt
        /// </summary>
        /// <remarks>
        /// Gets the transaction receipt for a committed transaction. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionReceiptRequest"></param>
        /// <returns>ApiResponse of TransactionReceiptResponse</returns>
        ApiResponse<TransactionReceiptResponse> TransactionReceiptPostWithHttpInfo(TransactionReceiptRequest transactionReceiptRequest);
        /// <summary>
        /// Get Transaction Status
        /// </summary>
        /// <remarks>
        /// Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <returns>TransactionStatusResponse</returns>
        TransactionStatusResponse TransactionStatusPost(TransactionStatusRequest transactionStatusRequest);

        /// <summary>
        /// Get Transaction Status
        /// </summary>
        /// <remarks>
        /// Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <returns>ApiResponse of TransactionStatusResponse</returns>
        ApiResponse<TransactionStatusResponse> TransactionStatusPostWithHttpInfo(TransactionStatusRequest transactionStatusRequest);
        /// <summary>
        /// Transaction Submit
        /// </summary>
        /// <remarks>
        /// Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <returns>TransactionSubmitResponse</returns>
        TransactionSubmitResponse TransactionSubmitPost(TransactionSubmitRequest transactionSubmitRequest);

        /// <summary>
        /// Transaction Submit
        /// </summary>
        /// <remarks>
        /// Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <returns>ApiResponse of TransactionSubmitResponse</returns>
        ApiResponse<TransactionSubmitResponse> TransactionSubmitPostWithHttpInfo(TransactionSubmitRequest transactionSubmitRequest);
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ITransactionApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// Scrypto Call Preview
        /// </summary>
        /// <remarks>
        /// Preview a scrypto function or method call against the latest network state. Returns the result of the scrypto function or method call. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCallPreviewRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionCallPreviewResponse</returns>
        System.Threading.Tasks.Task<TransactionCallPreviewResponse> TransactionCallPreviewPostAsync(TransactionCallPreviewRequest transactionCallPreviewRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Scrypto Call Preview
        /// </summary>
        /// <remarks>
        /// Preview a scrypto function or method call against the latest network state. Returns the result of the scrypto function or method call. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCallPreviewRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionCallPreviewResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<TransactionCallPreviewResponse>> TransactionCallPreviewPostWithHttpInfoAsync(TransactionCallPreviewRequest transactionCallPreviewRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Parse Transaction Payload
        /// </summary>
        /// <remarks>
        /// Extracts the contents and hashes of various types of transaction payloads, or sub-payloads.
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionParseRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionParseResponse</returns>
        System.Threading.Tasks.Task<TransactionParseResponse> TransactionParsePostAsync(TransactionParseRequest transactionParseRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Parse Transaction Payload
        /// </summary>
        /// <remarks>
        /// Extracts the contents and hashes of various types of transaction payloads, or sub-payloads.
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionParseRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionParseResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<TransactionParseResponse>> TransactionParsePostWithHttpInfoAsync(TransactionParseRequest transactionParseRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Transaction Preview
        /// </summary>
        /// <remarks>
        /// Preview a transaction against the latest network state, and returns the preview receipt. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionPreviewResponse</returns>
        System.Threading.Tasks.Task<TransactionPreviewResponse> TransactionPreviewPostAsync(TransactionPreviewRequest transactionPreviewRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Transaction Preview
        /// </summary>
        /// <remarks>
        /// Preview a transaction against the latest network state, and returns the preview receipt. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionPreviewResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<TransactionPreviewResponse>> TransactionPreviewPostWithHttpInfoAsync(TransactionPreviewRequest transactionPreviewRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Transaction Receipt
        /// </summary>
        /// <remarks>
        /// Gets the transaction receipt for a committed transaction. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionReceiptRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionReceiptResponse</returns>
        System.Threading.Tasks.Task<TransactionReceiptResponse> TransactionReceiptPostAsync(TransactionReceiptRequest transactionReceiptRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Transaction Receipt
        /// </summary>
        /// <remarks>
        /// Gets the transaction receipt for a committed transaction. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionReceiptRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionReceiptResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<TransactionReceiptResponse>> TransactionReceiptPostWithHttpInfoAsync(TransactionReceiptRequest transactionReceiptRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Transaction Status
        /// </summary>
        /// <remarks>
        /// Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionStatusResponse</returns>
        System.Threading.Tasks.Task<TransactionStatusResponse> TransactionStatusPostAsync(TransactionStatusRequest transactionStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Transaction Status
        /// </summary>
        /// <remarks>
        /// Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionStatusResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<TransactionStatusResponse>> TransactionStatusPostWithHttpInfoAsync(TransactionStatusRequest transactionStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Transaction Submit
        /// </summary>
        /// <remarks>
        /// Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionSubmitResponse</returns>
        System.Threading.Tasks.Task<TransactionSubmitResponse> TransactionSubmitPostAsync(TransactionSubmitRequest transactionSubmitRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Transaction Submit
        /// </summary>
        /// <remarks>
        /// Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionSubmitResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<TransactionSubmitResponse>> TransactionSubmitPostWithHttpInfoAsync(TransactionSubmitRequest transactionSubmitRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
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
        private RadixDlt.CoreApiSdk.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

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
            this.Configuration = RadixDlt.CoreApiSdk.Client.Configuration.MergeConfigurations(
                RadixDlt.CoreApiSdk.Client.GlobalConfiguration.Instance,
                new RadixDlt.CoreApiSdk.Client.Configuration { BasePath = basePath }
            );
            this.ApiClient = new RadixDlt.CoreApiSdk.Client.ApiClient(this.Configuration.BasePath);
            this.Client =  this.ApiClient;
            this.AsynchronousClient = this.ApiClient;
            this.ExceptionFactory = RadixDlt.CoreApiSdk.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionApi"/> class using Configuration object.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="configuration">An instance of Configuration.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public TransactionApi(RadixDlt.CoreApiSdk.Client.Configuration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            this.Configuration = RadixDlt.CoreApiSdk.Client.Configuration.MergeConfigurations(
                RadixDlt.CoreApiSdk.Client.GlobalConfiguration.Instance,
                configuration
            );
            this.ApiClient = new RadixDlt.CoreApiSdk.Client.ApiClient(this.Configuration.BasePath);
            this.Client = this.ApiClient;
            this.AsynchronousClient = this.ApiClient;
            ExceptionFactory = RadixDlt.CoreApiSdk.Client.Configuration.DefaultExceptionFactory;
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

            this.Configuration = RadixDlt.CoreApiSdk.Client.Configuration.MergeConfigurations(
                RadixDlt.CoreApiSdk.Client.GlobalConfiguration.Instance,
                new RadixDlt.CoreApiSdk.Client.Configuration { BasePath = basePath }
            );
            this.ApiClient = new RadixDlt.CoreApiSdk.Client.ApiClient(client, this.Configuration.BasePath, handler);
            this.Client =  this.ApiClient;
            this.AsynchronousClient = this.ApiClient;
            this.ExceptionFactory = RadixDlt.CoreApiSdk.Client.Configuration.DefaultExceptionFactory;
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
        public TransactionApi(HttpClient client, RadixDlt.CoreApiSdk.Client.Configuration configuration, HttpClientHandler handler = null)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (client == null) throw new ArgumentNullException("client");

            this.Configuration = RadixDlt.CoreApiSdk.Client.Configuration.MergeConfigurations(
                RadixDlt.CoreApiSdk.Client.GlobalConfiguration.Instance,
                configuration
            );
            this.ApiClient = new RadixDlt.CoreApiSdk.Client.ApiClient(client, this.Configuration.BasePath, handler);
            this.Client = this.ApiClient;
            this.AsynchronousClient = this.ApiClient;
            ExceptionFactory = RadixDlt.CoreApiSdk.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TransactionApi(RadixDlt.CoreApiSdk.Client.ISynchronousClient client, RadixDlt.CoreApiSdk.Client.IAsynchronousClient asyncClient, RadixDlt.CoreApiSdk.Client.IReadableConfiguration configuration)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (asyncClient == null) throw new ArgumentNullException("asyncClient");
            if (configuration == null) throw new ArgumentNullException("configuration");

            this.Client = client;
            this.AsynchronousClient = asyncClient;
            this.Configuration = configuration;
            this.ExceptionFactory = RadixDlt.CoreApiSdk.Client.Configuration.DefaultExceptionFactory;
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
        public RadixDlt.CoreApiSdk.Client.ApiClient ApiClient { get; set; } = null;

        /// <summary>
        /// The client for accessing this underlying API asynchronously.
        /// </summary>
        public RadixDlt.CoreApiSdk.Client.IAsynchronousClient AsynchronousClient { get; set; }

        /// <summary>
        /// The client for accessing this underlying API synchronously.
        /// </summary>
        public RadixDlt.CoreApiSdk.Client.ISynchronousClient Client { get; set; }

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
        public RadixDlt.CoreApiSdk.Client.IReadableConfiguration Configuration { get; set; }

        /// <summary>
        /// Provides a factory method hook for the creation of exceptions.
        /// </summary>
        public RadixDlt.CoreApiSdk.Client.ExceptionFactory ExceptionFactory
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
        /// Scrypto Call Preview Preview a scrypto function or method call against the latest network state. Returns the result of the scrypto function or method call. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCallPreviewRequest"></param>
        /// <returns>TransactionCallPreviewResponse</returns>
        public TransactionCallPreviewResponse TransactionCallPreviewPost(TransactionCallPreviewRequest transactionCallPreviewRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionCallPreviewResponse> localVarResponse = TransactionCallPreviewPostWithHttpInfo(transactionCallPreviewRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Scrypto Call Preview Preview a scrypto function or method call against the latest network state. Returns the result of the scrypto function or method call. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCallPreviewRequest"></param>
        /// <returns>ApiResponse of TransactionCallPreviewResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionCallPreviewResponse> TransactionCallPreviewPostWithHttpInfo(TransactionCallPreviewRequest transactionCallPreviewRequest)
        {
            // verify the required parameter 'transactionCallPreviewRequest' is set
            if (transactionCallPreviewRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'transactionCallPreviewRequest' when calling TransactionApi->TransactionCallPreviewPost");

            RadixDlt.CoreApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.CoreApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionCallPreviewRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<TransactionCallPreviewResponse>("/transaction/call-preview", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionCallPreviewPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Scrypto Call Preview Preview a scrypto function or method call against the latest network state. Returns the result of the scrypto function or method call. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCallPreviewRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionCallPreviewResponse</returns>
        public async System.Threading.Tasks.Task<TransactionCallPreviewResponse> TransactionCallPreviewPostAsync(TransactionCallPreviewRequest transactionCallPreviewRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionCallPreviewResponse> localVarResponse = await TransactionCallPreviewPostWithHttpInfoAsync(transactionCallPreviewRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Scrypto Call Preview Preview a scrypto function or method call against the latest network state. Returns the result of the scrypto function or method call. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionCallPreviewRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionCallPreviewResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionCallPreviewResponse>> TransactionCallPreviewPostWithHttpInfoAsync(TransactionCallPreviewRequest transactionCallPreviewRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'transactionCallPreviewRequest' is set
            if (transactionCallPreviewRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'transactionCallPreviewRequest' when calling TransactionApi->TransactionCallPreviewPost");


            RadixDlt.CoreApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.CoreApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };


            var localVarContentType = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionCallPreviewRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<TransactionCallPreviewResponse>("/transaction/call-preview", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionCallPreviewPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Parse Transaction Payload Extracts the contents and hashes of various types of transaction payloads, or sub-payloads.
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionParseRequest"></param>
        /// <returns>TransactionParseResponse</returns>
        public TransactionParseResponse TransactionParsePost(TransactionParseRequest transactionParseRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionParseResponse> localVarResponse = TransactionParsePostWithHttpInfo(transactionParseRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Parse Transaction Payload Extracts the contents and hashes of various types of transaction payloads, or sub-payloads.
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionParseRequest"></param>
        /// <returns>ApiResponse of TransactionParseResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionParseResponse> TransactionParsePostWithHttpInfo(TransactionParseRequest transactionParseRequest)
        {
            // verify the required parameter 'transactionParseRequest' is set
            if (transactionParseRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'transactionParseRequest' when calling TransactionApi->TransactionParsePost");

            RadixDlt.CoreApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.CoreApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionParseRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<TransactionParseResponse>("/transaction/parse", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionParsePost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Parse Transaction Payload Extracts the contents and hashes of various types of transaction payloads, or sub-payloads.
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionParseRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionParseResponse</returns>
        public async System.Threading.Tasks.Task<TransactionParseResponse> TransactionParsePostAsync(TransactionParseRequest transactionParseRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionParseResponse> localVarResponse = await TransactionParsePostWithHttpInfoAsync(transactionParseRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Parse Transaction Payload Extracts the contents and hashes of various types of transaction payloads, or sub-payloads.
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionParseRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionParseResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionParseResponse>> TransactionParsePostWithHttpInfoAsync(TransactionParseRequest transactionParseRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'transactionParseRequest' is set
            if (transactionParseRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'transactionParseRequest' when calling TransactionApi->TransactionParsePost");


            RadixDlt.CoreApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.CoreApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };


            var localVarContentType = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionParseRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<TransactionParseResponse>("/transaction/parse", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionParsePost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Transaction Preview Preview a transaction against the latest network state, and returns the preview receipt. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <returns>TransactionPreviewResponse</returns>
        public TransactionPreviewResponse TransactionPreviewPost(TransactionPreviewRequest transactionPreviewRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionPreviewResponse> localVarResponse = TransactionPreviewPostWithHttpInfo(transactionPreviewRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Transaction Preview Preview a transaction against the latest network state, and returns the preview receipt. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <returns>ApiResponse of TransactionPreviewResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionPreviewResponse> TransactionPreviewPostWithHttpInfo(TransactionPreviewRequest transactionPreviewRequest)
        {
            // verify the required parameter 'transactionPreviewRequest' is set
            if (transactionPreviewRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'transactionPreviewRequest' when calling TransactionApi->TransactionPreviewPost");

            RadixDlt.CoreApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.CoreApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionPreviewRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<TransactionPreviewResponse>("/transaction/preview", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionPreviewPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Transaction Preview Preview a transaction against the latest network state, and returns the preview receipt. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionPreviewResponse</returns>
        public async System.Threading.Tasks.Task<TransactionPreviewResponse> TransactionPreviewPostAsync(TransactionPreviewRequest transactionPreviewRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionPreviewResponse> localVarResponse = await TransactionPreviewPostWithHttpInfoAsync(transactionPreviewRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Transaction Preview Preview a transaction against the latest network state, and returns the preview receipt. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionPreviewRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionPreviewResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionPreviewResponse>> TransactionPreviewPostWithHttpInfoAsync(TransactionPreviewRequest transactionPreviewRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'transactionPreviewRequest' is set
            if (transactionPreviewRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'transactionPreviewRequest' when calling TransactionApi->TransactionPreviewPost");


            RadixDlt.CoreApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.CoreApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };


            var localVarContentType = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionPreviewRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<TransactionPreviewResponse>("/transaction/preview", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionPreviewPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Transaction Receipt Gets the transaction receipt for a committed transaction. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionReceiptRequest"></param>
        /// <returns>TransactionReceiptResponse</returns>
        public TransactionReceiptResponse TransactionReceiptPost(TransactionReceiptRequest transactionReceiptRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionReceiptResponse> localVarResponse = TransactionReceiptPostWithHttpInfo(transactionReceiptRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Transaction Receipt Gets the transaction receipt for a committed transaction. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionReceiptRequest"></param>
        /// <returns>ApiResponse of TransactionReceiptResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionReceiptResponse> TransactionReceiptPostWithHttpInfo(TransactionReceiptRequest transactionReceiptRequest)
        {
            // verify the required parameter 'transactionReceiptRequest' is set
            if (transactionReceiptRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'transactionReceiptRequest' when calling TransactionApi->TransactionReceiptPost");

            RadixDlt.CoreApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.CoreApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionReceiptRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<TransactionReceiptResponse>("/transaction/receipt", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionReceiptPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Transaction Receipt Gets the transaction receipt for a committed transaction. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionReceiptRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionReceiptResponse</returns>
        public async System.Threading.Tasks.Task<TransactionReceiptResponse> TransactionReceiptPostAsync(TransactionReceiptRequest transactionReceiptRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionReceiptResponse> localVarResponse = await TransactionReceiptPostWithHttpInfoAsync(transactionReceiptRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Transaction Receipt Gets the transaction receipt for a committed transaction. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionReceiptRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionReceiptResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionReceiptResponse>> TransactionReceiptPostWithHttpInfoAsync(TransactionReceiptRequest transactionReceiptRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'transactionReceiptRequest' is set
            if (transactionReceiptRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'transactionReceiptRequest' when calling TransactionApi->TransactionReceiptPost");


            RadixDlt.CoreApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.CoreApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };


            var localVarContentType = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionReceiptRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<TransactionReceiptResponse>("/transaction/receipt", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionReceiptPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Transaction Status Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <returns>TransactionStatusResponse</returns>
        public TransactionStatusResponse TransactionStatusPost(TransactionStatusRequest transactionStatusRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionStatusResponse> localVarResponse = TransactionStatusPostWithHttpInfo(transactionStatusRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Transaction Status Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <returns>ApiResponse of TransactionStatusResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionStatusResponse> TransactionStatusPostWithHttpInfo(TransactionStatusRequest transactionStatusRequest)
        {
            // verify the required parameter 'transactionStatusRequest' is set
            if (transactionStatusRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'transactionStatusRequest' when calling TransactionApi->TransactionStatusPost");

            RadixDlt.CoreApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.CoreApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionStatusRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<TransactionStatusResponse>("/transaction/status", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionStatusPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Transaction Status Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionStatusResponse</returns>
        public async System.Threading.Tasks.Task<TransactionStatusResponse> TransactionStatusPostAsync(TransactionStatusRequest transactionStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionStatusResponse> localVarResponse = await TransactionStatusPostWithHttpInfoAsync(transactionStatusRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Transaction Status Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionStatusResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionStatusResponse>> TransactionStatusPostWithHttpInfoAsync(TransactionStatusRequest transactionStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'transactionStatusRequest' is set
            if (transactionStatusRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'transactionStatusRequest' when calling TransactionApi->TransactionStatusPost");


            RadixDlt.CoreApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.CoreApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };


            var localVarContentType = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionStatusRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<TransactionStatusResponse>("/transaction/status", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionStatusPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Transaction Submit Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <returns>TransactionSubmitResponse</returns>
        public TransactionSubmitResponse TransactionSubmitPost(TransactionSubmitRequest transactionSubmitRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionSubmitResponse> localVarResponse = TransactionSubmitPostWithHttpInfo(transactionSubmitRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Transaction Submit Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <returns>ApiResponse of TransactionSubmitResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionSubmitResponse> TransactionSubmitPostWithHttpInfo(TransactionSubmitRequest transactionSubmitRequest)
        {
            // verify the required parameter 'transactionSubmitRequest' is set
            if (transactionSubmitRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'transactionSubmitRequest' when calling TransactionApi->TransactionSubmitPost");

            RadixDlt.CoreApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.CoreApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionSubmitRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<TransactionSubmitResponse>("/transaction/submit", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionSubmitPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Transaction Submit Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of TransactionSubmitResponse</returns>
        public async System.Threading.Tasks.Task<TransactionSubmitResponse> TransactionSubmitPostAsync(TransactionSubmitRequest transactionSubmitRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionSubmitResponse> localVarResponse = await TransactionSubmitPostWithHttpInfoAsync(transactionSubmitRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Transaction Submit Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="transactionSubmitRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (TransactionSubmitResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<TransactionSubmitResponse>> TransactionSubmitPostWithHttpInfoAsync(TransactionSubmitRequest transactionSubmitRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'transactionSubmitRequest' is set
            if (transactionSubmitRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'transactionSubmitRequest' when calling TransactionApi->TransactionSubmitPost");


            RadixDlt.CoreApiSdk.Client.RequestOptions localVarRequestOptions = new RadixDlt.CoreApiSdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };


            var localVarContentType = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = RadixDlt.CoreApiSdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = transactionSubmitRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<TransactionSubmitResponse>("/transaction/submit", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("TransactionSubmitPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

    }
}
