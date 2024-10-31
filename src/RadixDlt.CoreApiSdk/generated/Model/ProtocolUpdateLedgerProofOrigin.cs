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
using JsonSubTypes;
using FileParameter = RadixDlt.CoreApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.CoreApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.CoreApiSdk.Model
{
    /// <summary>
    /// ProtocolUpdateLedgerProofOrigin
    /// </summary>
    [DataContract(Name = "ProtocolUpdateLedgerProofOrigin")]
    [JsonConverter(typeof(JsonSubtypes), "type")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusLedgerProofOrigin), "Consensus")]
    [JsonSubtypes.KnownSubType(typeof(GenesisLedgerProofOrigin), "Genesis")]
    [JsonSubtypes.KnownSubType(typeof(ProtocolUpdateLedgerProofOrigin), "ProtocolUpdate")]
    public partial class ProtocolUpdateLedgerProofOrigin : LedgerProofOrigin, IEquatable<ProtocolUpdateLedgerProofOrigin>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolUpdateLedgerProofOrigin" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected ProtocolUpdateLedgerProofOrigin() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolUpdateLedgerProofOrigin" /> class.
        /// </summary>
        /// <param name="protocolVersionName">protocolVersionName (required).</param>
        /// <param name="configHash">configHash (required).</param>
        /// <param name="batchGroupIdx">batchGroupIdx (required).</param>
        /// <param name="batchGroupName">batchGroupName (required).</param>
        /// <param name="batchIdx">batchIdx (required).</param>
        /// <param name="batchName">batchName (required).</param>
        /// <param name="isEndOfUpdate">isEndOfUpdate (required).</param>
        /// <param name="type">type (required) (default to LedgerProofOriginType.ProtocolUpdate).</param>
        public ProtocolUpdateLedgerProofOrigin(string protocolVersionName = default(string), string configHash = default(string), long batchGroupIdx = default(long), string batchGroupName = default(string), long batchIdx = default(long), string batchName = default(string), bool isEndOfUpdate = default(bool), LedgerProofOriginType type = LedgerProofOriginType.ProtocolUpdate) : base(type)
        {
            // to ensure "protocolVersionName" is required (not null)
            if (protocolVersionName == null)
            {
                throw new ArgumentNullException("protocolVersionName is a required property for ProtocolUpdateLedgerProofOrigin and cannot be null");
            }
            this.ProtocolVersionName = protocolVersionName;
            // to ensure "configHash" is required (not null)
            if (configHash == null)
            {
                throw new ArgumentNullException("configHash is a required property for ProtocolUpdateLedgerProofOrigin and cannot be null");
            }
            this.ConfigHash = configHash;
            this.BatchGroupIdx = batchGroupIdx;
            // to ensure "batchGroupName" is required (not null)
            if (batchGroupName == null)
            {
                throw new ArgumentNullException("batchGroupName is a required property for ProtocolUpdateLedgerProofOrigin and cannot be null");
            }
            this.BatchGroupName = batchGroupName;
            this.BatchIdx = batchIdx;
            // to ensure "batchName" is required (not null)
            if (batchName == null)
            {
                throw new ArgumentNullException("batchName is a required property for ProtocolUpdateLedgerProofOrigin and cannot be null");
            }
            this.BatchName = batchName;
            this.IsEndOfUpdate = isEndOfUpdate;
        }

        /// <summary>
        /// Gets or Sets ProtocolVersionName
        /// </summary>
        [DataMember(Name = "protocol_version_name", IsRequired = true, EmitDefaultValue = true)]
        public string ProtocolVersionName { get; set; }

        /// <summary>
        /// Gets or Sets ConfigHash
        /// </summary>
        [DataMember(Name = "config_hash", IsRequired = true, EmitDefaultValue = true)]
        public string ConfigHash { get; set; }

        /// <summary>
        /// Gets or Sets BatchGroupIdx
        /// </summary>
        [DataMember(Name = "batch_group_idx", IsRequired = true, EmitDefaultValue = true)]
        public long BatchGroupIdx { get; set; }

        /// <summary>
        /// Gets or Sets BatchGroupName
        /// </summary>
        [DataMember(Name = "batch_group_name", IsRequired = true, EmitDefaultValue = true)]
        public string BatchGroupName { get; set; }

        /// <summary>
        /// Gets or Sets BatchIdx
        /// </summary>
        [DataMember(Name = "batch_idx", IsRequired = true, EmitDefaultValue = true)]
        public long BatchIdx { get; set; }

        /// <summary>
        /// Gets or Sets BatchName
        /// </summary>
        [DataMember(Name = "batch_name", IsRequired = true, EmitDefaultValue = true)]
        public string BatchName { get; set; }

        /// <summary>
        /// Gets or Sets IsEndOfUpdate
        /// </summary>
        [DataMember(Name = "is_end_of_update", IsRequired = true, EmitDefaultValue = true)]
        public bool IsEndOfUpdate { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ProtocolUpdateLedgerProofOrigin {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  ProtocolVersionName: ").Append(ProtocolVersionName).Append("\n");
            sb.Append("  ConfigHash: ").Append(ConfigHash).Append("\n");
            sb.Append("  BatchGroupIdx: ").Append(BatchGroupIdx).Append("\n");
            sb.Append("  BatchGroupName: ").Append(BatchGroupName).Append("\n");
            sb.Append("  BatchIdx: ").Append(BatchIdx).Append("\n");
            sb.Append("  BatchName: ").Append(BatchName).Append("\n");
            sb.Append("  IsEndOfUpdate: ").Append(IsEndOfUpdate).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public override string ToJson()
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
            return this.Equals(input as ProtocolUpdateLedgerProofOrigin);
        }

        /// <summary>
        /// Returns true if ProtocolUpdateLedgerProofOrigin instances are equal
        /// </summary>
        /// <param name="input">Instance of ProtocolUpdateLedgerProofOrigin to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ProtocolUpdateLedgerProofOrigin input)
        {
            if (input == null)
            {
                return false;
            }
            return base.Equals(input) && 
                (
                    this.ProtocolVersionName == input.ProtocolVersionName ||
                    (this.ProtocolVersionName != null &&
                    this.ProtocolVersionName.Equals(input.ProtocolVersionName))
                ) && base.Equals(input) && 
                (
                    this.ConfigHash == input.ConfigHash ||
                    (this.ConfigHash != null &&
                    this.ConfigHash.Equals(input.ConfigHash))
                ) && base.Equals(input) && 
                (
                    this.BatchGroupIdx == input.BatchGroupIdx ||
                    this.BatchGroupIdx.Equals(input.BatchGroupIdx)
                ) && base.Equals(input) && 
                (
                    this.BatchGroupName == input.BatchGroupName ||
                    (this.BatchGroupName != null &&
                    this.BatchGroupName.Equals(input.BatchGroupName))
                ) && base.Equals(input) && 
                (
                    this.BatchIdx == input.BatchIdx ||
                    this.BatchIdx.Equals(input.BatchIdx)
                ) && base.Equals(input) && 
                (
                    this.BatchName == input.BatchName ||
                    (this.BatchName != null &&
                    this.BatchName.Equals(input.BatchName))
                ) && base.Equals(input) && 
                (
                    this.IsEndOfUpdate == input.IsEndOfUpdate ||
                    this.IsEndOfUpdate.Equals(input.IsEndOfUpdate)
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
                int hashCode = base.GetHashCode();
                if (this.ProtocolVersionName != null)
                {
                    hashCode = (hashCode * 59) + this.ProtocolVersionName.GetHashCode();
                }
                if (this.ConfigHash != null)
                {
                    hashCode = (hashCode * 59) + this.ConfigHash.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.BatchGroupIdx.GetHashCode();
                if (this.BatchGroupName != null)
                {
                    hashCode = (hashCode * 59) + this.BatchGroupName.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.BatchIdx.GetHashCode();
                if (this.BatchName != null)
                {
                    hashCode = (hashCode * 59) + this.BatchName.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.IsEndOfUpdate.GetHashCode();
                return hashCode;
            }
        }

    }

}
