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
 * Babylon Core API - RCnet V2
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  It is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  This version of the Core API belongs to the first release candidate of the Radix Babylon network (\"RCnet-V1\").  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` are guaranteed to be forward compatible to Babylon mainnet launch (and beyond). We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  We give no guarantees that other endpoints will not change before Babylon mainnet launch, although changes are expected to be minimal. 
 *
 * The version of the OpenAPI document: 0.4.0
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
    public interface IStateApiSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Get Access Controller Details
        /// </summary>
        /// <remarks>
        /// Reads the access controller&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateAccessControllerRequest"></param>
        /// <returns>StateAccessControllerResponse</returns>
        StateAccessControllerResponse StateAccessControllerPost(StateAccessControllerRequest stateAccessControllerRequest);

        /// <summary>
        /// Get Access Controller Details
        /// </summary>
        /// <remarks>
        /// Reads the access controller&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateAccessControllerRequest"></param>
        /// <returns>ApiResponse of StateAccessControllerResponse</returns>
        ApiResponse<StateAccessControllerResponse> StateAccessControllerPostWithHttpInfo(StateAccessControllerRequest stateAccessControllerRequest);
        /// <summary>
        /// Get Clock Details
        /// </summary>
        /// <remarks>
        /// Reads the clock&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateClockRequest"></param>
        /// <returns>StateClockResponse</returns>
        StateClockResponse StateClockPost(StateClockRequest stateClockRequest);

        /// <summary>
        /// Get Clock Details
        /// </summary>
        /// <remarks>
        /// Reads the clock&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateClockRequest"></param>
        /// <returns>ApiResponse of StateClockResponse</returns>
        ApiResponse<StateClockResponse> StateClockPostWithHttpInfo(StateClockRequest stateClockRequest);
        /// <summary>
        /// Get Component Details
        /// </summary>
        /// <remarks>
        /// Reads the component&#39;s substate/s from the top of the current ledger. Also recursively extracts vault balance totals from the component&#39;s entity subtree. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateComponentRequest"></param>
        /// <returns>StateComponentResponse</returns>
        StateComponentResponse StateComponentPost(StateComponentRequest stateComponentRequest);

        /// <summary>
        /// Get Component Details
        /// </summary>
        /// <remarks>
        /// Reads the component&#39;s substate/s from the top of the current ledger. Also recursively extracts vault balance totals from the component&#39;s entity subtree. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateComponentRequest"></param>
        /// <returns>ApiResponse of StateComponentResponse</returns>
        ApiResponse<StateComponentResponse> StateComponentPostWithHttpInfo(StateComponentRequest stateComponentRequest);
        /// <summary>
        /// Get Epoch Details
        /// </summary>
        /// <remarks>
        /// Reads the epoch manager&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEpochRequest"></param>
        /// <returns>StateEpochResponse</returns>
        StateEpochResponse StateEpochPost(StateEpochRequest stateEpochRequest);

        /// <summary>
        /// Get Epoch Details
        /// </summary>
        /// <remarks>
        /// Reads the epoch manager&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEpochRequest"></param>
        /// <returns>ApiResponse of StateEpochResponse</returns>
        ApiResponse<StateEpochResponse> StateEpochPostWithHttpInfo(StateEpochRequest stateEpochRequest);
        /// <summary>
        /// Get Non-Fungible Details
        /// </summary>
        /// <remarks>
        /// Reads the data associated with a single Non-Fungible Unit under a Non-Fungible Resource. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleRequest"></param>
        /// <returns>StateNonFungibleResponse</returns>
        StateNonFungibleResponse StateNonFungiblePost(StateNonFungibleRequest stateNonFungibleRequest);

        /// <summary>
        /// Get Non-Fungible Details
        /// </summary>
        /// <remarks>
        /// Reads the data associated with a single Non-Fungible Unit under a Non-Fungible Resource. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleRequest"></param>
        /// <returns>ApiResponse of StateNonFungibleResponse</returns>
        ApiResponse<StateNonFungibleResponse> StateNonFungiblePostWithHttpInfo(StateNonFungibleRequest stateNonFungibleRequest);
        /// <summary>
        /// Get Package Details
        /// </summary>
        /// <remarks>
        /// Reads the package&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="statePackageRequest"></param>
        /// <returns>StatePackageResponse</returns>
        StatePackageResponse StatePackagePost(StatePackageRequest statePackageRequest);

        /// <summary>
        /// Get Package Details
        /// </summary>
        /// <remarks>
        /// Reads the package&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="statePackageRequest"></param>
        /// <returns>ApiResponse of StatePackageResponse</returns>
        ApiResponse<StatePackageResponse> StatePackagePostWithHttpInfo(StatePackageRequest statePackageRequest);
        /// <summary>
        /// Get Resource Details
        /// </summary>
        /// <remarks>
        /// Reads the resource manager&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateResourceRequest"></param>
        /// <returns>StateResourceResponse</returns>
        StateResourceResponse StateResourcePost(StateResourceRequest stateResourceRequest);

        /// <summary>
        /// Get Resource Details
        /// </summary>
        /// <remarks>
        /// Reads the resource manager&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateResourceRequest"></param>
        /// <returns>ApiResponse of StateResourceResponse</returns>
        ApiResponse<StateResourceResponse> StateResourcePostWithHttpInfo(StateResourceRequest stateResourceRequest);
        /// <summary>
        /// Get Validator Details
        /// </summary>
        /// <remarks>
        /// Reads the validator&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorRequest"></param>
        /// <returns>StateValidatorResponse</returns>
        StateValidatorResponse StateValidatorPost(StateValidatorRequest stateValidatorRequest);

        /// <summary>
        /// Get Validator Details
        /// </summary>
        /// <remarks>
        /// Reads the validator&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorRequest"></param>
        /// <returns>ApiResponse of StateValidatorResponse</returns>
        ApiResponse<StateValidatorResponse> StateValidatorPostWithHttpInfo(StateValidatorRequest stateValidatorRequest);
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IStateApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// Get Access Controller Details
        /// </summary>
        /// <remarks>
        /// Reads the access controller&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateAccessControllerRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateAccessControllerResponse</returns>
        System.Threading.Tasks.Task<StateAccessControllerResponse> StateAccessControllerPostAsync(StateAccessControllerRequest stateAccessControllerRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Access Controller Details
        /// </summary>
        /// <remarks>
        /// Reads the access controller&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateAccessControllerRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateAccessControllerResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateAccessControllerResponse>> StateAccessControllerPostWithHttpInfoAsync(StateAccessControllerRequest stateAccessControllerRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Clock Details
        /// </summary>
        /// <remarks>
        /// Reads the clock&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateClockRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateClockResponse</returns>
        System.Threading.Tasks.Task<StateClockResponse> StateClockPostAsync(StateClockRequest stateClockRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Clock Details
        /// </summary>
        /// <remarks>
        /// Reads the clock&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateClockRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateClockResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateClockResponse>> StateClockPostWithHttpInfoAsync(StateClockRequest stateClockRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Component Details
        /// </summary>
        /// <remarks>
        /// Reads the component&#39;s substate/s from the top of the current ledger. Also recursively extracts vault balance totals from the component&#39;s entity subtree. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateComponentRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateComponentResponse</returns>
        System.Threading.Tasks.Task<StateComponentResponse> StateComponentPostAsync(StateComponentRequest stateComponentRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Component Details
        /// </summary>
        /// <remarks>
        /// Reads the component&#39;s substate/s from the top of the current ledger. Also recursively extracts vault balance totals from the component&#39;s entity subtree. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateComponentRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateComponentResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateComponentResponse>> StateComponentPostWithHttpInfoAsync(StateComponentRequest stateComponentRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Epoch Details
        /// </summary>
        /// <remarks>
        /// Reads the epoch manager&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEpochRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEpochResponse</returns>
        System.Threading.Tasks.Task<StateEpochResponse> StateEpochPostAsync(StateEpochRequest stateEpochRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Epoch Details
        /// </summary>
        /// <remarks>
        /// Reads the epoch manager&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEpochRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEpochResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateEpochResponse>> StateEpochPostWithHttpInfoAsync(StateEpochRequest stateEpochRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Non-Fungible Details
        /// </summary>
        /// <remarks>
        /// Reads the data associated with a single Non-Fungible Unit under a Non-Fungible Resource. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateNonFungibleResponse</returns>
        System.Threading.Tasks.Task<StateNonFungibleResponse> StateNonFungiblePostAsync(StateNonFungibleRequest stateNonFungibleRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Non-Fungible Details
        /// </summary>
        /// <remarks>
        /// Reads the data associated with a single Non-Fungible Unit under a Non-Fungible Resource. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateNonFungibleResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateNonFungibleResponse>> StateNonFungiblePostWithHttpInfoAsync(StateNonFungibleRequest stateNonFungibleRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Package Details
        /// </summary>
        /// <remarks>
        /// Reads the package&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="statePackageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StatePackageResponse</returns>
        System.Threading.Tasks.Task<StatePackageResponse> StatePackagePostAsync(StatePackageRequest statePackageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Package Details
        /// </summary>
        /// <remarks>
        /// Reads the package&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="statePackageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StatePackageResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StatePackageResponse>> StatePackagePostWithHttpInfoAsync(StatePackageRequest statePackageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Resource Details
        /// </summary>
        /// <remarks>
        /// Reads the resource manager&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateResourceRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateResourceResponse</returns>
        System.Threading.Tasks.Task<StateResourceResponse> StateResourcePostAsync(StateResourceRequest stateResourceRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Resource Details
        /// </summary>
        /// <remarks>
        /// Reads the resource manager&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateResourceRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateResourceResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateResourceResponse>> StateResourcePostWithHttpInfoAsync(StateResourceRequest stateResourceRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Validator Details
        /// </summary>
        /// <remarks>
        /// Reads the validator&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateValidatorResponse</returns>
        System.Threading.Tasks.Task<StateValidatorResponse> StateValidatorPostAsync(StateValidatorRequest stateValidatorRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Validator Details
        /// </summary>
        /// <remarks>
        /// Reads the validator&#39;s substate/s from the top of the current ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateValidatorResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateValidatorResponse>> StateValidatorPostWithHttpInfoAsync(StateValidatorRequest stateValidatorRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IStateApi : IStateApiSync, IStateApiAsync
    {

    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class StateApi : IDisposable, IStateApi
    {
        private RadixDlt.CoreApiSdk.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <returns></returns>
        public StateApi() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="basePath">The target service's base path in URL format.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public StateApi(string basePath)
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
        /// Initializes a new instance of the <see cref="StateApi"/> class using Configuration object.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="configuration">An instance of Configuration.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public StateApi(RadixDlt.CoreApiSdk.Client.Configuration configuration)
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
        /// Initializes a new instance of the <see cref="StateApi"/> class.
        /// </summary>
        /// <param name="client">An instance of HttpClient.</param>
        /// <param name="handler">An optional instance of HttpClientHandler that is used by HttpClient.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        /// <remarks>
        /// Some configuration settings will not be applied without passing an HttpClientHandler.
        /// The features affected are: Setting and Retrieving Cookies, Client Certificates, Proxy settings.
        /// </remarks>
        public StateApi(HttpClient client, HttpClientHandler handler = null) : this(client, (string)null, handler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateApi"/> class.
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
        public StateApi(HttpClient client, string basePath, HttpClientHandler handler = null)
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
        /// Initializes a new instance of the <see cref="StateApi"/> class using Configuration object.
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
        public StateApi(HttpClient client, RadixDlt.CoreApiSdk.Client.Configuration configuration, HttpClientHandler handler = null)
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
        /// Initializes a new instance of the <see cref="StateApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public StateApi(RadixDlt.CoreApiSdk.Client.ISynchronousClient client, RadixDlt.CoreApiSdk.Client.IAsynchronousClient asyncClient, RadixDlt.CoreApiSdk.Client.IReadableConfiguration configuration)
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
        /// Get Access Controller Details Reads the access controller&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateAccessControllerRequest"></param>
        /// <returns>StateAccessControllerResponse</returns>
        public StateAccessControllerResponse StateAccessControllerPost(StateAccessControllerRequest stateAccessControllerRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StateAccessControllerResponse> localVarResponse = StateAccessControllerPostWithHttpInfo(stateAccessControllerRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Access Controller Details Reads the access controller&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateAccessControllerRequest"></param>
        /// <returns>ApiResponse of StateAccessControllerResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<StateAccessControllerResponse> StateAccessControllerPostWithHttpInfo(StateAccessControllerRequest stateAccessControllerRequest)
        {
            // verify the required parameter 'stateAccessControllerRequest' is set
            if (stateAccessControllerRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'stateAccessControllerRequest' when calling StateApi->StateAccessControllerPost");

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

            localVarRequestOptions.Data = stateAccessControllerRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateAccessControllerResponse>("/state/access-controller", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateAccessControllerPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Access Controller Details Reads the access controller&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateAccessControllerRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateAccessControllerResponse</returns>
        public async System.Threading.Tasks.Task<StateAccessControllerResponse> StateAccessControllerPostAsync(StateAccessControllerRequest stateAccessControllerRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StateAccessControllerResponse> localVarResponse = await StateAccessControllerPostWithHttpInfoAsync(stateAccessControllerRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Access Controller Details Reads the access controller&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateAccessControllerRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateAccessControllerResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<StateAccessControllerResponse>> StateAccessControllerPostWithHttpInfoAsync(StateAccessControllerRequest stateAccessControllerRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateAccessControllerRequest' is set
            if (stateAccessControllerRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'stateAccessControllerRequest' when calling StateApi->StateAccessControllerPost");


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

            localVarRequestOptions.Data = stateAccessControllerRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateAccessControllerResponse>("/state/access-controller", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateAccessControllerPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Clock Details Reads the clock&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateClockRequest"></param>
        /// <returns>StateClockResponse</returns>
        public StateClockResponse StateClockPost(StateClockRequest stateClockRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StateClockResponse> localVarResponse = StateClockPostWithHttpInfo(stateClockRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Clock Details Reads the clock&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateClockRequest"></param>
        /// <returns>ApiResponse of StateClockResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<StateClockResponse> StateClockPostWithHttpInfo(StateClockRequest stateClockRequest)
        {
            // verify the required parameter 'stateClockRequest' is set
            if (stateClockRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'stateClockRequest' when calling StateApi->StateClockPost");

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

            localVarRequestOptions.Data = stateClockRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateClockResponse>("/state/clock", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateClockPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Clock Details Reads the clock&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateClockRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateClockResponse</returns>
        public async System.Threading.Tasks.Task<StateClockResponse> StateClockPostAsync(StateClockRequest stateClockRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StateClockResponse> localVarResponse = await StateClockPostWithHttpInfoAsync(stateClockRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Clock Details Reads the clock&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateClockRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateClockResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<StateClockResponse>> StateClockPostWithHttpInfoAsync(StateClockRequest stateClockRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateClockRequest' is set
            if (stateClockRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'stateClockRequest' when calling StateApi->StateClockPost");


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

            localVarRequestOptions.Data = stateClockRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateClockResponse>("/state/clock", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateClockPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Component Details Reads the component&#39;s substate/s from the top of the current ledger. Also recursively extracts vault balance totals from the component&#39;s entity subtree. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateComponentRequest"></param>
        /// <returns>StateComponentResponse</returns>
        public StateComponentResponse StateComponentPost(StateComponentRequest stateComponentRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StateComponentResponse> localVarResponse = StateComponentPostWithHttpInfo(stateComponentRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Component Details Reads the component&#39;s substate/s from the top of the current ledger. Also recursively extracts vault balance totals from the component&#39;s entity subtree. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateComponentRequest"></param>
        /// <returns>ApiResponse of StateComponentResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<StateComponentResponse> StateComponentPostWithHttpInfo(StateComponentRequest stateComponentRequest)
        {
            // verify the required parameter 'stateComponentRequest' is set
            if (stateComponentRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'stateComponentRequest' when calling StateApi->StateComponentPost");

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

            localVarRequestOptions.Data = stateComponentRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateComponentResponse>("/state/component", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateComponentPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Component Details Reads the component&#39;s substate/s from the top of the current ledger. Also recursively extracts vault balance totals from the component&#39;s entity subtree. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateComponentRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateComponentResponse</returns>
        public async System.Threading.Tasks.Task<StateComponentResponse> StateComponentPostAsync(StateComponentRequest stateComponentRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StateComponentResponse> localVarResponse = await StateComponentPostWithHttpInfoAsync(stateComponentRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Component Details Reads the component&#39;s substate/s from the top of the current ledger. Also recursively extracts vault balance totals from the component&#39;s entity subtree. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateComponentRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateComponentResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<StateComponentResponse>> StateComponentPostWithHttpInfoAsync(StateComponentRequest stateComponentRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateComponentRequest' is set
            if (stateComponentRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'stateComponentRequest' when calling StateApi->StateComponentPost");


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

            localVarRequestOptions.Data = stateComponentRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateComponentResponse>("/state/component", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateComponentPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Epoch Details Reads the epoch manager&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEpochRequest"></param>
        /// <returns>StateEpochResponse</returns>
        public StateEpochResponse StateEpochPost(StateEpochRequest stateEpochRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StateEpochResponse> localVarResponse = StateEpochPostWithHttpInfo(stateEpochRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Epoch Details Reads the epoch manager&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEpochRequest"></param>
        /// <returns>ApiResponse of StateEpochResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<StateEpochResponse> StateEpochPostWithHttpInfo(StateEpochRequest stateEpochRequest)
        {
            // verify the required parameter 'stateEpochRequest' is set
            if (stateEpochRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'stateEpochRequest' when calling StateApi->StateEpochPost");

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

            localVarRequestOptions.Data = stateEpochRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateEpochResponse>("/state/epoch", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateEpochPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Epoch Details Reads the epoch manager&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEpochRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEpochResponse</returns>
        public async System.Threading.Tasks.Task<StateEpochResponse> StateEpochPostAsync(StateEpochRequest stateEpochRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StateEpochResponse> localVarResponse = await StateEpochPostWithHttpInfoAsync(stateEpochRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Epoch Details Reads the epoch manager&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEpochRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEpochResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<StateEpochResponse>> StateEpochPostWithHttpInfoAsync(StateEpochRequest stateEpochRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateEpochRequest' is set
            if (stateEpochRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'stateEpochRequest' when calling StateApi->StateEpochPost");


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

            localVarRequestOptions.Data = stateEpochRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateEpochResponse>("/state/epoch", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateEpochPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Non-Fungible Details Reads the data associated with a single Non-Fungible Unit under a Non-Fungible Resource. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleRequest"></param>
        /// <returns>StateNonFungibleResponse</returns>
        public StateNonFungibleResponse StateNonFungiblePost(StateNonFungibleRequest stateNonFungibleRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StateNonFungibleResponse> localVarResponse = StateNonFungiblePostWithHttpInfo(stateNonFungibleRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Non-Fungible Details Reads the data associated with a single Non-Fungible Unit under a Non-Fungible Resource. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleRequest"></param>
        /// <returns>ApiResponse of StateNonFungibleResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<StateNonFungibleResponse> StateNonFungiblePostWithHttpInfo(StateNonFungibleRequest stateNonFungibleRequest)
        {
            // verify the required parameter 'stateNonFungibleRequest' is set
            if (stateNonFungibleRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'stateNonFungibleRequest' when calling StateApi->StateNonFungiblePost");

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

            localVarRequestOptions.Data = stateNonFungibleRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateNonFungibleResponse>("/state/non-fungible", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateNonFungiblePost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Non-Fungible Details Reads the data associated with a single Non-Fungible Unit under a Non-Fungible Resource. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateNonFungibleResponse</returns>
        public async System.Threading.Tasks.Task<StateNonFungibleResponse> StateNonFungiblePostAsync(StateNonFungibleRequest stateNonFungibleRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StateNonFungibleResponse> localVarResponse = await StateNonFungiblePostWithHttpInfoAsync(stateNonFungibleRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Non-Fungible Details Reads the data associated with a single Non-Fungible Unit under a Non-Fungible Resource. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateNonFungibleResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<StateNonFungibleResponse>> StateNonFungiblePostWithHttpInfoAsync(StateNonFungibleRequest stateNonFungibleRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateNonFungibleRequest' is set
            if (stateNonFungibleRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'stateNonFungibleRequest' when calling StateApi->StateNonFungiblePost");


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

            localVarRequestOptions.Data = stateNonFungibleRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateNonFungibleResponse>("/state/non-fungible", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateNonFungiblePost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Package Details Reads the package&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="statePackageRequest"></param>
        /// <returns>StatePackageResponse</returns>
        public StatePackageResponse StatePackagePost(StatePackageRequest statePackageRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StatePackageResponse> localVarResponse = StatePackagePostWithHttpInfo(statePackageRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Package Details Reads the package&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="statePackageRequest"></param>
        /// <returns>ApiResponse of StatePackageResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<StatePackageResponse> StatePackagePostWithHttpInfo(StatePackageRequest statePackageRequest)
        {
            // verify the required parameter 'statePackageRequest' is set
            if (statePackageRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'statePackageRequest' when calling StateApi->StatePackagePost");

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

            localVarRequestOptions.Data = statePackageRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StatePackageResponse>("/state/package", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StatePackagePost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Package Details Reads the package&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="statePackageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StatePackageResponse</returns>
        public async System.Threading.Tasks.Task<StatePackageResponse> StatePackagePostAsync(StatePackageRequest statePackageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StatePackageResponse> localVarResponse = await StatePackagePostWithHttpInfoAsync(statePackageRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Package Details Reads the package&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="statePackageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StatePackageResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<StatePackageResponse>> StatePackagePostWithHttpInfoAsync(StatePackageRequest statePackageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'statePackageRequest' is set
            if (statePackageRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'statePackageRequest' when calling StateApi->StatePackagePost");


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

            localVarRequestOptions.Data = statePackageRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StatePackageResponse>("/state/package", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StatePackagePost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Resource Details Reads the resource manager&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateResourceRequest"></param>
        /// <returns>StateResourceResponse</returns>
        public StateResourceResponse StateResourcePost(StateResourceRequest stateResourceRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StateResourceResponse> localVarResponse = StateResourcePostWithHttpInfo(stateResourceRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Resource Details Reads the resource manager&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateResourceRequest"></param>
        /// <returns>ApiResponse of StateResourceResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<StateResourceResponse> StateResourcePostWithHttpInfo(StateResourceRequest stateResourceRequest)
        {
            // verify the required parameter 'stateResourceRequest' is set
            if (stateResourceRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'stateResourceRequest' when calling StateApi->StateResourcePost");

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

            localVarRequestOptions.Data = stateResourceRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateResourceResponse>("/state/resource", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateResourcePost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Resource Details Reads the resource manager&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateResourceRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateResourceResponse</returns>
        public async System.Threading.Tasks.Task<StateResourceResponse> StateResourcePostAsync(StateResourceRequest stateResourceRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StateResourceResponse> localVarResponse = await StateResourcePostWithHttpInfoAsync(stateResourceRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Resource Details Reads the resource manager&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateResourceRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateResourceResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<StateResourceResponse>> StateResourcePostWithHttpInfoAsync(StateResourceRequest stateResourceRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateResourceRequest' is set
            if (stateResourceRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'stateResourceRequest' when calling StateApi->StateResourcePost");


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

            localVarRequestOptions.Data = stateResourceRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateResourceResponse>("/state/resource", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateResourcePost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Validator Details Reads the validator&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorRequest"></param>
        /// <returns>StateValidatorResponse</returns>
        public StateValidatorResponse StateValidatorPost(StateValidatorRequest stateValidatorRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StateValidatorResponse> localVarResponse = StateValidatorPostWithHttpInfo(stateValidatorRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Validator Details Reads the validator&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorRequest"></param>
        /// <returns>ApiResponse of StateValidatorResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<StateValidatorResponse> StateValidatorPostWithHttpInfo(StateValidatorRequest stateValidatorRequest)
        {
            // verify the required parameter 'stateValidatorRequest' is set
            if (stateValidatorRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'stateValidatorRequest' when calling StateApi->StateValidatorPost");

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

            localVarRequestOptions.Data = stateValidatorRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateValidatorResponse>("/state/validator", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateValidatorPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Validator Details Reads the validator&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateValidatorResponse</returns>
        public async System.Threading.Tasks.Task<StateValidatorResponse> StateValidatorPostAsync(StateValidatorRequest stateValidatorRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<StateValidatorResponse> localVarResponse = await StateValidatorPostWithHttpInfoAsync(stateValidatorRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Validator Details Reads the validator&#39;s substate/s from the top of the current ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateValidatorResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<StateValidatorResponse>> StateValidatorPostWithHttpInfoAsync(StateValidatorRequest stateValidatorRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateValidatorRequest' is set
            if (stateValidatorRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'stateValidatorRequest' when calling StateApi->StateValidatorPost");


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

            localVarRequestOptions.Data = stateValidatorRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateValidatorResponse>("/state/validator", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateValidatorPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

    }
}
