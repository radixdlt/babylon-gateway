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

// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace RadixDlt.CoreApiSdk.Kiota.Models {
    public class CostingParameters : IBackedModel, IParsable 
    {
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }
        /// <summary>An integer between `0` and `2^32 - 1`, representing the maximum amount of cost units available for the transaction execution.</summary>
        public long? ExecutionCostUnitLimit {
            get { return BackingStore?.Get<long?>("execution_cost_unit_limit"); }
            set { BackingStore?.Set("execution_cost_unit_limit", value); }
        }
        /// <summary>An integer between `0` and `2^32 - 1`, representing the number of execution cost units loaned from system.</summary>
        public long? ExecutionCostUnitLoan {
            get { return BackingStore?.Get<long?>("execution_cost_unit_loan"); }
            set { BackingStore?.Set("execution_cost_unit_loan", value); }
        }
        /// <summary>The string-encoded decimal representing the XRD price of a single cost unit of transaction execution.A decimal is formed of some signed integer `m` of attos (`10^(-18)`) units, where `-2^(192 - 1) &lt;= m &lt; 2^(192 - 1)`.</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? ExecutionCostUnitPrice {
            get { return BackingStore?.Get<string?>("execution_cost_unit_price"); }
            set { BackingStore?.Set("execution_cost_unit_price", value); }
        }
#nullable restore
#else
        public string ExecutionCostUnitPrice {
            get { return BackingStore?.Get<string>("execution_cost_unit_price"); }
            set { BackingStore?.Set("execution_cost_unit_price", value); }
        }
#endif
        /// <summary>An integer between `0` and `2^32 - 1`, representing the maximum amount of cost units available for the transaction finalization.</summary>
        public long? FinalizationCostUnitLimit {
            get { return BackingStore?.Get<long?>("finalization_cost_unit_limit"); }
            set { BackingStore?.Set("finalization_cost_unit_limit", value); }
        }
        /// <summary>The string-encoded decimal representing the XRD price of a single cost unit of transaction finalization.A decimal is formed of some signed integer `m` of attos (`10^(-18)`) units, where `-2^(192 - 1) &lt;= m &lt; 2^(192 - 1)`.</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? FinalizationCostUnitPrice {
            get { return BackingStore?.Get<string?>("finalization_cost_unit_price"); }
            set { BackingStore?.Set("finalization_cost_unit_price", value); }
        }
#nullable restore
#else
        public string FinalizationCostUnitPrice {
            get { return BackingStore?.Get<string>("finalization_cost_unit_price"); }
            set { BackingStore?.Set("finalization_cost_unit_price", value); }
        }
#endif
        /// <summary>An integer between `0` and `65535`, giving the validator tip as a percentage amount. A value of `1` corresponds to 1% of the fee.</summary>
        public int? TipPercentage {
            get { return BackingStore?.Get<int?>("tip_percentage"); }
            set { BackingStore?.Set("tip_percentage", value); }
        }
        /// <summary>The string-encoded decimal representing the price of 1 byte of archive storage, expressed in XRD.A decimal is formed of some signed integer `m` of attos (`10^(-18)`) units, where `-2^(192 - 1) &lt;= m &lt; 2^(192 - 1)`.</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? XrdArchiveStoragePrice {
            get { return BackingStore?.Get<string?>("xrd_archive_storage_price"); }
            set { BackingStore?.Set("xrd_archive_storage_price", value); }
        }
#nullable restore
#else
        public string XrdArchiveStoragePrice {
            get { return BackingStore?.Get<string>("xrd_archive_storage_price"); }
            set { BackingStore?.Set("xrd_archive_storage_price", value); }
        }
#endif
        /// <summary>The string-encoded decimal representing the price of 1 byte of state storage, expressed in XRD.A decimal is formed of some signed integer `m` of attos (`10^(-18)`) units, where `-2^(192 - 1) &lt;= m &lt; 2^(192 - 1)`.</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? XrdStoragePrice {
            get { return BackingStore?.Get<string?>("xrd_storage_price"); }
            set { BackingStore?.Set("xrd_storage_price", value); }
        }
#nullable restore
#else
        public string XrdStoragePrice {
            get { return BackingStore?.Get<string>("xrd_storage_price"); }
            set { BackingStore?.Set("xrd_storage_price", value); }
        }
#endif
        /// <summary>The string-encoded decimal representing the price of 1 USD, expressed in XRD.A decimal is formed of some signed integer `m` of attos (`10^(-18)`) units, where `-2^(192 - 1) &lt;= m &lt; 2^(192 - 1)`.</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? XrdUsdPrice {
            get { return BackingStore?.Get<string?>("xrd_usd_price"); }
            set { BackingStore?.Set("xrd_usd_price", value); }
        }
#nullable restore
#else
        public string XrdUsdPrice {
            get { return BackingStore?.Get<string>("xrd_usd_price"); }
            set { BackingStore?.Set("xrd_usd_price", value); }
        }
#endif
        /// <summary>
        /// Instantiates a new <see cref="CostingParameters"/> and sets the default values.
        /// </summary>
        public CostingParameters()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="CostingParameters"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static CostingParameters CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new CostingParameters();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                {"execution_cost_unit_limit", n => { ExecutionCostUnitLimit = n.GetLongValue(); } },
                {"execution_cost_unit_loan", n => { ExecutionCostUnitLoan = n.GetLongValue(); } },
                {"execution_cost_unit_price", n => { ExecutionCostUnitPrice = n.GetStringValue(); } },
                {"finalization_cost_unit_limit", n => { FinalizationCostUnitLimit = n.GetLongValue(); } },
                {"finalization_cost_unit_price", n => { FinalizationCostUnitPrice = n.GetStringValue(); } },
                {"tip_percentage", n => { TipPercentage = n.GetIntValue(); } },
                {"xrd_archive_storage_price", n => { XrdArchiveStoragePrice = n.GetStringValue(); } },
                {"xrd_storage_price", n => { XrdStoragePrice = n.GetStringValue(); } },
                {"xrd_usd_price", n => { XrdUsdPrice = n.GetStringValue(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteLongValue("execution_cost_unit_limit", ExecutionCostUnitLimit);
            writer.WriteLongValue("execution_cost_unit_loan", ExecutionCostUnitLoan);
            writer.WriteStringValue("execution_cost_unit_price", ExecutionCostUnitPrice);
            writer.WriteLongValue("finalization_cost_unit_limit", FinalizationCostUnitLimit);
            writer.WriteStringValue("finalization_cost_unit_price", FinalizationCostUnitPrice);
            writer.WriteIntValue("tip_percentage", TipPercentage);
            writer.WriteStringValue("xrd_archive_storage_price", XrdArchiveStoragePrice);
            writer.WriteStringValue("xrd_storage_price", XrdStoragePrice);
            writer.WriteStringValue("xrd_usd_price", XrdUsdPrice);
        }
    }
}
