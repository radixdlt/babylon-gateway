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
 * Radix Core API - Babylon (Anemone)
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  The default configuration is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function. The node exposes a configuration flag which allows disabling certain endpoints which may be problematic, but monitoring is advised. This configuration parameter is `api.core.flags.enable_unbounded_endpoints` / `RADIXDLT_CORE_API_FLAGS_ENABLE_UNBOUNDED_ENDPOINTS`.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` have high guarantees of forward compatibility in future node versions. We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  Other endpoints may be changed with new node versions carrying protocol-updates, although any breaking changes will be flagged clearly in the corresponding release notes.  All responses may have additional fields added, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects. 
 *
 * The version of the OpenAPI document: v1.1.3
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
using FileParameter = RadixDlt.CoreApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.CoreApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.CoreApiSdk.Model
{
    /// <summary>
    /// A request to retrieve a sublist of committed transactions from the ledger. 
    /// </summary>
    [DataContract(Name = "StreamTransactionsRequest")]
    public partial class StreamTransactionsRequest : IEquatable<StreamTransactionsRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamTransactionsRequest" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected StreamTransactionsRequest() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamTransactionsRequest" /> class.
        /// </summary>
        /// <param name="network">The logical name of the network (required).</param>
        /// <param name="fromStateVersion">fromStateVersion (required).</param>
        /// <param name="limit">The maximum number of transactions that will be returned. (required).</param>
        /// <param name="sborFormatOptions">sborFormatOptions.</param>
        /// <param name="transactionFormatOptions">transactionFormatOptions.</param>
        /// <param name="substateFormatOptions">substateFormatOptions.</param>
        /// <param name="includeProofs">Whether to include LedgerProofs (default false).</param>
        public StreamTransactionsRequest(string network = default(string), long fromStateVersion = default(long), int limit = default(int), SborFormatOptions sborFormatOptions = default(SborFormatOptions), TransactionFormatOptions transactionFormatOptions = default(TransactionFormatOptions), SubstateFormatOptions substateFormatOptions = default(SubstateFormatOptions), bool? includeProofs = default(bool?))
        {
            // to ensure "network" is required (not null)
            if (network == null)
            {
                throw new ArgumentNullException("network is a required property for StreamTransactionsRequest and cannot be null");
            }
            this.Network = network;
            this.FromStateVersion = fromStateVersion;
            this.Limit = limit;
            this.SborFormatOptions = sborFormatOptions;
            this.TransactionFormatOptions = transactionFormatOptions;
            this.SubstateFormatOptions = substateFormatOptions;
            this.IncludeProofs = includeProofs;
        }

        /// <summary>
        /// The logical name of the network
        /// </summary>
        /// <value>The logical name of the network</value>
        [DataMember(Name = "network", IsRequired = true, EmitDefaultValue = true)]
        public string Network { get; set; }

        /// <summary>
        /// Gets or Sets FromStateVersion
        /// </summary>
        [DataMember(Name = "from_state_version", IsRequired = true, EmitDefaultValue = true)]
        public long FromStateVersion { get; set; }

        /// <summary>
        /// The maximum number of transactions that will be returned.
        /// </summary>
        /// <value>The maximum number of transactions that will be returned.</value>
        [DataMember(Name = "limit", IsRequired = true, EmitDefaultValue = true)]
        public int Limit { get; set; }

        /// <summary>
        /// Gets or Sets SborFormatOptions
        /// </summary>
        [DataMember(Name = "sbor_format_options", EmitDefaultValue = true)]
        public SborFormatOptions SborFormatOptions { get; set; }

        /// <summary>
        /// Gets or Sets TransactionFormatOptions
        /// </summary>
        [DataMember(Name = "transaction_format_options", EmitDefaultValue = true)]
        public TransactionFormatOptions TransactionFormatOptions { get; set; }

        /// <summary>
        /// Gets or Sets SubstateFormatOptions
        /// </summary>
        [DataMember(Name = "substate_format_options", EmitDefaultValue = true)]
        public SubstateFormatOptions SubstateFormatOptions { get; set; }

        /// <summary>
        /// Whether to include LedgerProofs (default false)
        /// </summary>
        /// <value>Whether to include LedgerProofs (default false)</value>
        [DataMember(Name = "include_proofs", EmitDefaultValue = false)]
        public bool? IncludeProofs { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class StreamTransactionsRequest {\n");
            sb.Append("  Network: ").Append(Network).Append("\n");
            sb.Append("  FromStateVersion: ").Append(FromStateVersion).Append("\n");
            sb.Append("  Limit: ").Append(Limit).Append("\n");
            sb.Append("  SborFormatOptions: ").Append(SborFormatOptions).Append("\n");
            sb.Append("  TransactionFormatOptions: ").Append(TransactionFormatOptions).Append("\n");
            sb.Append("  SubstateFormatOptions: ").Append(SubstateFormatOptions).Append("\n");
            sb.Append("  IncludeProofs: ").Append(IncludeProofs).Append("\n");
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
            return this.Equals(input as StreamTransactionsRequest);
        }

        /// <summary>
        /// Returns true if StreamTransactionsRequest instances are equal
        /// </summary>
        /// <param name="input">Instance of StreamTransactionsRequest to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(StreamTransactionsRequest input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Network == input.Network ||
                    (this.Network != null &&
                    this.Network.Equals(input.Network))
                ) && 
                (
                    this.FromStateVersion == input.FromStateVersion ||
                    this.FromStateVersion.Equals(input.FromStateVersion)
                ) && 
                (
                    this.Limit == input.Limit ||
                    this.Limit.Equals(input.Limit)
                ) && 
                (
                    this.SborFormatOptions == input.SborFormatOptions ||
                    (this.SborFormatOptions != null &&
                    this.SborFormatOptions.Equals(input.SborFormatOptions))
                ) && 
                (
                    this.TransactionFormatOptions == input.TransactionFormatOptions ||
                    (this.TransactionFormatOptions != null &&
                    this.TransactionFormatOptions.Equals(input.TransactionFormatOptions))
                ) && 
                (
                    this.SubstateFormatOptions == input.SubstateFormatOptions ||
                    (this.SubstateFormatOptions != null &&
                    this.SubstateFormatOptions.Equals(input.SubstateFormatOptions))
                ) && 
                (
                    this.IncludeProofs == input.IncludeProofs ||
                    (this.IncludeProofs != null &&
                    this.IncludeProofs.Equals(input.IncludeProofs))
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
                if (this.Network != null)
                {
                    hashCode = (hashCode * 59) + this.Network.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.FromStateVersion.GetHashCode();
                hashCode = (hashCode * 59) + this.Limit.GetHashCode();
                if (this.SborFormatOptions != null)
                {
                    hashCode = (hashCode * 59) + this.SborFormatOptions.GetHashCode();
                }
                if (this.TransactionFormatOptions != null)
                {
                    hashCode = (hashCode * 59) + this.TransactionFormatOptions.GetHashCode();
                }
                if (this.SubstateFormatOptions != null)
                {
                    hashCode = (hashCode * 59) + this.SubstateFormatOptions.GetHashCode();
                }
                if (this.IncludeProofs != null)
                {
                    hashCode = (hashCode * 59) + this.IncludeProofs.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
