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
 * This API provides endpoints from a node for integration with the Radix ledger.  # Overview  > WARNING > > The Core API is __NOT__ intended to be available on the public web. It is > designed to be accessed in a private network.  The Core API is separated into three: * The **Data API** is a read-only api which allows you to view and sync to the state of the ledger. * The **Construction API** allows you to construct and submit a transaction to the network. * The **Key API** allows you to use the keys managed by the node to sign transactions.  The Core API is a low level API primarily designed for network integrations such as exchanges, ledger analytics providers, or hosted ledger data dashboards where detailed ledger data is required and the integrator can be expected to run their node to provide the Core API for their own consumption.  For a higher level API, see the [Gateway API](https://redocly.github.io/redoc/?url=https://raw.githubusercontent.com/radixdlt/radixdlt-network-gateway/main/generation/gateway-api-spec.yaml).  For node monitoring, see the [System API](https://redocly.github.io/redoc/?url=https://raw.githubusercontent.com/radixdlt/radixdlt/main/radixdlt-core/radixdlt/src/main/java/com/radixdlt/api/system/api.yaml).  ## Rosetta  The Data API and Construction API is inspired from [Rosetta API](https://www.rosetta-api.org/) most notably:   * Use of a JSON-Based RPC protocol on top of HTTP Post requests   * Use of Operations, Amounts, and Identifiers as universal language to   express asset movement for reading and writing  There are a few notable exceptions to note:   * Fetching of ledger data is through a Transaction stream rather than a   Block stream   * Use of `EntityIdentifier` rather than `AccountIdentifier`   * Use of `OperationGroup` rather than `related_operations` to express related   operations   * Construction endpoints perform coin selection on behalf of the caller.   This has the unfortunate effect of not being able to support high frequency   transactions from a single account. This will be addressed in future updates.   * Construction endpoints are online rather than offline as required by Rosetta  Future versions of the api will aim towards a fully-compliant Rosetta API.  ## Enabling Endpoints  All endpoints are enabled when running a node with the exception of two endpoints, each of which need to be manually configured to access: * `/transactions` endpoint must be enabled with configuration `api.transaction.enable=true`. This is because the transactions endpoint requires additional database storage which may not be needed for users who aren't using this endpoint * `/key/sign` endpoint must be enable with configuration `api.sign.enable=true`. This is a potentially dangerous endpoint if accessible publicly so it must be enabled manually.  ## Client Code Generation  We have found success with generating clients against the [api.yaml specification](https://raw.githubusercontent.com/radixdlt/radixdlt/main/radixdlt-core/radixdlt/src/main/java/com/radixdlt/api/core/api.yaml). See https://openapi-generator.tech/ for more details.  The OpenAPI generator only supports openapi version 3.0.0 at present, but you can change 3.1.0 to 3.0.0 in the first line of the spec without affecting generation.  # Data API Flow  The Data API can be used to synchronize a full or partial view of the ledger, transaction by transaction.  ![Data API Flow](https://raw.githubusercontent.com/radixdlt/radixdlt/feature/update-documentation/radixdlt-core/radixdlt/src/main/java/com/radixdlt/api/core/documentation/data_sequence_flow.png)  # Construction API Flow  The Construction API can be used to construct and submit transactions to the network.  ![Construction API Flow](https://raw.githubusercontent.com/radixdlt/radixdlt/feature/open-api/radixdlt-core/radixdlt/src/main/java/com/radixdlt/api/core/documentation/construction_sequence_flow.png)  Unlike the Rosetta Construction API [specification](https://www.rosetta-api.org/docs/construction_api_introduction.html), this Construction API selects UTXOs on behalf of the caller. This has the unfortunate side effect of not being able to support high frequency transactions from a single account due to UTXO conflicts. This will be addressed in a future release. 
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
    public interface IEngineApiSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Get Engine Configuration
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineConfigurationRequest"></param>
        /// <returns>EngineConfigurationResponse</returns>
        EngineConfigurationResponse EngineConfigurationPost(EngineConfigurationRequest engineConfigurationRequest);

        /// <summary>
        /// Get Engine Configuration
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineConfigurationRequest"></param>
        /// <returns>ApiResponse of EngineConfigurationResponse</returns>
        ApiResponse<EngineConfigurationResponse> EngineConfigurationPostWithHttpInfo(EngineConfigurationRequest engineConfigurationRequest);
        /// <summary>
        /// Get Engine Current Status
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineStatusRequest"></param>
        /// <returns>EngineStatusResponse</returns>
        EngineStatusResponse EngineStatusPost(EngineStatusRequest engineStatusRequest);

        /// <summary>
        /// Get Engine Current Status
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineStatusRequest"></param>
        /// <returns>ApiResponse of EngineStatusResponse</returns>
        ApiResponse<EngineStatusResponse> EngineStatusPostWithHttpInfo(EngineStatusRequest engineStatusRequest);
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IEngineApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// Get Engine Configuration
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineConfigurationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of EngineConfigurationResponse</returns>
        System.Threading.Tasks.Task<EngineConfigurationResponse> EngineConfigurationPostAsync(EngineConfigurationRequest engineConfigurationRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Engine Configuration
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineConfigurationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (EngineConfigurationResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<EngineConfigurationResponse>> EngineConfigurationPostWithHttpInfoAsync(EngineConfigurationRequest engineConfigurationRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Engine Current Status
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of EngineStatusResponse</returns>
        System.Threading.Tasks.Task<EngineStatusResponse> EngineStatusPostAsync(EngineStatusRequest engineStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Engine Current Status
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (EngineStatusResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<EngineStatusResponse>> EngineStatusPostWithHttpInfoAsync(EngineStatusRequest engineStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IEngineApi : IEngineApiSync, IEngineApiAsync
    {

    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class EngineApi : IDisposable, IEngineApi
    {
        private RadixDlt.CoreApiSdk.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <returns></returns>
        public EngineApi() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="basePath">The target service's base path in URL format.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public EngineApi(string basePath)
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
        /// Initializes a new instance of the <see cref="EngineApi"/> class using Configuration object.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="configuration">An instance of Configuration.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public EngineApi(RadixDlt.CoreApiSdk.Client.Configuration configuration)
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
        /// Initializes a new instance of the <see cref="EngineApi"/> class.
        /// </summary>
        /// <param name="client">An instance of HttpClient.</param>
        /// <param name="handler">An optional instance of HttpClientHandler that is used by HttpClient.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        /// <remarks>
        /// Some configuration settings will not be applied without passing an HttpClientHandler.
        /// The features affected are: Setting and Retrieving Cookies, Client Certificates, Proxy settings.
        /// </remarks>
        public EngineApi(HttpClient client, HttpClientHandler handler = null) : this(client, (string)null, handler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineApi"/> class.
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
        public EngineApi(HttpClient client, string basePath, HttpClientHandler handler = null)
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
        /// Initializes a new instance of the <see cref="EngineApi"/> class using Configuration object.
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
        public EngineApi(HttpClient client, RadixDlt.CoreApiSdk.Client.Configuration configuration, HttpClientHandler handler = null)
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
        /// Initializes a new instance of the <see cref="EngineApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public EngineApi(RadixDlt.CoreApiSdk.Client.ISynchronousClient client, RadixDlt.CoreApiSdk.Client.IAsynchronousClient asyncClient, RadixDlt.CoreApiSdk.Client.IReadableConfiguration configuration)
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
        /// Get Engine Configuration 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineConfigurationRequest"></param>
        /// <returns>EngineConfigurationResponse</returns>
        public EngineConfigurationResponse EngineConfigurationPost(EngineConfigurationRequest engineConfigurationRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<EngineConfigurationResponse> localVarResponse = EngineConfigurationPostWithHttpInfo(engineConfigurationRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Engine Configuration 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineConfigurationRequest"></param>
        /// <returns>ApiResponse of EngineConfigurationResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<EngineConfigurationResponse> EngineConfigurationPostWithHttpInfo(EngineConfigurationRequest engineConfigurationRequest)
        {
            // verify the required parameter 'engineConfigurationRequest' is set
            if (engineConfigurationRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'engineConfigurationRequest' when calling EngineApi->EngineConfigurationPost");

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

            localVarRequestOptions.Data = engineConfigurationRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<EngineConfigurationResponse>("/engine/configuration", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EngineConfigurationPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Engine Configuration 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineConfigurationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of EngineConfigurationResponse</returns>
        public async System.Threading.Tasks.Task<EngineConfigurationResponse> EngineConfigurationPostAsync(EngineConfigurationRequest engineConfigurationRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<EngineConfigurationResponse> localVarResponse = await EngineConfigurationPostWithHttpInfoAsync(engineConfigurationRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Engine Configuration 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineConfigurationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (EngineConfigurationResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<EngineConfigurationResponse>> EngineConfigurationPostWithHttpInfoAsync(EngineConfigurationRequest engineConfigurationRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'engineConfigurationRequest' is set
            if (engineConfigurationRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'engineConfigurationRequest' when calling EngineApi->EngineConfigurationPost");


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

            localVarRequestOptions.Data = engineConfigurationRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<EngineConfigurationResponse>("/engine/configuration", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EngineConfigurationPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Engine Current Status 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineStatusRequest"></param>
        /// <returns>EngineStatusResponse</returns>
        public EngineStatusResponse EngineStatusPost(EngineStatusRequest engineStatusRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<EngineStatusResponse> localVarResponse = EngineStatusPostWithHttpInfo(engineStatusRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Engine Current Status 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineStatusRequest"></param>
        /// <returns>ApiResponse of EngineStatusResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<EngineStatusResponse> EngineStatusPostWithHttpInfo(EngineStatusRequest engineStatusRequest)
        {
            // verify the required parameter 'engineStatusRequest' is set
            if (engineStatusRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'engineStatusRequest' when calling EngineApi->EngineStatusPost");

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

            localVarRequestOptions.Data = engineStatusRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<EngineStatusResponse>("/engine/status", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EngineStatusPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Engine Current Status 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of EngineStatusResponse</returns>
        public async System.Threading.Tasks.Task<EngineStatusResponse> EngineStatusPostAsync(EngineStatusRequest engineStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<EngineStatusResponse> localVarResponse = await EngineStatusPostWithHttpInfoAsync(engineStatusRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Engine Current Status 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="engineStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (EngineStatusResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<EngineStatusResponse>> EngineStatusPostWithHttpInfoAsync(EngineStatusRequest engineStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'engineStatusRequest' is set
            if (engineStatusRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'engineStatusRequest' when calling EngineApi->EngineStatusPost");


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

            localVarRequestOptions.Data = engineStatusRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<EngineStatusResponse>("/engine/status", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EngineStatusPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

    }
}
