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
 * The version of the OpenAPI document: v1.6.1
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
    /// CaAccessControllerFieldStateValue
    /// </summary>
    [DataContract(Name = "CaAccessControllerFieldStateValue")]
    public partial class CaAccessControllerFieldStateValue : IEquatable<CaAccessControllerFieldStateValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaAccessControllerFieldStateValue" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected CaAccessControllerFieldStateValue() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="CaAccessControllerFieldStateValue" /> class.
        /// </summary>
        /// <param name="controlledVault">controlledVault (required).</param>
        /// <param name="xrdFeeVault">xrdFeeVault.</param>
        /// <param name="timedRecoveryDelayMinutes">An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, specifying the amount of time (in minutes) that it takes for timed recovery to be done. When not present, then timed recovery can not be performed through this access controller. .</param>
        /// <param name="recoveryBadgeResourceAddress">The Bech32m-encoded human readable version of the resource address (required).</param>
        /// <param name="isPrimaryRoleLocked">Whether the primary role is currently locked. (required).</param>
        /// <param name="primaryRoleRecoveryAttempt">primaryRoleRecoveryAttempt.</param>
        /// <param name="hasPrimaryRoleBadgeWithdrawAttempt">Whether the primary role badge withdraw is currently being attempted. (required).</param>
        /// <param name="recoveryRoleRecoveryAttempt">recoveryRoleRecoveryAttempt.</param>
        /// <param name="hasRecoveryRoleBadgeWithdrawAttempt">Whether the recovery role badge withdraw is currently being attempted. (required).</param>
        public CaAccessControllerFieldStateValue(CaEntityReference controlledVault = default(CaEntityReference), CaEntityReference xrdFeeVault = default(CaEntityReference), long timedRecoveryDelayMinutes = default(long), string recoveryBadgeResourceAddress = default(string), bool isPrimaryRoleLocked = default(bool), CaPrimaryRoleRecoveryAttempt primaryRoleRecoveryAttempt = default(CaPrimaryRoleRecoveryAttempt), bool hasPrimaryRoleBadgeWithdrawAttempt = default(bool), CaRecoveryRoleRecoveryAttempt recoveryRoleRecoveryAttempt = default(CaRecoveryRoleRecoveryAttempt), bool hasRecoveryRoleBadgeWithdrawAttempt = default(bool))
        {
            // to ensure "controlledVault" is required (not null)
            if (controlledVault == null)
            {
                throw new ArgumentNullException("controlledVault is a required property for CaAccessControllerFieldStateValue and cannot be null");
            }
            this.ControlledVault = controlledVault;
            // to ensure "recoveryBadgeResourceAddress" is required (not null)
            if (recoveryBadgeResourceAddress == null)
            {
                throw new ArgumentNullException("recoveryBadgeResourceAddress is a required property for CaAccessControllerFieldStateValue and cannot be null");
            }
            this.RecoveryBadgeResourceAddress = recoveryBadgeResourceAddress;
            this.IsPrimaryRoleLocked = isPrimaryRoleLocked;
            this.HasPrimaryRoleBadgeWithdrawAttempt = hasPrimaryRoleBadgeWithdrawAttempt;
            this.HasRecoveryRoleBadgeWithdrawAttempt = hasRecoveryRoleBadgeWithdrawAttempt;
            this.XrdFeeVault = xrdFeeVault;
            this.TimedRecoveryDelayMinutes = timedRecoveryDelayMinutes;
            this.PrimaryRoleRecoveryAttempt = primaryRoleRecoveryAttempt;
            this.RecoveryRoleRecoveryAttempt = recoveryRoleRecoveryAttempt;
        }

        /// <summary>
        /// Gets or Sets ControlledVault
        /// </summary>
        [DataMember(Name = "controlled_vault", IsRequired = true, EmitDefaultValue = true)]
        public CaEntityReference ControlledVault { get; set; }

        /// <summary>
        /// Gets or Sets XrdFeeVault
        /// </summary>
        [DataMember(Name = "xrd_fee_vault", EmitDefaultValue = true)]
        public CaEntityReference XrdFeeVault { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, specifying the amount of time (in minutes) that it takes for timed recovery to be done. When not present, then timed recovery can not be performed through this access controller. 
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, specifying the amount of time (in minutes) that it takes for timed recovery to be done. When not present, then timed recovery can not be performed through this access controller. </value>
        [DataMember(Name = "timed_recovery_delay_minutes", EmitDefaultValue = true)]
        public long TimedRecoveryDelayMinutes { get; set; }

        /// <summary>
        /// The Bech32m-encoded human readable version of the resource address
        /// </summary>
        /// <value>The Bech32m-encoded human readable version of the resource address</value>
        [DataMember(Name = "recovery_badge_resource_address", IsRequired = true, EmitDefaultValue = true)]
        public string RecoveryBadgeResourceAddress { get; set; }

        /// <summary>
        /// Whether the primary role is currently locked.
        /// </summary>
        /// <value>Whether the primary role is currently locked.</value>
        [DataMember(Name = "is_primary_role_locked", IsRequired = true, EmitDefaultValue = true)]
        public bool IsPrimaryRoleLocked { get; set; }

        /// <summary>
        /// Gets or Sets PrimaryRoleRecoveryAttempt
        /// </summary>
        [DataMember(Name = "primary_role_recovery_attempt", EmitDefaultValue = true)]
        public CaPrimaryRoleRecoveryAttempt PrimaryRoleRecoveryAttempt { get; set; }

        /// <summary>
        /// Whether the primary role badge withdraw is currently being attempted.
        /// </summary>
        /// <value>Whether the primary role badge withdraw is currently being attempted.</value>
        [DataMember(Name = "has_primary_role_badge_withdraw_attempt", IsRequired = true, EmitDefaultValue = true)]
        public bool HasPrimaryRoleBadgeWithdrawAttempt { get; set; }

        /// <summary>
        /// Gets or Sets RecoveryRoleRecoveryAttempt
        /// </summary>
        [DataMember(Name = "recovery_role_recovery_attempt", EmitDefaultValue = true)]
        public CaRecoveryRoleRecoveryAttempt RecoveryRoleRecoveryAttempt { get; set; }

        /// <summary>
        /// Whether the recovery role badge withdraw is currently being attempted.
        /// </summary>
        /// <value>Whether the recovery role badge withdraw is currently being attempted.</value>
        [DataMember(Name = "has_recovery_role_badge_withdraw_attempt", IsRequired = true, EmitDefaultValue = true)]
        public bool HasRecoveryRoleBadgeWithdrawAttempt { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class CaAccessControllerFieldStateValue {\n");
            sb.Append("  ControlledVault: ").Append(ControlledVault).Append("\n");
            sb.Append("  XrdFeeVault: ").Append(XrdFeeVault).Append("\n");
            sb.Append("  TimedRecoveryDelayMinutes: ").Append(TimedRecoveryDelayMinutes).Append("\n");
            sb.Append("  RecoveryBadgeResourceAddress: ").Append(RecoveryBadgeResourceAddress).Append("\n");
            sb.Append("  IsPrimaryRoleLocked: ").Append(IsPrimaryRoleLocked).Append("\n");
            sb.Append("  PrimaryRoleRecoveryAttempt: ").Append(PrimaryRoleRecoveryAttempt).Append("\n");
            sb.Append("  HasPrimaryRoleBadgeWithdrawAttempt: ").Append(HasPrimaryRoleBadgeWithdrawAttempt).Append("\n");
            sb.Append("  RecoveryRoleRecoveryAttempt: ").Append(RecoveryRoleRecoveryAttempt).Append("\n");
            sb.Append("  HasRecoveryRoleBadgeWithdrawAttempt: ").Append(HasRecoveryRoleBadgeWithdrawAttempt).Append("\n");
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
            return this.Equals(input as CaAccessControllerFieldStateValue);
        }

        /// <summary>
        /// Returns true if CaAccessControllerFieldStateValue instances are equal
        /// </summary>
        /// <param name="input">Instance of CaAccessControllerFieldStateValue to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(CaAccessControllerFieldStateValue input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.ControlledVault == input.ControlledVault ||
                    (this.ControlledVault != null &&
                    this.ControlledVault.Equals(input.ControlledVault))
                ) && 
                (
                    this.XrdFeeVault == input.XrdFeeVault ||
                    (this.XrdFeeVault != null &&
                    this.XrdFeeVault.Equals(input.XrdFeeVault))
                ) && 
                (
                    this.TimedRecoveryDelayMinutes == input.TimedRecoveryDelayMinutes ||
                    this.TimedRecoveryDelayMinutes.Equals(input.TimedRecoveryDelayMinutes)
                ) && 
                (
                    this.RecoveryBadgeResourceAddress == input.RecoveryBadgeResourceAddress ||
                    (this.RecoveryBadgeResourceAddress != null &&
                    this.RecoveryBadgeResourceAddress.Equals(input.RecoveryBadgeResourceAddress))
                ) && 
                (
                    this.IsPrimaryRoleLocked == input.IsPrimaryRoleLocked ||
                    this.IsPrimaryRoleLocked.Equals(input.IsPrimaryRoleLocked)
                ) && 
                (
                    this.PrimaryRoleRecoveryAttempt == input.PrimaryRoleRecoveryAttempt ||
                    (this.PrimaryRoleRecoveryAttempt != null &&
                    this.PrimaryRoleRecoveryAttempt.Equals(input.PrimaryRoleRecoveryAttempt))
                ) && 
                (
                    this.HasPrimaryRoleBadgeWithdrawAttempt == input.HasPrimaryRoleBadgeWithdrawAttempt ||
                    this.HasPrimaryRoleBadgeWithdrawAttempt.Equals(input.HasPrimaryRoleBadgeWithdrawAttempt)
                ) && 
                (
                    this.RecoveryRoleRecoveryAttempt == input.RecoveryRoleRecoveryAttempt ||
                    (this.RecoveryRoleRecoveryAttempt != null &&
                    this.RecoveryRoleRecoveryAttempt.Equals(input.RecoveryRoleRecoveryAttempt))
                ) && 
                (
                    this.HasRecoveryRoleBadgeWithdrawAttempt == input.HasRecoveryRoleBadgeWithdrawAttempt ||
                    this.HasRecoveryRoleBadgeWithdrawAttempt.Equals(input.HasRecoveryRoleBadgeWithdrawAttempt)
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
                if (this.ControlledVault != null)
                {
                    hashCode = (hashCode * 59) + this.ControlledVault.GetHashCode();
                }
                if (this.XrdFeeVault != null)
                {
                    hashCode = (hashCode * 59) + this.XrdFeeVault.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.TimedRecoveryDelayMinutes.GetHashCode();
                if (this.RecoveryBadgeResourceAddress != null)
                {
                    hashCode = (hashCode * 59) + this.RecoveryBadgeResourceAddress.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.IsPrimaryRoleLocked.GetHashCode();
                if (this.PrimaryRoleRecoveryAttempt != null)
                {
                    hashCode = (hashCode * 59) + this.PrimaryRoleRecoveryAttempt.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.HasPrimaryRoleBadgeWithdrawAttempt.GetHashCode();
                if (this.RecoveryRoleRecoveryAttempt != null)
                {
                    hashCode = (hashCode * 59) + this.RecoveryRoleRecoveryAttempt.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.HasRecoveryRoleBadgeWithdrawAttempt.GetHashCode();
                return hashCode;
            }
        }

    }

}
