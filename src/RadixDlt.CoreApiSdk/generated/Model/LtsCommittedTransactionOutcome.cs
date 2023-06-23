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
    /// For the given transaction, contains the status, total fee summary and individual entity resource balance changes. The balance changes accounts for the fee payments as well. Current implementation does not take into account recalls, but this will change in a future update. For failed transactions, current implementation does not return any balance changes (not even the fee payments). This will also change in a future update. 
    /// </summary>
    [DataContract(Name = "LtsCommittedTransactionOutcome")]
    public partial class LtsCommittedTransactionOutcome : IEquatable<LtsCommittedTransactionOutcome>
    {

        /// <summary>
        /// Gets or Sets Status
        /// </summary>
        [DataMember(Name = "status", IsRequired = true, EmitDefaultValue = true)]
        public LtsCommittedTransactionStatus Status { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="LtsCommittedTransactionOutcome" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected LtsCommittedTransactionOutcome() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="LtsCommittedTransactionOutcome" /> class.
        /// </summary>
        /// <param name="stateVersion">An integer between &#x60;1&#x60; and &#x60;10^13&#x60;, giving the resultant state version after the transaction has been committed (required).</param>
        /// <param name="accumulatorHash">The hex-encoded transaction accumulator hash. This hash captures the order of all transactions on ledger. This hash is &#x60;ACC_{N+1} &#x3D; combine(ACC_N, LEDGER_HASH_{N}))&#x60; (where &#x60;combine()&#x60; is an arbitrary deterministic function we use).  (required).</param>
        /// <param name="userTransactionIdentifiers">userTransactionIdentifiers.</param>
        /// <param name="status">status (required).</param>
        /// <param name="fungibleEntityBalanceChanges">THE FEE ASSIGNMENT IS NOT CURRENTLY FULLY ACCURATE FOR SOME TRANSACTIONS. THIS WILL BE FIXED AT RCNET-V2. A list of all fungible balance updates which occurred in this transaction, aggregated by the global entity (such as account) which owns the vaults which were updated.  (required).</param>
        /// <param name="resultantAccountFungibleBalances">THIS CURRENTLY RETURNS AN EMPTY LIST. THIS FEATURE WILL BE COMING AT RCNET-V2. A list of the resultant balances of any account balances changed in this transaction. Only balances for accounts are returned, not any other kind of entity.  (required).</param>
        /// <param name="totalFee">The string-encoded decimal representing the total amount of XRD payed as fee (execution, validator tip and royalties). A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(256 - 1) &lt;&#x3D; m &lt; 2^(256 - 1)&#x60;.  (required).</param>
        public LtsCommittedTransactionOutcome(long stateVersion = default(long), string accumulatorHash = default(string), TransactionIdentifiers userTransactionIdentifiers = default(TransactionIdentifiers), LtsCommittedTransactionStatus status = default(LtsCommittedTransactionStatus), List<LtsEntityFungibleBalanceChanges> fungibleEntityBalanceChanges = default(List<LtsEntityFungibleBalanceChanges>), List<LtsResultantAccountFungibleBalances> resultantAccountFungibleBalances = default(List<LtsResultantAccountFungibleBalances>), string totalFee = default(string))
        {
            this.StateVersion = stateVersion;
            // to ensure "accumulatorHash" is required (not null)
            if (accumulatorHash == null)
            {
                throw new ArgumentNullException("accumulatorHash is a required property for LtsCommittedTransactionOutcome and cannot be null");
            }
            this.AccumulatorHash = accumulatorHash;
            this.Status = status;
            // to ensure "fungibleEntityBalanceChanges" is required (not null)
            if (fungibleEntityBalanceChanges == null)
            {
                throw new ArgumentNullException("fungibleEntityBalanceChanges is a required property for LtsCommittedTransactionOutcome and cannot be null");
            }
            this.FungibleEntityBalanceChanges = fungibleEntityBalanceChanges;
            // to ensure "resultantAccountFungibleBalances" is required (not null)
            if (resultantAccountFungibleBalances == null)
            {
                throw new ArgumentNullException("resultantAccountFungibleBalances is a required property for LtsCommittedTransactionOutcome and cannot be null");
            }
            this.ResultantAccountFungibleBalances = resultantAccountFungibleBalances;
            // to ensure "totalFee" is required (not null)
            if (totalFee == null)
            {
                throw new ArgumentNullException("totalFee is a required property for LtsCommittedTransactionOutcome and cannot be null");
            }
            this.TotalFee = totalFee;
            this.UserTransactionIdentifiers = userTransactionIdentifiers;
        }

        /// <summary>
        /// An integer between &#x60;1&#x60; and &#x60;10^13&#x60;, giving the resultant state version after the transaction has been committed
        /// </summary>
        /// <value>An integer between &#x60;1&#x60; and &#x60;10^13&#x60;, giving the resultant state version after the transaction has been committed</value>
        [DataMember(Name = "state_version", IsRequired = true, EmitDefaultValue = true)]
        public long StateVersion { get; set; }

        /// <summary>
        /// The hex-encoded transaction accumulator hash. This hash captures the order of all transactions on ledger. This hash is &#x60;ACC_{N+1} &#x3D; combine(ACC_N, LEDGER_HASH_{N}))&#x60; (where &#x60;combine()&#x60; is an arbitrary deterministic function we use). 
        /// </summary>
        /// <value>The hex-encoded transaction accumulator hash. This hash captures the order of all transactions on ledger. This hash is &#x60;ACC_{N+1} &#x3D; combine(ACC_N, LEDGER_HASH_{N}))&#x60; (where &#x60;combine()&#x60; is an arbitrary deterministic function we use). </value>
        [DataMember(Name = "accumulator_hash", IsRequired = true, EmitDefaultValue = true)]
        public string AccumulatorHash { get; set; }

        /// <summary>
        /// Gets or Sets UserTransactionIdentifiers
        /// </summary>
        [DataMember(Name = "user_transaction_identifiers", EmitDefaultValue = true)]
        public TransactionIdentifiers UserTransactionIdentifiers { get; set; }

        /// <summary>
        /// THE FEE ASSIGNMENT IS NOT CURRENTLY FULLY ACCURATE FOR SOME TRANSACTIONS. THIS WILL BE FIXED AT RCNET-V2. A list of all fungible balance updates which occurred in this transaction, aggregated by the global entity (such as account) which owns the vaults which were updated. 
        /// </summary>
        /// <value>THE FEE ASSIGNMENT IS NOT CURRENTLY FULLY ACCURATE FOR SOME TRANSACTIONS. THIS WILL BE FIXED AT RCNET-V2. A list of all fungible balance updates which occurred in this transaction, aggregated by the global entity (such as account) which owns the vaults which were updated. </value>
        [DataMember(Name = "fungible_entity_balance_changes", IsRequired = true, EmitDefaultValue = true)]
        public List<LtsEntityFungibleBalanceChanges> FungibleEntityBalanceChanges { get; set; }

        /// <summary>
        /// THIS CURRENTLY RETURNS AN EMPTY LIST. THIS FEATURE WILL BE COMING AT RCNET-V2. A list of the resultant balances of any account balances changed in this transaction. Only balances for accounts are returned, not any other kind of entity. 
        /// </summary>
        /// <value>THIS CURRENTLY RETURNS AN EMPTY LIST. THIS FEATURE WILL BE COMING AT RCNET-V2. A list of the resultant balances of any account balances changed in this transaction. Only balances for accounts are returned, not any other kind of entity. </value>
        [DataMember(Name = "resultant_account_fungible_balances", IsRequired = true, EmitDefaultValue = true)]
        public List<LtsResultantAccountFungibleBalances> ResultantAccountFungibleBalances { get; set; }

        /// <summary>
        /// The string-encoded decimal representing the total amount of XRD payed as fee (execution, validator tip and royalties). A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(256 - 1) &lt;&#x3D; m &lt; 2^(256 - 1)&#x60;. 
        /// </summary>
        /// <value>The string-encoded decimal representing the total amount of XRD payed as fee (execution, validator tip and royalties). A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(256 - 1) &lt;&#x3D; m &lt; 2^(256 - 1)&#x60;. </value>
        [DataMember(Name = "total_fee", IsRequired = true, EmitDefaultValue = true)]
        public string TotalFee { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class LtsCommittedTransactionOutcome {\n");
            sb.Append("  StateVersion: ").Append(StateVersion).Append("\n");
            sb.Append("  AccumulatorHash: ").Append(AccumulatorHash).Append("\n");
            sb.Append("  UserTransactionIdentifiers: ").Append(UserTransactionIdentifiers).Append("\n");
            sb.Append("  Status: ").Append(Status).Append("\n");
            sb.Append("  FungibleEntityBalanceChanges: ").Append(FungibleEntityBalanceChanges).Append("\n");
            sb.Append("  ResultantAccountFungibleBalances: ").Append(ResultantAccountFungibleBalances).Append("\n");
            sb.Append("  TotalFee: ").Append(TotalFee).Append("\n");
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
            return this.Equals(input as LtsCommittedTransactionOutcome);
        }

        /// <summary>
        /// Returns true if LtsCommittedTransactionOutcome instances are equal
        /// </summary>
        /// <param name="input">Instance of LtsCommittedTransactionOutcome to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(LtsCommittedTransactionOutcome input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.StateVersion == input.StateVersion ||
                    this.StateVersion.Equals(input.StateVersion)
                ) && 
                (
                    this.AccumulatorHash == input.AccumulatorHash ||
                    (this.AccumulatorHash != null &&
                    this.AccumulatorHash.Equals(input.AccumulatorHash))
                ) && 
                (
                    this.UserTransactionIdentifiers == input.UserTransactionIdentifiers ||
                    (this.UserTransactionIdentifiers != null &&
                    this.UserTransactionIdentifiers.Equals(input.UserTransactionIdentifiers))
                ) && 
                (
                    this.Status == input.Status ||
                    this.Status.Equals(input.Status)
                ) && 
                (
                    this.FungibleEntityBalanceChanges == input.FungibleEntityBalanceChanges ||
                    this.FungibleEntityBalanceChanges != null &&
                    input.FungibleEntityBalanceChanges != null &&
                    this.FungibleEntityBalanceChanges.SequenceEqual(input.FungibleEntityBalanceChanges)
                ) && 
                (
                    this.ResultantAccountFungibleBalances == input.ResultantAccountFungibleBalances ||
                    this.ResultantAccountFungibleBalances != null &&
                    input.ResultantAccountFungibleBalances != null &&
                    this.ResultantAccountFungibleBalances.SequenceEqual(input.ResultantAccountFungibleBalances)
                ) && 
                (
                    this.TotalFee == input.TotalFee ||
                    (this.TotalFee != null &&
                    this.TotalFee.Equals(input.TotalFee))
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
                hashCode = (hashCode * 59) + this.StateVersion.GetHashCode();
                if (this.AccumulatorHash != null)
                {
                    hashCode = (hashCode * 59) + this.AccumulatorHash.GetHashCode();
                }
                if (this.UserTransactionIdentifiers != null)
                {
                    hashCode = (hashCode * 59) + this.UserTransactionIdentifiers.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.Status.GetHashCode();
                if (this.FungibleEntityBalanceChanges != null)
                {
                    hashCode = (hashCode * 59) + this.FungibleEntityBalanceChanges.GetHashCode();
                }
                if (this.ResultantAccountFungibleBalances != null)
                {
                    hashCode = (hashCode * 59) + this.ResultantAccountFungibleBalances.GetHashCode();
                }
                if (this.TotalFee != null)
                {
                    hashCode = (hashCode * 59) + this.TotalFee.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
