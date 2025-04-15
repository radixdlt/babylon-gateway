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
 * The version of the OpenAPI document: v1.10.1
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
    /// CommittedTransactionInfo
    /// </summary>
    [DataContract(Name = "CommittedTransactionInfo")]
    public partial class CommittedTransactionInfo
    {

        /// <summary>
        /// Gets or Sets TransactionStatus
        /// </summary>
        [DataMember(Name = "transaction_status", IsRequired = true, EmitDefaultValue = true)]
        public TransactionStatus TransactionStatus { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="CommittedTransactionInfo" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected CommittedTransactionInfo() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="CommittedTransactionInfo" /> class.
        /// </summary>
        /// <param name="stateVersion">stateVersion (required).</param>
        /// <param name="epoch">epoch (required).</param>
        /// <param name="round">round (required).</param>
        /// <param name="roundTimestamp">roundTimestamp (required).</param>
        /// <param name="transactionStatus">transactionStatus (required).</param>
        /// <param name="payloadHash">Bech32m-encoded hash..</param>
        /// <param name="intentHash">Bech32m-encoded hash..</param>
        /// <param name="feePaid">String-encoded decimal representing the amount of a related fungible resource..</param>
        /// <param name="affectedGlobalEntities">affectedGlobalEntities.</param>
        /// <param name="confirmedAt">confirmedAt.</param>
        /// <param name="errorMessage">errorMessage.</param>
        /// <param name="rawHex">Hex-encoded binary blob..</param>
        /// <param name="receipt">receipt.</param>
        /// <param name="manifestInstructions">A text-representation of a transaction manifest. This field will be present only for user transactions and when explicitly opted-in using the &#x60;manifest_instructions&#x60; flag. .</param>
        /// <param name="manifestClasses">A collection of zero or more manifest classes ordered from the most specific class to the least specific one. This field will be present only for user transactions. For user transactions with subintents only the root transaction intent is currently used to determine the manifest classes. .</param>
        /// <param name="message">The optional transaction message. This type is defined in the Core API as &#x60;TransactionMessage&#x60;. See the Core API documentation for more details. .</param>
        /// <param name="balanceChanges">balanceChanges.</param>
        /// <param name="subintentDetails">Subintent details. Please note that it is returned regardless of whether the transaction was committed successfully or failed, and it can be returned in multiple transactions. .</param>
        /// <param name="childSubintentHashes">The child subintent hashes of the root transaction intent. Please note that it is returned regardless of whether the transaction was committed successfully or failed, and it can be returned in multiple transactions. .</param>
        public CommittedTransactionInfo(long stateVersion = default(long), long epoch = default(long), long round = default(long), string roundTimestamp = default(string), TransactionStatus transactionStatus = default(TransactionStatus), string payloadHash = default(string), string intentHash = default(string), string feePaid = default(string), List<string> affectedGlobalEntities = default(List<string>), DateTime? confirmedAt = default(DateTime?), string errorMessage = default(string), string rawHex = default(string), TransactionReceipt receipt = default(TransactionReceipt), string manifestInstructions = default(string), List<ManifestClass> manifestClasses = default(List<ManifestClass>), Object message = default(Object), TransactionBalanceChanges balanceChanges = default(TransactionBalanceChanges), List<TransactionSubintentDetails> subintentDetails = default(List<TransactionSubintentDetails>), List<string> childSubintentHashes = default(List<string>))
        {
            this.StateVersion = stateVersion;
            this.Epoch = epoch;
            this.Round = round;
            // to ensure "roundTimestamp" is required (not null)
            if (roundTimestamp == null)
            {
                throw new ArgumentNullException("roundTimestamp is a required property for CommittedTransactionInfo and cannot be null");
            }
            this.RoundTimestamp = roundTimestamp;
            this.TransactionStatus = transactionStatus;
            this.PayloadHash = payloadHash;
            this.IntentHash = intentHash;
            this.FeePaid = feePaid;
            this.AffectedGlobalEntities = affectedGlobalEntities;
            this.ConfirmedAt = confirmedAt;
            this.ErrorMessage = errorMessage;
            this.RawHex = rawHex;
            this.Receipt = receipt;
            this.ManifestInstructions = manifestInstructions;
            this.ManifestClasses = manifestClasses;
            this.Message = message;
            this.BalanceChanges = balanceChanges;
            this.SubintentDetails = subintentDetails;
            this.ChildSubintentHashes = childSubintentHashes;
        }

        /// <summary>
        /// Gets or Sets StateVersion
        /// </summary>
        [DataMember(Name = "state_version", IsRequired = true, EmitDefaultValue = true)]
        public long StateVersion { get; set; }

        /// <summary>
        /// Gets or Sets Epoch
        /// </summary>
        [DataMember(Name = "epoch", IsRequired = true, EmitDefaultValue = true)]
        public long Epoch { get; set; }

        /// <summary>
        /// Gets or Sets Round
        /// </summary>
        [DataMember(Name = "round", IsRequired = true, EmitDefaultValue = true)]
        public long Round { get; set; }

        /// <summary>
        /// Gets or Sets RoundTimestamp
        /// </summary>
        [DataMember(Name = "round_timestamp", IsRequired = true, EmitDefaultValue = true)]
        public string RoundTimestamp { get; set; }

        /// <summary>
        /// Bech32m-encoded hash.
        /// </summary>
        /// <value>Bech32m-encoded hash.</value>
        [DataMember(Name = "payload_hash", EmitDefaultValue = true)]
        public string PayloadHash { get; set; }

        /// <summary>
        /// Bech32m-encoded hash.
        /// </summary>
        /// <value>Bech32m-encoded hash.</value>
        [DataMember(Name = "intent_hash", EmitDefaultValue = true)]
        public string IntentHash { get; set; }

        /// <summary>
        /// String-encoded decimal representing the amount of a related fungible resource.
        /// </summary>
        /// <value>String-encoded decimal representing the amount of a related fungible resource.</value>
        [DataMember(Name = "fee_paid", EmitDefaultValue = true)]
        public string FeePaid { get; set; }

        /// <summary>
        /// Gets or Sets AffectedGlobalEntities
        /// </summary>
        [DataMember(Name = "affected_global_entities", EmitDefaultValue = true)]
        public List<string> AffectedGlobalEntities { get; set; }

        /// <summary>
        /// Gets or Sets ConfirmedAt
        /// </summary>
        [DataMember(Name = "confirmed_at", EmitDefaultValue = true)]
        public DateTime? ConfirmedAt { get; set; }

        /// <summary>
        /// Gets or Sets ErrorMessage
        /// </summary>
        [DataMember(Name = "error_message", EmitDefaultValue = true)]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Hex-encoded binary blob.
        /// </summary>
        /// <value>Hex-encoded binary blob.</value>
        [DataMember(Name = "raw_hex", EmitDefaultValue = true)]
        public string RawHex { get; set; }

        /// <summary>
        /// Gets or Sets Receipt
        /// </summary>
        [DataMember(Name = "receipt", EmitDefaultValue = true)]
        public TransactionReceipt Receipt { get; set; }

        /// <summary>
        /// A text-representation of a transaction manifest. This field will be present only for user transactions and when explicitly opted-in using the &#x60;manifest_instructions&#x60; flag. 
        /// </summary>
        /// <value>A text-representation of a transaction manifest. This field will be present only for user transactions and when explicitly opted-in using the &#x60;manifest_instructions&#x60; flag. </value>
        [DataMember(Name = "manifest_instructions", EmitDefaultValue = true)]
        public string ManifestInstructions { get; set; }

        /// <summary>
        /// A collection of zero or more manifest classes ordered from the most specific class to the least specific one. This field will be present only for user transactions. For user transactions with subintents only the root transaction intent is currently used to determine the manifest classes. 
        /// </summary>
        /// <value>A collection of zero or more manifest classes ordered from the most specific class to the least specific one. This field will be present only for user transactions. For user transactions with subintents only the root transaction intent is currently used to determine the manifest classes. </value>
        [DataMember(Name = "manifest_classes", EmitDefaultValue = true)]
        public List<ManifestClass> ManifestClasses { get; set; }

        /// <summary>
        /// The optional transaction message. This type is defined in the Core API as &#x60;TransactionMessage&#x60;. See the Core API documentation for more details. 
        /// </summary>
        /// <value>The optional transaction message. This type is defined in the Core API as &#x60;TransactionMessage&#x60;. See the Core API documentation for more details. </value>
        [DataMember(Name = "message", EmitDefaultValue = true)]
        public Object Message { get; set; }

        /// <summary>
        /// Gets or Sets BalanceChanges
        /// </summary>
        [DataMember(Name = "balance_changes", EmitDefaultValue = true)]
        public TransactionBalanceChanges BalanceChanges { get; set; }

        /// <summary>
        /// Subintent details. Please note that it is returned regardless of whether the transaction was committed successfully or failed, and it can be returned in multiple transactions. 
        /// </summary>
        /// <value>Subintent details. Please note that it is returned regardless of whether the transaction was committed successfully or failed, and it can be returned in multiple transactions. </value>
        [DataMember(Name = "subintent_details", EmitDefaultValue = true)]
        public List<TransactionSubintentDetails> SubintentDetails { get; set; }

        /// <summary>
        /// The child subintent hashes of the root transaction intent. Please note that it is returned regardless of whether the transaction was committed successfully or failed, and it can be returned in multiple transactions. 
        /// </summary>
        /// <value>The child subintent hashes of the root transaction intent. Please note that it is returned regardless of whether the transaction was committed successfully or failed, and it can be returned in multiple transactions. </value>
        [DataMember(Name = "child_subintent_hashes", EmitDefaultValue = true)]
        public List<string> ChildSubintentHashes { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class CommittedTransactionInfo {\n");
            sb.Append("  StateVersion: ").Append(StateVersion).Append("\n");
            sb.Append("  Epoch: ").Append(Epoch).Append("\n");
            sb.Append("  Round: ").Append(Round).Append("\n");
            sb.Append("  RoundTimestamp: ").Append(RoundTimestamp).Append("\n");
            sb.Append("  TransactionStatus: ").Append(TransactionStatus).Append("\n");
            sb.Append("  PayloadHash: ").Append(PayloadHash).Append("\n");
            sb.Append("  IntentHash: ").Append(IntentHash).Append("\n");
            sb.Append("  FeePaid: ").Append(FeePaid).Append("\n");
            sb.Append("  AffectedGlobalEntities: ").Append(AffectedGlobalEntities).Append("\n");
            sb.Append("  ConfirmedAt: ").Append(ConfirmedAt).Append("\n");
            sb.Append("  ErrorMessage: ").Append(ErrorMessage).Append("\n");
            sb.Append("  RawHex: ").Append(RawHex).Append("\n");
            sb.Append("  Receipt: ").Append(Receipt).Append("\n");
            sb.Append("  ManifestInstructions: ").Append(ManifestInstructions).Append("\n");
            sb.Append("  ManifestClasses: ").Append(ManifestClasses).Append("\n");
            sb.Append("  Message: ").Append(Message).Append("\n");
            sb.Append("  BalanceChanges: ").Append(BalanceChanges).Append("\n");
            sb.Append("  SubintentDetails: ").Append(SubintentDetails).Append("\n");
            sb.Append("  ChildSubintentHashes: ").Append(ChildSubintentHashes).Append("\n");
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
            return this.Equals(input as CommittedTransactionInfo);
        }

        /// <summary>
        /// Returns true if CommittedTransactionInfo instances are equal
        /// </summary>
        /// <param name="input">Instance of CommittedTransactionInfo to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(CommittedTransactionInfo input)
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
                    this.Epoch == input.Epoch ||
                    this.Epoch.Equals(input.Epoch)
                ) && 
                (
                    this.Round == input.Round ||
                    this.Round.Equals(input.Round)
                ) && 
                (
                    this.RoundTimestamp == input.RoundTimestamp ||
                    (this.RoundTimestamp != null &&
                    this.RoundTimestamp.Equals(input.RoundTimestamp))
                ) && 
                (
                    this.TransactionStatus == input.TransactionStatus ||
                    this.TransactionStatus.Equals(input.TransactionStatus)
                ) && 
                (
                    this.PayloadHash == input.PayloadHash ||
                    (this.PayloadHash != null &&
                    this.PayloadHash.Equals(input.PayloadHash))
                ) && 
                (
                    this.IntentHash == input.IntentHash ||
                    (this.IntentHash != null &&
                    this.IntentHash.Equals(input.IntentHash))
                ) && 
                (
                    this.FeePaid == input.FeePaid ||
                    (this.FeePaid != null &&
                    this.FeePaid.Equals(input.FeePaid))
                ) && 
                (
                    this.AffectedGlobalEntities == input.AffectedGlobalEntities ||
                    this.AffectedGlobalEntities != null &&
                    input.AffectedGlobalEntities != null &&
                    this.AffectedGlobalEntities.SequenceEqual(input.AffectedGlobalEntities)
                ) && 
                (
                    this.ConfirmedAt == input.ConfirmedAt ||
                    (this.ConfirmedAt != null &&
                    this.ConfirmedAt.Equals(input.ConfirmedAt))
                ) && 
                (
                    this.ErrorMessage == input.ErrorMessage ||
                    (this.ErrorMessage != null &&
                    this.ErrorMessage.Equals(input.ErrorMessage))
                ) && 
                (
                    this.RawHex == input.RawHex ||
                    (this.RawHex != null &&
                    this.RawHex.Equals(input.RawHex))
                ) && 
                (
                    this.Receipt == input.Receipt ||
                    (this.Receipt != null &&
                    this.Receipt.Equals(input.Receipt))
                ) && 
                (
                    this.ManifestInstructions == input.ManifestInstructions ||
                    (this.ManifestInstructions != null &&
                    this.ManifestInstructions.Equals(input.ManifestInstructions))
                ) && 
                (
                    this.ManifestClasses == input.ManifestClasses ||
                    this.ManifestClasses != null &&
                    input.ManifestClasses != null &&
                    this.ManifestClasses.SequenceEqual(input.ManifestClasses)
                ) && 
                (
                    this.Message == input.Message ||
                    (this.Message != null &&
                    this.Message.Equals(input.Message))
                ) && 
                (
                    this.BalanceChanges == input.BalanceChanges ||
                    (this.BalanceChanges != null &&
                    this.BalanceChanges.Equals(input.BalanceChanges))
                ) && 
                (
                    this.SubintentDetails == input.SubintentDetails ||
                    this.SubintentDetails != null &&
                    input.SubintentDetails != null &&
                    this.SubintentDetails.SequenceEqual(input.SubintentDetails)
                ) && 
                (
                    this.ChildSubintentHashes == input.ChildSubintentHashes ||
                    this.ChildSubintentHashes != null &&
                    input.ChildSubintentHashes != null &&
                    this.ChildSubintentHashes.SequenceEqual(input.ChildSubintentHashes)
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
                hashCode = (hashCode * 59) + this.Epoch.GetHashCode();
                hashCode = (hashCode * 59) + this.Round.GetHashCode();
                if (this.RoundTimestamp != null)
                {
                    hashCode = (hashCode * 59) + this.RoundTimestamp.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.TransactionStatus.GetHashCode();
                if (this.PayloadHash != null)
                {
                    hashCode = (hashCode * 59) + this.PayloadHash.GetHashCode();
                }
                if (this.IntentHash != null)
                {
                    hashCode = (hashCode * 59) + this.IntentHash.GetHashCode();
                }
                if (this.FeePaid != null)
                {
                    hashCode = (hashCode * 59) + this.FeePaid.GetHashCode();
                }
                if (this.AffectedGlobalEntities != null)
                {
                    hashCode = (hashCode * 59) + this.AffectedGlobalEntities.GetHashCode();
                }
                if (this.ConfirmedAt != null)
                {
                    hashCode = (hashCode * 59) + this.ConfirmedAt.GetHashCode();
                }
                if (this.ErrorMessage != null)
                {
                    hashCode = (hashCode * 59) + this.ErrorMessage.GetHashCode();
                }
                if (this.RawHex != null)
                {
                    hashCode = (hashCode * 59) + this.RawHex.GetHashCode();
                }
                if (this.Receipt != null)
                {
                    hashCode = (hashCode * 59) + this.Receipt.GetHashCode();
                }
                if (this.ManifestInstructions != null)
                {
                    hashCode = (hashCode * 59) + this.ManifestInstructions.GetHashCode();
                }
                if (this.ManifestClasses != null)
                {
                    hashCode = (hashCode * 59) + this.ManifestClasses.GetHashCode();
                }
                if (this.Message != null)
                {
                    hashCode = (hashCode * 59) + this.Message.GetHashCode();
                }
                if (this.BalanceChanges != null)
                {
                    hashCode = (hashCode * 59) + this.BalanceChanges.GetHashCode();
                }
                if (this.SubintentDetails != null)
                {
                    hashCode = (hashCode * 59) + this.SubintentDetails.GetHashCode();
                }
                if (this.ChildSubintentHashes != null)
                {
                    hashCode = (hashCode * 59) + this.ChildSubintentHashes.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
