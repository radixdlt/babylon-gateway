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
    /// TransactionSubintentDetails
    /// </summary>
    [DataContract(Name = "TransactionSubintentDetails")]
    public partial class TransactionSubintentDetails : IEquatable<TransactionSubintentDetails>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionSubintentDetails" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected TransactionSubintentDetails() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionSubintentDetails" /> class.
        /// </summary>
        /// <param name="subintentHash">Bech32m-encoded hash. (required).</param>
        /// <param name="manifestInstructions">A text-representation of a subintent manifest. This field will be present only for user transactions and when explicitly opted-in using the &#x60;manifest_instructions&#x60; flag. .</param>
        /// <param name="message">The optional subintent message. This type is defined in the Core API as &#x60;TransactionMessage&#x60;. See the Core API documentation for more details. .</param>
        /// <param name="childSubintentHashes">The subintent hash of each child of the subintent..</param>
        public TransactionSubintentDetails(string subintentHash = default(string), string manifestInstructions = default(string), Object message = default(Object), List<string> childSubintentHashes = default(List<string>))
        {
            // to ensure "subintentHash" is required (not null)
            if (subintentHash == null)
            {
                throw new ArgumentNullException("subintentHash is a required property for TransactionSubintentDetails and cannot be null");
            }
            this.SubintentHash = subintentHash;
            this.ManifestInstructions = manifestInstructions;
            this.Message = message;
            this.ChildSubintentHashes = childSubintentHashes;
        }

        /// <summary>
        /// Bech32m-encoded hash.
        /// </summary>
        /// <value>Bech32m-encoded hash.</value>
        [DataMember(Name = "subintent_hash", IsRequired = true, EmitDefaultValue = true)]
        public string SubintentHash { get; set; }

        /// <summary>
        /// A text-representation of a subintent manifest. This field will be present only for user transactions and when explicitly opted-in using the &#x60;manifest_instructions&#x60; flag. 
        /// </summary>
        /// <value>A text-representation of a subintent manifest. This field will be present only for user transactions and when explicitly opted-in using the &#x60;manifest_instructions&#x60; flag. </value>
        [DataMember(Name = "manifest_instructions", EmitDefaultValue = true)]
        public string ManifestInstructions { get; set; }

        /// <summary>
        /// The optional subintent message. This type is defined in the Core API as &#x60;TransactionMessage&#x60;. See the Core API documentation for more details. 
        /// </summary>
        /// <value>The optional subintent message. This type is defined in the Core API as &#x60;TransactionMessage&#x60;. See the Core API documentation for more details. </value>
        [DataMember(Name = "message", EmitDefaultValue = true)]
        public Object Message { get; set; }

        /// <summary>
        /// The subintent hash of each child of the subintent.
        /// </summary>
        /// <value>The subintent hash of each child of the subintent.</value>
        [DataMember(Name = "child_subintent_hashes", EmitDefaultValue = true)]
        public List<string> ChildSubintentHashes { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class TransactionSubintentDetails {\n");
            sb.Append("  SubintentHash: ").Append(SubintentHash).Append("\n");
            sb.Append("  ManifestInstructions: ").Append(ManifestInstructions).Append("\n");
            sb.Append("  Message: ").Append(Message).Append("\n");
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
            return this.Equals(input as TransactionSubintentDetails);
        }

        /// <summary>
        /// Returns true if TransactionSubintentDetails instances are equal
        /// </summary>
        /// <param name="input">Instance of TransactionSubintentDetails to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TransactionSubintentDetails input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.SubintentHash == input.SubintentHash ||
                    (this.SubintentHash != null &&
                    this.SubintentHash.Equals(input.SubintentHash))
                ) && 
                (
                    this.ManifestInstructions == input.ManifestInstructions ||
                    (this.ManifestInstructions != null &&
                    this.ManifestInstructions.Equals(input.ManifestInstructions))
                ) && 
                (
                    this.Message == input.Message ||
                    (this.Message != null &&
                    this.Message.Equals(input.Message))
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
                if (this.SubintentHash != null)
                {
                    hashCode = (hashCode * 59) + this.SubintentHash.GetHashCode();
                }
                if (this.ManifestInstructions != null)
                {
                    hashCode = (hashCode * 59) + this.ManifestInstructions.GetHashCode();
                }
                if (this.Message != null)
                {
                    hashCode = (hashCode * 59) + this.Message.GetHashCode();
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
