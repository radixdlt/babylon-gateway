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
using RadixDlt.NetworkGateway.GatewayApiSdk.Client;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Api
{

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IStateApiSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Get page of Global Entity Fungible Resource Vaults
        /// </summary>
        /// <remarks>
        /// Returns vaults for fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungibleResourceVaultsPageRequest"></param>
        /// <returns>StateEntityFungibleResourceVaultsPageResponse</returns>
        StateEntityFungibleResourceVaultsPageResponse EntityFungibleResourceVaultPage(StateEntityFungibleResourceVaultsPageRequest stateEntityFungibleResourceVaultsPageRequest);

        /// <summary>
        /// Get page of Global Entity Fungible Resource Vaults
        /// </summary>
        /// <remarks>
        /// Returns vaults for fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungibleResourceVaultsPageRequest"></param>
        /// <returns>ApiResponse of StateEntityFungibleResourceVaultsPageResponse</returns>
        ApiResponse<StateEntityFungibleResourceVaultsPageResponse> EntityFungibleResourceVaultPageWithHttpInfo(StateEntityFungibleResourceVaultsPageRequest stateEntityFungibleResourceVaultsPageRequest);
        /// <summary>
        /// Get page of Global Entity Fungible Resource Balances
        /// </summary>
        /// <remarks>
        /// Returns the total amount of each fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungiblesPageRequest"></param>
        /// <returns>StateEntityFungiblesPageResponse</returns>
        StateEntityFungiblesPageResponse EntityFungiblesPage(StateEntityFungiblesPageRequest stateEntityFungiblesPageRequest);

        /// <summary>
        /// Get page of Global Entity Fungible Resource Balances
        /// </summary>
        /// <remarks>
        /// Returns the total amount of each fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungiblesPageRequest"></param>
        /// <returns>ApiResponse of StateEntityFungiblesPageResponse</returns>
        ApiResponse<StateEntityFungiblesPageResponse> EntityFungiblesPageWithHttpInfo(StateEntityFungiblesPageRequest stateEntityFungiblesPageRequest);
        /// <summary>
        /// Get Entity Metadata Page
        /// </summary>
        /// <remarks>
        /// Returns all the metadata properties associated with a given global entity. The returned response is in a paginated format, ordered by first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityMetadataPageRequest"></param>
        /// <returns>StateEntityMetadataPageResponse</returns>
        StateEntityMetadataPageResponse EntityMetadataPage(StateEntityMetadataPageRequest stateEntityMetadataPageRequest);

        /// <summary>
        /// Get Entity Metadata Page
        /// </summary>
        /// <remarks>
        /// Returns all the metadata properties associated with a given global entity. The returned response is in a paginated format, ordered by first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityMetadataPageRequest"></param>
        /// <returns>ApiResponse of StateEntityMetadataPageResponse</returns>
        ApiResponse<StateEntityMetadataPageResponse> EntityMetadataPageWithHttpInfo(StateEntityMetadataPageRequest stateEntityMetadataPageRequest);
        /// <summary>
        /// Get page of Non-Fungibles in Vault
        /// </summary>
        /// <remarks>
        /// Returns all non-fungible IDs of a given non-fungible resource owned by a given entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearence on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleIdsPageRequest"></param>
        /// <returns>StateEntityNonFungibleIdsPageResponse</returns>
        StateEntityNonFungibleIdsPageResponse EntityNonFungibleIdsPage(StateEntityNonFungibleIdsPageRequest stateEntityNonFungibleIdsPageRequest);

        /// <summary>
        /// Get page of Non-Fungibles in Vault
        /// </summary>
        /// <remarks>
        /// Returns all non-fungible IDs of a given non-fungible resource owned by a given entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearence on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleIdsPageRequest"></param>
        /// <returns>ApiResponse of StateEntityNonFungibleIdsPageResponse</returns>
        ApiResponse<StateEntityNonFungibleIdsPageResponse> EntityNonFungibleIdsPageWithHttpInfo(StateEntityNonFungibleIdsPageRequest stateEntityNonFungibleIdsPageRequest);
        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Vaults
        /// </summary>
        /// <remarks>
        /// Returns vaults for non fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleResourceVaultsPageRequest"></param>
        /// <returns>StateEntityNonFungibleResourceVaultsPageResponse</returns>
        StateEntityNonFungibleResourceVaultsPageResponse EntityNonFungibleResourceVaultPage(StateEntityNonFungibleResourceVaultsPageRequest stateEntityNonFungibleResourceVaultsPageRequest);

        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Vaults
        /// </summary>
        /// <remarks>
        /// Returns vaults for non fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleResourceVaultsPageRequest"></param>
        /// <returns>ApiResponse of StateEntityNonFungibleResourceVaultsPageResponse</returns>
        ApiResponse<StateEntityNonFungibleResourceVaultsPageResponse> EntityNonFungibleResourceVaultPageWithHttpInfo(StateEntityNonFungibleResourceVaultsPageRequest stateEntityNonFungibleResourceVaultsPageRequest);
        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Balances
        /// </summary>
        /// <remarks>
        /// Returns the total amount of each non-fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungiblesPageRequest"></param>
        /// <returns>StateEntityNonFungiblesPageResponse</returns>
        StateEntityNonFungiblesPageResponse EntityNonFungiblesPage(StateEntityNonFungiblesPageRequest stateEntityNonFungiblesPageRequest);

        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Balances
        /// </summary>
        /// <remarks>
        /// Returns the total amount of each non-fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungiblesPageRequest"></param>
        /// <returns>ApiResponse of StateEntityNonFungiblesPageResponse</returns>
        ApiResponse<StateEntityNonFungiblesPageResponse> EntityNonFungiblesPageWithHttpInfo(StateEntityNonFungiblesPageRequest stateEntityNonFungiblesPageRequest);
        /// <summary>
        /// Get KeyValueStore Data
        /// </summary>
        /// <remarks>
        /// Returns data (value) associated with a given key of a given key-value store. [Check detailed documentation for explanation](#section/How-to-query-the-content-of-a-key-value-store-inside-a-component) 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateKeyValueStoreDataRequest"></param>
        /// <returns>StateKeyValueStoreDataResponse</returns>
        StateKeyValueStoreDataResponse KeyValueStoreData(StateKeyValueStoreDataRequest stateKeyValueStoreDataRequest);

        /// <summary>
        /// Get KeyValueStore Data
        /// </summary>
        /// <remarks>
        /// Returns data (value) associated with a given key of a given key-value store. [Check detailed documentation for explanation](#section/How-to-query-the-content-of-a-key-value-store-inside-a-component) 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateKeyValueStoreDataRequest"></param>
        /// <returns>ApiResponse of StateKeyValueStoreDataResponse</returns>
        ApiResponse<StateKeyValueStoreDataResponse> KeyValueStoreDataWithHttpInfo(StateKeyValueStoreDataRequest stateKeyValueStoreDataRequest);
        /// <summary>
        /// Get Non-Fungible Data
        /// </summary>
        /// <remarks>
        /// Returns data associated with a given non-fungible ID of a given non-fungible resource. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleDataRequest"></param>
        /// <returns>StateNonFungibleDataResponse</returns>
        StateNonFungibleDataResponse NonFungibleData(StateNonFungibleDataRequest stateNonFungibleDataRequest);

        /// <summary>
        /// Get Non-Fungible Data
        /// </summary>
        /// <remarks>
        /// Returns data associated with a given non-fungible ID of a given non-fungible resource. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleDataRequest"></param>
        /// <returns>ApiResponse of StateNonFungibleDataResponse</returns>
        ApiResponse<StateNonFungibleDataResponse> NonFungibleDataWithHttpInfo(StateNonFungibleDataRequest stateNonFungibleDataRequest);
        /// <summary>
        /// Get page of Non-Fungible Ids in Resource Collection
        /// </summary>
        /// <remarks>
        /// Returns the non-fungible IDs of a given non-fungible resource. Returned response is in a paginated format, ordered by their first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleIdsRequest"></param>
        /// <returns>StateNonFungibleIdsResponse</returns>
        StateNonFungibleIdsResponse NonFungibleIds(StateNonFungibleIdsRequest stateNonFungibleIdsRequest);

        /// <summary>
        /// Get page of Non-Fungible Ids in Resource Collection
        /// </summary>
        /// <remarks>
        /// Returns the non-fungible IDs of a given non-fungible resource. Returned response is in a paginated format, ordered by their first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleIdsRequest"></param>
        /// <returns>ApiResponse of StateNonFungibleIdsResponse</returns>
        ApiResponse<StateNonFungibleIdsResponse> NonFungibleIdsWithHttpInfo(StateNonFungibleIdsRequest stateNonFungibleIdsRequest);
        /// <summary>
        /// Get Non-Fungible Location
        /// </summary>
        /// <remarks>
        /// Returns location of a given non-fungible ID. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleLocationRequest"></param>
        /// <returns>StateNonFungibleLocationResponse</returns>
        StateNonFungibleLocationResponse NonFungibleLocation(StateNonFungibleLocationRequest stateNonFungibleLocationRequest);

        /// <summary>
        /// Get Non-Fungible Location
        /// </summary>
        /// <remarks>
        /// Returns location of a given non-fungible ID. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleLocationRequest"></param>
        /// <returns>ApiResponse of StateNonFungibleLocationResponse</returns>
        ApiResponse<StateNonFungibleLocationResponse> NonFungibleLocationWithHttpInfo(StateNonFungibleLocationRequest stateNonFungibleLocationRequest);
        /// <summary>
        /// Get Entity Details
        /// </summary>
        /// <remarks>
        /// Returns detailed information for collection of entities. Aggregate resources globally by default. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityDetailsRequest"></param>
        /// <returns>StateEntityDetailsResponse</returns>
        StateEntityDetailsResponse StateEntityDetails(StateEntityDetailsRequest stateEntityDetailsRequest);

        /// <summary>
        /// Get Entity Details
        /// </summary>
        /// <remarks>
        /// Returns detailed information for collection of entities. Aggregate resources globally by default. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityDetailsRequest"></param>
        /// <returns>ApiResponse of StateEntityDetailsResponse</returns>
        ApiResponse<StateEntityDetailsResponse> StateEntityDetailsWithHttpInfo(StateEntityDetailsRequest stateEntityDetailsRequest);
        /// <summary>
        /// Get Validators List
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorsListRequest"></param>
        /// <returns>StateValidatorsListResponse</returns>
        StateValidatorsListResponse StateValidatorsList(StateValidatorsListRequest stateValidatorsListRequest);

        /// <summary>
        /// Get Validators List
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorsListRequest"></param>
        /// <returns>ApiResponse of StateValidatorsListResponse</returns>
        ApiResponse<StateValidatorsListResponse> StateValidatorsListWithHttpInfo(StateValidatorsListRequest stateValidatorsListRequest);
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IStateApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// Get page of Global Entity Fungible Resource Vaults
        /// </summary>
        /// <remarks>
        /// Returns vaults for fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungibleResourceVaultsPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEntityFungibleResourceVaultsPageResponse</returns>
        System.Threading.Tasks.Task<StateEntityFungibleResourceVaultsPageResponse> EntityFungibleResourceVaultPageAsync(StateEntityFungibleResourceVaultsPageRequest stateEntityFungibleResourceVaultsPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get page of Global Entity Fungible Resource Vaults
        /// </summary>
        /// <remarks>
        /// Returns vaults for fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungibleResourceVaultsPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEntityFungibleResourceVaultsPageResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateEntityFungibleResourceVaultsPageResponse>> EntityFungibleResourceVaultPageWithHttpInfoAsync(StateEntityFungibleResourceVaultsPageRequest stateEntityFungibleResourceVaultsPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get page of Global Entity Fungible Resource Balances
        /// </summary>
        /// <remarks>
        /// Returns the total amount of each fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungiblesPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEntityFungiblesPageResponse</returns>
        System.Threading.Tasks.Task<StateEntityFungiblesPageResponse> EntityFungiblesPageAsync(StateEntityFungiblesPageRequest stateEntityFungiblesPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get page of Global Entity Fungible Resource Balances
        /// </summary>
        /// <remarks>
        /// Returns the total amount of each fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungiblesPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEntityFungiblesPageResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateEntityFungiblesPageResponse>> EntityFungiblesPageWithHttpInfoAsync(StateEntityFungiblesPageRequest stateEntityFungiblesPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Entity Metadata Page
        /// </summary>
        /// <remarks>
        /// Returns all the metadata properties associated with a given global entity. The returned response is in a paginated format, ordered by first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityMetadataPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEntityMetadataPageResponse</returns>
        System.Threading.Tasks.Task<StateEntityMetadataPageResponse> EntityMetadataPageAsync(StateEntityMetadataPageRequest stateEntityMetadataPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Entity Metadata Page
        /// </summary>
        /// <remarks>
        /// Returns all the metadata properties associated with a given global entity. The returned response is in a paginated format, ordered by first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityMetadataPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEntityMetadataPageResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateEntityMetadataPageResponse>> EntityMetadataPageWithHttpInfoAsync(StateEntityMetadataPageRequest stateEntityMetadataPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get page of Non-Fungibles in Vault
        /// </summary>
        /// <remarks>
        /// Returns all non-fungible IDs of a given non-fungible resource owned by a given entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearence on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleIdsPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEntityNonFungibleIdsPageResponse</returns>
        System.Threading.Tasks.Task<StateEntityNonFungibleIdsPageResponse> EntityNonFungibleIdsPageAsync(StateEntityNonFungibleIdsPageRequest stateEntityNonFungibleIdsPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get page of Non-Fungibles in Vault
        /// </summary>
        /// <remarks>
        /// Returns all non-fungible IDs of a given non-fungible resource owned by a given entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearence on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleIdsPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEntityNonFungibleIdsPageResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateEntityNonFungibleIdsPageResponse>> EntityNonFungibleIdsPageWithHttpInfoAsync(StateEntityNonFungibleIdsPageRequest stateEntityNonFungibleIdsPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Vaults
        /// </summary>
        /// <remarks>
        /// Returns vaults for non fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleResourceVaultsPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEntityNonFungibleResourceVaultsPageResponse</returns>
        System.Threading.Tasks.Task<StateEntityNonFungibleResourceVaultsPageResponse> EntityNonFungibleResourceVaultPageAsync(StateEntityNonFungibleResourceVaultsPageRequest stateEntityNonFungibleResourceVaultsPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Vaults
        /// </summary>
        /// <remarks>
        /// Returns vaults for non fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleResourceVaultsPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEntityNonFungibleResourceVaultsPageResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateEntityNonFungibleResourceVaultsPageResponse>> EntityNonFungibleResourceVaultPageWithHttpInfoAsync(StateEntityNonFungibleResourceVaultsPageRequest stateEntityNonFungibleResourceVaultsPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Balances
        /// </summary>
        /// <remarks>
        /// Returns the total amount of each non-fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungiblesPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEntityNonFungiblesPageResponse</returns>
        System.Threading.Tasks.Task<StateEntityNonFungiblesPageResponse> EntityNonFungiblesPageAsync(StateEntityNonFungiblesPageRequest stateEntityNonFungiblesPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Balances
        /// </summary>
        /// <remarks>
        /// Returns the total amount of each non-fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungiblesPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEntityNonFungiblesPageResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateEntityNonFungiblesPageResponse>> EntityNonFungiblesPageWithHttpInfoAsync(StateEntityNonFungiblesPageRequest stateEntityNonFungiblesPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get KeyValueStore Data
        /// </summary>
        /// <remarks>
        /// Returns data (value) associated with a given key of a given key-value store. [Check detailed documentation for explanation](#section/How-to-query-the-content-of-a-key-value-store-inside-a-component) 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateKeyValueStoreDataRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateKeyValueStoreDataResponse</returns>
        System.Threading.Tasks.Task<StateKeyValueStoreDataResponse> KeyValueStoreDataAsync(StateKeyValueStoreDataRequest stateKeyValueStoreDataRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get KeyValueStore Data
        /// </summary>
        /// <remarks>
        /// Returns data (value) associated with a given key of a given key-value store. [Check detailed documentation for explanation](#section/How-to-query-the-content-of-a-key-value-store-inside-a-component) 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateKeyValueStoreDataRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateKeyValueStoreDataResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateKeyValueStoreDataResponse>> KeyValueStoreDataWithHttpInfoAsync(StateKeyValueStoreDataRequest stateKeyValueStoreDataRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Non-Fungible Data
        /// </summary>
        /// <remarks>
        /// Returns data associated with a given non-fungible ID of a given non-fungible resource. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleDataRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateNonFungibleDataResponse</returns>
        System.Threading.Tasks.Task<StateNonFungibleDataResponse> NonFungibleDataAsync(StateNonFungibleDataRequest stateNonFungibleDataRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Non-Fungible Data
        /// </summary>
        /// <remarks>
        /// Returns data associated with a given non-fungible ID of a given non-fungible resource. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleDataRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateNonFungibleDataResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateNonFungibleDataResponse>> NonFungibleDataWithHttpInfoAsync(StateNonFungibleDataRequest stateNonFungibleDataRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get page of Non-Fungible Ids in Resource Collection
        /// </summary>
        /// <remarks>
        /// Returns the non-fungible IDs of a given non-fungible resource. Returned response is in a paginated format, ordered by their first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleIdsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateNonFungibleIdsResponse</returns>
        System.Threading.Tasks.Task<StateNonFungibleIdsResponse> NonFungibleIdsAsync(StateNonFungibleIdsRequest stateNonFungibleIdsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get page of Non-Fungible Ids in Resource Collection
        /// </summary>
        /// <remarks>
        /// Returns the non-fungible IDs of a given non-fungible resource. Returned response is in a paginated format, ordered by their first appearance on the ledger. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleIdsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateNonFungibleIdsResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateNonFungibleIdsResponse>> NonFungibleIdsWithHttpInfoAsync(StateNonFungibleIdsRequest stateNonFungibleIdsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Non-Fungible Location
        /// </summary>
        /// <remarks>
        /// Returns location of a given non-fungible ID. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleLocationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateNonFungibleLocationResponse</returns>
        System.Threading.Tasks.Task<StateNonFungibleLocationResponse> NonFungibleLocationAsync(StateNonFungibleLocationRequest stateNonFungibleLocationRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Non-Fungible Location
        /// </summary>
        /// <remarks>
        /// Returns location of a given non-fungible ID. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleLocationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateNonFungibleLocationResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateNonFungibleLocationResponse>> NonFungibleLocationWithHttpInfoAsync(StateNonFungibleLocationRequest stateNonFungibleLocationRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Entity Details
        /// </summary>
        /// <remarks>
        /// Returns detailed information for collection of entities. Aggregate resources globally by default. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityDetailsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEntityDetailsResponse</returns>
        System.Threading.Tasks.Task<StateEntityDetailsResponse> StateEntityDetailsAsync(StateEntityDetailsRequest stateEntityDetailsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Entity Details
        /// </summary>
        /// <remarks>
        /// Returns detailed information for collection of entities. Aggregate resources globally by default. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityDetailsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEntityDetailsResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateEntityDetailsResponse>> StateEntityDetailsWithHttpInfoAsync(StateEntityDetailsRequest stateEntityDetailsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Validators List
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorsListRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateValidatorsListResponse</returns>
        System.Threading.Tasks.Task<StateValidatorsListResponse> StateValidatorsListAsync(StateValidatorsListRequest stateValidatorsListRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Validators List
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorsListRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateValidatorsListResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<StateValidatorsListResponse>> StateValidatorsListWithHttpInfoAsync(StateValidatorsListRequest stateValidatorsListRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
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
        private RadixDlt.NetworkGateway.GatewayApiSdk.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

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
        /// Initializes a new instance of the <see cref="StateApi"/> class using Configuration object.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="configuration">An instance of Configuration.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public StateApi(RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration configuration)
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
        public StateApi(HttpClient client, RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration configuration, HttpClientHandler handler = null)
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
        /// Initializes a new instance of the <see cref="StateApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public StateApi(RadixDlt.NetworkGateway.GatewayApiSdk.Client.ISynchronousClient client, RadixDlt.NetworkGateway.GatewayApiSdk.Client.IAsynchronousClient asyncClient, RadixDlt.NetworkGateway.GatewayApiSdk.Client.IReadableConfiguration configuration)
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
        /// Get page of Global Entity Fungible Resource Vaults Returns vaults for fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungibleResourceVaultsPageRequest"></param>
        /// <returns>StateEntityFungibleResourceVaultsPageResponse</returns>
        public StateEntityFungibleResourceVaultsPageResponse EntityFungibleResourceVaultPage(StateEntityFungibleResourceVaultsPageRequest stateEntityFungibleResourceVaultsPageRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityFungibleResourceVaultsPageResponse> localVarResponse = EntityFungibleResourceVaultPageWithHttpInfo(stateEntityFungibleResourceVaultsPageRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get page of Global Entity Fungible Resource Vaults Returns vaults for fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungibleResourceVaultsPageRequest"></param>
        /// <returns>ApiResponse of StateEntityFungibleResourceVaultsPageResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityFungibleResourceVaultsPageResponse> EntityFungibleResourceVaultPageWithHttpInfo(StateEntityFungibleResourceVaultsPageRequest stateEntityFungibleResourceVaultsPageRequest)
        {
            // verify the required parameter 'stateEntityFungibleResourceVaultsPageRequest' is set
            if (stateEntityFungibleResourceVaultsPageRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateEntityFungibleResourceVaultsPageRequest' when calling StateApi->EntityFungibleResourceVaultPage");

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

            localVarRequestOptions.Data = stateEntityFungibleResourceVaultsPageRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateEntityFungibleResourceVaultsPageResponse>("/state/entity/page/fungible-vaults/", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntityFungibleResourceVaultPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get page of Global Entity Fungible Resource Vaults Returns vaults for fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungibleResourceVaultsPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEntityFungibleResourceVaultsPageResponse</returns>
        public async System.Threading.Tasks.Task<StateEntityFungibleResourceVaultsPageResponse> EntityFungibleResourceVaultPageAsync(StateEntityFungibleResourceVaultsPageRequest stateEntityFungibleResourceVaultsPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityFungibleResourceVaultsPageResponse> localVarResponse = await EntityFungibleResourceVaultPageWithHttpInfoAsync(stateEntityFungibleResourceVaultsPageRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get page of Global Entity Fungible Resource Vaults Returns vaults for fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungibleResourceVaultsPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEntityFungibleResourceVaultsPageResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityFungibleResourceVaultsPageResponse>> EntityFungibleResourceVaultPageWithHttpInfoAsync(StateEntityFungibleResourceVaultsPageRequest stateEntityFungibleResourceVaultsPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateEntityFungibleResourceVaultsPageRequest' is set
            if (stateEntityFungibleResourceVaultsPageRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateEntityFungibleResourceVaultsPageRequest' when calling StateApi->EntityFungibleResourceVaultPage");


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

            localVarRequestOptions.Data = stateEntityFungibleResourceVaultsPageRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateEntityFungibleResourceVaultsPageResponse>("/state/entity/page/fungible-vaults/", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntityFungibleResourceVaultPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get page of Global Entity Fungible Resource Balances Returns the total amount of each fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungiblesPageRequest"></param>
        /// <returns>StateEntityFungiblesPageResponse</returns>
        public StateEntityFungiblesPageResponse EntityFungiblesPage(StateEntityFungiblesPageRequest stateEntityFungiblesPageRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityFungiblesPageResponse> localVarResponse = EntityFungiblesPageWithHttpInfo(stateEntityFungiblesPageRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get page of Global Entity Fungible Resource Balances Returns the total amount of each fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungiblesPageRequest"></param>
        /// <returns>ApiResponse of StateEntityFungiblesPageResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityFungiblesPageResponse> EntityFungiblesPageWithHttpInfo(StateEntityFungiblesPageRequest stateEntityFungiblesPageRequest)
        {
            // verify the required parameter 'stateEntityFungiblesPageRequest' is set
            if (stateEntityFungiblesPageRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateEntityFungiblesPageRequest' when calling StateApi->EntityFungiblesPage");

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

            localVarRequestOptions.Data = stateEntityFungiblesPageRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateEntityFungiblesPageResponse>("/state/entity/page/fungibles/", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntityFungiblesPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get page of Global Entity Fungible Resource Balances Returns the total amount of each fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungiblesPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEntityFungiblesPageResponse</returns>
        public async System.Threading.Tasks.Task<StateEntityFungiblesPageResponse> EntityFungiblesPageAsync(StateEntityFungiblesPageRequest stateEntityFungiblesPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityFungiblesPageResponse> localVarResponse = await EntityFungiblesPageWithHttpInfoAsync(stateEntityFungiblesPageRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get page of Global Entity Fungible Resource Balances Returns the total amount of each fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityFungiblesPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEntityFungiblesPageResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityFungiblesPageResponse>> EntityFungiblesPageWithHttpInfoAsync(StateEntityFungiblesPageRequest stateEntityFungiblesPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateEntityFungiblesPageRequest' is set
            if (stateEntityFungiblesPageRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateEntityFungiblesPageRequest' when calling StateApi->EntityFungiblesPage");


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

            localVarRequestOptions.Data = stateEntityFungiblesPageRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateEntityFungiblesPageResponse>("/state/entity/page/fungibles/", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntityFungiblesPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Entity Metadata Page Returns all the metadata properties associated with a given global entity. The returned response is in a paginated format, ordered by first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityMetadataPageRequest"></param>
        /// <returns>StateEntityMetadataPageResponse</returns>
        public StateEntityMetadataPageResponse EntityMetadataPage(StateEntityMetadataPageRequest stateEntityMetadataPageRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityMetadataPageResponse> localVarResponse = EntityMetadataPageWithHttpInfo(stateEntityMetadataPageRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Entity Metadata Page Returns all the metadata properties associated with a given global entity. The returned response is in a paginated format, ordered by first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityMetadataPageRequest"></param>
        /// <returns>ApiResponse of StateEntityMetadataPageResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityMetadataPageResponse> EntityMetadataPageWithHttpInfo(StateEntityMetadataPageRequest stateEntityMetadataPageRequest)
        {
            // verify the required parameter 'stateEntityMetadataPageRequest' is set
            if (stateEntityMetadataPageRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateEntityMetadataPageRequest' when calling StateApi->EntityMetadataPage");

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

            localVarRequestOptions.Data = stateEntityMetadataPageRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateEntityMetadataPageResponse>("/state/entity/page/metadata", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntityMetadataPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Entity Metadata Page Returns all the metadata properties associated with a given global entity. The returned response is in a paginated format, ordered by first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityMetadataPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEntityMetadataPageResponse</returns>
        public async System.Threading.Tasks.Task<StateEntityMetadataPageResponse> EntityMetadataPageAsync(StateEntityMetadataPageRequest stateEntityMetadataPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityMetadataPageResponse> localVarResponse = await EntityMetadataPageWithHttpInfoAsync(stateEntityMetadataPageRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Entity Metadata Page Returns all the metadata properties associated with a given global entity. The returned response is in a paginated format, ordered by first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityMetadataPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEntityMetadataPageResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityMetadataPageResponse>> EntityMetadataPageWithHttpInfoAsync(StateEntityMetadataPageRequest stateEntityMetadataPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateEntityMetadataPageRequest' is set
            if (stateEntityMetadataPageRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateEntityMetadataPageRequest' when calling StateApi->EntityMetadataPage");


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

            localVarRequestOptions.Data = stateEntityMetadataPageRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateEntityMetadataPageResponse>("/state/entity/page/metadata", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntityMetadataPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get page of Non-Fungibles in Vault Returns all non-fungible IDs of a given non-fungible resource owned by a given entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearence on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleIdsPageRequest"></param>
        /// <returns>StateEntityNonFungibleIdsPageResponse</returns>
        public StateEntityNonFungibleIdsPageResponse EntityNonFungibleIdsPage(StateEntityNonFungibleIdsPageRequest stateEntityNonFungibleIdsPageRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityNonFungibleIdsPageResponse> localVarResponse = EntityNonFungibleIdsPageWithHttpInfo(stateEntityNonFungibleIdsPageRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get page of Non-Fungibles in Vault Returns all non-fungible IDs of a given non-fungible resource owned by a given entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearence on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleIdsPageRequest"></param>
        /// <returns>ApiResponse of StateEntityNonFungibleIdsPageResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityNonFungibleIdsPageResponse> EntityNonFungibleIdsPageWithHttpInfo(StateEntityNonFungibleIdsPageRequest stateEntityNonFungibleIdsPageRequest)
        {
            // verify the required parameter 'stateEntityNonFungibleIdsPageRequest' is set
            if (stateEntityNonFungibleIdsPageRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateEntityNonFungibleIdsPageRequest' when calling StateApi->EntityNonFungibleIdsPage");

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

            localVarRequestOptions.Data = stateEntityNonFungibleIdsPageRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateEntityNonFungibleIdsPageResponse>("/state/entity/page/non-fungible-vault/ids", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntityNonFungibleIdsPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get page of Non-Fungibles in Vault Returns all non-fungible IDs of a given non-fungible resource owned by a given entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearence on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleIdsPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEntityNonFungibleIdsPageResponse</returns>
        public async System.Threading.Tasks.Task<StateEntityNonFungibleIdsPageResponse> EntityNonFungibleIdsPageAsync(StateEntityNonFungibleIdsPageRequest stateEntityNonFungibleIdsPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityNonFungibleIdsPageResponse> localVarResponse = await EntityNonFungibleIdsPageWithHttpInfoAsync(stateEntityNonFungibleIdsPageRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get page of Non-Fungibles in Vault Returns all non-fungible IDs of a given non-fungible resource owned by a given entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearence on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleIdsPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEntityNonFungibleIdsPageResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityNonFungibleIdsPageResponse>> EntityNonFungibleIdsPageWithHttpInfoAsync(StateEntityNonFungibleIdsPageRequest stateEntityNonFungibleIdsPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateEntityNonFungibleIdsPageRequest' is set
            if (stateEntityNonFungibleIdsPageRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateEntityNonFungibleIdsPageRequest' when calling StateApi->EntityNonFungibleIdsPage");


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

            localVarRequestOptions.Data = stateEntityNonFungibleIdsPageRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateEntityNonFungibleIdsPageResponse>("/state/entity/page/non-fungible-vault/ids", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntityNonFungibleIdsPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Vaults Returns vaults for non fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleResourceVaultsPageRequest"></param>
        /// <returns>StateEntityNonFungibleResourceVaultsPageResponse</returns>
        public StateEntityNonFungibleResourceVaultsPageResponse EntityNonFungibleResourceVaultPage(StateEntityNonFungibleResourceVaultsPageRequest stateEntityNonFungibleResourceVaultsPageRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityNonFungibleResourceVaultsPageResponse> localVarResponse = EntityNonFungibleResourceVaultPageWithHttpInfo(stateEntityNonFungibleResourceVaultsPageRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Vaults Returns vaults for non fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleResourceVaultsPageRequest"></param>
        /// <returns>ApiResponse of StateEntityNonFungibleResourceVaultsPageResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityNonFungibleResourceVaultsPageResponse> EntityNonFungibleResourceVaultPageWithHttpInfo(StateEntityNonFungibleResourceVaultsPageRequest stateEntityNonFungibleResourceVaultsPageRequest)
        {
            // verify the required parameter 'stateEntityNonFungibleResourceVaultsPageRequest' is set
            if (stateEntityNonFungibleResourceVaultsPageRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateEntityNonFungibleResourceVaultsPageRequest' when calling StateApi->EntityNonFungibleResourceVaultPage");

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

            localVarRequestOptions.Data = stateEntityNonFungibleResourceVaultsPageRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateEntityNonFungibleResourceVaultsPageResponse>("/state/entity/page/non-fungible-vaults/", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntityNonFungibleResourceVaultPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Vaults Returns vaults for non fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleResourceVaultsPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEntityNonFungibleResourceVaultsPageResponse</returns>
        public async System.Threading.Tasks.Task<StateEntityNonFungibleResourceVaultsPageResponse> EntityNonFungibleResourceVaultPageAsync(StateEntityNonFungibleResourceVaultsPageRequest stateEntityNonFungibleResourceVaultsPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityNonFungibleResourceVaultsPageResponse> localVarResponse = await EntityNonFungibleResourceVaultPageWithHttpInfoAsync(stateEntityNonFungibleResourceVaultsPageRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Vaults Returns vaults for non fungible resource owned by a given global entity. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungibleResourceVaultsPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEntityNonFungibleResourceVaultsPageResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityNonFungibleResourceVaultsPageResponse>> EntityNonFungibleResourceVaultPageWithHttpInfoAsync(StateEntityNonFungibleResourceVaultsPageRequest stateEntityNonFungibleResourceVaultsPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateEntityNonFungibleResourceVaultsPageRequest' is set
            if (stateEntityNonFungibleResourceVaultsPageRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateEntityNonFungibleResourceVaultsPageRequest' when calling StateApi->EntityNonFungibleResourceVaultPage");


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

            localVarRequestOptions.Data = stateEntityNonFungibleResourceVaultsPageRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateEntityNonFungibleResourceVaultsPageResponse>("/state/entity/page/non-fungible-vaults/", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntityNonFungibleResourceVaultPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Balances Returns the total amount of each non-fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungiblesPageRequest"></param>
        /// <returns>StateEntityNonFungiblesPageResponse</returns>
        public StateEntityNonFungiblesPageResponse EntityNonFungiblesPage(StateEntityNonFungiblesPageRequest stateEntityNonFungiblesPageRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityNonFungiblesPageResponse> localVarResponse = EntityNonFungiblesPageWithHttpInfo(stateEntityNonFungiblesPageRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Balances Returns the total amount of each non-fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungiblesPageRequest"></param>
        /// <returns>ApiResponse of StateEntityNonFungiblesPageResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityNonFungiblesPageResponse> EntityNonFungiblesPageWithHttpInfo(StateEntityNonFungiblesPageRequest stateEntityNonFungiblesPageRequest)
        {
            // verify the required parameter 'stateEntityNonFungiblesPageRequest' is set
            if (stateEntityNonFungiblesPageRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateEntityNonFungiblesPageRequest' when calling StateApi->EntityNonFungiblesPage");

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

            localVarRequestOptions.Data = stateEntityNonFungiblesPageRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateEntityNonFungiblesPageResponse>("/state/entity/page/non-fungibles/", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntityNonFungiblesPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Balances Returns the total amount of each non-fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungiblesPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEntityNonFungiblesPageResponse</returns>
        public async System.Threading.Tasks.Task<StateEntityNonFungiblesPageResponse> EntityNonFungiblesPageAsync(StateEntityNonFungiblesPageRequest stateEntityNonFungiblesPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityNonFungiblesPageResponse> localVarResponse = await EntityNonFungiblesPageWithHttpInfoAsync(stateEntityNonFungiblesPageRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get page of Global Entity Non-Fungible Resource Balances Returns the total amount of each non-fungible resource owned by a given global entity. Result can be aggregated globally or per vault. The returned response is in a paginated format, ordered by the resource&#39;s first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityNonFungiblesPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEntityNonFungiblesPageResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityNonFungiblesPageResponse>> EntityNonFungiblesPageWithHttpInfoAsync(StateEntityNonFungiblesPageRequest stateEntityNonFungiblesPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateEntityNonFungiblesPageRequest' is set
            if (stateEntityNonFungiblesPageRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateEntityNonFungiblesPageRequest' when calling StateApi->EntityNonFungiblesPage");


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

            localVarRequestOptions.Data = stateEntityNonFungiblesPageRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateEntityNonFungiblesPageResponse>("/state/entity/page/non-fungibles/", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntityNonFungiblesPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get KeyValueStore Data Returns data (value) associated with a given key of a given key-value store. [Check detailed documentation for explanation](#section/How-to-query-the-content-of-a-key-value-store-inside-a-component) 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateKeyValueStoreDataRequest"></param>
        /// <returns>StateKeyValueStoreDataResponse</returns>
        public StateKeyValueStoreDataResponse KeyValueStoreData(StateKeyValueStoreDataRequest stateKeyValueStoreDataRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateKeyValueStoreDataResponse> localVarResponse = KeyValueStoreDataWithHttpInfo(stateKeyValueStoreDataRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get KeyValueStore Data Returns data (value) associated with a given key of a given key-value store. [Check detailed documentation for explanation](#section/How-to-query-the-content-of-a-key-value-store-inside-a-component) 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateKeyValueStoreDataRequest"></param>
        /// <returns>ApiResponse of StateKeyValueStoreDataResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateKeyValueStoreDataResponse> KeyValueStoreDataWithHttpInfo(StateKeyValueStoreDataRequest stateKeyValueStoreDataRequest)
        {
            // verify the required parameter 'stateKeyValueStoreDataRequest' is set
            if (stateKeyValueStoreDataRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateKeyValueStoreDataRequest' when calling StateApi->KeyValueStoreData");

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

            localVarRequestOptions.Data = stateKeyValueStoreDataRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateKeyValueStoreDataResponse>("/state/key-value-store/data", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("KeyValueStoreData", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get KeyValueStore Data Returns data (value) associated with a given key of a given key-value store. [Check detailed documentation for explanation](#section/How-to-query-the-content-of-a-key-value-store-inside-a-component) 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateKeyValueStoreDataRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateKeyValueStoreDataResponse</returns>
        public async System.Threading.Tasks.Task<StateKeyValueStoreDataResponse> KeyValueStoreDataAsync(StateKeyValueStoreDataRequest stateKeyValueStoreDataRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateKeyValueStoreDataResponse> localVarResponse = await KeyValueStoreDataWithHttpInfoAsync(stateKeyValueStoreDataRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get KeyValueStore Data Returns data (value) associated with a given key of a given key-value store. [Check detailed documentation for explanation](#section/How-to-query-the-content-of-a-key-value-store-inside-a-component) 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateKeyValueStoreDataRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateKeyValueStoreDataResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateKeyValueStoreDataResponse>> KeyValueStoreDataWithHttpInfoAsync(StateKeyValueStoreDataRequest stateKeyValueStoreDataRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateKeyValueStoreDataRequest' is set
            if (stateKeyValueStoreDataRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateKeyValueStoreDataRequest' when calling StateApi->KeyValueStoreData");


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

            localVarRequestOptions.Data = stateKeyValueStoreDataRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateKeyValueStoreDataResponse>("/state/key-value-store/data", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("KeyValueStoreData", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Non-Fungible Data Returns data associated with a given non-fungible ID of a given non-fungible resource. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleDataRequest"></param>
        /// <returns>StateNonFungibleDataResponse</returns>
        public StateNonFungibleDataResponse NonFungibleData(StateNonFungibleDataRequest stateNonFungibleDataRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateNonFungibleDataResponse> localVarResponse = NonFungibleDataWithHttpInfo(stateNonFungibleDataRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Non-Fungible Data Returns data associated with a given non-fungible ID of a given non-fungible resource. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleDataRequest"></param>
        /// <returns>ApiResponse of StateNonFungibleDataResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateNonFungibleDataResponse> NonFungibleDataWithHttpInfo(StateNonFungibleDataRequest stateNonFungibleDataRequest)
        {
            // verify the required parameter 'stateNonFungibleDataRequest' is set
            if (stateNonFungibleDataRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateNonFungibleDataRequest' when calling StateApi->NonFungibleData");

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

            localVarRequestOptions.Data = stateNonFungibleDataRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateNonFungibleDataResponse>("/state/non-fungible/data", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("NonFungibleData", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Non-Fungible Data Returns data associated with a given non-fungible ID of a given non-fungible resource. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleDataRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateNonFungibleDataResponse</returns>
        public async System.Threading.Tasks.Task<StateNonFungibleDataResponse> NonFungibleDataAsync(StateNonFungibleDataRequest stateNonFungibleDataRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateNonFungibleDataResponse> localVarResponse = await NonFungibleDataWithHttpInfoAsync(stateNonFungibleDataRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Non-Fungible Data Returns data associated with a given non-fungible ID of a given non-fungible resource. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleDataRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateNonFungibleDataResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateNonFungibleDataResponse>> NonFungibleDataWithHttpInfoAsync(StateNonFungibleDataRequest stateNonFungibleDataRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateNonFungibleDataRequest' is set
            if (stateNonFungibleDataRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateNonFungibleDataRequest' when calling StateApi->NonFungibleData");


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

            localVarRequestOptions.Data = stateNonFungibleDataRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateNonFungibleDataResponse>("/state/non-fungible/data", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("NonFungibleData", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get page of Non-Fungible Ids in Resource Collection Returns the non-fungible IDs of a given non-fungible resource. Returned response is in a paginated format, ordered by their first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleIdsRequest"></param>
        /// <returns>StateNonFungibleIdsResponse</returns>
        public StateNonFungibleIdsResponse NonFungibleIds(StateNonFungibleIdsRequest stateNonFungibleIdsRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateNonFungibleIdsResponse> localVarResponse = NonFungibleIdsWithHttpInfo(stateNonFungibleIdsRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get page of Non-Fungible Ids in Resource Collection Returns the non-fungible IDs of a given non-fungible resource. Returned response is in a paginated format, ordered by their first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleIdsRequest"></param>
        /// <returns>ApiResponse of StateNonFungibleIdsResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateNonFungibleIdsResponse> NonFungibleIdsWithHttpInfo(StateNonFungibleIdsRequest stateNonFungibleIdsRequest)
        {
            // verify the required parameter 'stateNonFungibleIdsRequest' is set
            if (stateNonFungibleIdsRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateNonFungibleIdsRequest' when calling StateApi->NonFungibleIds");

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

            localVarRequestOptions.Data = stateNonFungibleIdsRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateNonFungibleIdsResponse>("/state/non-fungible/ids", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("NonFungibleIds", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get page of Non-Fungible Ids in Resource Collection Returns the non-fungible IDs of a given non-fungible resource. Returned response is in a paginated format, ordered by their first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleIdsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateNonFungibleIdsResponse</returns>
        public async System.Threading.Tasks.Task<StateNonFungibleIdsResponse> NonFungibleIdsAsync(StateNonFungibleIdsRequest stateNonFungibleIdsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateNonFungibleIdsResponse> localVarResponse = await NonFungibleIdsWithHttpInfoAsync(stateNonFungibleIdsRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get page of Non-Fungible Ids in Resource Collection Returns the non-fungible IDs of a given non-fungible resource. Returned response is in a paginated format, ordered by their first appearance on the ledger. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleIdsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateNonFungibleIdsResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateNonFungibleIdsResponse>> NonFungibleIdsWithHttpInfoAsync(StateNonFungibleIdsRequest stateNonFungibleIdsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateNonFungibleIdsRequest' is set
            if (stateNonFungibleIdsRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateNonFungibleIdsRequest' when calling StateApi->NonFungibleIds");


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

            localVarRequestOptions.Data = stateNonFungibleIdsRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateNonFungibleIdsResponse>("/state/non-fungible/ids", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("NonFungibleIds", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Non-Fungible Location Returns location of a given non-fungible ID. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleLocationRequest"></param>
        /// <returns>StateNonFungibleLocationResponse</returns>
        public StateNonFungibleLocationResponse NonFungibleLocation(StateNonFungibleLocationRequest stateNonFungibleLocationRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateNonFungibleLocationResponse> localVarResponse = NonFungibleLocationWithHttpInfo(stateNonFungibleLocationRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Non-Fungible Location Returns location of a given non-fungible ID. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleLocationRequest"></param>
        /// <returns>ApiResponse of StateNonFungibleLocationResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateNonFungibleLocationResponse> NonFungibleLocationWithHttpInfo(StateNonFungibleLocationRequest stateNonFungibleLocationRequest)
        {
            // verify the required parameter 'stateNonFungibleLocationRequest' is set
            if (stateNonFungibleLocationRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateNonFungibleLocationRequest' when calling StateApi->NonFungibleLocation");

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

            localVarRequestOptions.Data = stateNonFungibleLocationRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateNonFungibleLocationResponse>("/state/non-fungible/location", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("NonFungibleLocation", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Non-Fungible Location Returns location of a given non-fungible ID. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleLocationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateNonFungibleLocationResponse</returns>
        public async System.Threading.Tasks.Task<StateNonFungibleLocationResponse> NonFungibleLocationAsync(StateNonFungibleLocationRequest stateNonFungibleLocationRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateNonFungibleLocationResponse> localVarResponse = await NonFungibleLocationWithHttpInfoAsync(stateNonFungibleLocationRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Non-Fungible Location Returns location of a given non-fungible ID. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateNonFungibleLocationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateNonFungibleLocationResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateNonFungibleLocationResponse>> NonFungibleLocationWithHttpInfoAsync(StateNonFungibleLocationRequest stateNonFungibleLocationRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateNonFungibleLocationRequest' is set
            if (stateNonFungibleLocationRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateNonFungibleLocationRequest' when calling StateApi->NonFungibleLocation");


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

            localVarRequestOptions.Data = stateNonFungibleLocationRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateNonFungibleLocationResponse>("/state/non-fungible/location", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("NonFungibleLocation", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Entity Details Returns detailed information for collection of entities. Aggregate resources globally by default. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityDetailsRequest"></param>
        /// <returns>StateEntityDetailsResponse</returns>
        public StateEntityDetailsResponse StateEntityDetails(StateEntityDetailsRequest stateEntityDetailsRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityDetailsResponse> localVarResponse = StateEntityDetailsWithHttpInfo(stateEntityDetailsRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Entity Details Returns detailed information for collection of entities. Aggregate resources globally by default. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityDetailsRequest"></param>
        /// <returns>ApiResponse of StateEntityDetailsResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityDetailsResponse> StateEntityDetailsWithHttpInfo(StateEntityDetailsRequest stateEntityDetailsRequest)
        {
            // verify the required parameter 'stateEntityDetailsRequest' is set
            if (stateEntityDetailsRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateEntityDetailsRequest' when calling StateApi->StateEntityDetails");

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

            localVarRequestOptions.Data = stateEntityDetailsRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateEntityDetailsResponse>("/state/entity/details", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateEntityDetails", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Entity Details Returns detailed information for collection of entities. Aggregate resources globally by default. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityDetailsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateEntityDetailsResponse</returns>
        public async System.Threading.Tasks.Task<StateEntityDetailsResponse> StateEntityDetailsAsync(StateEntityDetailsRequest stateEntityDetailsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityDetailsResponse> localVarResponse = await StateEntityDetailsWithHttpInfoAsync(stateEntityDetailsRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Entity Details Returns detailed information for collection of entities. Aggregate resources globally by default. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateEntityDetailsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateEntityDetailsResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateEntityDetailsResponse>> StateEntityDetailsWithHttpInfoAsync(StateEntityDetailsRequest stateEntityDetailsRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateEntityDetailsRequest' is set
            if (stateEntityDetailsRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateEntityDetailsRequest' when calling StateApi->StateEntityDetails");


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

            localVarRequestOptions.Data = stateEntityDetailsRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateEntityDetailsResponse>("/state/entity/details", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateEntityDetails", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Validators List 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorsListRequest"></param>
        /// <returns>StateValidatorsListResponse</returns>
        public StateValidatorsListResponse StateValidatorsList(StateValidatorsListRequest stateValidatorsListRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateValidatorsListResponse> localVarResponse = StateValidatorsListWithHttpInfo(stateValidatorsListRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Validators List 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorsListRequest"></param>
        /// <returns>ApiResponse of StateValidatorsListResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateValidatorsListResponse> StateValidatorsListWithHttpInfo(StateValidatorsListRequest stateValidatorsListRequest)
        {
            // verify the required parameter 'stateValidatorsListRequest' is set
            if (stateValidatorsListRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateValidatorsListRequest' when calling StateApi->StateValidatorsList");

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

            localVarRequestOptions.Data = stateValidatorsListRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<StateValidatorsListResponse>("/state/validators/list", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateValidatorsList", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Validators List 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorsListRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of StateValidatorsListResponse</returns>
        public async System.Threading.Tasks.Task<StateValidatorsListResponse> StateValidatorsListAsync(StateValidatorsListRequest stateValidatorsListRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateValidatorsListResponse> localVarResponse = await StateValidatorsListWithHttpInfoAsync(stateValidatorsListRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Validators List 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="stateValidatorsListRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (StateValidatorsListResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<StateValidatorsListResponse>> StateValidatorsListWithHttpInfoAsync(StateValidatorsListRequest stateValidatorsListRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'stateValidatorsListRequest' is set
            if (stateValidatorsListRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'stateValidatorsListRequest' when calling StateApi->StateValidatorsList");


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

            localVarRequestOptions.Data = stateValidatorsListRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<StateValidatorsListResponse>("/state/validators/list", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("StateValidatorsList", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

    }
}
