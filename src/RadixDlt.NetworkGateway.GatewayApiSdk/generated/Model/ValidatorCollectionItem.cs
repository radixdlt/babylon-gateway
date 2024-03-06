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
 * The version of the OpenAPI document: v1.4.2
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
    /// ValidatorCollectionItem
    /// </summary>
    [DataContract(Name = "ValidatorCollectionItem")]
    public partial class ValidatorCollectionItem : IEquatable<ValidatorCollectionItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorCollectionItem" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected ValidatorCollectionItem() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorCollectionItem" /> class.
        /// </summary>
        /// <param name="address">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="stakeVault">stakeVault (required).</param>
        /// <param name="pendingXrdWithdrawVault">pendingXrdWithdrawVault (required).</param>
        /// <param name="lockedOwnerStakeUnitVault">lockedOwnerStakeUnitVault (required).</param>
        /// <param name="pendingOwnerStakeUnitUnlockVault">pendingOwnerStakeUnitUnlockVault (required).</param>
        /// <param name="state">Validator inner state representation. This type is defined in the Core API as &#x60;ValidatorFieldStateValue&#x60;. See the Core API documentation for more details.  (required).</param>
        /// <param name="activeInEpoch">activeInEpoch.</param>
        /// <param name="metadata">metadata (required).</param>
        /// <param name="effectiveFeeFactor">effectiveFeeFactor (required).</param>
        public ValidatorCollectionItem(string address = default(string), ValidatorVaultItem stakeVault = default(ValidatorVaultItem), ValidatorVaultItem pendingXrdWithdrawVault = default(ValidatorVaultItem), ValidatorVaultItem lockedOwnerStakeUnitVault = default(ValidatorVaultItem), ValidatorVaultItem pendingOwnerStakeUnitUnlockVault = default(ValidatorVaultItem), Object state = default(Object), ValidatorCollectionItemActiveInEpoch activeInEpoch = default(ValidatorCollectionItemActiveInEpoch), EntityMetadataCollection metadata = default(EntityMetadataCollection), ValidatorCollectionItemEffectiveFeeFactor effectiveFeeFactor = default(ValidatorCollectionItemEffectiveFeeFactor))
        {
            // to ensure "address" is required (not null)
            if (address == null)
            {
                throw new ArgumentNullException("address is a required property for ValidatorCollectionItem and cannot be null");
            }
            this.Address = address;
            // to ensure "stakeVault" is required (not null)
            if (stakeVault == null)
            {
                throw new ArgumentNullException("stakeVault is a required property for ValidatorCollectionItem and cannot be null");
            }
            this.StakeVault = stakeVault;
            // to ensure "pendingXrdWithdrawVault" is required (not null)
            if (pendingXrdWithdrawVault == null)
            {
                throw new ArgumentNullException("pendingXrdWithdrawVault is a required property for ValidatorCollectionItem and cannot be null");
            }
            this.PendingXrdWithdrawVault = pendingXrdWithdrawVault;
            // to ensure "lockedOwnerStakeUnitVault" is required (not null)
            if (lockedOwnerStakeUnitVault == null)
            {
                throw new ArgumentNullException("lockedOwnerStakeUnitVault is a required property for ValidatorCollectionItem and cannot be null");
            }
            this.LockedOwnerStakeUnitVault = lockedOwnerStakeUnitVault;
            // to ensure "pendingOwnerStakeUnitUnlockVault" is required (not null)
            if (pendingOwnerStakeUnitUnlockVault == null)
            {
                throw new ArgumentNullException("pendingOwnerStakeUnitUnlockVault is a required property for ValidatorCollectionItem and cannot be null");
            }
            this.PendingOwnerStakeUnitUnlockVault = pendingOwnerStakeUnitUnlockVault;
            // to ensure "state" is required (not null)
            if (state == null)
            {
                throw new ArgumentNullException("state is a required property for ValidatorCollectionItem and cannot be null");
            }
            this.State = state;
            // to ensure "metadata" is required (not null)
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata is a required property for ValidatorCollectionItem and cannot be null");
            }
            this.Metadata = metadata;
            // to ensure "effectiveFeeFactor" is required (not null)
            if (effectiveFeeFactor == null)
            {
                throw new ArgumentNullException("effectiveFeeFactor is a required property for ValidatorCollectionItem and cannot be null");
            }
            this.EffectiveFeeFactor = effectiveFeeFactor;
            this.ActiveInEpoch = activeInEpoch;
        }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "address", IsRequired = true, EmitDefaultValue = true)]
        public string Address { get; set; }

        /// <summary>
        /// Gets or Sets StakeVault
        /// </summary>
        [DataMember(Name = "stake_vault", IsRequired = true, EmitDefaultValue = true)]
        public ValidatorVaultItem StakeVault { get; set; }

        /// <summary>
        /// Gets or Sets PendingXrdWithdrawVault
        /// </summary>
        [DataMember(Name = "pending_xrd_withdraw_vault", IsRequired = true, EmitDefaultValue = true)]
        public ValidatorVaultItem PendingXrdWithdrawVault { get; set; }

        /// <summary>
        /// Gets or Sets LockedOwnerStakeUnitVault
        /// </summary>
        [DataMember(Name = "locked_owner_stake_unit_vault", IsRequired = true, EmitDefaultValue = true)]
        public ValidatorVaultItem LockedOwnerStakeUnitVault { get; set; }

        /// <summary>
        /// Gets or Sets PendingOwnerStakeUnitUnlockVault
        /// </summary>
        [DataMember(Name = "pending_owner_stake_unit_unlock_vault", IsRequired = true, EmitDefaultValue = true)]
        public ValidatorVaultItem PendingOwnerStakeUnitUnlockVault { get; set; }

        /// <summary>
        /// Validator inner state representation. This type is defined in the Core API as &#x60;ValidatorFieldStateValue&#x60;. See the Core API documentation for more details. 
        /// </summary>
        /// <value>Validator inner state representation. This type is defined in the Core API as &#x60;ValidatorFieldStateValue&#x60;. See the Core API documentation for more details. </value>
        [DataMember(Name = "state", IsRequired = true, EmitDefaultValue = true)]
        public Object State { get; set; }

        /// <summary>
        /// Gets or Sets ActiveInEpoch
        /// </summary>
        [DataMember(Name = "active_in_epoch", EmitDefaultValue = true)]
        public ValidatorCollectionItemActiveInEpoch ActiveInEpoch { get; set; }

        /// <summary>
        /// Gets or Sets Metadata
        /// </summary>
        [DataMember(Name = "metadata", IsRequired = true, EmitDefaultValue = true)]
        public EntityMetadataCollection Metadata { get; set; }

        /// <summary>
        /// Gets or Sets EffectiveFeeFactor
        /// </summary>
        [DataMember(Name = "effective_fee_factor", IsRequired = true, EmitDefaultValue = true)]
        public ValidatorCollectionItemEffectiveFeeFactor EffectiveFeeFactor { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ValidatorCollectionItem {\n");
            sb.Append("  Address: ").Append(Address).Append("\n");
            sb.Append("  StakeVault: ").Append(StakeVault).Append("\n");
            sb.Append("  PendingXrdWithdrawVault: ").Append(PendingXrdWithdrawVault).Append("\n");
            sb.Append("  LockedOwnerStakeUnitVault: ").Append(LockedOwnerStakeUnitVault).Append("\n");
            sb.Append("  PendingOwnerStakeUnitUnlockVault: ").Append(PendingOwnerStakeUnitUnlockVault).Append("\n");
            sb.Append("  State: ").Append(State).Append("\n");
            sb.Append("  ActiveInEpoch: ").Append(ActiveInEpoch).Append("\n");
            sb.Append("  Metadata: ").Append(Metadata).Append("\n");
            sb.Append("  EffectiveFeeFactor: ").Append(EffectiveFeeFactor).Append("\n");
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
            return this.Equals(input as ValidatorCollectionItem);
        }

        /// <summary>
        /// Returns true if ValidatorCollectionItem instances are equal
        /// </summary>
        /// <param name="input">Instance of ValidatorCollectionItem to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ValidatorCollectionItem input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Address == input.Address ||
                    (this.Address != null &&
                    this.Address.Equals(input.Address))
                ) && 
                (
                    this.StakeVault == input.StakeVault ||
                    (this.StakeVault != null &&
                    this.StakeVault.Equals(input.StakeVault))
                ) && 
                (
                    this.PendingXrdWithdrawVault == input.PendingXrdWithdrawVault ||
                    (this.PendingXrdWithdrawVault != null &&
                    this.PendingXrdWithdrawVault.Equals(input.PendingXrdWithdrawVault))
                ) && 
                (
                    this.LockedOwnerStakeUnitVault == input.LockedOwnerStakeUnitVault ||
                    (this.LockedOwnerStakeUnitVault != null &&
                    this.LockedOwnerStakeUnitVault.Equals(input.LockedOwnerStakeUnitVault))
                ) && 
                (
                    this.PendingOwnerStakeUnitUnlockVault == input.PendingOwnerStakeUnitUnlockVault ||
                    (this.PendingOwnerStakeUnitUnlockVault != null &&
                    this.PendingOwnerStakeUnitUnlockVault.Equals(input.PendingOwnerStakeUnitUnlockVault))
                ) && 
                (
                    this.State == input.State ||
                    (this.State != null &&
                    this.State.Equals(input.State))
                ) && 
                (
                    this.ActiveInEpoch == input.ActiveInEpoch ||
                    (this.ActiveInEpoch != null &&
                    this.ActiveInEpoch.Equals(input.ActiveInEpoch))
                ) && 
                (
                    this.Metadata == input.Metadata ||
                    (this.Metadata != null &&
                    this.Metadata.Equals(input.Metadata))
                ) && 
                (
                    this.EffectiveFeeFactor == input.EffectiveFeeFactor ||
                    (this.EffectiveFeeFactor != null &&
                    this.EffectiveFeeFactor.Equals(input.EffectiveFeeFactor))
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
                if (this.Address != null)
                {
                    hashCode = (hashCode * 59) + this.Address.GetHashCode();
                }
                if (this.StakeVault != null)
                {
                    hashCode = (hashCode * 59) + this.StakeVault.GetHashCode();
                }
                if (this.PendingXrdWithdrawVault != null)
                {
                    hashCode = (hashCode * 59) + this.PendingXrdWithdrawVault.GetHashCode();
                }
                if (this.LockedOwnerStakeUnitVault != null)
                {
                    hashCode = (hashCode * 59) + this.LockedOwnerStakeUnitVault.GetHashCode();
                }
                if (this.PendingOwnerStakeUnitUnlockVault != null)
                {
                    hashCode = (hashCode * 59) + this.PendingOwnerStakeUnitUnlockVault.GetHashCode();
                }
                if (this.State != null)
                {
                    hashCode = (hashCode * 59) + this.State.GetHashCode();
                }
                if (this.ActiveInEpoch != null)
                {
                    hashCode = (hashCode * 59) + this.ActiveInEpoch.GetHashCode();
                }
                if (this.Metadata != null)
                {
                    hashCode = (hashCode * 59) + this.Metadata.GetHashCode();
                }
                if (this.EffectiveFeeFactor != null)
                {
                    hashCode = (hashCode * 59) + this.EffectiveFeeFactor.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
