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
 * Babylon Core API - RCnet v3
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  It is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  This version of the Core API belongs to the second release candidate of the Radix Babylon network (\"RCnet v3\").  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` are guaranteed to be forward compatible to Babylon mainnet launch (and beyond). We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code. 
 *
 * The version of the OpenAPI document: 0.5.0
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
    /// AccessControllerFieldStateValue
    /// </summary>
    [DataContract(Name = "AccessControllerFieldStateValue")]
    public partial class AccessControllerFieldStateValue : IEquatable<AccessControllerFieldStateValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccessControllerFieldStateValue" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected AccessControllerFieldStateValue() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="AccessControllerFieldStateValue" /> class.
        /// </summary>
        /// <param name="controlledVault">controlledVault (required).</param>
        /// <param name="timedRecoveryDelayMinutes">An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, specifying the amount of time (in minutes) that it takes for timed recovery to be done. When not present, then timed recovery can not be performed through this access controller. .</param>
        /// <param name="recoveryBadgeResourceAddress">The Bech32m-encoded human readable version of the resource address (required).</param>
        /// <param name="isPrimaryRoleLocked">Whether the primary role is currently locked. (required).</param>
        /// <param name="primaryRoleRecoveryAttempt">primaryRoleRecoveryAttempt.</param>
        /// <param name="hasPrimaryRoleBadgeWithdrawAttempt">Whether the primary role badge withdraw is currently being attempted. (required).</param>
        /// <param name="recoveryRoleRecoveryAttempt">recoveryRoleRecoveryAttempt.</param>
        /// <param name="hasRecoveryRoleBadgeWithdrawAttempt">Whether the recovery role badge withdraw is currently being attempted. (required).</param>
        public AccessControllerFieldStateValue(EntityReference controlledVault = default(EntityReference), long timedRecoveryDelayMinutes = default(long), string recoveryBadgeResourceAddress = default(string), bool isPrimaryRoleLocked = default(bool), PrimaryRoleRecoveryAttempt primaryRoleRecoveryAttempt = default(PrimaryRoleRecoveryAttempt), bool hasPrimaryRoleBadgeWithdrawAttempt = default(bool), RecoveryRoleRecoveryAttempt recoveryRoleRecoveryAttempt = default(RecoveryRoleRecoveryAttempt), bool hasRecoveryRoleBadgeWithdrawAttempt = default(bool))
        {
            // to ensure "controlledVault" is required (not null)
            if (controlledVault == null)
            {
                throw new ArgumentNullException("controlledVault is a required property for AccessControllerFieldStateValue and cannot be null");
            }
            this.ControlledVault = controlledVault;
            // to ensure "recoveryBadgeResourceAddress" is required (not null)
            if (recoveryBadgeResourceAddress == null)
            {
                throw new ArgumentNullException("recoveryBadgeResourceAddress is a required property for AccessControllerFieldStateValue and cannot be null");
            }
            this.RecoveryBadgeResourceAddress = recoveryBadgeResourceAddress;
            this.IsPrimaryRoleLocked = isPrimaryRoleLocked;
            this.HasPrimaryRoleBadgeWithdrawAttempt = hasPrimaryRoleBadgeWithdrawAttempt;
            this.HasRecoveryRoleBadgeWithdrawAttempt = hasRecoveryRoleBadgeWithdrawAttempt;
            this.TimedRecoveryDelayMinutes = timedRecoveryDelayMinutes;
            this.PrimaryRoleRecoveryAttempt = primaryRoleRecoveryAttempt;
            this.RecoveryRoleRecoveryAttempt = recoveryRoleRecoveryAttempt;
        }

        /// <summary>
        /// Gets or Sets ControlledVault
        /// </summary>
        [DataMember(Name = "controlled_vault", IsRequired = true, EmitDefaultValue = true)]
        public EntityReference ControlledVault { get; set; }

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
        public PrimaryRoleRecoveryAttempt PrimaryRoleRecoveryAttempt { get; set; }

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
        public RecoveryRoleRecoveryAttempt RecoveryRoleRecoveryAttempt { get; set; }

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
            sb.Append("class AccessControllerFieldStateValue {\n");
            sb.Append("  ControlledVault: ").Append(ControlledVault).Append("\n");
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
            return this.Equals(input as AccessControllerFieldStateValue);
        }

        /// <summary>
        /// Returns true if AccessControllerFieldStateValue instances are equal
        /// </summary>
        /// <param name="input">Instance of AccessControllerFieldStateValue to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(AccessControllerFieldStateValue input)
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
