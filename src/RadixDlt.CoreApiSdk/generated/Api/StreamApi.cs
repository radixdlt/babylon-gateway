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
    public interface IStreamApiSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Stream Proofs
        /// </summary>
        /// <remarks>
        /// Returns a stream of proofs committed to the node&#39;s ledger.  NOTE: This endpoint may return different results on different nodes: * Each node may persist different subset of signatures on a given proofs, as long as enough of the validator set has signed. * Inside an epoch, different nodes may receive and persist / keep different proofs, subject to constraints on gaps between proofs.  Proofs during an epoch can also be garbage collected by the node after the fact. Therefore proofs may disappear from this stream.  Some proofs (such as during genesis and protocol update enactment) are created on a node and don&#39;t include signatures.  This stream accepts four different options in the request: * All proofs forward (from state version) * All end-of-epoch proofs (from epoch number) * All end-of-epoch proofs triggering a protocol update * All node-injected proofs enacting genesis or a protocol update (for protocol update name, from state version)  The end-of-epoch proofs can be used to \&quot;trustlessly\&quot; verify the validator set for a given epoch. By tracking the fact that validators for epoch N sign the next validator set for epoch N + 1, this chain of proofs can be used to provide proof of the current validator set from a hardcoded start.  When a validator set is known for a given epoch, this can be used to verify the various transaction hash trees in the epoch, and to prove other data.  NOTE: This endpoint was built after agreeing the new Radix convention for paged APIs. Its models therefore follow the new convention, rather than attempting to align with existing loose Core API conventions. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamProofsRequest"></param>
        /// <returns>StreamProofsResponse</returns>
        StreamProofsResponse StreamProofsPost(StreamProofsRequest streamProofsRequest);

        /// <summary>
        /// Stream Proofs
        /// </summary>
        /// <remarks>
        /// Returns a stream of proofs committed to the node&#39;s ledger.  NOTE: This endpoint may return different results on different nodes: * Each node may persist different subset of signatures on a given proofs, as long as enough of the validator set has signed. * Inside an epoch, different nodes may receive and persist / keep different proofs, subject to constraints on gaps between proofs.  Proofs during an epoch can also be garbage collected by the node after the fact. Therefore proofs may disappear from this stream.  Some proofs (such as during genesis and protocol update enactment) are created on a node and don&#39;t include signatures.  This stream accepts four different options in the request: * All proofs forward (from state version) * All end-of-epoch proofs (from epoch number) * All end-of-epoch proofs triggering a protocol update * All node-injected proofs enacting genesis or a protocol update (for protocol update name, from state version)  The end-of-epoch proofs can be used to \&quot;trustlessly\&quot; verify the validator set for a given epoch. By tracking the fact that validators for epoch N sign the next validator set for epoch N + 1, this chain of proofs can be used to provide proof of the current validator set from a hardcoded start.  When a validator set is known for a given epoch, this can be used to verify the various transaction hash trees in the epoch, and to prove other data.  NOTE: This endpoint was built after agreeing the new Radix convention for paged APIs. Its models therefore follow the new convention, rather than attempting to align with existing loose Core API conventions. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamProofsRequest"></param>
        /// <returns>ApiResponse of StreamProofsResponse</returns>
        ApiResponse<StreamProofsResponse> StreamProofsPostWithHttpInfo(StreamProofsRequest streamProofsRequest);
        /// <summary>
        /// Get Committed Transactions
        /// </summary>
        /// <remarks>
        /// Returns the list of committed transactions. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <returns>StreamTransactionsResponse</returns>
        StreamTransactionsResponse StreamTransactionsPost(StreamTransactionsRequest streamTransactionsRequest);

        /// <summary>
        /// Get Committed Transactions
        /// </summary>
        /// <remarks>
        /// Returns the list of committed transactions. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <returns>ApiResponse of StreamTransactionsResponse</returns>
        ApiResponse<StreamTransactionsResponse> StreamTransactionsPostWithHttpInfo(StreamTransactionsRequest streamTransactionsRequest);
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IStreamApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// Stream Proofs
        /// </summary>
        /// <remarks>
        /// Returns a stream of proofs committed to the node&#39;s ledger.  NOTE: This endpoint may return different results on different nodes: * Each node may persist different subset of signatures on a given proofs, as long as enough of the validator set has signed. * Inside an epoch, different nodes may receive and persist / keep different proofs, subject to constraints on gaps between proofs.  Proofs during an epoch can also be garbage collected by the node after the fact. Therefore proofs may disappear from this stream.  Some proofs (such as during genesis and protocol update enactment) are created on a node and don&#39;t include signatures.  This stream accepts four different options in the request: * All proofs forward (from state version) * All end-of-epoch proofs (from epoch number) * All end-of-epoch proofs triggering a protocol update * All node-injected proofs enacting genesis or a protocol update (for protocol update name, from state version)  The end-of-epoch proofs can be used to \&quot;trustlessly\&quot; verify the validator set for a given epoch. By tracking the fact that validators for epoch N sign the next validator set for epoch N + 1, this chain of proofs can be used to provide proof of the current validator set from a hardcoded start.  When a validator set is known for a given epoch, this can be used to verify the various transaction hash trees in the epoch, and to prove other data.  NOTE: This endpoint was built after agreeing the new Radix convention for paged APIs. Its models therefore follow the new convention, rather than attempting to align with existing loose Core API conventions. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamProofsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StreamProofsResponse</returns>
        System.Threading.Tasks.Task<StreamProofsResponse> StreamProofsPostAsync(StreamProofsRequest streamProofsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Stream Proofs
        /// </summary>
        /// <remarks>
        /// Returns a stream of proofs committed to the node&#39;s ledger.  NOTE: This endpoint may return different results on different nodes: * Each node may persist different subset of signatures on a given proofs, as long as enough of the validator set has signed. * Inside an epoch, different nodes may receive and persist / keep different proofs, subject to constraints on gaps between proofs.  Proofs during an epoch can also be garbage collected by the node after the fact. Therefore proofs may disappear from this stream.  Some proofs (such as during genesis and protocol update enactment) are created on a node and don&#39;t include signatures.  This stream accepts four different options in the request: * All proofs forward (from state version) * All end-of-epoch proofs (from epoch number) * All end-of-epoch proofs triggering a protocol update * All node-injected proofs enacting genesis or a protocol update (for protocol update name, from state version)  The end-of-epoch proofs can be used to \&quot;trustlessly\&quot; verify the validator set for a given epoch. By tracking the fact that validators for epoch N sign the next validator set for epoch N + 1, this chain of proofs can be used to provide proof of the current validator set from a hardcoded start.  When a validator set is known for a given epoch, this can be used to verify the various transaction hash trees in the epoch, and to prove other data.  NOTE: This endpoint was built after agreeing the new Radix convention for paged APIs. Its models therefore follow the new convention, rather than attempting to align with existing loose Core API conventions. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamProofsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StreamProofsResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StreamProofsResponse>> StreamProofsPostWithHttpInfoAsync(StreamProofsRequest streamProofsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Committed Transactions
        /// </summary>
        /// <remarks>
        /// Returns the list of committed transactions. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StreamTransactionsResponse</returns>
        System.Threading.Tasks.Task<StreamTransactionsResponse> StreamTransactionsPostAsync(StreamTransactionsRequest streamTransactionsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Committed Transactions
        /// </summary>
        /// <remarks>
        /// Returns the list of committed transactions. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StreamTransactionsResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StreamTransactionsResponse>> StreamTransactionsPostWithHttpInfoAsync(StreamTransactionsRequest streamTransactionsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
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
        private RadixDlt.CoreApiSdk.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

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
        /// Initializes a new instance of the <see cref="StreamApi"/> class using Configuration object.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="configuration">An instance of Configuration.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public StreamApi(RadixDlt.CoreApiSdk.Client.Configuration configuration)
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
        public StreamApi(HttpClient client, RadixDlt.CoreApiSdk.Client.Configuration configuration, HttpClientHandler handler = null)
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
        /// Initializes a new instance of the <see cref="StreamApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public StreamApi(RadixDlt.CoreApiSdk.Client.ISynchronousClient client, RadixDlt.CoreApiSdk.Client.IAsynchronousClient asyncClient, RadixDlt.CoreApiSdk.Client.IReadableConfiguration configuration)
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
        /// Stream Proofs Returns a stream of proofs committed to the node&#39;s ledger.  NOTE: This endpoint may return different results on different nodes: * Each node may persist different subset of signatures on a given proofs, as long as enough of the validator set has signed. * Inside an epoch, different nodes may receive and persist / keep different proofs, subject to constraints on gaps between proofs.  Proofs during an epoch can also be garbage collected by the node after the fact. Therefore proofs may disappear from this stream.  Some proofs (such as during genesis and protocol update enactment) are created on a node and don&#39;t include signatures.  This stream accepts four different options in the request: * All proofs forward (from state version) * All end-of-epoch proofs (from epoch number) * All end-of-epoch proofs triggering a protocol update * All node-injected proofs enacting genesis or a protocol update (for protocol update name, from state version)  The end-of-epoch proofs can be used to \&quot;trustlessly\&quot; verify the validator set for a given epoch. By tracking the fact that validators for epoch N sign the next validator set for epoch N + 1, this chain of proofs can be used to provide proof of the current validator set from a hardcoded start.  When a validator set is known for a given epoch, this can be used to verify the various transaction hash trees in the epoch, and to prove other data.  NOTE: This endpoint was built after agreeing the new Radix convention for paged APIs. Its models therefore follow the new convention, rather than attempting to align with existing loose Core API conventions. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamProofsRequest"></param>
        /// <returns>StreamProofsResponse</returns>
        public StreamProofsResponse StreamProofsPost(StreamProofsRequest streamProofsRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StreamProofsResponse> localVarResponse = StreamProofsPostWithHttpInfo(streamProofsRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Stream Proofs Returns a stream of proofs committed to the node&#39;s ledger.  NOTE: This endpoint may return different results on different nodes: * Each node may persist different subset of signatures on a given proofs, as long as enough of the validator set has signed. * Inside an epoch, different nodes may receive and persist / keep different proofs, subject to constraints on gaps between proofs.  Proofs during an epoch can also be garbage collected by the node after the fact. Therefore proofs may disappear from this stream.  Some proofs (such as during genesis and protocol update enactment) are created on a node and don&#39;t include signatures.  This stream accepts four different options in the request: * All proofs forward (from state version) * All end-of-epoch proofs (from epoch number) * All end-of-epoch proofs triggering a protocol update * All node-injected proofs enacting genesis or a protocol update (for protocol update name, from state version)  The end-of-epoch proofs can be used to \&quot;trustlessly\&quot; verify the validator set for a given epoch. By tracking the fact that validators for epoch N sign the next validator set for epoch N + 1, this chain of proofs can be used to provide proof of the current validator set from a hardcoded start.  When a validator set is known for a given epoch, this can be used to verify the various transaction hash trees in the epoch, and to prove other data.  NOTE: This endpoint was built after agreeing the new Radix convention for paged APIs. Its models therefore follow the new convention, rather than attempting to align with existing loose Core API conventions. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamProofsRequest"></param>
        /// <returns>ApiResponse of StreamProofsResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<StreamProofsResponse> StreamProofsPostWithHttpInfo(StreamProofsRequest streamProofsRequest)
        {
            // verify the required parameter 'streamProofsRequest' is set
            if (streamProofsRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'streamProofsRequest' when calling StreamApi->StreamProofsPost");

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

            localVarRequestOptions.Data = streamProofsRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StreamProofsResponse>("/stream/proofs", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StreamProofsPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Stream Proofs Returns a stream of proofs committed to the node&#39;s ledger.  NOTE: This endpoint may return different results on different nodes: * Each node may persist different subset of signatures on a given proofs, as long as enough of the validator set has signed. * Inside an epoch, different nodes may receive and persist / keep different proofs, subject to constraints on gaps between proofs.  Proofs during an epoch can also be garbage collected by the node after the fact. Therefore proofs may disappear from this stream.  Some proofs (such as during genesis and protocol update enactment) are created on a node and don&#39;t include signatures.  This stream accepts four different options in the request: * All proofs forward (from state version) * All end-of-epoch proofs (from epoch number) * All end-of-epoch proofs triggering a protocol update * All node-injected proofs enacting genesis or a protocol update (for protocol update name, from state version)  The end-of-epoch proofs can be used to \&quot;trustlessly\&quot; verify the validator set for a given epoch. By tracking the fact that validators for epoch N sign the next validator set for epoch N + 1, this chain of proofs can be used to provide proof of the current validator set from a hardcoded start.  When a validator set is known for a given epoch, this can be used to verify the various transaction hash trees in the epoch, and to prove other data.  NOTE: This endpoint was built after agreeing the new Radix convention for paged APIs. Its models therefore follow the new convention, rather than attempting to align with existing loose Core API conventions. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamProofsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StreamProofsResponse</returns>
        public async System.Threading.Tasks.Task<StreamProofsResponse> StreamProofsPostAsync(StreamProofsRequest streamProofsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StreamProofsResponse> localVarResponse = await StreamProofsPostWithHttpInfoAsync(streamProofsRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Stream Proofs Returns a stream of proofs committed to the node&#39;s ledger.  NOTE: This endpoint may return different results on different nodes: * Each node may persist different subset of signatures on a given proofs, as long as enough of the validator set has signed. * Inside an epoch, different nodes may receive and persist / keep different proofs, subject to constraints on gaps between proofs.  Proofs during an epoch can also be garbage collected by the node after the fact. Therefore proofs may disappear from this stream.  Some proofs (such as during genesis and protocol update enactment) are created on a node and don&#39;t include signatures.  This stream accepts four different options in the request: * All proofs forward (from state version) * All end-of-epoch proofs (from epoch number) * All end-of-epoch proofs triggering a protocol update * All node-injected proofs enacting genesis or a protocol update (for protocol update name, from state version)  The end-of-epoch proofs can be used to \&quot;trustlessly\&quot; verify the validator set for a given epoch. By tracking the fact that validators for epoch N sign the next validator set for epoch N + 1, this chain of proofs can be used to provide proof of the current validator set from a hardcoded start.  When a validator set is known for a given epoch, this can be used to verify the various transaction hash trees in the epoch, and to prove other data.  NOTE: This endpoint was built after agreeing the new Radix convention for paged APIs. Its models therefore follow the new convention, rather than attempting to align with existing loose Core API conventions. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamProofsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StreamProofsResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<StreamProofsResponse>> StreamProofsPostWithHttpInfoAsync(StreamProofsRequest streamProofsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'streamProofsRequest' is set
            if (streamProofsRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'streamProofsRequest' when calling StreamApi->StreamProofsPost");


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

            localVarRequestOptions.Data = streamProofsRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StreamProofsResponse>("/stream/proofs", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StreamProofsPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Committed Transactions Returns the list of committed transactions. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <returns>StreamTransactionsResponse</returns>
        public StreamTransactionsResponse StreamTransactionsPost(StreamTransactionsRequest streamTransactionsRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StreamTransactionsResponse> localVarResponse = StreamTransactionsPostWithHttpInfo(streamTransactionsRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Committed Transactions Returns the list of committed transactions. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <returns>ApiResponse of StreamTransactionsResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<StreamTransactionsResponse> StreamTransactionsPostWithHttpInfo(StreamTransactionsRequest streamTransactionsRequest)
        {
            // verify the required parameter 'streamTransactionsRequest' is set
            if (streamTransactionsRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'streamTransactionsRequest' when calling StreamApi->StreamTransactionsPost");

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

            localVarRequestOptions.Data = streamTransactionsRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StreamTransactionsResponse>("/stream/transactions", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StreamTransactionsPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Committed Transactions Returns the list of committed transactions. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StreamTransactionsResponse</returns>
        public async System.Threading.Tasks.Task<StreamTransactionsResponse> StreamTransactionsPostAsync(StreamTransactionsRequest streamTransactionsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StreamTransactionsResponse> localVarResponse = await StreamTransactionsPostWithHttpInfoAsync(streamTransactionsRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Committed Transactions Returns the list of committed transactions. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StreamTransactionsResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<StreamTransactionsResponse>> StreamTransactionsPostWithHttpInfoAsync(StreamTransactionsRequest streamTransactionsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'streamTransactionsRequest' is set
            if (streamTransactionsRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'streamTransactionsRequest' when calling StreamApi->StreamTransactionsPost");


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

            localVarRequestOptions.Data = streamTransactionsRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StreamTransactionsResponse>("/stream/transactions", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StreamTransactionsPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

    }
}
