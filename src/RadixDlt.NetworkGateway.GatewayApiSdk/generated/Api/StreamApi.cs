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
 * The version of the OpenAPI document: v1.7.2
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
    public interface IStreamApiSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Get Transactions Stream
        /// </summary>
        /// <remarks>
        /// Returns transactions which have been committed to the ledger. [Check detailed documentation for brief explanation](#section/Using-the-streamtransactions-endpoint) 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <returns>StreamTransactionsResponse</returns>
        StreamTransactionsResponse StreamTransactions(StreamTransactionsRequest streamTransactionsRequest);

        /// <summary>
        /// Get Transactions Stream
        /// </summary>
        /// <remarks>
        /// Returns transactions which have been committed to the ledger. [Check detailed documentation for brief explanation](#section/Using-the-streamtransactions-endpoint) 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <returns>ApiResponse of StreamTransactionsResponse</returns>
        ApiResponse<StreamTransactionsResponse> StreamTransactionsWithHttpInfo(StreamTransactionsRequest streamTransactionsRequest);
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IStreamApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// Get Transactions Stream
        /// </summary>
        /// <remarks>
        /// Returns transactions which have been committed to the ledger. [Check detailed documentation for brief explanation](#section/Using-the-streamtransactions-endpoint) 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StreamTransactionsResponse</returns>
        System.Threading.Tasks.Task<StreamTransactionsResponse> StreamTransactionsAsync(StreamTransactionsRequest streamTransactionsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Transactions Stream
        /// </summary>
        /// <remarks>
        /// Returns transactions which have been committed to the ledger. [Check detailed documentation for brief explanation](#section/Using-the-streamtransactions-endpoint) 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StreamTransactionsResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StreamTransactionsResponse>> StreamTransactionsWithHttpInfoAsync(StreamTransactionsRequest streamTransactionsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IStreamApi : IStreamApiSync, IStreamApiAsync
    {

    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class StreamApi : IDisposable, IStreamApi
    {
        private RadixDlt.NetworkGateway.GatewayApiSdk.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <returns></returns>
        public StreamApi() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="basePath">The target service's base path in URL format.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public StreamApi(string basePath)
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
        /// Initializes a new instance of the <see cref="StreamApi"/> class using Configuration object.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="configuration">An instance of Configuration.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public StreamApi(RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration configuration)
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
        /// Initializes a new instance of the <see cref="StreamApi"/> class.
        /// </summary>
        /// <param name="client">An instance of HttpClient.</param>
        /// <param name="handler">An optional instance of HttpClientHandler that is used by HttpClient.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        /// <remarks>
        /// Some configuration settings will not be applied without passing an HttpClientHandler.
        /// The features affected are: Setting and Retrieving Cookies, Client Certificates, Proxy settings.
        /// </remarks>
        public StreamApi(HttpClient client, HttpClientHandler handler = null) : this(client, (string)null, handler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamApi"/> class.
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
        public StreamApi(HttpClient client, string basePath, HttpClientHandler handler = null)
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
        /// Initializes a new instance of the <see cref="StreamApi"/> class using Configuration object.
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
        public StreamApi(HttpClient client, RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration configuration, HttpClientHandler handler = null)
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
        /// Initializes a new instance of the <see cref="StreamApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public StreamApi(RadixDlt.NetworkGateway.GatewayApiSdk.Client.ISynchronousClient client, RadixDlt.NetworkGateway.GatewayApiSdk.Client.IAsynchronousClient asyncClient, RadixDlt.NetworkGateway.GatewayApiSdk.Client.IReadableConfiguration configuration)
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
        /// Get Transactions Stream Returns transactions which have been committed to the ledger. [Check detailed documentation for brief explanation](#section/Using-the-streamtransactions-endpoint) 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <returns>StreamTransactionsResponse</returns>
        public StreamTransactionsResponse StreamTransactions(StreamTransactionsRequest streamTransactionsRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StreamTransactionsResponse> localVarResponse = StreamTransactionsWithHttpInfo(streamTransactionsRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Transactions Stream Returns transactions which have been committed to the ledger. [Check detailed documentation for brief explanation](#section/Using-the-streamtransactions-endpoint) 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <returns>ApiResponse of StreamTransactionsResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StreamTransactionsResponse> StreamTransactionsWithHttpInfo(StreamTransactionsRequest streamTransactionsRequest)
        {
            // verify the required parameter 'streamTransactionsRequest' is set
            if (streamTransactionsRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'streamTransactionsRequest' when calling StreamApi->StreamTransactions");

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

            localVarRequestOptions.Data = streamTransactionsRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StreamTransactionsResponse>("/stream/transactions", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StreamTransactions", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Transactions Stream Returns transactions which have been committed to the ledger. [Check detailed documentation for brief explanation](#section/Using-the-streamtransactions-endpoint) 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StreamTransactionsResponse</returns>
        public async System.Threading.Tasks.Task<StreamTransactionsResponse> StreamTransactionsAsync(StreamTransactionsRequest streamTransactionsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StreamTransactionsResponse> localVarResponse = await StreamTransactionsWithHttpInfoAsync(streamTransactionsRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Transactions Stream Returns transactions which have been committed to the ledger. [Check detailed documentation for brief explanation](#section/Using-the-streamtransactions-endpoint) 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StreamTransactionsResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StreamTransactionsResponse>> StreamTransactionsWithHttpInfoAsync(StreamTransactionsRequest streamTransactionsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'streamTransactionsRequest' is set
            if (streamTransactionsRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'streamTransactionsRequest' when calling StreamApi->StreamTransactions");


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

            localVarRequestOptions.Data = streamTransactionsRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StreamTransactionsResponse>("/stream/transactions", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StreamTransactions", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

    }
}
