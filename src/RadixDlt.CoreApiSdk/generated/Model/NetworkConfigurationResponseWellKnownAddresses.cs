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
    /// Key addresses for this network.
    /// </summary>
    [DataContract(Name = "NetworkConfigurationResponse_well_known_addresses")]
    public partial class NetworkConfigurationResponseWellKnownAddresses : IEquatable<NetworkConfigurationResponseWellKnownAddresses>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkConfigurationResponseWellKnownAddresses" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected NetworkConfigurationResponseWellKnownAddresses() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkConfigurationResponseWellKnownAddresses" /> class.
        /// </summary>
        /// <param name="faucet">faucet (required).</param>
        /// <param name="epochManager">epochManager (required).</param>
        /// <param name="clock">clock (required).</param>
        /// <param name="ecdsaSecp256k1">ecdsaSecp256k1 (required).</param>
        /// <param name="eddsaEd25519">eddsaEd25519 (required).</param>
        /// <param name="xrd">xrd (required).</param>
        public NetworkConfigurationResponseWellKnownAddresses(string faucet = default(string), string epochManager = default(string), string clock = default(string), string ecdsaSecp256k1 = default(string), string eddsaEd25519 = default(string), string xrd = default(string))
        {
            // to ensure "faucet" is required (not null)
            if (faucet == null)
            {
                throw new ArgumentNullException("faucet is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.Faucet = faucet;
            // to ensure "epochManager" is required (not null)
            if (epochManager == null)
            {
                throw new ArgumentNullException("epochManager is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.EpochManager = epochManager;
            // to ensure "clock" is required (not null)
            if (clock == null)
            {
                throw new ArgumentNullException("clock is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.Clock = clock;
            // to ensure "ecdsaSecp256k1" is required (not null)
            if (ecdsaSecp256k1 == null)
            {
                throw new ArgumentNullException("ecdsaSecp256k1 is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.EcdsaSecp256k1 = ecdsaSecp256k1;
            // to ensure "eddsaEd25519" is required (not null)
            if (eddsaEd25519 == null)
            {
                throw new ArgumentNullException("eddsaEd25519 is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.EddsaEd25519 = eddsaEd25519;
            // to ensure "xrd" is required (not null)
            if (xrd == null)
            {
                throw new ArgumentNullException("xrd is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.Xrd = xrd;
        }

        /// <summary>
        /// Gets or Sets Faucet
        /// </summary>
        [DataMember(Name = "faucet", IsRequired = true, EmitDefaultValue = true)]
        public string Faucet { get; set; }

        /// <summary>
        /// Gets or Sets EpochManager
        /// </summary>
        [DataMember(Name = "epoch_manager", IsRequired = true, EmitDefaultValue = true)]
        public string EpochManager { get; set; }

        /// <summary>
        /// Gets or Sets Clock
        /// </summary>
        [DataMember(Name = "clock", IsRequired = true, EmitDefaultValue = true)]
        public string Clock { get; set; }

        /// <summary>
        /// Gets or Sets EcdsaSecp256k1
        /// </summary>
        [DataMember(Name = "ecdsa_secp256k1", IsRequired = true, EmitDefaultValue = true)]
        public string EcdsaSecp256k1 { get; set; }

        /// <summary>
        /// Gets or Sets EddsaEd25519
        /// </summary>
        [DataMember(Name = "eddsa_ed25519", IsRequired = true, EmitDefaultValue = true)]
        public string EddsaEd25519 { get; set; }

        /// <summary>
        /// Gets or Sets Xrd
        /// </summary>
        [DataMember(Name = "xrd", IsRequired = true, EmitDefaultValue = true)]
        public string Xrd { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class NetworkConfigurationResponseWellKnownAddresses {\n");
            sb.Append("  Faucet: ").Append(Faucet).Append("\n");
            sb.Append("  EpochManager: ").Append(EpochManager).Append("\n");
            sb.Append("  Clock: ").Append(Clock).Append("\n");
            sb.Append("  EcdsaSecp256k1: ").Append(EcdsaSecp256k1).Append("\n");
            sb.Append("  EddsaEd25519: ").Append(EddsaEd25519).Append("\n");
            sb.Append("  Xrd: ").Append(Xrd).Append("\n");
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
            return this.Equals(input as NetworkConfigurationResponseWellKnownAddresses);
        }

        /// <summary>
        /// Returns true if NetworkConfigurationResponseWellKnownAddresses instances are equal
        /// </summary>
        /// <param name="input">Instance of NetworkConfigurationResponseWellKnownAddresses to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(NetworkConfigurationResponseWellKnownAddresses input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Faucet == input.Faucet ||
                    (this.Faucet != null &&
                    this.Faucet.Equals(input.Faucet))
                ) && 
                (
                    this.EpochManager == input.EpochManager ||
                    (this.EpochManager != null &&
                    this.EpochManager.Equals(input.EpochManager))
                ) && 
                (
                    this.Clock == input.Clock ||
                    (this.Clock != null &&
                    this.Clock.Equals(input.Clock))
                ) && 
                (
                    this.EcdsaSecp256k1 == input.EcdsaSecp256k1 ||
                    (this.EcdsaSecp256k1 != null &&
                    this.EcdsaSecp256k1.Equals(input.EcdsaSecp256k1))
                ) && 
                (
                    this.EddsaEd25519 == input.EddsaEd25519 ||
                    (this.EddsaEd25519 != null &&
                    this.EddsaEd25519.Equals(input.EddsaEd25519))
                ) && 
                (
                    this.Xrd == input.Xrd ||
                    (this.Xrd != null &&
                    this.Xrd.Equals(input.Xrd))
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
                if (this.Faucet != null)
                {
                    hashCode = (hashCode * 59) + this.Faucet.GetHashCode();
                }
                if (this.EpochManager != null)
                {
                    hashCode = (hashCode * 59) + this.EpochManager.GetHashCode();
                }
                if (this.Clock != null)
                {
                    hashCode = (hashCode * 59) + this.Clock.GetHashCode();
                }
                if (this.EcdsaSecp256k1 != null)
                {
                    hashCode = (hashCode * 59) + this.EcdsaSecp256k1.GetHashCode();
                }
                if (this.EddsaEd25519 != null)
                {
                    hashCode = (hashCode * 59) + this.EddsaEd25519.GetHashCode();
                }
                if (this.Xrd != null)
                {
                    hashCode = (hashCode * 59) + this.Xrd.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
