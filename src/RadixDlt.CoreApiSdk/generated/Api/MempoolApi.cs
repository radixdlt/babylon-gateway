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
 * Radix Core API
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  The default configuration is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function. The node exposes a configuration flag which allows disabling certain endpoints which may be problematic, but monitoring is advised. This configuration parameter is `api.core.flags.enable_unbounded_endpoints` / `RADIXDLT_CORE_API_FLAGS_ENABLE_UNBOUNDED_ENDPOINTS`.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` have high guarantees of forward compatibility in future node versions. We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  Other endpoints may be changed with new node versions carrying protocol-updates, although any breaking changes will be flagged clearly in the corresponding release notes.  All responses may have additional fields added, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects. 
 *
 * The version of the OpenAPI document: v1.2.3
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
    public interface IMempoolApiSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Get Mempool List
        /// </summary>
        /// <remarks>
        /// Returns the hashes of all the transactions currently in the mempool
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolListRequest"></param>
        /// <returns>MempoolListResponse</returns>
        MempoolListResponse MempoolListPost(MempoolListRequest mempoolListRequest);

        /// <summary>
        /// Get Mempool List
        /// </summary>
        /// <remarks>
        /// Returns the hashes of all the transactions currently in the mempool
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolListRequest"></param>
        /// <returns>ApiResponse of MempoolListResponse</returns>
        ApiResponse<MempoolListResponse> MempoolListPostWithHttpInfo(MempoolListRequest mempoolListRequest);
        /// <summary>
        /// Get Mempool Transaction
        /// </summary>
        /// <remarks>
        /// Returns the payload of a transaction currently in the mempool
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolTransactionRequest"></param>
        /// <returns>MempoolTransactionResponse</returns>
        MempoolTransactionResponse MempoolTransactionPost(MempoolTransactionRequest mempoolTransactionRequest);

        /// <summary>
        /// Get Mempool Transaction
        /// </summary>
        /// <remarks>
        /// Returns the payload of a transaction currently in the mempool
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolTransactionRequest"></param>
        /// <returns>ApiResponse of MempoolTransactionResponse</returns>
        ApiResponse<MempoolTransactionResponse> MempoolTransactionPostWithHttpInfo(MempoolTransactionRequest mempoolTransactionRequest);
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IMempoolApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// Get Mempool List
        /// </summary>
        /// <remarks>
        /// Returns the hashes of all the transactions currently in the mempool
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolListRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of MempoolListResponse</returns>
        System.Threading.Tasks.Task<MempoolListResponse> MempoolListPostAsync(MempoolListRequest mempoolListRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Mempool List
        /// </summary>
        /// <remarks>
        /// Returns the hashes of all the transactions currently in the mempool
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolListRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (MempoolListResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<MempoolListResponse>> MempoolListPostWithHttpInfoAsync(MempoolListRequest mempoolListRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Mempool Transaction
        /// </summary>
        /// <remarks>
        /// Returns the payload of a transaction currently in the mempool
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolTransactionRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of MempoolTransactionResponse</returns>
        System.Threading.Tasks.Task<MempoolTransactionResponse> MempoolTransactionPostAsync(MempoolTransactionRequest mempoolTransactionRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Mempool Transaction
        /// </summary>
        /// <remarks>
        /// Returns the payload of a transaction currently in the mempool
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolTransactionRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (MempoolTransactionResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<MempoolTransactionResponse>> MempoolTransactionPostWithHttpInfoAsync(MempoolTransactionRequest mempoolTransactionRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IMempoolApi : IMempoolApiSync, IMempoolApiAsync
    {

    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class MempoolApi : IDisposable, IMempoolApi
    {
        private RadixDlt.CoreApiSdk.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MempoolApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <returns></returns>
        public MempoolApi() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MempoolApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="basePath">The target service's base path in URL format.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public MempoolApi(string basePath)
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
        /// Initializes a new instance of the <see cref="MempoolApi"/> class using Configuration object.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="configuration">An instance of Configuration.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public MempoolApi(RadixDlt.CoreApiSdk.Client.Configuration configuration)
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
        /// Initializes a new instance of the <see cref="MempoolApi"/> class.
        /// </summary>
        /// <param name="client">An instance of HttpClient.</param>
        /// <param name="handler">An optional instance of HttpClientHandler that is used by HttpClient.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        /// <remarks>
        /// Some configuration settings will not be applied without passing an HttpClientHandler.
        /// The features affected are: Setting and Retrieving Cookies, Client Certificates, Proxy settings.
        /// </remarks>
        public MempoolApi(HttpClient client, HttpClientHandler handler = null) : this(client, (string)null, handler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MempoolApi"/> class.
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
        public MempoolApi(HttpClient client, string basePath, HttpClientHandler handler = null)
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
        /// Initializes a new instance of the <see cref="MempoolApi"/> class using Configuration object.
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
        public MempoolApi(HttpClient client, RadixDlt.CoreApiSdk.Client.Configuration configuration, HttpClientHandler handler = null)
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
        /// Initializes a new instance of the <see cref="MempoolApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public MempoolApi(RadixDlt.CoreApiSdk.Client.ISynchronousClient client, RadixDlt.CoreApiSdk.Client.IAsynchronousClient asyncClient, RadixDlt.CoreApiSdk.Client.IReadableConfiguration configuration)
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
        /// Get Mempool List Returns the hashes of all the transactions currently in the mempool
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolListRequest"></param>
        /// <returns>MempoolListResponse</returns>
        public MempoolListResponse MempoolListPost(MempoolListRequest mempoolListRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<MempoolListResponse> localVarResponse = MempoolListPostWithHttpInfo(mempoolListRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Mempool List Returns the hashes of all the transactions currently in the mempool
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolListRequest"></param>
        /// <returns>ApiResponse of MempoolListResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<MempoolListResponse> MempoolListPostWithHttpInfo(MempoolListRequest mempoolListRequest)
        {
            // verify the required parameter 'mempoolListRequest' is set
            if (mempoolListRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'mempoolListRequest' when calling MempoolApi->MempoolListPost");

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

            localVarRequestOptions.Data = mempoolListRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<MempoolListResponse>("/mempool/list", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("MempoolListPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Mempool List Returns the hashes of all the transactions currently in the mempool
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolListRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of MempoolListResponse</returns>
        public async System.Threading.Tasks.Task<MempoolListResponse> MempoolListPostAsync(MempoolListRequest mempoolListRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<MempoolListResponse> localVarResponse = await MempoolListPostWithHttpInfoAsync(mempoolListRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Mempool List Returns the hashes of all the transactions currently in the mempool
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolListRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (MempoolListResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<MempoolListResponse>> MempoolListPostWithHttpInfoAsync(MempoolListRequest mempoolListRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'mempoolListRequest' is set
            if (mempoolListRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'mempoolListRequest' when calling MempoolApi->MempoolListPost");


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

            localVarRequestOptions.Data = mempoolListRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<MempoolListResponse>("/mempool/list", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("MempoolListPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Mempool Transaction Returns the payload of a transaction currently in the mempool
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolTransactionRequest"></param>
        /// <returns>MempoolTransactionResponse</returns>
        public MempoolTransactionResponse MempoolTransactionPost(MempoolTransactionRequest mempoolTransactionRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<MempoolTransactionResponse> localVarResponse = MempoolTransactionPostWithHttpInfo(mempoolTransactionRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Mempool Transaction Returns the payload of a transaction currently in the mempool
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolTransactionRequest"></param>
        /// <returns>ApiResponse of MempoolTransactionResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<MempoolTransactionResponse> MempoolTransactionPostWithHttpInfo(MempoolTransactionRequest mempoolTransactionRequest)
        {
            // verify the required parameter 'mempoolTransactionRequest' is set
            if (mempoolTransactionRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'mempoolTransactionRequest' when calling MempoolApi->MempoolTransactionPost");

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

            localVarRequestOptions.Data = mempoolTransactionRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<MempoolTransactionResponse>("/mempool/transaction", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("MempoolTransactionPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Mempool Transaction Returns the payload of a transaction currently in the mempool
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolTransactionRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of MempoolTransactionResponse</returns>
        public async System.Threading.Tasks.Task<MempoolTransactionResponse> MempoolTransactionPostAsync(MempoolTransactionRequest mempoolTransactionRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<MempoolTransactionResponse> localVarResponse = await MempoolTransactionPostWithHttpInfoAsync(mempoolTransactionRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Mempool Transaction Returns the payload of a transaction currently in the mempool
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="mempoolTransactionRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (MempoolTransactionResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<MempoolTransactionResponse>> MempoolTransactionPostWithHttpInfoAsync(MempoolTransactionRequest mempoolTransactionRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'mempoolTransactionRequest' is set
            if (mempoolTransactionRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'mempoolTransactionRequest' when calling MempoolApi->MempoolTransactionPost");


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

            localVarRequestOptions.Data = mempoolTransactionRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<MempoolTransactionResponse>("/mempool/transaction", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("MempoolTransactionPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

    }
}
