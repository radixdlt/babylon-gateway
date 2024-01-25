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
using FileParameter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Model
{
    /// <summary>
    /// TransactionStatusResponseKnownPayloadItem
    /// </summary>
    [DataContract(Name = "TransactionStatusResponseKnownPayloadItem")]
    public partial class TransactionStatusResponseKnownPayloadItem : IEquatable<TransactionStatusResponseKnownPayloadItem>
    {

        /// <summary>
        /// Gets or Sets Status
        /// </summary>
        [DataMember(Name = "status", IsRequired = true, EmitDefaultValue = true)]
        public TransactionStatus Status { get; set; }

        /// <summary>
        /// Gets or Sets PayloadStatus
        /// </summary>
        [DataMember(Name = "payload_status", EmitDefaultValue = true)]
        public TransactionPayloadStatus? PayloadStatus { get; set; }

        /// <summary>
        /// Gets or Sets HandlingStatus
        /// </summary>
        [DataMember(Name = "handling_status", EmitDefaultValue = true)]
        public TransactionPayloadGatewayHandlingStatus? HandlingStatus { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionStatusResponseKnownPayloadItem" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected TransactionStatusResponseKnownPayloadItem() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionStatusResponseKnownPayloadItem" /> class.
        /// </summary>
        /// <param name="payloadHash">Bech32m-encoded hash. (required).</param>
        /// <param name="status">status (required).</param>
        /// <param name="payloadStatus">payloadStatus.</param>
        /// <param name="payloadStatusDescription">An additional description to clarify the payload status. .</param>
        /// <param name="errorMessage">The initial error message received for a rejection or failure during transaction execution. This will typically be the useful error message, explaining the root cause of the issue. Please note that presence of an error message doesn&#39;t imply that this payload will definitely reject or fail. This could represent an error during a temporary rejection (such as out of fees) which then gets resolved (e.g. by depositing money to pay the fee), allowing the transaction to be committed. .</param>
        /// <param name="latestErrorMessage">The latest error message received for a rejection or failure during transaction execution, this is only returned if it is different from the initial error message. This is more current than the initial error message, but may be less useful, as it could be a message regarding the expiry of the transaction at the end of its epoch validity window. Please note that presence of an error message doesn&#39;t imply that this payload will definitely reject or fail. This could represent an error during a temporary rejection (such as out of fees) which then gets resolved (e.g. by depositing money to pay the fee), allowing the transaction to be committed. .</param>
        /// <param name="handlingStatus">handlingStatus.</param>
        /// <param name="handlingStatusReason">Additional reason for why the Gateway has its current handling status. .</param>
        /// <param name="submissionError">The most recent error message received when submitting this transaction to the network. Please note that the presence of an error message doesn&#39;t imply that this transaction payload will definitely reject or fail. This could be a transient error. .</param>
        public TransactionStatusResponseKnownPayloadItem(string payloadHash = default(string), TransactionStatus status = default(TransactionStatus), TransactionPayloadStatus? payloadStatus = default(TransactionPayloadStatus?), string payloadStatusDescription = default(string), string errorMessage = default(string), string latestErrorMessage = default(string), TransactionPayloadGatewayHandlingStatus? handlingStatus = default(TransactionPayloadGatewayHandlingStatus?), string handlingStatusReason = default(string), string submissionError = default(string))
        {
            // to ensure "payloadHash" is required (not null)
            if (payloadHash == null)
            {
                throw new ArgumentNullException("payloadHash is a required property for TransactionStatusResponseKnownPayloadItem and cannot be null");
            }
            this.PayloadHash = payloadHash;
            this.Status = status;
            this.PayloadStatus = payloadStatus;
            this.PayloadStatusDescription = payloadStatusDescription;
            this.ErrorMessage = errorMessage;
            this.LatestErrorMessage = latestErrorMessage;
            this.HandlingStatus = handlingStatus;
            this.HandlingStatusReason = handlingStatusReason;
            this.SubmissionError = submissionError;
        }

        /// <summary>
        /// Bech32m-encoded hash.
        /// </summary>
        /// <value>Bech32m-encoded hash.</value>
        [DataMember(Name = "payload_hash", IsRequired = true, EmitDefaultValue = true)]
        public string PayloadHash { get; set; }

        /// <summary>
        /// An additional description to clarify the payload status. 
        /// </summary>
        /// <value>An additional description to clarify the payload status. </value>
        [DataMember(Name = "payload_status_description", EmitDefaultValue = true)]
        public string PayloadStatusDescription { get; set; }

        /// <summary>
        /// The initial error message received for a rejection or failure during transaction execution. This will typically be the useful error message, explaining the root cause of the issue. Please note that presence of an error message doesn&#39;t imply that this payload will definitely reject or fail. This could represent an error during a temporary rejection (such as out of fees) which then gets resolved (e.g. by depositing money to pay the fee), allowing the transaction to be committed. 
        /// </summary>
        /// <value>The initial error message received for a rejection or failure during transaction execution. This will typically be the useful error message, explaining the root cause of the issue. Please note that presence of an error message doesn&#39;t imply that this payload will definitely reject or fail. This could represent an error during a temporary rejection (such as out of fees) which then gets resolved (e.g. by depositing money to pay the fee), allowing the transaction to be committed. </value>
        [DataMember(Name = "error_message", EmitDefaultValue = true)]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The latest error message received for a rejection or failure during transaction execution, this is only returned if it is different from the initial error message. This is more current than the initial error message, but may be less useful, as it could be a message regarding the expiry of the transaction at the end of its epoch validity window. Please note that presence of an error message doesn&#39;t imply that this payload will definitely reject or fail. This could represent an error during a temporary rejection (such as out of fees) which then gets resolved (e.g. by depositing money to pay the fee), allowing the transaction to be committed. 
        /// </summary>
        /// <value>The latest error message received for a rejection or failure during transaction execution, this is only returned if it is different from the initial error message. This is more current than the initial error message, but may be less useful, as it could be a message regarding the expiry of the transaction at the end of its epoch validity window. Please note that presence of an error message doesn&#39;t imply that this payload will definitely reject or fail. This could represent an error during a temporary rejection (such as out of fees) which then gets resolved (e.g. by depositing money to pay the fee), allowing the transaction to be committed. </value>
        [DataMember(Name = "latest_error_message", EmitDefaultValue = true)]
        public string LatestErrorMessage { get; set; }

        /// <summary>
        /// Additional reason for why the Gateway has its current handling status. 
        /// </summary>
        /// <value>Additional reason for why the Gateway has its current handling status. </value>
        [DataMember(Name = "handling_status_reason", EmitDefaultValue = true)]
        public string HandlingStatusReason { get; set; }

        /// <summary>
        /// The most recent error message received when submitting this transaction to the network. Please note that the presence of an error message doesn&#39;t imply that this transaction payload will definitely reject or fail. This could be a transient error. 
        /// </summary>
        /// <value>The most recent error message received when submitting this transaction to the network. Please note that the presence of an error message doesn&#39;t imply that this transaction payload will definitely reject or fail. This could be a transient error. </value>
        [DataMember(Name = "submission_error", EmitDefaultValue = true)]
        public string SubmissionError { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class TransactionStatusResponseKnownPayloadItem {\n");
            sb.Append("  PayloadHash: ").Append(PayloadHash).Append("\n");
            sb.Append("  Status: ").Append(Status).Append("\n");
            sb.Append("  PayloadStatus: ").Append(PayloadStatus).Append("\n");
            sb.Append("  PayloadStatusDescription: ").Append(PayloadStatusDescription).Append("\n");
            sb.Append("  ErrorMessage: ").Append(ErrorMessage).Append("\n");
            sb.Append("  LatestErrorMessage: ").Append(LatestErrorMessage).Append("\n");
            sb.Append("  HandlingStatus: ").Append(HandlingStatus).Append("\n");
            sb.Append("  HandlingStatusReason: ").Append(HandlingStatusReason).Append("\n");
            sb.Append("  SubmissionError: ").Append(SubmissionError).Append("\n");
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
            return this.Equals(input as TransactionStatusResponseKnownPayloadItem);
        }

        /// <summary>
        /// Returns true if TransactionStatusResponseKnownPayloadItem instances are equal
        /// </summary>
        /// <param name="input">Instance of TransactionStatusResponseKnownPayloadItem to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TransactionStatusResponseKnownPayloadItem input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.PayloadHash == input.PayloadHash ||
                    (this.PayloadHash != null &&
                    this.PayloadHash.Equals(input.PayloadHash))
                ) && 
                (
                    this.Status == input.Status ||
                    this.Status.Equals(input.Status)
                ) && 
                (
                    this.PayloadStatus == input.PayloadStatus ||
                    this.PayloadStatus.Equals(input.PayloadStatus)
                ) && 
                (
                    this.PayloadStatusDescription == input.PayloadStatusDescription ||
                    (this.PayloadStatusDescription != null &&
                    this.PayloadStatusDescription.Equals(input.PayloadStatusDescription))
                ) && 
                (
                    this.ErrorMessage == input.ErrorMessage ||
                    (this.ErrorMessage != null &&
                    this.ErrorMessage.Equals(input.ErrorMessage))
                ) && 
                (
                    this.LatestErrorMessage == input.LatestErrorMessage ||
                    (this.LatestErrorMessage != null &&
                    this.LatestErrorMessage.Equals(input.LatestErrorMessage))
                ) && 
                (
                    this.HandlingStatus == input.HandlingStatus ||
                    this.HandlingStatus.Equals(input.HandlingStatus)
                ) && 
                (
                    this.HandlingStatusReason == input.HandlingStatusReason ||
                    (this.HandlingStatusReason != null &&
                    this.HandlingStatusReason.Equals(input.HandlingStatusReason))
                ) && 
                (
                    this.SubmissionError == input.SubmissionError ||
                    (this.SubmissionError != null &&
                    this.SubmissionError.Equals(input.SubmissionError))
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
                if (this.PayloadHash != null)
                {
                    hashCode = (hashCode * 59) + this.PayloadHash.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.Status.GetHashCode();
                hashCode = (hashCode * 59) + this.PayloadStatus.GetHashCode();
                if (this.PayloadStatusDescription != null)
                {
                    hashCode = (hashCode * 59) + this.PayloadStatusDescription.GetHashCode();
                }
                if (this.ErrorMessage != null)
                {
                    hashCode = (hashCode * 59) + this.ErrorMessage.GetHashCode();
                }
                if (this.LatestErrorMessage != null)
                {
                    hashCode = (hashCode * 59) + this.LatestErrorMessage.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.HandlingStatus.GetHashCode();
                if (this.HandlingStatusReason != null)
                {
                    hashCode = (hashCode * 59) + this.HandlingStatusReason.GetHashCode();
                }
                if (this.SubmissionError != null)
                {
                    hashCode = (hashCode * 59) + this.SubmissionError.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
