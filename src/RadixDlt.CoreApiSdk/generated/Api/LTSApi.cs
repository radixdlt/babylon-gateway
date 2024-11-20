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
 * The version of the OpenAPI document: v1.3.0
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
    public interface ILTSApiSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Get All Account Balances
        /// </summary>
        /// <remarks>
        /// Returns balances for all resources associated with an account
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountAllFungibleResourceBalancesRequest"></param>
        /// <returns>LtsStateAccountAllFungibleResourceBalancesResponse</returns>
        LtsStateAccountAllFungibleResourceBalancesResponse LtsStateAccountAllFungibleResourceBalancesPost(LtsStateAccountAllFungibleResourceBalancesRequest ltsStateAccountAllFungibleResourceBalancesRequest);

        /// <summary>
        /// Get All Account Balances
        /// </summary>
        /// <remarks>
        /// Returns balances for all resources associated with an account
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountAllFungibleResourceBalancesRequest"></param>
        /// <returns>ApiResponse of LtsStateAccountAllFungibleResourceBalancesResponse</returns>
        ApiResponse<LtsStateAccountAllFungibleResourceBalancesResponse> LtsStateAccountAllFungibleResourceBalancesPostWithHttpInfo(LtsStateAccountAllFungibleResourceBalancesRequest ltsStateAccountAllFungibleResourceBalancesRequest);
        /// <summary>
        /// Get Account Deposit Behaviour
        /// </summary>
        /// <remarks>
        /// Returns deposit behaviour of a single account for multiple resource addresses
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountDepositBehaviourRequest"></param>
        /// <returns>LtsStateAccountDepositBehaviourResponse</returns>
        LtsStateAccountDepositBehaviourResponse LtsStateAccountDepositBehaviourPost(LtsStateAccountDepositBehaviourRequest ltsStateAccountDepositBehaviourRequest);

        /// <summary>
        /// Get Account Deposit Behaviour
        /// </summary>
        /// <remarks>
        /// Returns deposit behaviour of a single account for multiple resource addresses
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountDepositBehaviourRequest"></param>
        /// <returns>ApiResponse of LtsStateAccountDepositBehaviourResponse</returns>
        ApiResponse<LtsStateAccountDepositBehaviourResponse> LtsStateAccountDepositBehaviourPostWithHttpInfo(LtsStateAccountDepositBehaviourRequest ltsStateAccountDepositBehaviourRequest);
        /// <summary>
        /// Get Single Account Balance
        /// </summary>
        /// <remarks>
        /// Returns balance of a single fungible resource in an account
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountFungibleResourceBalanceRequest"></param>
        /// <returns>LtsStateAccountFungibleResourceBalanceResponse</returns>
        LtsStateAccountFungibleResourceBalanceResponse LtsStateAccountFungibleResourceBalancePost(LtsStateAccountFungibleResourceBalanceRequest ltsStateAccountFungibleResourceBalanceRequest);

        /// <summary>
        /// Get Single Account Balance
        /// </summary>
        /// <remarks>
        /// Returns balance of a single fungible resource in an account
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountFungibleResourceBalanceRequest"></param>
        /// <returns>ApiResponse of LtsStateAccountFungibleResourceBalanceResponse</returns>
        ApiResponse<LtsStateAccountFungibleResourceBalanceResponse> LtsStateAccountFungibleResourceBalancePostWithHttpInfo(LtsStateAccountFungibleResourceBalanceRequest ltsStateAccountFungibleResourceBalanceRequest);
        /// <summary>
        /// Get Account Transaction Outcomes
        /// </summary>
        /// <remarks>
        /// Returns a list of committed transaction outcomes (containing balance changes) from a given state version, filtered to only transactions which involved the given account. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamAccountTransactionOutcomesRequest"></param>
        /// <returns>LtsStreamAccountTransactionOutcomesResponse</returns>
        LtsStreamAccountTransactionOutcomesResponse LtsStreamAccountTransactionOutcomesPost(LtsStreamAccountTransactionOutcomesRequest ltsStreamAccountTransactionOutcomesRequest);

        /// <summary>
        /// Get Account Transaction Outcomes
        /// </summary>
        /// <remarks>
        /// Returns a list of committed transaction outcomes (containing balance changes) from a given state version, filtered to only transactions which involved the given account. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamAccountTransactionOutcomesRequest"></param>
        /// <returns>ApiResponse of LtsStreamAccountTransactionOutcomesResponse</returns>
        ApiResponse<LtsStreamAccountTransactionOutcomesResponse> LtsStreamAccountTransactionOutcomesPostWithHttpInfo(LtsStreamAccountTransactionOutcomesRequest ltsStreamAccountTransactionOutcomesRequest);
        /// <summary>
        /// Get Transaction Outcomes
        /// </summary>
        /// <remarks>
        /// Returns a list of committed transaction outcomes (containing balance changes) from a given state version. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamTransactionOutcomesRequest"></param>
        /// <returns>LtsStreamTransactionOutcomesResponse</returns>
        LtsStreamTransactionOutcomesResponse LtsStreamTransactionOutcomesPost(LtsStreamTransactionOutcomesRequest ltsStreamTransactionOutcomesRequest);

        /// <summary>
        /// Get Transaction Outcomes
        /// </summary>
        /// <remarks>
        /// Returns a list of committed transaction outcomes (containing balance changes) from a given state version. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamTransactionOutcomesRequest"></param>
        /// <returns>ApiResponse of LtsStreamTransactionOutcomesResponse</returns>
        ApiResponse<LtsStreamTransactionOutcomesResponse> LtsStreamTransactionOutcomesPostWithHttpInfo(LtsStreamTransactionOutcomesRequest ltsStreamTransactionOutcomesRequest);
        /// <summary>
        /// Get Construction Metadata
        /// </summary>
        /// <remarks>
        /// Returns information necessary to build a transaction
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionConstructionRequest"></param>
        /// <returns>LtsTransactionConstructionResponse</returns>
        LtsTransactionConstructionResponse LtsTransactionConstructionPost(LtsTransactionConstructionRequest ltsTransactionConstructionRequest);

        /// <summary>
        /// Get Construction Metadata
        /// </summary>
        /// <remarks>
        /// Returns information necessary to build a transaction
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionConstructionRequest"></param>
        /// <returns>ApiResponse of LtsTransactionConstructionResponse</returns>
        ApiResponse<LtsTransactionConstructionResponse> LtsTransactionConstructionPostWithHttpInfo(LtsTransactionConstructionRequest ltsTransactionConstructionRequest);
        /// <summary>
        /// Get Transaction Status
        /// </summary>
        /// <remarks>
        /// Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionStatusRequest"></param>
        /// <returns>LtsTransactionStatusResponse</returns>
        LtsTransactionStatusResponse LtsTransactionStatusPost(LtsTransactionStatusRequest ltsTransactionStatusRequest);

        /// <summary>
        /// Get Transaction Status
        /// </summary>
        /// <remarks>
        /// Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionStatusRequest"></param>
        /// <returns>ApiResponse of LtsTransactionStatusResponse</returns>
        ApiResponse<LtsTransactionStatusResponse> LtsTransactionStatusPostWithHttpInfo(LtsTransactionStatusRequest ltsTransactionStatusRequest);
        /// <summary>
        /// Submit Transaction
        /// </summary>
        /// <remarks>
        /// Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionSubmitRequest"></param>
        /// <returns>LtsTransactionSubmitResponse</returns>
        LtsTransactionSubmitResponse LtsTransactionSubmitPost(LtsTransactionSubmitRequest ltsTransactionSubmitRequest);

        /// <summary>
        /// Submit Transaction
        /// </summary>
        /// <remarks>
        /// Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionSubmitRequest"></param>
        /// <returns>ApiResponse of LtsTransactionSubmitResponse</returns>
        ApiResponse<LtsTransactionSubmitResponse> LtsTransactionSubmitPostWithHttpInfo(LtsTransactionSubmitRequest ltsTransactionSubmitRequest);
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ILTSApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// Get All Account Balances
        /// </summary>
        /// <remarks>
        /// Returns balances for all resources associated with an account
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountAllFungibleResourceBalancesRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsStateAccountAllFungibleResourceBalancesResponse</returns>
        System.Threading.Tasks.Task<LtsStateAccountAllFungibleResourceBalancesResponse> LtsStateAccountAllFungibleResourceBalancesPostAsync(LtsStateAccountAllFungibleResourceBalancesRequest ltsStateAccountAllFungibleResourceBalancesRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get All Account Balances
        /// </summary>
        /// <remarks>
        /// Returns balances for all resources associated with an account
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountAllFungibleResourceBalancesRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsStateAccountAllFungibleResourceBalancesResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<LtsStateAccountAllFungibleResourceBalancesResponse>> LtsStateAccountAllFungibleResourceBalancesPostWithHttpInfoAsync(LtsStateAccountAllFungibleResourceBalancesRequest ltsStateAccountAllFungibleResourceBalancesRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Account Deposit Behaviour
        /// </summary>
        /// <remarks>
        /// Returns deposit behaviour of a single account for multiple resource addresses
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountDepositBehaviourRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsStateAccountDepositBehaviourResponse</returns>
        System.Threading.Tasks.Task<LtsStateAccountDepositBehaviourResponse> LtsStateAccountDepositBehaviourPostAsync(LtsStateAccountDepositBehaviourRequest ltsStateAccountDepositBehaviourRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Account Deposit Behaviour
        /// </summary>
        /// <remarks>
        /// Returns deposit behaviour of a single account for multiple resource addresses
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountDepositBehaviourRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsStateAccountDepositBehaviourResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<LtsStateAccountDepositBehaviourResponse>> LtsStateAccountDepositBehaviourPostWithHttpInfoAsync(LtsStateAccountDepositBehaviourRequest ltsStateAccountDepositBehaviourRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Single Account Balance
        /// </summary>
        /// <remarks>
        /// Returns balance of a single fungible resource in an account
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountFungibleResourceBalanceRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsStateAccountFungibleResourceBalanceResponse</returns>
        System.Threading.Tasks.Task<LtsStateAccountFungibleResourceBalanceResponse> LtsStateAccountFungibleResourceBalancePostAsync(LtsStateAccountFungibleResourceBalanceRequest ltsStateAccountFungibleResourceBalanceRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Single Account Balance
        /// </summary>
        /// <remarks>
        /// Returns balance of a single fungible resource in an account
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountFungibleResourceBalanceRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsStateAccountFungibleResourceBalanceResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<LtsStateAccountFungibleResourceBalanceResponse>> LtsStateAccountFungibleResourceBalancePostWithHttpInfoAsync(LtsStateAccountFungibleResourceBalanceRequest ltsStateAccountFungibleResourceBalanceRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Account Transaction Outcomes
        /// </summary>
        /// <remarks>
        /// Returns a list of committed transaction outcomes (containing balance changes) from a given state version, filtered to only transactions which involved the given account. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamAccountTransactionOutcomesRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsStreamAccountTransactionOutcomesResponse</returns>
        System.Threading.Tasks.Task<LtsStreamAccountTransactionOutcomesResponse> LtsStreamAccountTransactionOutcomesPostAsync(LtsStreamAccountTransactionOutcomesRequest ltsStreamAccountTransactionOutcomesRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Account Transaction Outcomes
        /// </summary>
        /// <remarks>
        /// Returns a list of committed transaction outcomes (containing balance changes) from a given state version, filtered to only transactions which involved the given account. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamAccountTransactionOutcomesRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsStreamAccountTransactionOutcomesResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<LtsStreamAccountTransactionOutcomesResponse>> LtsStreamAccountTransactionOutcomesPostWithHttpInfoAsync(LtsStreamAccountTransactionOutcomesRequest ltsStreamAccountTransactionOutcomesRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Transaction Outcomes
        /// </summary>
        /// <remarks>
        /// Returns a list of committed transaction outcomes (containing balance changes) from a given state version. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamTransactionOutcomesRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsStreamTransactionOutcomesResponse</returns>
        System.Threading.Tasks.Task<LtsStreamTransactionOutcomesResponse> LtsStreamTransactionOutcomesPostAsync(LtsStreamTransactionOutcomesRequest ltsStreamTransactionOutcomesRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Transaction Outcomes
        /// </summary>
        /// <remarks>
        /// Returns a list of committed transaction outcomes (containing balance changes) from a given state version. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamTransactionOutcomesRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsStreamTransactionOutcomesResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<LtsStreamTransactionOutcomesResponse>> LtsStreamTransactionOutcomesPostWithHttpInfoAsync(LtsStreamTransactionOutcomesRequest ltsStreamTransactionOutcomesRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Construction Metadata
        /// </summary>
        /// <remarks>
        /// Returns information necessary to build a transaction
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionConstructionRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsTransactionConstructionResponse</returns>
        System.Threading.Tasks.Task<LtsTransactionConstructionResponse> LtsTransactionConstructionPostAsync(LtsTransactionConstructionRequest ltsTransactionConstructionRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Construction Metadata
        /// </summary>
        /// <remarks>
        /// Returns information necessary to build a transaction
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionConstructionRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsTransactionConstructionResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<LtsTransactionConstructionResponse>> LtsTransactionConstructionPostWithHttpInfoAsync(LtsTransactionConstructionRequest ltsTransactionConstructionRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Transaction Status
        /// </summary>
        /// <remarks>
        /// Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsTransactionStatusResponse</returns>
        System.Threading.Tasks.Task<LtsTransactionStatusResponse> LtsTransactionStatusPostAsync(LtsTransactionStatusRequest ltsTransactionStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Transaction Status
        /// </summary>
        /// <remarks>
        /// Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsTransactionStatusResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<LtsTransactionStatusResponse>> LtsTransactionStatusPostWithHttpInfoAsync(LtsTransactionStatusRequest ltsTransactionStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Submit Transaction
        /// </summary>
        /// <remarks>
        /// Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionSubmitRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsTransactionSubmitResponse</returns>
        System.Threading.Tasks.Task<LtsTransactionSubmitResponse> LtsTransactionSubmitPostAsync(LtsTransactionSubmitRequest ltsTransactionSubmitRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Submit Transaction
        /// </summary>
        /// <remarks>
        /// Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </remarks>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionSubmitRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsTransactionSubmitResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<LtsTransactionSubmitResponse>> LtsTransactionSubmitPostWithHttpInfoAsync(LtsTransactionSubmitRequest ltsTransactionSubmitRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ILTSApi : ILTSApiSync, ILTSApiAsync
    {

    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class LTSApi : IDisposable, ILTSApi
    {
        private RadixDlt.CoreApiSdk.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="LTSApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <returns></returns>
        public LTSApi() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LTSApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="basePath">The target service's base path in URL format.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public LTSApi(string basePath)
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
        /// Initializes a new instance of the <see cref="LTSApi"/> class using Configuration object.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="configuration">An instance of Configuration.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public LTSApi(RadixDlt.CoreApiSdk.Client.Configuration configuration)
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
        /// Initializes a new instance of the <see cref="LTSApi"/> class.
        /// </summary>
        /// <param name="client">An instance of HttpClient.</param>
        /// <param name="handler">An optional instance of HttpClientHandler that is used by HttpClient.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        /// <remarks>
        /// Some configuration settings will not be applied without passing an HttpClientHandler.
        /// The features affected are: Setting and Retrieving Cookies, Client Certificates, Proxy settings.
        /// </remarks>
        public LTSApi(HttpClient client, HttpClientHandler handler = null) : this(client, (string)null, handler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LTSApi"/> class.
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
        public LTSApi(HttpClient client, string basePath, HttpClientHandler handler = null)
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
        /// Initializes a new instance of the <see cref="LTSApi"/> class using Configuration object.
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
        public LTSApi(HttpClient client, RadixDlt.CoreApiSdk.Client.Configuration configuration, HttpClientHandler handler = null)
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
        /// Initializes a new instance of the <see cref="LTSApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public LTSApi(RadixDlt.CoreApiSdk.Client.ISynchronousClient client, RadixDlt.CoreApiSdk.Client.IAsynchronousClient asyncClient, RadixDlt.CoreApiSdk.Client.IReadableConfiguration configuration)
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
        /// Get All Account Balances Returns balances for all resources associated with an account
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountAllFungibleResourceBalancesRequest"></param>
        /// <returns>LtsStateAccountAllFungibleResourceBalancesResponse</returns>
        public LtsStateAccountAllFungibleResourceBalancesResponse LtsStateAccountAllFungibleResourceBalancesPost(LtsStateAccountAllFungibleResourceBalancesRequest ltsStateAccountAllFungibleResourceBalancesRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStateAccountAllFungibleResourceBalancesResponse> localVarResponse = LtsStateAccountAllFungibleResourceBalancesPostWithHttpInfo(ltsStateAccountAllFungibleResourceBalancesRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get All Account Balances Returns balances for all resources associated with an account
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountAllFungibleResourceBalancesRequest"></param>
        /// <returns>ApiResponse of LtsStateAccountAllFungibleResourceBalancesResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStateAccountAllFungibleResourceBalancesResponse> LtsStateAccountAllFungibleResourceBalancesPostWithHttpInfo(LtsStateAccountAllFungibleResourceBalancesRequest ltsStateAccountAllFungibleResourceBalancesRequest)
        {
            // verify the required parameter 'ltsStateAccountAllFungibleResourceBalancesRequest' is set
            if (ltsStateAccountAllFungibleResourceBalancesRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsStateAccountAllFungibleResourceBalancesRequest' when calling LTSApi->LtsStateAccountAllFungibleResourceBalancesPost");

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

            localVarRequestOptions.Data = ltsStateAccountAllFungibleResourceBalancesRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<LtsStateAccountAllFungibleResourceBalancesResponse>("/lts/state/account-all-fungible-resource-balances", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsStateAccountAllFungibleResourceBalancesPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get All Account Balances Returns balances for all resources associated with an account
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountAllFungibleResourceBalancesRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsStateAccountAllFungibleResourceBalancesResponse</returns>
        public async System.Threading.Tasks.Task<LtsStateAccountAllFungibleResourceBalancesResponse> LtsStateAccountAllFungibleResourceBalancesPostAsync(LtsStateAccountAllFungibleResourceBalancesRequest ltsStateAccountAllFungibleResourceBalancesRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStateAccountAllFungibleResourceBalancesResponse> localVarResponse = await LtsStateAccountAllFungibleResourceBalancesPostWithHttpInfoAsync(ltsStateAccountAllFungibleResourceBalancesRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get All Account Balances Returns balances for all resources associated with an account
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountAllFungibleResourceBalancesRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsStateAccountAllFungibleResourceBalancesResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStateAccountAllFungibleResourceBalancesResponse>> LtsStateAccountAllFungibleResourceBalancesPostWithHttpInfoAsync(LtsStateAccountAllFungibleResourceBalancesRequest ltsStateAccountAllFungibleResourceBalancesRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'ltsStateAccountAllFungibleResourceBalancesRequest' is set
            if (ltsStateAccountAllFungibleResourceBalancesRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsStateAccountAllFungibleResourceBalancesRequest' when calling LTSApi->LtsStateAccountAllFungibleResourceBalancesPost");


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

            localVarRequestOptions.Data = ltsStateAccountAllFungibleResourceBalancesRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<LtsStateAccountAllFungibleResourceBalancesResponse>("/lts/state/account-all-fungible-resource-balances", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsStateAccountAllFungibleResourceBalancesPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Account Deposit Behaviour Returns deposit behaviour of a single account for multiple resource addresses
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountDepositBehaviourRequest"></param>
        /// <returns>LtsStateAccountDepositBehaviourResponse</returns>
        public LtsStateAccountDepositBehaviourResponse LtsStateAccountDepositBehaviourPost(LtsStateAccountDepositBehaviourRequest ltsStateAccountDepositBehaviourRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStateAccountDepositBehaviourResponse> localVarResponse = LtsStateAccountDepositBehaviourPostWithHttpInfo(ltsStateAccountDepositBehaviourRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Account Deposit Behaviour Returns deposit behaviour of a single account for multiple resource addresses
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountDepositBehaviourRequest"></param>
        /// <returns>ApiResponse of LtsStateAccountDepositBehaviourResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStateAccountDepositBehaviourResponse> LtsStateAccountDepositBehaviourPostWithHttpInfo(LtsStateAccountDepositBehaviourRequest ltsStateAccountDepositBehaviourRequest)
        {
            // verify the required parameter 'ltsStateAccountDepositBehaviourRequest' is set
            if (ltsStateAccountDepositBehaviourRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsStateAccountDepositBehaviourRequest' when calling LTSApi->LtsStateAccountDepositBehaviourPost");

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

            localVarRequestOptions.Data = ltsStateAccountDepositBehaviourRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<LtsStateAccountDepositBehaviourResponse>("/lts/state/account-deposit-behaviour", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsStateAccountDepositBehaviourPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Account Deposit Behaviour Returns deposit behaviour of a single account for multiple resource addresses
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountDepositBehaviourRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsStateAccountDepositBehaviourResponse</returns>
        public async System.Threading.Tasks.Task<LtsStateAccountDepositBehaviourResponse> LtsStateAccountDepositBehaviourPostAsync(LtsStateAccountDepositBehaviourRequest ltsStateAccountDepositBehaviourRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStateAccountDepositBehaviourResponse> localVarResponse = await LtsStateAccountDepositBehaviourPostWithHttpInfoAsync(ltsStateAccountDepositBehaviourRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Account Deposit Behaviour Returns deposit behaviour of a single account for multiple resource addresses
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountDepositBehaviourRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsStateAccountDepositBehaviourResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStateAccountDepositBehaviourResponse>> LtsStateAccountDepositBehaviourPostWithHttpInfoAsync(LtsStateAccountDepositBehaviourRequest ltsStateAccountDepositBehaviourRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'ltsStateAccountDepositBehaviourRequest' is set
            if (ltsStateAccountDepositBehaviourRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsStateAccountDepositBehaviourRequest' when calling LTSApi->LtsStateAccountDepositBehaviourPost");


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

            localVarRequestOptions.Data = ltsStateAccountDepositBehaviourRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<LtsStateAccountDepositBehaviourResponse>("/lts/state/account-deposit-behaviour", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsStateAccountDepositBehaviourPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Single Account Balance Returns balance of a single fungible resource in an account
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountFungibleResourceBalanceRequest"></param>
        /// <returns>LtsStateAccountFungibleResourceBalanceResponse</returns>
        public LtsStateAccountFungibleResourceBalanceResponse LtsStateAccountFungibleResourceBalancePost(LtsStateAccountFungibleResourceBalanceRequest ltsStateAccountFungibleResourceBalanceRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStateAccountFungibleResourceBalanceResponse> localVarResponse = LtsStateAccountFungibleResourceBalancePostWithHttpInfo(ltsStateAccountFungibleResourceBalanceRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Single Account Balance Returns balance of a single fungible resource in an account
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountFungibleResourceBalanceRequest"></param>
        /// <returns>ApiResponse of LtsStateAccountFungibleResourceBalanceResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStateAccountFungibleResourceBalanceResponse> LtsStateAccountFungibleResourceBalancePostWithHttpInfo(LtsStateAccountFungibleResourceBalanceRequest ltsStateAccountFungibleResourceBalanceRequest)
        {
            // verify the required parameter 'ltsStateAccountFungibleResourceBalanceRequest' is set
            if (ltsStateAccountFungibleResourceBalanceRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsStateAccountFungibleResourceBalanceRequest' when calling LTSApi->LtsStateAccountFungibleResourceBalancePost");

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

            localVarRequestOptions.Data = ltsStateAccountFungibleResourceBalanceRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<LtsStateAccountFungibleResourceBalanceResponse>("/lts/state/account-fungible-resource-balance", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsStateAccountFungibleResourceBalancePost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Single Account Balance Returns balance of a single fungible resource in an account
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountFungibleResourceBalanceRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsStateAccountFungibleResourceBalanceResponse</returns>
        public async System.Threading.Tasks.Task<LtsStateAccountFungibleResourceBalanceResponse> LtsStateAccountFungibleResourceBalancePostAsync(LtsStateAccountFungibleResourceBalanceRequest ltsStateAccountFungibleResourceBalanceRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStateAccountFungibleResourceBalanceResponse> localVarResponse = await LtsStateAccountFungibleResourceBalancePostWithHttpInfoAsync(ltsStateAccountFungibleResourceBalanceRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Single Account Balance Returns balance of a single fungible resource in an account
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStateAccountFungibleResourceBalanceRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsStateAccountFungibleResourceBalanceResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStateAccountFungibleResourceBalanceResponse>> LtsStateAccountFungibleResourceBalancePostWithHttpInfoAsync(LtsStateAccountFungibleResourceBalanceRequest ltsStateAccountFungibleResourceBalanceRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'ltsStateAccountFungibleResourceBalanceRequest' is set
            if (ltsStateAccountFungibleResourceBalanceRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsStateAccountFungibleResourceBalanceRequest' when calling LTSApi->LtsStateAccountFungibleResourceBalancePost");


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

            localVarRequestOptions.Data = ltsStateAccountFungibleResourceBalanceRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<LtsStateAccountFungibleResourceBalanceResponse>("/lts/state/account-fungible-resource-balance", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsStateAccountFungibleResourceBalancePost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Account Transaction Outcomes Returns a list of committed transaction outcomes (containing balance changes) from a given state version, filtered to only transactions which involved the given account. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamAccountTransactionOutcomesRequest"></param>
        /// <returns>LtsStreamAccountTransactionOutcomesResponse</returns>
        public LtsStreamAccountTransactionOutcomesResponse LtsStreamAccountTransactionOutcomesPost(LtsStreamAccountTransactionOutcomesRequest ltsStreamAccountTransactionOutcomesRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStreamAccountTransactionOutcomesResponse> localVarResponse = LtsStreamAccountTransactionOutcomesPostWithHttpInfo(ltsStreamAccountTransactionOutcomesRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Account Transaction Outcomes Returns a list of committed transaction outcomes (containing balance changes) from a given state version, filtered to only transactions which involved the given account. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamAccountTransactionOutcomesRequest"></param>
        /// <returns>ApiResponse of LtsStreamAccountTransactionOutcomesResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStreamAccountTransactionOutcomesResponse> LtsStreamAccountTransactionOutcomesPostWithHttpInfo(LtsStreamAccountTransactionOutcomesRequest ltsStreamAccountTransactionOutcomesRequest)
        {
            // verify the required parameter 'ltsStreamAccountTransactionOutcomesRequest' is set
            if (ltsStreamAccountTransactionOutcomesRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsStreamAccountTransactionOutcomesRequest' when calling LTSApi->LtsStreamAccountTransactionOutcomesPost");

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

            localVarRequestOptions.Data = ltsStreamAccountTransactionOutcomesRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<LtsStreamAccountTransactionOutcomesResponse>("/lts/stream/account-transaction-outcomes", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsStreamAccountTransactionOutcomesPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Account Transaction Outcomes Returns a list of committed transaction outcomes (containing balance changes) from a given state version, filtered to only transactions which involved the given account. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamAccountTransactionOutcomesRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsStreamAccountTransactionOutcomesResponse</returns>
        public async System.Threading.Tasks.Task<LtsStreamAccountTransactionOutcomesResponse> LtsStreamAccountTransactionOutcomesPostAsync(LtsStreamAccountTransactionOutcomesRequest ltsStreamAccountTransactionOutcomesRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStreamAccountTransactionOutcomesResponse> localVarResponse = await LtsStreamAccountTransactionOutcomesPostWithHttpInfoAsync(ltsStreamAccountTransactionOutcomesRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Account Transaction Outcomes Returns a list of committed transaction outcomes (containing balance changes) from a given state version, filtered to only transactions which involved the given account. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamAccountTransactionOutcomesRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsStreamAccountTransactionOutcomesResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStreamAccountTransactionOutcomesResponse>> LtsStreamAccountTransactionOutcomesPostWithHttpInfoAsync(LtsStreamAccountTransactionOutcomesRequest ltsStreamAccountTransactionOutcomesRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'ltsStreamAccountTransactionOutcomesRequest' is set
            if (ltsStreamAccountTransactionOutcomesRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsStreamAccountTransactionOutcomesRequest' when calling LTSApi->LtsStreamAccountTransactionOutcomesPost");


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

            localVarRequestOptions.Data = ltsStreamAccountTransactionOutcomesRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<LtsStreamAccountTransactionOutcomesResponse>("/lts/stream/account-transaction-outcomes", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsStreamAccountTransactionOutcomesPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Transaction Outcomes Returns a list of committed transaction outcomes (containing balance changes) from a given state version. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamTransactionOutcomesRequest"></param>
        /// <returns>LtsStreamTransactionOutcomesResponse</returns>
        public LtsStreamTransactionOutcomesResponse LtsStreamTransactionOutcomesPost(LtsStreamTransactionOutcomesRequest ltsStreamTransactionOutcomesRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStreamTransactionOutcomesResponse> localVarResponse = LtsStreamTransactionOutcomesPostWithHttpInfo(ltsStreamTransactionOutcomesRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Transaction Outcomes Returns a list of committed transaction outcomes (containing balance changes) from a given state version. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamTransactionOutcomesRequest"></param>
        /// <returns>ApiResponse of LtsStreamTransactionOutcomesResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStreamTransactionOutcomesResponse> LtsStreamTransactionOutcomesPostWithHttpInfo(LtsStreamTransactionOutcomesRequest ltsStreamTransactionOutcomesRequest)
        {
            // verify the required parameter 'ltsStreamTransactionOutcomesRequest' is set
            if (ltsStreamTransactionOutcomesRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsStreamTransactionOutcomesRequest' when calling LTSApi->LtsStreamTransactionOutcomesPost");

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

            localVarRequestOptions.Data = ltsStreamTransactionOutcomesRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<LtsStreamTransactionOutcomesResponse>("/lts/stream/transaction-outcomes", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsStreamTransactionOutcomesPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Transaction Outcomes Returns a list of committed transaction outcomes (containing balance changes) from a given state version. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamTransactionOutcomesRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsStreamTransactionOutcomesResponse</returns>
        public async System.Threading.Tasks.Task<LtsStreamTransactionOutcomesResponse> LtsStreamTransactionOutcomesPostAsync(LtsStreamTransactionOutcomesRequest ltsStreamTransactionOutcomesRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStreamTransactionOutcomesResponse> localVarResponse = await LtsStreamTransactionOutcomesPostWithHttpInfoAsync(ltsStreamTransactionOutcomesRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Transaction Outcomes Returns a list of committed transaction outcomes (containing balance changes) from a given state version. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsStreamTransactionOutcomesRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsStreamTransactionOutcomesResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<LtsStreamTransactionOutcomesResponse>> LtsStreamTransactionOutcomesPostWithHttpInfoAsync(LtsStreamTransactionOutcomesRequest ltsStreamTransactionOutcomesRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'ltsStreamTransactionOutcomesRequest' is set
            if (ltsStreamTransactionOutcomesRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsStreamTransactionOutcomesRequest' when calling LTSApi->LtsStreamTransactionOutcomesPost");


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

            localVarRequestOptions.Data = ltsStreamTransactionOutcomesRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<LtsStreamTransactionOutcomesResponse>("/lts/stream/transaction-outcomes", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsStreamTransactionOutcomesPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Construction Metadata Returns information necessary to build a transaction
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionConstructionRequest"></param>
        /// <returns>LtsTransactionConstructionResponse</returns>
        public LtsTransactionConstructionResponse LtsTransactionConstructionPost(LtsTransactionConstructionRequest ltsTransactionConstructionRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsTransactionConstructionResponse> localVarResponse = LtsTransactionConstructionPostWithHttpInfo(ltsTransactionConstructionRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Construction Metadata Returns information necessary to build a transaction
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionConstructionRequest"></param>
        /// <returns>ApiResponse of LtsTransactionConstructionResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<LtsTransactionConstructionResponse> LtsTransactionConstructionPostWithHttpInfo(LtsTransactionConstructionRequest ltsTransactionConstructionRequest)
        {
            // verify the required parameter 'ltsTransactionConstructionRequest' is set
            if (ltsTransactionConstructionRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsTransactionConstructionRequest' when calling LTSApi->LtsTransactionConstructionPost");

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

            localVarRequestOptions.Data = ltsTransactionConstructionRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<LtsTransactionConstructionResponse>("/lts/transaction/construction", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsTransactionConstructionPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Construction Metadata Returns information necessary to build a transaction
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionConstructionRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsTransactionConstructionResponse</returns>
        public async System.Threading.Tasks.Task<LtsTransactionConstructionResponse> LtsTransactionConstructionPostAsync(LtsTransactionConstructionRequest ltsTransactionConstructionRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsTransactionConstructionResponse> localVarResponse = await LtsTransactionConstructionPostWithHttpInfoAsync(ltsTransactionConstructionRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Construction Metadata Returns information necessary to build a transaction
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionConstructionRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsTransactionConstructionResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<LtsTransactionConstructionResponse>> LtsTransactionConstructionPostWithHttpInfoAsync(LtsTransactionConstructionRequest ltsTransactionConstructionRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'ltsTransactionConstructionRequest' is set
            if (ltsTransactionConstructionRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsTransactionConstructionRequest' when calling LTSApi->LtsTransactionConstructionPost");


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

            localVarRequestOptions.Data = ltsTransactionConstructionRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<LtsTransactionConstructionResponse>("/lts/transaction/construction", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsTransactionConstructionPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Transaction Status Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionStatusRequest"></param>
        /// <returns>LtsTransactionStatusResponse</returns>
        public LtsTransactionStatusResponse LtsTransactionStatusPost(LtsTransactionStatusRequest ltsTransactionStatusRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsTransactionStatusResponse> localVarResponse = LtsTransactionStatusPostWithHttpInfo(ltsTransactionStatusRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Transaction Status Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionStatusRequest"></param>
        /// <returns>ApiResponse of LtsTransactionStatusResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<LtsTransactionStatusResponse> LtsTransactionStatusPostWithHttpInfo(LtsTransactionStatusRequest ltsTransactionStatusRequest)
        {
            // verify the required parameter 'ltsTransactionStatusRequest' is set
            if (ltsTransactionStatusRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsTransactionStatusRequest' when calling LTSApi->LtsTransactionStatusPost");

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

            localVarRequestOptions.Data = ltsTransactionStatusRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<LtsTransactionStatusResponse>("/lts/transaction/status", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsTransactionStatusPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Transaction Status Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsTransactionStatusResponse</returns>
        public async System.Threading.Tasks.Task<LtsTransactionStatusResponse> LtsTransactionStatusPostAsync(LtsTransactionStatusRequest ltsTransactionStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsTransactionStatusResponse> localVarResponse = await LtsTransactionStatusPostWithHttpInfoAsync(ltsTransactionStatusRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Transaction Status Shares the node&#39;s knowledge of any payloads associated with the given intent hash. Generally there will be a single payload for a given intent, but it&#39;s theoretically possible there may be multiple. This knowledge is summarised into a status for the intent. This summarised status in the response is likely sufficient for most clients. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionStatusRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsTransactionStatusResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<LtsTransactionStatusResponse>> LtsTransactionStatusPostWithHttpInfoAsync(LtsTransactionStatusRequest ltsTransactionStatusRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'ltsTransactionStatusRequest' is set
            if (ltsTransactionStatusRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsTransactionStatusRequest' when calling LTSApi->LtsTransactionStatusPost");


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

            localVarRequestOptions.Data = ltsTransactionStatusRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<LtsTransactionStatusResponse>("/lts/transaction/status", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsTransactionStatusPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Submit Transaction Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionSubmitRequest"></param>
        /// <returns>LtsTransactionSubmitResponse</returns>
        public LtsTransactionSubmitResponse LtsTransactionSubmitPost(LtsTransactionSubmitRequest ltsTransactionSubmitRequest)
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsTransactionSubmitResponse> localVarResponse = LtsTransactionSubmitPostWithHttpInfo(ltsTransactionSubmitRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Submit Transaction Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionSubmitRequest"></param>
        /// <returns>ApiResponse of LtsTransactionSubmitResponse</returns>
        public RadixDlt.CoreApiSdk.Client.ApiResponse<LtsTransactionSubmitResponse> LtsTransactionSubmitPostWithHttpInfo(LtsTransactionSubmitRequest ltsTransactionSubmitRequest)
        {
            // verify the required parameter 'ltsTransactionSubmitRequest' is set
            if (ltsTransactionSubmitRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsTransactionSubmitRequest' when calling LTSApi->LtsTransactionSubmitPost");

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

            localVarRequestOptions.Data = ltsTransactionSubmitRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<LtsTransactionSubmitResponse>("/lts/transaction/submit", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsTransactionSubmitPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Submit Transaction Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionSubmitRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LtsTransactionSubmitResponse</returns>
        public async System.Threading.Tasks.Task<LtsTransactionSubmitResponse> LtsTransactionSubmitPostAsync(LtsTransactionSubmitRequest ltsTransactionSubmitRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.CoreApiSdk.Client.ApiResponse<LtsTransactionSubmitResponse> localVarResponse = await LtsTransactionSubmitPostWithHttpInfoAsync(ltsTransactionSubmitRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Submit Transaction Submits a notarized transaction to the network. Returns whether the transaction submission was already included in the node&#39;s mempool. 
        /// </summary>
        /// <exception cref="RadixDlt.CoreApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ltsTransactionSubmitRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LtsTransactionSubmitResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.CoreApiSdk.Client.ApiResponse<LtsTransactionSubmitResponse>> LtsTransactionSubmitPostWithHttpInfoAsync(LtsTransactionSubmitRequest ltsTransactionSubmitRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'ltsTransactionSubmitRequest' is set
            if (ltsTransactionSubmitRequest == null)
                throw new RadixDlt.CoreApiSdk.Client.ApiException(400, "Missing required parameter 'ltsTransactionSubmitRequest' when calling LTSApi->LtsTransactionSubmitPost");


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

            localVarRequestOptions.Data = ltsTransactionSubmitRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<LtsTransactionSubmitResponse>("/lts/transaction/submit", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LtsTransactionSubmitPost", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

    }
}
