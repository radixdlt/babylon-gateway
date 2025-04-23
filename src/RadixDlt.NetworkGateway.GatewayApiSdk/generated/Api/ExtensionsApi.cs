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
 * The version of the OpenAPI document: v1.10.2
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
    public interface IExtensionsApiSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Get entities by role requirement lookup
        /// </summary>
        /// <remarks>
        /// This endpoint is intended to query for entities that have ever used a given requirement (resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by multiple requirements. A maximum of &#x60;50&#x60; requirements can be queried. A maximum of 20 entities per requirement will be returned. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementLookupRequest"></param>
        /// <returns>EntitiesByRoleRequirementLookupResponse</returns>
        EntitiesByRoleRequirementLookupResponse EntitiesByRoleRequirementLookup(EntitiesByRoleRequirementLookupRequest entitiesByRoleRequirementLookupRequest);

        /// <summary>
        /// Get entities by role requirement lookup
        /// </summary>
        /// <remarks>
        /// This endpoint is intended to query for entities that have ever used a given requirement (resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by multiple requirements. A maximum of &#x60;50&#x60; requirements can be queried. A maximum of 20 entities per requirement will be returned. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementLookupRequest"></param>
        /// <returns>ApiResponse of EntitiesByRoleRequirementLookupResponse</returns>
        ApiResponse<EntitiesByRoleRequirementLookupResponse> EntitiesByRoleRequirementLookupWithHttpInfo(EntitiesByRoleRequirementLookupRequest entitiesByRoleRequirementLookupRequest);
        /// <summary>
        /// Get entities by role requirement page
        /// </summary>
        /// <remarks>
        /// Paginated endpoint returns a page of global entities that have ever used the provided requirement (such as a resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by a single requirement. By default it returns up to 100 entries are returned per response. This limit can be increased to a maximum of 1000 entries per page using the &#x60;limit_per_page&#x60; parameter. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  To lookup multiple requirements, please call the &#x60;/extensions/entities-by-role-requirement/lookup&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementPageRequest"></param>
        /// <returns>EntitiesByRoleRequirementPageResponse</returns>
        EntitiesByRoleRequirementPageResponse EntitiesByRoleRequirementPage(EntitiesByRoleRequirementPageRequest entitiesByRoleRequirementPageRequest);

        /// <summary>
        /// Get entities by role requirement page
        /// </summary>
        /// <remarks>
        /// Paginated endpoint returns a page of global entities that have ever used the provided requirement (such as a resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by a single requirement. By default it returns up to 100 entries are returned per response. This limit can be increased to a maximum of 1000 entries per page using the &#x60;limit_per_page&#x60; parameter. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  To lookup multiple requirements, please call the &#x60;/extensions/entities-by-role-requirement/lookup&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementPageRequest"></param>
        /// <returns>ApiResponse of EntitiesByRoleRequirementPageResponse</returns>
        ApiResponse<EntitiesByRoleRequirementPageResponse> EntitiesByRoleRequirementPageWithHttpInfo(EntitiesByRoleRequirementPageRequest entitiesByRoleRequirementPageRequest);
        /// <summary>
        /// Resolve implicit requirement target from global non-fungible id
        /// </summary>
        /// <remarks>
        /// Access rules can include [implicit requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) referencing special system-reserved resource addresses, which have specific meanings for the Radix Engine and are not part of the standard authorization zone system. These implicit requirements typically store their details as a hash, which means the subject of the requirement can&#39;t be easily resolved. This is where this endpoint comes in. It can resolve the subject of the implicit requirements, using a database of reverse hash lookups populated from ledger data.  The following [resource addresses](https://docs.radixdlt.com/docs/well-known-addresses) are supported: - **Secp256k1 Signature Resource** - **Ed25519 Signature Resource** - **Package of Direct Caller Resource** - **Global Caller Resource** - **System Execution Resource**  When querying, you must provide a pair of the following for each requirement to resolve: - &#x60;resource_address&#x60; (one of the above) - &#x60;non_fungible_id&#x60;,  of the requirement.  You can query a maximum of &#x60;100&#x60; implicit requirements at a time.  See the documentation on [implicit-requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) for more information. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="implicitRequirementsLookupRequest"></param>
        /// <returns>ImplicitRequirementsLookupResponse</returns>
        ImplicitRequirementsLookupResponse ImplicitRequirementsLookup(ImplicitRequirementsLookupRequest implicitRequirementsLookupRequest);

        /// <summary>
        /// Resolve implicit requirement target from global non-fungible id
        /// </summary>
        /// <remarks>
        /// Access rules can include [implicit requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) referencing special system-reserved resource addresses, which have specific meanings for the Radix Engine and are not part of the standard authorization zone system. These implicit requirements typically store their details as a hash, which means the subject of the requirement can&#39;t be easily resolved. This is where this endpoint comes in. It can resolve the subject of the implicit requirements, using a database of reverse hash lookups populated from ledger data.  The following [resource addresses](https://docs.radixdlt.com/docs/well-known-addresses) are supported: - **Secp256k1 Signature Resource** - **Ed25519 Signature Resource** - **Package of Direct Caller Resource** - **Global Caller Resource** - **System Execution Resource**  When querying, you must provide a pair of the following for each requirement to resolve: - &#x60;resource_address&#x60; (one of the above) - &#x60;non_fungible_id&#x60;,  of the requirement.  You can query a maximum of &#x60;100&#x60; implicit requirements at a time.  See the documentation on [implicit-requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) for more information. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="implicitRequirementsLookupRequest"></param>
        /// <returns>ApiResponse of ImplicitRequirementsLookupResponse</returns>
        ApiResponse<ImplicitRequirementsLookupResponse> ImplicitRequirementsLookupWithHttpInfo(ImplicitRequirementsLookupRequest implicitRequirementsLookupRequest);
        /// <summary>
        /// Get Resource Holders Page
        /// </summary>
        /// <remarks>
        /// A paginated endpoint to discover which global entities hold the most of a given resource. More specifically, it returns a page of global entities which hold the given resource, ordered descending by the total fungible balance / total count of non-fungibles stored in vaults in the state tree of that entity (excluding unclaimed royalty balances).  This endpoint operates only at the **current state version**, it is not possible to browse historical data. Because of that, it is not possible to offer stable pagination as data constantly changes. Balances might change between pages being read, which might result in gaps or some entries being returned twice.  Under default Gateway configuration, up to 100 entries are returned per response. This can be increased up to 1000 entries per page with the &#x60;limit_per_page&#x60; parameter. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="resourceHoldersRequest"></param>
        /// <returns>ResourceHoldersResponse</returns>
        ResourceHoldersResponse ResourceHoldersPage(ResourceHoldersRequest resourceHoldersRequest);

        /// <summary>
        /// Get Resource Holders Page
        /// </summary>
        /// <remarks>
        /// A paginated endpoint to discover which global entities hold the most of a given resource. More specifically, it returns a page of global entities which hold the given resource, ordered descending by the total fungible balance / total count of non-fungibles stored in vaults in the state tree of that entity (excluding unclaimed royalty balances).  This endpoint operates only at the **current state version**, it is not possible to browse historical data. Because of that, it is not possible to offer stable pagination as data constantly changes. Balances might change between pages being read, which might result in gaps or some entries being returned twice.  Under default Gateway configuration, up to 100 entries are returned per response. This can be increased up to 1000 entries per page with the &#x60;limit_per_page&#x60; parameter. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="resourceHoldersRequest"></param>
        /// <returns>ApiResponse of ResourceHoldersResponse</returns>
        ApiResponse<ResourceHoldersResponse> ResourceHoldersPageWithHttpInfo(ResourceHoldersRequest resourceHoldersRequest);
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IExtensionsApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// Get entities by role requirement lookup
        /// </summary>
        /// <remarks>
        /// This endpoint is intended to query for entities that have ever used a given requirement (resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by multiple requirements. A maximum of &#x60;50&#x60; requirements can be queried. A maximum of 20 entities per requirement will be returned. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementLookupRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of EntitiesByRoleRequirementLookupResponse</returns>
        System.Threading.Tasks.Task<EntitiesByRoleRequirementLookupResponse> EntitiesByRoleRequirementLookupAsync(EntitiesByRoleRequirementLookupRequest entitiesByRoleRequirementLookupRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get entities by role requirement lookup
        /// </summary>
        /// <remarks>
        /// This endpoint is intended to query for entities that have ever used a given requirement (resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by multiple requirements. A maximum of &#x60;50&#x60; requirements can be queried. A maximum of 20 entities per requirement will be returned. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementLookupRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (EntitiesByRoleRequirementLookupResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<EntitiesByRoleRequirementLookupResponse>> EntitiesByRoleRequirementLookupWithHttpInfoAsync(EntitiesByRoleRequirementLookupRequest entitiesByRoleRequirementLookupRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get entities by role requirement page
        /// </summary>
        /// <remarks>
        /// Paginated endpoint returns a page of global entities that have ever used the provided requirement (such as a resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by a single requirement. By default it returns up to 100 entries are returned per response. This limit can be increased to a maximum of 1000 entries per page using the &#x60;limit_per_page&#x60; parameter. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  To lookup multiple requirements, please call the &#x60;/extensions/entities-by-role-requirement/lookup&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of EntitiesByRoleRequirementPageResponse</returns>
        System.Threading.Tasks.Task<EntitiesByRoleRequirementPageResponse> EntitiesByRoleRequirementPageAsync(EntitiesByRoleRequirementPageRequest entitiesByRoleRequirementPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get entities by role requirement page
        /// </summary>
        /// <remarks>
        /// Paginated endpoint returns a page of global entities that have ever used the provided requirement (such as a resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by a single requirement. By default it returns up to 100 entries are returned per response. This limit can be increased to a maximum of 1000 entries per page using the &#x60;limit_per_page&#x60; parameter. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  To lookup multiple requirements, please call the &#x60;/extensions/entities-by-role-requirement/lookup&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (EntitiesByRoleRequirementPageResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<EntitiesByRoleRequirementPageResponse>> EntitiesByRoleRequirementPageWithHttpInfoAsync(EntitiesByRoleRequirementPageRequest entitiesByRoleRequirementPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Resolve implicit requirement target from global non-fungible id
        /// </summary>
        /// <remarks>
        /// Access rules can include [implicit requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) referencing special system-reserved resource addresses, which have specific meanings for the Radix Engine and are not part of the standard authorization zone system. These implicit requirements typically store their details as a hash, which means the subject of the requirement can&#39;t be easily resolved. This is where this endpoint comes in. It can resolve the subject of the implicit requirements, using a database of reverse hash lookups populated from ledger data.  The following [resource addresses](https://docs.radixdlt.com/docs/well-known-addresses) are supported: - **Secp256k1 Signature Resource** - **Ed25519 Signature Resource** - **Package of Direct Caller Resource** - **Global Caller Resource** - **System Execution Resource**  When querying, you must provide a pair of the following for each requirement to resolve: - &#x60;resource_address&#x60; (one of the above) - &#x60;non_fungible_id&#x60;,  of the requirement.  You can query a maximum of &#x60;100&#x60; implicit requirements at a time.  See the documentation on [implicit-requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) for more information. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="implicitRequirementsLookupRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ImplicitRequirementsLookupResponse</returns>
        System.Threading.Tasks.Task<ImplicitRequirementsLookupResponse> ImplicitRequirementsLookupAsync(ImplicitRequirementsLookupRequest implicitRequirementsLookupRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Resolve implicit requirement target from global non-fungible id
        /// </summary>
        /// <remarks>
        /// Access rules can include [implicit requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) referencing special system-reserved resource addresses, which have specific meanings for the Radix Engine and are not part of the standard authorization zone system. These implicit requirements typically store their details as a hash, which means the subject of the requirement can&#39;t be easily resolved. This is where this endpoint comes in. It can resolve the subject of the implicit requirements, using a database of reverse hash lookups populated from ledger data.  The following [resource addresses](https://docs.radixdlt.com/docs/well-known-addresses) are supported: - **Secp256k1 Signature Resource** - **Ed25519 Signature Resource** - **Package of Direct Caller Resource** - **Global Caller Resource** - **System Execution Resource**  When querying, you must provide a pair of the following for each requirement to resolve: - &#x60;resource_address&#x60; (one of the above) - &#x60;non_fungible_id&#x60;,  of the requirement.  You can query a maximum of &#x60;100&#x60; implicit requirements at a time.  See the documentation on [implicit-requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) for more information. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="implicitRequirementsLookupRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (ImplicitRequirementsLookupResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<ImplicitRequirementsLookupResponse>> ImplicitRequirementsLookupWithHttpInfoAsync(ImplicitRequirementsLookupRequest implicitRequirementsLookupRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get Resource Holders Page
        /// </summary>
        /// <remarks>
        /// A paginated endpoint to discover which global entities hold the most of a given resource. More specifically, it returns a page of global entities which hold the given resource, ordered descending by the total fungible balance / total count of non-fungibles stored in vaults in the state tree of that entity (excluding unclaimed royalty balances).  This endpoint operates only at the **current state version**, it is not possible to browse historical data. Because of that, it is not possible to offer stable pagination as data constantly changes. Balances might change between pages being read, which might result in gaps or some entries being returned twice.  Under default Gateway configuration, up to 100 entries are returned per response. This can be increased up to 1000 entries per page with the &#x60;limit_per_page&#x60; parameter. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="resourceHoldersRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ResourceHoldersResponse</returns>
        System.Threading.Tasks.Task<ResourceHoldersResponse> ResourceHoldersPageAsync(ResourceHoldersRequest resourceHoldersRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get Resource Holders Page
        /// </summary>
        /// <remarks>
        /// A paginated endpoint to discover which global entities hold the most of a given resource. More specifically, it returns a page of global entities which hold the given resource, ordered descending by the total fungible balance / total count of non-fungibles stored in vaults in the state tree of that entity (excluding unclaimed royalty balances).  This endpoint operates only at the **current state version**, it is not possible to browse historical data. Because of that, it is not possible to offer stable pagination as data constantly changes. Balances might change between pages being read, which might result in gaps or some entries being returned twice.  Under default Gateway configuration, up to 100 entries are returned per response. This can be increased up to 1000 entries per page with the &#x60;limit_per_page&#x60; parameter. 
        /// </remarks>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="resourceHoldersRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (ResourceHoldersResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<ResourceHoldersResponse>> ResourceHoldersPageWithHttpInfoAsync(ResourceHoldersRequest resourceHoldersRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IExtensionsApi : IExtensionsApiSync, IExtensionsApiAsync
    {

    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class ExtensionsApi : IDisposable, IExtensionsApi
    {
        private RadixDlt.NetworkGateway.GatewayApiSdk.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionsApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <returns></returns>
        public ExtensionsApi() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionsApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="basePath">The target service's base path in URL format.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public ExtensionsApi(string basePath)
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
        /// Initializes a new instance of the <see cref="ExtensionsApi"/> class using Configuration object.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="configuration">An instance of Configuration.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public ExtensionsApi(RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration configuration)
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
        /// Initializes a new instance of the <see cref="ExtensionsApi"/> class.
        /// </summary>
        /// <param name="client">An instance of HttpClient.</param>
        /// <param name="handler">An optional instance of HttpClientHandler that is used by HttpClient.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        /// <remarks>
        /// Some configuration settings will not be applied without passing an HttpClientHandler.
        /// The features affected are: Setting and Retrieving Cookies, Client Certificates, Proxy settings.
        /// </remarks>
        public ExtensionsApi(HttpClient client, HttpClientHandler handler = null) : this(client, (string)null, handler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionsApi"/> class.
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
        public ExtensionsApi(HttpClient client, string basePath, HttpClientHandler handler = null)
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
        /// Initializes a new instance of the <see cref="ExtensionsApi"/> class using Configuration object.
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
        public ExtensionsApi(HttpClient client, RadixDlt.NetworkGateway.GatewayApiSdk.Client.Configuration configuration, HttpClientHandler handler = null)
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
        /// Initializes a new instance of the <see cref="ExtensionsApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ExtensionsApi(RadixDlt.NetworkGateway.GatewayApiSdk.Client.ISynchronousClient client, RadixDlt.NetworkGateway.GatewayApiSdk.Client.IAsynchronousClient asyncClient, RadixDlt.NetworkGateway.GatewayApiSdk.Client.IReadableConfiguration configuration)
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
        /// Get entities by role requirement lookup This endpoint is intended to query for entities that have ever used a given requirement (resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by multiple requirements. A maximum of &#x60;50&#x60; requirements can be queried. A maximum of 20 entities per requirement will be returned. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementLookupRequest"></param>
        /// <returns>EntitiesByRoleRequirementLookupResponse</returns>
        public EntitiesByRoleRequirementLookupResponse EntitiesByRoleRequirementLookup(EntitiesByRoleRequirementLookupRequest entitiesByRoleRequirementLookupRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<EntitiesByRoleRequirementLookupResponse> localVarResponse = EntitiesByRoleRequirementLookupWithHttpInfo(entitiesByRoleRequirementLookupRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get entities by role requirement lookup This endpoint is intended to query for entities that have ever used a given requirement (resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by multiple requirements. A maximum of &#x60;50&#x60; requirements can be queried. A maximum of 20 entities per requirement will be returned. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementLookupRequest"></param>
        /// <returns>ApiResponse of EntitiesByRoleRequirementLookupResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<EntitiesByRoleRequirementLookupResponse> EntitiesByRoleRequirementLookupWithHttpInfo(EntitiesByRoleRequirementLookupRequest entitiesByRoleRequirementLookupRequest)
        {
            // verify the required parameter 'entitiesByRoleRequirementLookupRequest' is set
            if (entitiesByRoleRequirementLookupRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'entitiesByRoleRequirementLookupRequest' when calling ExtensionsApi->EntitiesByRoleRequirementLookup");

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

            localVarRequestOptions.Data = entitiesByRoleRequirementLookupRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<EntitiesByRoleRequirementLookupResponse>("/extensions/entities-by-role-requirement/lookup", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntitiesByRoleRequirementLookup", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get entities by role requirement lookup This endpoint is intended to query for entities that have ever used a given requirement (resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by multiple requirements. A maximum of &#x60;50&#x60; requirements can be queried. A maximum of 20 entities per requirement will be returned. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementLookupRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of EntitiesByRoleRequirementLookupResponse</returns>
        public async System.Threading.Tasks.Task<EntitiesByRoleRequirementLookupResponse> EntitiesByRoleRequirementLookupAsync(EntitiesByRoleRequirementLookupRequest entitiesByRoleRequirementLookupRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<EntitiesByRoleRequirementLookupResponse> localVarResponse = await EntitiesByRoleRequirementLookupWithHttpInfoAsync(entitiesByRoleRequirementLookupRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get entities by role requirement lookup This endpoint is intended to query for entities that have ever used a given requirement (resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by multiple requirements. A maximum of &#x60;50&#x60; requirements can be queried. A maximum of 20 entities per requirement will be returned. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementLookupRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (EntitiesByRoleRequirementLookupResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<EntitiesByRoleRequirementLookupResponse>> EntitiesByRoleRequirementLookupWithHttpInfoAsync(EntitiesByRoleRequirementLookupRequest entitiesByRoleRequirementLookupRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'entitiesByRoleRequirementLookupRequest' is set
            if (entitiesByRoleRequirementLookupRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'entitiesByRoleRequirementLookupRequest' when calling ExtensionsApi->EntitiesByRoleRequirementLookup");


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

            localVarRequestOptions.Data = entitiesByRoleRequirementLookupRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<EntitiesByRoleRequirementLookupResponse>("/extensions/entities-by-role-requirement/lookup", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntitiesByRoleRequirementLookup", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get entities by role requirement page Paginated endpoint returns a page of global entities that have ever used the provided requirement (such as a resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by a single requirement. By default it returns up to 100 entries are returned per response. This limit can be increased to a maximum of 1000 entries per page using the &#x60;limit_per_page&#x60; parameter. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  To lookup multiple requirements, please call the &#x60;/extensions/entities-by-role-requirement/lookup&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementPageRequest"></param>
        /// <returns>EntitiesByRoleRequirementPageResponse</returns>
        public EntitiesByRoleRequirementPageResponse EntitiesByRoleRequirementPage(EntitiesByRoleRequirementPageRequest entitiesByRoleRequirementPageRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<EntitiesByRoleRequirementPageResponse> localVarResponse = EntitiesByRoleRequirementPageWithHttpInfo(entitiesByRoleRequirementPageRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get entities by role requirement page Paginated endpoint returns a page of global entities that have ever used the provided requirement (such as a resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by a single requirement. By default it returns up to 100 entries are returned per response. This limit can be increased to a maximum of 1000 entries per page using the &#x60;limit_per_page&#x60; parameter. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  To lookup multiple requirements, please call the &#x60;/extensions/entities-by-role-requirement/lookup&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementPageRequest"></param>
        /// <returns>ApiResponse of EntitiesByRoleRequirementPageResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<EntitiesByRoleRequirementPageResponse> EntitiesByRoleRequirementPageWithHttpInfo(EntitiesByRoleRequirementPageRequest entitiesByRoleRequirementPageRequest)
        {
            // verify the required parameter 'entitiesByRoleRequirementPageRequest' is set
            if (entitiesByRoleRequirementPageRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'entitiesByRoleRequirementPageRequest' when calling ExtensionsApi->EntitiesByRoleRequirementPage");

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

            localVarRequestOptions.Data = entitiesByRoleRequirementPageRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<EntitiesByRoleRequirementPageResponse>("/extensions/entities-by-role-requirement/page", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntitiesByRoleRequirementPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get entities by role requirement page Paginated endpoint returns a page of global entities that have ever used the provided requirement (such as a resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by a single requirement. By default it returns up to 100 entries are returned per response. This limit can be increased to a maximum of 1000 entries per page using the &#x60;limit_per_page&#x60; parameter. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  To lookup multiple requirements, please call the &#x60;/extensions/entities-by-role-requirement/lookup&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of EntitiesByRoleRequirementPageResponse</returns>
        public async System.Threading.Tasks.Task<EntitiesByRoleRequirementPageResponse> EntitiesByRoleRequirementPageAsync(EntitiesByRoleRequirementPageRequest entitiesByRoleRequirementPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<EntitiesByRoleRequirementPageResponse> localVarResponse = await EntitiesByRoleRequirementPageWithHttpInfoAsync(entitiesByRoleRequirementPageRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get entities by role requirement page Paginated endpoint returns a page of global entities that have ever used the provided requirement (such as a resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments). This endpoint allows querying by a single requirement. By default it returns up to 100 entries are returned per response. This limit can be increased to a maximum of 1000 entries per page using the &#x60;limit_per_page&#x60; parameter. To retrieve subsequent pages, use the returned cursor and call the &#x60;/extensions/entities-by-role-requirement/page&#x60; endpoint.  To lookup multiple requirements, please call the &#x60;/extensions/entities-by-role-requirement/lookup&#x60; endpoint.  Behaviour: - Entities are returned in ascending order by the state version in which the requirement was first observed on the ledger. - It may include entities that no longer use the requirement as part of an access rule. - If no entities are found, an empty list will be returned. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="entitiesByRoleRequirementPageRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (EntitiesByRoleRequirementPageResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<EntitiesByRoleRequirementPageResponse>> EntitiesByRoleRequirementPageWithHttpInfoAsync(EntitiesByRoleRequirementPageRequest entitiesByRoleRequirementPageRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'entitiesByRoleRequirementPageRequest' is set
            if (entitiesByRoleRequirementPageRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'entitiesByRoleRequirementPageRequest' when calling ExtensionsApi->EntitiesByRoleRequirementPage");


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

            localVarRequestOptions.Data = entitiesByRoleRequirementPageRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<EntitiesByRoleRequirementPageResponse>("/extensions/entities-by-role-requirement/page", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("EntitiesByRoleRequirementPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Resolve implicit requirement target from global non-fungible id Access rules can include [implicit requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) referencing special system-reserved resource addresses, which have specific meanings for the Radix Engine and are not part of the standard authorization zone system. These implicit requirements typically store their details as a hash, which means the subject of the requirement can&#39;t be easily resolved. This is where this endpoint comes in. It can resolve the subject of the implicit requirements, using a database of reverse hash lookups populated from ledger data.  The following [resource addresses](https://docs.radixdlt.com/docs/well-known-addresses) are supported: - **Secp256k1 Signature Resource** - **Ed25519 Signature Resource** - **Package of Direct Caller Resource** - **Global Caller Resource** - **System Execution Resource**  When querying, you must provide a pair of the following for each requirement to resolve: - &#x60;resource_address&#x60; (one of the above) - &#x60;non_fungible_id&#x60;,  of the requirement.  You can query a maximum of &#x60;100&#x60; implicit requirements at a time.  See the documentation on [implicit-requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) for more information. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="implicitRequirementsLookupRequest"></param>
        /// <returns>ImplicitRequirementsLookupResponse</returns>
        public ImplicitRequirementsLookupResponse ImplicitRequirementsLookup(ImplicitRequirementsLookupRequest implicitRequirementsLookupRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<ImplicitRequirementsLookupResponse> localVarResponse = ImplicitRequirementsLookupWithHttpInfo(implicitRequirementsLookupRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Resolve implicit requirement target from global non-fungible id Access rules can include [implicit requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) referencing special system-reserved resource addresses, which have specific meanings for the Radix Engine and are not part of the standard authorization zone system. These implicit requirements typically store their details as a hash, which means the subject of the requirement can&#39;t be easily resolved. This is where this endpoint comes in. It can resolve the subject of the implicit requirements, using a database of reverse hash lookups populated from ledger data.  The following [resource addresses](https://docs.radixdlt.com/docs/well-known-addresses) are supported: - **Secp256k1 Signature Resource** - **Ed25519 Signature Resource** - **Package of Direct Caller Resource** - **Global Caller Resource** - **System Execution Resource**  When querying, you must provide a pair of the following for each requirement to resolve: - &#x60;resource_address&#x60; (one of the above) - &#x60;non_fungible_id&#x60;,  of the requirement.  You can query a maximum of &#x60;100&#x60; implicit requirements at a time.  See the documentation on [implicit-requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) for more information. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="implicitRequirementsLookupRequest"></param>
        /// <returns>ApiResponse of ImplicitRequirementsLookupResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<ImplicitRequirementsLookupResponse> ImplicitRequirementsLookupWithHttpInfo(ImplicitRequirementsLookupRequest implicitRequirementsLookupRequest)
        {
            // verify the required parameter 'implicitRequirementsLookupRequest' is set
            if (implicitRequirementsLookupRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'implicitRequirementsLookupRequest' when calling ExtensionsApi->ImplicitRequirementsLookup");

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

            localVarRequestOptions.Data = implicitRequirementsLookupRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<ImplicitRequirementsLookupResponse>("/extensions/implicit-requirements/lookup", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ImplicitRequirementsLookup", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Resolve implicit requirement target from global non-fungible id Access rules can include [implicit requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) referencing special system-reserved resource addresses, which have specific meanings for the Radix Engine and are not part of the standard authorization zone system. These implicit requirements typically store their details as a hash, which means the subject of the requirement can&#39;t be easily resolved. This is where this endpoint comes in. It can resolve the subject of the implicit requirements, using a database of reverse hash lookups populated from ledger data.  The following [resource addresses](https://docs.radixdlt.com/docs/well-known-addresses) are supported: - **Secp256k1 Signature Resource** - **Ed25519 Signature Resource** - **Package of Direct Caller Resource** - **Global Caller Resource** - **System Execution Resource**  When querying, you must provide a pair of the following for each requirement to resolve: - &#x60;resource_address&#x60; (one of the above) - &#x60;non_fungible_id&#x60;,  of the requirement.  You can query a maximum of &#x60;100&#x60; implicit requirements at a time.  See the documentation on [implicit-requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) for more information. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="implicitRequirementsLookupRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ImplicitRequirementsLookupResponse</returns>
        public async System.Threading.Tasks.Task<ImplicitRequirementsLookupResponse> ImplicitRequirementsLookupAsync(ImplicitRequirementsLookupRequest implicitRequirementsLookupRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<ImplicitRequirementsLookupResponse> localVarResponse = await ImplicitRequirementsLookupWithHttpInfoAsync(implicitRequirementsLookupRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Resolve implicit requirement target from global non-fungible id Access rules can include [implicit requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) referencing special system-reserved resource addresses, which have specific meanings for the Radix Engine and are not part of the standard authorization zone system. These implicit requirements typically store their details as a hash, which means the subject of the requirement can&#39;t be easily resolved. This is where this endpoint comes in. It can resolve the subject of the implicit requirements, using a database of reverse hash lookups populated from ledger data.  The following [resource addresses](https://docs.radixdlt.com/docs/well-known-addresses) are supported: - **Secp256k1 Signature Resource** - **Ed25519 Signature Resource** - **Package of Direct Caller Resource** - **Global Caller Resource** - **System Execution Resource**  When querying, you must provide a pair of the following for each requirement to resolve: - &#x60;resource_address&#x60; (one of the above) - &#x60;non_fungible_id&#x60;,  of the requirement.  You can query a maximum of &#x60;100&#x60; implicit requirements at a time.  See the documentation on [implicit-requirements](https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements) for more information. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="implicitRequirementsLookupRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (ImplicitRequirementsLookupResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<ImplicitRequirementsLookupResponse>> ImplicitRequirementsLookupWithHttpInfoAsync(ImplicitRequirementsLookupRequest implicitRequirementsLookupRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'implicitRequirementsLookupRequest' is set
            if (implicitRequirementsLookupRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'implicitRequirementsLookupRequest' when calling ExtensionsApi->ImplicitRequirementsLookup");


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

            localVarRequestOptions.Data = implicitRequirementsLookupRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<ImplicitRequirementsLookupResponse>("/extensions/implicit-requirements/lookup", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ImplicitRequirementsLookup", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Resource Holders Page A paginated endpoint to discover which global entities hold the most of a given resource. More specifically, it returns a page of global entities which hold the given resource, ordered descending by the total fungible balance / total count of non-fungibles stored in vaults in the state tree of that entity (excluding unclaimed royalty balances).  This endpoint operates only at the **current state version**, it is not possible to browse historical data. Because of that, it is not possible to offer stable pagination as data constantly changes. Balances might change between pages being read, which might result in gaps or some entries being returned twice.  Under default Gateway configuration, up to 100 entries are returned per response. This can be increased up to 1000 entries per page with the &#x60;limit_per_page&#x60; parameter. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="resourceHoldersRequest"></param>
        /// <returns>ResourceHoldersResponse</returns>
        public ResourceHoldersResponse ResourceHoldersPage(ResourceHoldersRequest resourceHoldersRequest)
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<ResourceHoldersResponse> localVarResponse = ResourceHoldersPageWithHttpInfo(resourceHoldersRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Resource Holders Page A paginated endpoint to discover which global entities hold the most of a given resource. More specifically, it returns a page of global entities which hold the given resource, ordered descending by the total fungible balance / total count of non-fungibles stored in vaults in the state tree of that entity (excluding unclaimed royalty balances).  This endpoint operates only at the **current state version**, it is not possible to browse historical data. Because of that, it is not possible to offer stable pagination as data constantly changes. Balances might change between pages being read, which might result in gaps or some entries being returned twice.  Under default Gateway configuration, up to 100 entries are returned per response. This can be increased up to 1000 entries per page with the &#x60;limit_per_page&#x60; parameter. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="resourceHoldersRequest"></param>
        /// <returns>ApiResponse of ResourceHoldersResponse</returns>
        public RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<ResourceHoldersResponse> ResourceHoldersPageWithHttpInfo(ResourceHoldersRequest resourceHoldersRequest)
        {
            // verify the required parameter 'resourceHoldersRequest' is set
            if (resourceHoldersRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'resourceHoldersRequest' when calling ExtensionsApi->ResourceHoldersPage");

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

            localVarRequestOptions.Data = resourceHoldersRequest;


            // make the HTTP request
            var localVarResponse = this.Client.Post<ResourceHoldersResponse>("/extensions/resource-holders/page", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ResourceHoldersPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get Resource Holders Page A paginated endpoint to discover which global entities hold the most of a given resource. More specifically, it returns a page of global entities which hold the given resource, ordered descending by the total fungible balance / total count of non-fungibles stored in vaults in the state tree of that entity (excluding unclaimed royalty balances).  This endpoint operates only at the **current state version**, it is not possible to browse historical data. Because of that, it is not possible to offer stable pagination as data constantly changes. Balances might change between pages being read, which might result in gaps or some entries being returned twice.  Under default Gateway configuration, up to 100 entries are returned per response. This can be increased up to 1000 entries per page with the &#x60;limit_per_page&#x60; parameter. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="resourceHoldersRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ResourceHoldersResponse</returns>
        public async System.Threading.Tasks.Task<ResourceHoldersResponse> ResourceHoldersPageAsync(ResourceHoldersRequest resourceHoldersRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<ResourceHoldersResponse> localVarResponse = await ResourceHoldersPageWithHttpInfoAsync(resourceHoldersRequest, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Resource Holders Page A paginated endpoint to discover which global entities hold the most of a given resource. More specifically, it returns a page of global entities which hold the given resource, ordered descending by the total fungible balance / total count of non-fungibles stored in vaults in the state tree of that entity (excluding unclaimed royalty balances).  This endpoint operates only at the **current state version**, it is not possible to browse historical data. Because of that, it is not possible to offer stable pagination as data constantly changes. Balances might change between pages being read, which might result in gaps or some entries being returned twice.  Under default Gateway configuration, up to 100 entries are returned per response. This can be increased up to 1000 entries per page with the &#x60;limit_per_page&#x60; parameter. 
        /// </summary>
        /// <exception cref="RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="resourceHoldersRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (ResourceHoldersResponse)</returns>
        public async System.Threading.Tasks.Task<RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiResponse<ResourceHoldersResponse>> ResourceHoldersPageWithHttpInfoAsync(ResourceHoldersRequest resourceHoldersRequest, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'resourceHoldersRequest' is set
            if (resourceHoldersRequest == null)
                throw new RadixDlt.NetworkGateway.GatewayApiSdk.Client.ApiException(400, "Missing required parameter 'resourceHoldersRequest' when calling ExtensionsApi->ResourceHoldersPage");


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

            localVarRequestOptions.Data = resourceHoldersRequest;


            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<ResourceHoldersResponse>("/extensions/resource-holders/page", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ResourceHoldersPage", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

    }
}
