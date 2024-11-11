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
 * The version of the OpenAPI document: v1.9.0
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
    /// TransactionPreviewV2Response
    /// </summary>
    [DataContract(Name = "TransactionPreviewV2Response")]
    public partial class TransactionPreviewV2Response : IEquatable<TransactionPreviewV2Response>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionPreviewV2Response" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected TransactionPreviewV2Response() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionPreviewV2Response" /> class.
        /// </summary>
        /// <param name="atLedgerStateVersion">A summarized state of the ledger on top of which the preview was performed. (required).</param>
        /// <param name="receipt">This is provided unless the &#x60;core_api_receipt&#x60; flag is set to false in the &#x60;opt_ins &#x60; property of the request.  This type is defined in the Core API as &#x60;TransactionReceipt&#x60;. See the Core API documentation for more details. .</param>
        /// <param name="radixEngineToolkitReceipt">An optional field which is only provided if the &#x60;radix_engine_toolkit_receipt&#x60; flag is set to true in the &#x60;opt_ins&#x60; property of the request.  This receipt is primarily intended for use with the toolkit and may contain information that is already available in the receipt provided in the &#x60;receipt&#x60; field of this response.  A typical client of this API is not expected to use this receipt. The primary clients this receipt is intended for is the Radix wallet or any client that needs to perform execution summaries on their transactions. .</param>
        /// <param name="logs">An optional field which is only provided if the &#x60;logs&#x60; flag is set to true in the &#x60;opt_ins&#x60; property of the request.  If present, it gives the emitted logs from the transaction execution. .</param>
        public TransactionPreviewV2Response(long atLedgerStateVersion = default(long), Object receipt = default(Object), Object radixEngineToolkitReceipt = default(Object), List<TransactionPreviewResponseLogsInner> logs = default(List<TransactionPreviewResponseLogsInner>))
        {
            this.AtLedgerStateVersion = atLedgerStateVersion;
            this.Receipt = receipt;
            this.RadixEngineToolkitReceipt = radixEngineToolkitReceipt;
            this.Logs = logs;
        }

        /// <summary>
        /// A summarized state of the ledger on top of which the preview was performed.
        /// </summary>
        /// <value>A summarized state of the ledger on top of which the preview was performed.</value>
        [DataMember(Name = "at_ledger_state_version", IsRequired = true, EmitDefaultValue = true)]
        public long AtLedgerStateVersion { get; set; }

        /// <summary>
        /// This is provided unless the &#x60;core_api_receipt&#x60; flag is set to false in the &#x60;opt_ins &#x60; property of the request.  This type is defined in the Core API as &#x60;TransactionReceipt&#x60;. See the Core API documentation for more details. 
        /// </summary>
        /// <value>This is provided unless the &#x60;core_api_receipt&#x60; flag is set to false in the &#x60;opt_ins &#x60; property of the request.  This type is defined in the Core API as &#x60;TransactionReceipt&#x60;. See the Core API documentation for more details. </value>
        [DataMember(Name = "receipt", EmitDefaultValue = true)]
        public Object Receipt { get; set; }

        /// <summary>
        /// An optional field which is only provided if the &#x60;radix_engine_toolkit_receipt&#x60; flag is set to true in the &#x60;opt_ins&#x60; property of the request.  This receipt is primarily intended for use with the toolkit and may contain information that is already available in the receipt provided in the &#x60;receipt&#x60; field of this response.  A typical client of this API is not expected to use this receipt. The primary clients this receipt is intended for is the Radix wallet or any client that needs to perform execution summaries on their transactions. 
        /// </summary>
        /// <value>An optional field which is only provided if the &#x60;radix_engine_toolkit_receipt&#x60; flag is set to true in the &#x60;opt_ins&#x60; property of the request.  This receipt is primarily intended for use with the toolkit and may contain information that is already available in the receipt provided in the &#x60;receipt&#x60; field of this response.  A typical client of this API is not expected to use this receipt. The primary clients this receipt is intended for is the Radix wallet or any client that needs to perform execution summaries on their transactions. </value>
        [DataMember(Name = "radix_engine_toolkit_receipt", EmitDefaultValue = true)]
        public Object RadixEngineToolkitReceipt { get; set; }

        /// <summary>
        /// An optional field which is only provided if the &#x60;logs&#x60; flag is set to true in the &#x60;opt_ins&#x60; property of the request.  If present, it gives the emitted logs from the transaction execution. 
        /// </summary>
        /// <value>An optional field which is only provided if the &#x60;logs&#x60; flag is set to true in the &#x60;opt_ins&#x60; property of the request.  If present, it gives the emitted logs from the transaction execution. </value>
        [DataMember(Name = "logs", EmitDefaultValue = true)]
        public List<TransactionPreviewResponseLogsInner> Logs { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class TransactionPreviewV2Response {\n");
            sb.Append("  AtLedgerStateVersion: ").Append(AtLedgerStateVersion).Append("\n");
            sb.Append("  Receipt: ").Append(Receipt).Append("\n");
            sb.Append("  RadixEngineToolkitReceipt: ").Append(RadixEngineToolkitReceipt).Append("\n");
            sb.Append("  Logs: ").Append(Logs).Append("\n");
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
            return this.Equals(input as TransactionPreviewV2Response);
        }

        /// <summary>
        /// Returns true if TransactionPreviewV2Response instances are equal
        /// </summary>
        /// <param name="input">Instance of TransactionPreviewV2Response to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TransactionPreviewV2Response input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.AtLedgerStateVersion == input.AtLedgerStateVersion ||
                    this.AtLedgerStateVersion.Equals(input.AtLedgerStateVersion)
                ) && 
                (
                    this.Receipt == input.Receipt ||
                    (this.Receipt != null &&
                    this.Receipt.Equals(input.Receipt))
                ) && 
                (
                    this.RadixEngineToolkitReceipt == input.RadixEngineToolkitReceipt ||
                    (this.RadixEngineToolkitReceipt != null &&
                    this.RadixEngineToolkitReceipt.Equals(input.RadixEngineToolkitReceipt))
                ) && 
                (
                    this.Logs == input.Logs ||
                    this.Logs != null &&
                    input.Logs != null &&
                    this.Logs.SequenceEqual(input.Logs)
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
                hashCode = (hashCode * 59) + this.AtLedgerStateVersion.GetHashCode();
                if (this.Receipt != null)
                {
                    hashCode = (hashCode * 59) + this.Receipt.GetHashCode();
                }
                if (this.RadixEngineToolkitReceipt != null)
                {
                    hashCode = (hashCode * 59) + this.RadixEngineToolkitReceipt.GetHashCode();
                }
                if (this.Logs != null)
                {
                    hashCode = (hashCode * 59) + this.Logs.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
