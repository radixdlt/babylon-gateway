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
 * The version of the OpenAPI document: v1.10.0
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using FileParameter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Model
{
    /// <summary>
    /// StreamTransactionsRequestAllOf
    /// </summary>
    [DataContract(Name = "StreamTransactionsRequest_allOf")]
    public partial class StreamTransactionsRequestAllOf : IEquatable<StreamTransactionsRequestAllOf>
    {
        /// <summary>
        /// Limit returned transactions by their kind. Defaults to &#x60;user&#x60;.
        /// </summary>
        /// <value>Limit returned transactions by their kind. Defaults to &#x60;user&#x60;.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum KindFilterEnum
        {
            /// <summary>
            /// Enum User for value: User
            /// </summary>
            [EnumMember(Value = "User")]
            User = 1,

            /// <summary>
            /// Enum EpochChange for value: EpochChange
            /// </summary>
            [EnumMember(Value = "EpochChange")]
            EpochChange = 2,

            /// <summary>
            /// Enum All for value: All
            /// </summary>
            [EnumMember(Value = "All")]
            All = 3

        }


        /// <summary>
        /// Limit returned transactions by their kind. Defaults to &#x60;user&#x60;.
        /// </summary>
        /// <value>Limit returned transactions by their kind. Defaults to &#x60;user&#x60;.</value>
        [DataMember(Name = "kind_filter", EmitDefaultValue = true)]
        public KindFilterEnum? KindFilter { get; set; }
        /// <summary>
        /// Allows filtering by transaction status. Defaults to &#x60;All&#x60;.
        /// </summary>
        /// <value>Allows filtering by transaction status. Defaults to &#x60;All&#x60;.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TransactionStatusFilterEnum
        {
            /// <summary>
            /// Enum Successful for value: Successful
            /// </summary>
            [EnumMember(Value = "Successful")]
            Successful = 1,

            /// <summary>
            /// Enum Failed for value: Failed
            /// </summary>
            [EnumMember(Value = "Failed")]
            Failed = 2,

            /// <summary>
            /// Enum All for value: All
            /// </summary>
            [EnumMember(Value = "All")]
            All = 3

        }


        /// <summary>
        /// Allows filtering by transaction status. Defaults to &#x60;All&#x60;.
        /// </summary>
        /// <value>Allows filtering by transaction status. Defaults to &#x60;All&#x60;.</value>
        [DataMember(Name = "transaction_status_filter", EmitDefaultValue = true)]
        public TransactionStatusFilterEnum? TransactionStatusFilter { get; set; }
        /// <summary>
        /// Configures the order of returned result set. Defaults to &#x60;desc&#x60;.
        /// </summary>
        /// <value>Configures the order of returned result set. Defaults to &#x60;desc&#x60;.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum OrderEnum
        {
            /// <summary>
            /// Enum Asc for value: Asc
            /// </summary>
            [EnumMember(Value = "Asc")]
            Asc = 1,

            /// <summary>
            /// Enum Desc for value: Desc
            /// </summary>
            [EnumMember(Value = "Desc")]
            Desc = 2

        }


        /// <summary>
        /// Configures the order of returned result set. Defaults to &#x60;desc&#x60;.
        /// </summary>
        /// <value>Configures the order of returned result set. Defaults to &#x60;desc&#x60;.</value>
        [DataMember(Name = "order", EmitDefaultValue = true)]
        public OrderEnum? Order { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamTransactionsRequestAllOf" /> class.
        /// </summary>
        /// <param name="kindFilter">Limit returned transactions by their kind. Defaults to &#x60;user&#x60;..</param>
        /// <param name="manifestAccountsWithdrawnFromFilter">Allows specifying an array of account addresses. If specified, the response will contain only transactions with a manifest containing withdrawals from the given accounts..</param>
        /// <param name="manifestAccountsDepositedIntoFilter">Similar to &#x60;manifest_accounts_withdrawn_from_filter&#x60;, but will return only transactions with a manifest containing deposits to the given accounts..</param>
        /// <param name="manifestBadgesPresentedFilter">Allows specifying array of badge resource addresses. If specified, the response will contain only transactions where the given badges were presented..</param>
        /// <param name="manifestResourcesFilter">Allows specifying array of resource addresses. If specified, the response will contain only transactions containing the given resources in the manifest (regardless of their usage)..</param>
        /// <param name="affectedGlobalEntitiesFilter">Allows specifying an array of global addresses. If specified, the response will contain transactions that affected all of the given global entities. A global entity is marked as \&quot;affected\&quot; by a transaction if any of its state (or its descendents&#39; state) was modified as a result of the transaction. For performance reasons consensus manager and transaction tracker are excluded from that filter..</param>
        /// <param name="eventsFilter">Filters the transaction stream to transactions which emitted at least one event matching each filter (each filter can be satisfied by a different event). Currently *only* deposit and withdrawal events emitted by an internal vault entity are tracked. For the purpose of filtering, the emitter address is replaced by the global ancestor of the emitter, for example, the top-level account / component which contains the vault which emitted the event..</param>
        /// <param name="accountsWithManifestOwnerMethodCalls">Allows specifying an array of account addresses. If specified, the response will contain only transactions that, for all specified accounts, contain manifest method calls to that account which require the owner role. See the [account docs](https://docs.radixdlt.com/docs/account) for more information..</param>
        /// <param name="accountsWithoutManifestOwnerMethodCalls">Allows specifying an array of account addresses. If specified, the response will contain only transactions that, for all specified accounts, do NOT contain manifest method calls to that account which require owner role. See the [account docs](https://docs.radixdlt.com/docs/account) for more information..</param>
        /// <param name="manifestClassFilter">manifestClassFilter.</param>
        /// <param name="eventGlobalEmittersFilter">Allows specifying an array of global addresses. If specified, the response will contain transactions in which all entities emitted events. If an event was published by an internal entity, it is going to be indexed as it is a global ancestor. For performance reasons events published by consensus manager and native XRD resource are excluded from that filter..</param>
        /// <param name="balanceChangeResourcesFilter">Allows specifying an array of resources. If specified, the response will contain transactions which included non-fee related balance changes for all resources..</param>
        /// <param name="transactionStatusFilter">Allows filtering by transaction status. Defaults to &#x60;All&#x60;. (default to TransactionStatusFilterEnum.All).</param>
        /// <param name="order">Configures the order of returned result set. Defaults to &#x60;desc&#x60;..</param>
        /// <param name="optIns">optIns.</param>
        public StreamTransactionsRequestAllOf(KindFilterEnum? kindFilter = default(KindFilterEnum?), List<string> manifestAccountsWithdrawnFromFilter = default(List<string>), List<string> manifestAccountsDepositedIntoFilter = default(List<string>), List<string> manifestBadgesPresentedFilter = default(List<string>), List<string> manifestResourcesFilter = default(List<string>), List<string> affectedGlobalEntitiesFilter = default(List<string>), List<StreamTransactionsRequestEventFilterItem> eventsFilter = default(List<StreamTransactionsRequestEventFilterItem>), List<string> accountsWithManifestOwnerMethodCalls = default(List<string>), List<string> accountsWithoutManifestOwnerMethodCalls = default(List<string>), StreamTransactionsRequestAllOfManifestClassFilter manifestClassFilter = default(StreamTransactionsRequestAllOfManifestClassFilter), List<string> eventGlobalEmittersFilter = default(List<string>), List<string> balanceChangeResourcesFilter = default(List<string>), TransactionStatusFilterEnum? transactionStatusFilter = TransactionStatusFilterEnum.All, OrderEnum? order = default(OrderEnum?), TransactionDetailsOptIns optIns = default(TransactionDetailsOptIns))
        {
            this.KindFilter = kindFilter;
            this.ManifestAccountsWithdrawnFromFilter = manifestAccountsWithdrawnFromFilter;
            this.ManifestAccountsDepositedIntoFilter = manifestAccountsDepositedIntoFilter;
            this.ManifestBadgesPresentedFilter = manifestBadgesPresentedFilter;
            this.ManifestResourcesFilter = manifestResourcesFilter;
            this.AffectedGlobalEntitiesFilter = affectedGlobalEntitiesFilter;
            this.EventsFilter = eventsFilter;
            this.AccountsWithManifestOwnerMethodCalls = accountsWithManifestOwnerMethodCalls;
            this.AccountsWithoutManifestOwnerMethodCalls = accountsWithoutManifestOwnerMethodCalls;
            this.ManifestClassFilter = manifestClassFilter;
            this.EventGlobalEmittersFilter = eventGlobalEmittersFilter;
            this.BalanceChangeResourcesFilter = balanceChangeResourcesFilter;
            this.TransactionStatusFilter = transactionStatusFilter;
            this.Order = order;
            this.OptIns = optIns;
        }

        /// <summary>
        /// Allows specifying an array of account addresses. If specified, the response will contain only transactions with a manifest containing withdrawals from the given accounts.
        /// </summary>
        /// <value>Allows specifying an array of account addresses. If specified, the response will contain only transactions with a manifest containing withdrawals from the given accounts.</value>
        [DataMember(Name = "manifest_accounts_withdrawn_from_filter", EmitDefaultValue = true)]
        public List<string> ManifestAccountsWithdrawnFromFilter { get; set; }

        /// <summary>
        /// Similar to &#x60;manifest_accounts_withdrawn_from_filter&#x60;, but will return only transactions with a manifest containing deposits to the given accounts.
        /// </summary>
        /// <value>Similar to &#x60;manifest_accounts_withdrawn_from_filter&#x60;, but will return only transactions with a manifest containing deposits to the given accounts.</value>
        [DataMember(Name = "manifest_accounts_deposited_into_filter", EmitDefaultValue = true)]
        public List<string> ManifestAccountsDepositedIntoFilter { get; set; }

        /// <summary>
        /// Allows specifying array of badge resource addresses. If specified, the response will contain only transactions where the given badges were presented.
        /// </summary>
        /// <value>Allows specifying array of badge resource addresses. If specified, the response will contain only transactions where the given badges were presented.</value>
        [DataMember(Name = "manifest_badges_presented_filter", EmitDefaultValue = true)]
        public List<string> ManifestBadgesPresentedFilter { get; set; }

        /// <summary>
        /// Allows specifying array of resource addresses. If specified, the response will contain only transactions containing the given resources in the manifest (regardless of their usage).
        /// </summary>
        /// <value>Allows specifying array of resource addresses. If specified, the response will contain only transactions containing the given resources in the manifest (regardless of their usage).</value>
        [DataMember(Name = "manifest_resources_filter", EmitDefaultValue = true)]
        public List<string> ManifestResourcesFilter { get; set; }

        /// <summary>
        /// Allows specifying an array of global addresses. If specified, the response will contain transactions that affected all of the given global entities. A global entity is marked as \&quot;affected\&quot; by a transaction if any of its state (or its descendents&#39; state) was modified as a result of the transaction. For performance reasons consensus manager and transaction tracker are excluded from that filter.
        /// </summary>
        /// <value>Allows specifying an array of global addresses. If specified, the response will contain transactions that affected all of the given global entities. A global entity is marked as \&quot;affected\&quot; by a transaction if any of its state (or its descendents&#39; state) was modified as a result of the transaction. For performance reasons consensus manager and transaction tracker are excluded from that filter.</value>
        [DataMember(Name = "affected_global_entities_filter", EmitDefaultValue = true)]
        public List<string> AffectedGlobalEntitiesFilter { get; set; }

        /// <summary>
        /// Filters the transaction stream to transactions which emitted at least one event matching each filter (each filter can be satisfied by a different event). Currently *only* deposit and withdrawal events emitted by an internal vault entity are tracked. For the purpose of filtering, the emitter address is replaced by the global ancestor of the emitter, for example, the top-level account / component which contains the vault which emitted the event.
        /// </summary>
        /// <value>Filters the transaction stream to transactions which emitted at least one event matching each filter (each filter can be satisfied by a different event). Currently *only* deposit and withdrawal events emitted by an internal vault entity are tracked. For the purpose of filtering, the emitter address is replaced by the global ancestor of the emitter, for example, the top-level account / component which contains the vault which emitted the event.</value>
        [DataMember(Name = "events_filter", EmitDefaultValue = true)]
        public List<StreamTransactionsRequestEventFilterItem> EventsFilter { get; set; }

        /// <summary>
        /// Allows specifying an array of account addresses. If specified, the response will contain only transactions that, for all specified accounts, contain manifest method calls to that account which require the owner role. See the [account docs](https://docs.radixdlt.com/docs/account) for more information.
        /// </summary>
        /// <value>Allows specifying an array of account addresses. If specified, the response will contain only transactions that, for all specified accounts, contain manifest method calls to that account which require the owner role. See the [account docs](https://docs.radixdlt.com/docs/account) for more information.</value>
        [DataMember(Name = "accounts_with_manifest_owner_method_calls", EmitDefaultValue = true)]
        public List<string> AccountsWithManifestOwnerMethodCalls { get; set; }

        /// <summary>
        /// Allows specifying an array of account addresses. If specified, the response will contain only transactions that, for all specified accounts, do NOT contain manifest method calls to that account which require owner role. See the [account docs](https://docs.radixdlt.com/docs/account) for more information.
        /// </summary>
        /// <value>Allows specifying an array of account addresses. If specified, the response will contain only transactions that, for all specified accounts, do NOT contain manifest method calls to that account which require owner role. See the [account docs](https://docs.radixdlt.com/docs/account) for more information.</value>
        [DataMember(Name = "accounts_without_manifest_owner_method_calls", EmitDefaultValue = true)]
        public List<string> AccountsWithoutManifestOwnerMethodCalls { get; set; }

        /// <summary>
        /// Gets or Sets ManifestClassFilter
        /// </summary>
        [DataMember(Name = "manifest_class_filter", EmitDefaultValue = true)]
        public StreamTransactionsRequestAllOfManifestClassFilter ManifestClassFilter { get; set; }

        /// <summary>
        /// Allows specifying an array of global addresses. If specified, the response will contain transactions in which all entities emitted events. If an event was published by an internal entity, it is going to be indexed as it is a global ancestor. For performance reasons events published by consensus manager and native XRD resource are excluded from that filter.
        /// </summary>
        /// <value>Allows specifying an array of global addresses. If specified, the response will contain transactions in which all entities emitted events. If an event was published by an internal entity, it is going to be indexed as it is a global ancestor. For performance reasons events published by consensus manager and native XRD resource are excluded from that filter.</value>
        [DataMember(Name = "event_global_emitters_filter", EmitDefaultValue = true)]
        public List<string> EventGlobalEmittersFilter { get; set; }

        /// <summary>
        /// Allows specifying an array of resources. If specified, the response will contain transactions which included non-fee related balance changes for all resources.
        /// </summary>
        /// <value>Allows specifying an array of resources. If specified, the response will contain transactions which included non-fee related balance changes for all resources.</value>
        [DataMember(Name = "balance_change_resources_filter", EmitDefaultValue = true)]
        public List<string> BalanceChangeResourcesFilter { get; set; }

        /// <summary>
        /// Gets or Sets OptIns
        /// </summary>
        [DataMember(Name = "opt_ins", EmitDefaultValue = true)]
        public TransactionDetailsOptIns OptIns { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class StreamTransactionsRequestAllOf {\n");
            sb.Append("  KindFilter: ").Append(KindFilter).Append("\n");
            sb.Append("  ManifestAccountsWithdrawnFromFilter: ").Append(ManifestAccountsWithdrawnFromFilter).Append("\n");
            sb.Append("  ManifestAccountsDepositedIntoFilter: ").Append(ManifestAccountsDepositedIntoFilter).Append("\n");
            sb.Append("  ManifestBadgesPresentedFilter: ").Append(ManifestBadgesPresentedFilter).Append("\n");
            sb.Append("  ManifestResourcesFilter: ").Append(ManifestResourcesFilter).Append("\n");
            sb.Append("  AffectedGlobalEntitiesFilter: ").Append(AffectedGlobalEntitiesFilter).Append("\n");
            sb.Append("  EventsFilter: ").Append(EventsFilter).Append("\n");
            sb.Append("  AccountsWithManifestOwnerMethodCalls: ").Append(AccountsWithManifestOwnerMethodCalls).Append("\n");
            sb.Append("  AccountsWithoutManifestOwnerMethodCalls: ").Append(AccountsWithoutManifestOwnerMethodCalls).Append("\n");
            sb.Append("  ManifestClassFilter: ").Append(ManifestClassFilter).Append("\n");
            sb.Append("  EventGlobalEmittersFilter: ").Append(EventGlobalEmittersFilter).Append("\n");
            sb.Append("  BalanceChangeResourcesFilter: ").Append(BalanceChangeResourcesFilter).Append("\n");
            sb.Append("  TransactionStatusFilter: ").Append(TransactionStatusFilter).Append("\n");
            sb.Append("  Order: ").Append(Order).Append("\n");
            sb.Append("  OptIns: ").Append(OptIns).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as StreamTransactionsRequestAllOf);
        }

        /// <summary>
        /// Returns true if StreamTransactionsRequestAllOf instances are equal
        /// </summary>
        /// <param name="input">Instance of StreamTransactionsRequestAllOf to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(StreamTransactionsRequestAllOf input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.KindFilter == input.KindFilter ||
                    this.KindFilter.Equals(input.KindFilter)
                ) && 
                (
                    this.ManifestAccountsWithdrawnFromFilter == input.ManifestAccountsWithdrawnFromFilter ||
                    this.ManifestAccountsWithdrawnFromFilter != null &&
                    input.ManifestAccountsWithdrawnFromFilter != null &&
                    this.ManifestAccountsWithdrawnFromFilter.SequenceEqual(input.ManifestAccountsWithdrawnFromFilter)
                ) && 
                (
                    this.ManifestAccountsDepositedIntoFilter == input.ManifestAccountsDepositedIntoFilter ||
                    this.ManifestAccountsDepositedIntoFilter != null &&
                    input.ManifestAccountsDepositedIntoFilter != null &&
                    this.ManifestAccountsDepositedIntoFilter.SequenceEqual(input.ManifestAccountsDepositedIntoFilter)
                ) && 
                (
                    this.ManifestBadgesPresentedFilter == input.ManifestBadgesPresentedFilter ||
                    this.ManifestBadgesPresentedFilter != null &&
                    input.ManifestBadgesPresentedFilter != null &&
                    this.ManifestBadgesPresentedFilter.SequenceEqual(input.ManifestBadgesPresentedFilter)
                ) && 
                (
                    this.ManifestResourcesFilter == input.ManifestResourcesFilter ||
                    this.ManifestResourcesFilter != null &&
                    input.ManifestResourcesFilter != null &&
                    this.ManifestResourcesFilter.SequenceEqual(input.ManifestResourcesFilter)
                ) && 
                (
                    this.AffectedGlobalEntitiesFilter == input.AffectedGlobalEntitiesFilter ||
                    this.AffectedGlobalEntitiesFilter != null &&
                    input.AffectedGlobalEntitiesFilter != null &&
                    this.AffectedGlobalEntitiesFilter.SequenceEqual(input.AffectedGlobalEntitiesFilter)
                ) && 
                (
                    this.EventsFilter == input.EventsFilter ||
                    this.EventsFilter != null &&
                    input.EventsFilter != null &&
                    this.EventsFilter.SequenceEqual(input.EventsFilter)
                ) && 
                (
                    this.AccountsWithManifestOwnerMethodCalls == input.AccountsWithManifestOwnerMethodCalls ||
                    this.AccountsWithManifestOwnerMethodCalls != null &&
                    input.AccountsWithManifestOwnerMethodCalls != null &&
                    this.AccountsWithManifestOwnerMethodCalls.SequenceEqual(input.AccountsWithManifestOwnerMethodCalls)
                ) && 
                (
                    this.AccountsWithoutManifestOwnerMethodCalls == input.AccountsWithoutManifestOwnerMethodCalls ||
                    this.AccountsWithoutManifestOwnerMethodCalls != null &&
                    input.AccountsWithoutManifestOwnerMethodCalls != null &&
                    this.AccountsWithoutManifestOwnerMethodCalls.SequenceEqual(input.AccountsWithoutManifestOwnerMethodCalls)
                ) && 
                (
                    this.ManifestClassFilter == input.ManifestClassFilter ||
                    (this.ManifestClassFilter != null &&
                    this.ManifestClassFilter.Equals(input.ManifestClassFilter))
                ) && 
                (
                    this.EventGlobalEmittersFilter == input.EventGlobalEmittersFilter ||
                    this.EventGlobalEmittersFilter != null &&
                    input.EventGlobalEmittersFilter != null &&
                    this.EventGlobalEmittersFilter.SequenceEqual(input.EventGlobalEmittersFilter)
                ) && 
                (
                    this.BalanceChangeResourcesFilter == input.BalanceChangeResourcesFilter ||
                    this.BalanceChangeResourcesFilter != null &&
                    input.BalanceChangeResourcesFilter != null &&
                    this.BalanceChangeResourcesFilter.SequenceEqual(input.BalanceChangeResourcesFilter)
                ) && 
                (
                    this.TransactionStatusFilter == input.TransactionStatusFilter ||
                    this.TransactionStatusFilter.Equals(input.TransactionStatusFilter)
                ) && 
                (
                    this.Order == input.Order ||
                    this.Order.Equals(input.Order)
                ) && 
                (
                    this.OptIns == input.OptIns ||
                    (this.OptIns != null &&
                    this.OptIns.Equals(input.OptIns))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                hashCode = (hashCode * 59) + this.KindFilter.GetHashCode();
                if (this.ManifestAccountsWithdrawnFromFilter != null)
                {
                    hashCode = (hashCode * 59) + this.ManifestAccountsWithdrawnFromFilter.GetHashCode();
                }
                if (this.ManifestAccountsDepositedIntoFilter != null)
                {
                    hashCode = (hashCode * 59) + this.ManifestAccountsDepositedIntoFilter.GetHashCode();
                }
                if (this.ManifestBadgesPresentedFilter != null)
                {
                    hashCode = (hashCode * 59) + this.ManifestBadgesPresentedFilter.GetHashCode();
                }
                if (this.ManifestResourcesFilter != null)
                {
                    hashCode = (hashCode * 59) + this.ManifestResourcesFilter.GetHashCode();
                }
                if (this.AffectedGlobalEntitiesFilter != null)
                {
                    hashCode = (hashCode * 59) + this.AffectedGlobalEntitiesFilter.GetHashCode();
                }
                if (this.EventsFilter != null)
                {
                    hashCode = (hashCode * 59) + this.EventsFilter.GetHashCode();
                }
                if (this.AccountsWithManifestOwnerMethodCalls != null)
                {
                    hashCode = (hashCode * 59) + this.AccountsWithManifestOwnerMethodCalls.GetHashCode();
                }
                if (this.AccountsWithoutManifestOwnerMethodCalls != null)
                {
                    hashCode = (hashCode * 59) + this.AccountsWithoutManifestOwnerMethodCalls.GetHashCode();
                }
                if (this.ManifestClassFilter != null)
                {
                    hashCode = (hashCode * 59) + this.ManifestClassFilter.GetHashCode();
                }
                if (this.EventGlobalEmittersFilter != null)
                {
                    hashCode = (hashCode * 59) + this.EventGlobalEmittersFilter.GetHashCode();
                }
                if (this.BalanceChangeResourcesFilter != null)
                {
                    hashCode = (hashCode * 59) + this.BalanceChangeResourcesFilter.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.TransactionStatusFilter.GetHashCode();
                hashCode = (hashCode * 59) + this.Order.GetHashCode();
                if (this.OptIns != null)
                {
                    hashCode = (hashCode * 59) + this.OptIns.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
