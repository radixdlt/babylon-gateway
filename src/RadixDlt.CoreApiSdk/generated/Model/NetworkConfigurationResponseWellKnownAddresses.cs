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
 * Radix Core API - Babylon (Bottlenose)
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  The default configuration is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function. The node exposes a configuration flag which allows disabling certain endpoints which may be problematic, but monitoring is advised. This configuration parameter is `api.core.flags.enable_unbounded_endpoints` / `RADIXDLT_CORE_API_FLAGS_ENABLE_UNBOUNDED_ENDPOINTS`.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` have high guarantees of forward compatibility in future node versions. We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  Other endpoints may be changed with new node versions carrying protocol-updates, although any breaking changes will be flagged clearly in the corresponding release notes.  All responses may have additional fields added, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects. 
 *
 * The version of the OpenAPI document: v1.2.0
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
        /// <param name="xrd">xrd (required).</param>
        /// <param name="secp256k1SignatureVirtualBadge">secp256k1SignatureVirtualBadge (required).</param>
        /// <param name="ed25519SignatureVirtualBadge">ed25519SignatureVirtualBadge (required).</param>
        /// <param name="packageOfDirectCallerVirtualBadge">packageOfDirectCallerVirtualBadge (required).</param>
        /// <param name="globalCallerVirtualBadge">globalCallerVirtualBadge (required).</param>
        /// <param name="systemTransactionBadge">systemTransactionBadge (required).</param>
        /// <param name="packageOwnerBadge">packageOwnerBadge (required).</param>
        /// <param name="validatorOwnerBadge">validatorOwnerBadge (required).</param>
        /// <param name="accountOwnerBadge">accountOwnerBadge (required).</param>
        /// <param name="identityOwnerBadge">identityOwnerBadge (required).</param>
        /// <param name="packagePackage">packagePackage (required).</param>
        /// <param name="resourcePackage">resourcePackage (required).</param>
        /// <param name="accountPackage">accountPackage (required).</param>
        /// <param name="identityPackage">identityPackage (required).</param>
        /// <param name="consensusManagerPackage">consensusManagerPackage (required).</param>
        /// <param name="accessControllerPackage">accessControllerPackage (required).</param>
        /// <param name="transactionProcessorPackage">transactionProcessorPackage (required).</param>
        /// <param name="metadataModulePackage">metadataModulePackage (required).</param>
        /// <param name="royaltyModulePackage">royaltyModulePackage (required).</param>
        /// <param name="roleAssignmentModulePackage">roleAssignmentModulePackage (required).</param>
        /// <param name="genesisHelperPackage">genesisHelperPackage (required).</param>
        /// <param name="faucetPackage">faucetPackage (required).</param>
        /// <param name="poolPackage">poolPackage (required).</param>
        /// <param name="lockerPackage">lockerPackage.</param>
        /// <param name="consensusManager">consensusManager (required).</param>
        /// <param name="genesisHelper">genesisHelper (required).</param>
        /// <param name="faucet">faucet (required).</param>
        /// <param name="transactionTracker">transactionTracker (required).</param>
        public NetworkConfigurationResponseWellKnownAddresses(string xrd = default(string), string secp256k1SignatureVirtualBadge = default(string), string ed25519SignatureVirtualBadge = default(string), string packageOfDirectCallerVirtualBadge = default(string), string globalCallerVirtualBadge = default(string), string systemTransactionBadge = default(string), string packageOwnerBadge = default(string), string validatorOwnerBadge = default(string), string accountOwnerBadge = default(string), string identityOwnerBadge = default(string), string packagePackage = default(string), string resourcePackage = default(string), string accountPackage = default(string), string identityPackage = default(string), string consensusManagerPackage = default(string), string accessControllerPackage = default(string), string transactionProcessorPackage = default(string), string metadataModulePackage = default(string), string royaltyModulePackage = default(string), string roleAssignmentModulePackage = default(string), string genesisHelperPackage = default(string), string faucetPackage = default(string), string poolPackage = default(string), string lockerPackage = default(string), string consensusManager = default(string), string genesisHelper = default(string), string faucet = default(string), string transactionTracker = default(string))
        {
            // to ensure "xrd" is required (not null)
            if (xrd == null)
            {
                throw new ArgumentNullException("xrd is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.Xrd = xrd;
            // to ensure "secp256k1SignatureVirtualBadge" is required (not null)
            if (secp256k1SignatureVirtualBadge == null)
            {
                throw new ArgumentNullException("secp256k1SignatureVirtualBadge is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.Secp256k1SignatureVirtualBadge = secp256k1SignatureVirtualBadge;
            // to ensure "ed25519SignatureVirtualBadge" is required (not null)
            if (ed25519SignatureVirtualBadge == null)
            {
                throw new ArgumentNullException("ed25519SignatureVirtualBadge is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.Ed25519SignatureVirtualBadge = ed25519SignatureVirtualBadge;
            // to ensure "packageOfDirectCallerVirtualBadge" is required (not null)
            if (packageOfDirectCallerVirtualBadge == null)
            {
                throw new ArgumentNullException("packageOfDirectCallerVirtualBadge is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.PackageOfDirectCallerVirtualBadge = packageOfDirectCallerVirtualBadge;
            // to ensure "globalCallerVirtualBadge" is required (not null)
            if (globalCallerVirtualBadge == null)
            {
                throw new ArgumentNullException("globalCallerVirtualBadge is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.GlobalCallerVirtualBadge = globalCallerVirtualBadge;
            // to ensure "systemTransactionBadge" is required (not null)
            if (systemTransactionBadge == null)
            {
                throw new ArgumentNullException("systemTransactionBadge is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.SystemTransactionBadge = systemTransactionBadge;
            // to ensure "packageOwnerBadge" is required (not null)
            if (packageOwnerBadge == null)
            {
                throw new ArgumentNullException("packageOwnerBadge is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.PackageOwnerBadge = packageOwnerBadge;
            // to ensure "validatorOwnerBadge" is required (not null)
            if (validatorOwnerBadge == null)
            {
                throw new ArgumentNullException("validatorOwnerBadge is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.ValidatorOwnerBadge = validatorOwnerBadge;
            // to ensure "accountOwnerBadge" is required (not null)
            if (accountOwnerBadge == null)
            {
                throw new ArgumentNullException("accountOwnerBadge is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.AccountOwnerBadge = accountOwnerBadge;
            // to ensure "identityOwnerBadge" is required (not null)
            if (identityOwnerBadge == null)
            {
                throw new ArgumentNullException("identityOwnerBadge is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.IdentityOwnerBadge = identityOwnerBadge;
            // to ensure "packagePackage" is required (not null)
            if (packagePackage == null)
            {
                throw new ArgumentNullException("packagePackage is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.PackagePackage = packagePackage;
            // to ensure "resourcePackage" is required (not null)
            if (resourcePackage == null)
            {
                throw new ArgumentNullException("resourcePackage is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.ResourcePackage = resourcePackage;
            // to ensure "accountPackage" is required (not null)
            if (accountPackage == null)
            {
                throw new ArgumentNullException("accountPackage is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.AccountPackage = accountPackage;
            // to ensure "identityPackage" is required (not null)
            if (identityPackage == null)
            {
                throw new ArgumentNullException("identityPackage is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.IdentityPackage = identityPackage;
            // to ensure "consensusManagerPackage" is required (not null)
            if (consensusManagerPackage == null)
            {
                throw new ArgumentNullException("consensusManagerPackage is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.ConsensusManagerPackage = consensusManagerPackage;
            // to ensure "accessControllerPackage" is required (not null)
            if (accessControllerPackage == null)
            {
                throw new ArgumentNullException("accessControllerPackage is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.AccessControllerPackage = accessControllerPackage;
            // to ensure "transactionProcessorPackage" is required (not null)
            if (transactionProcessorPackage == null)
            {
                throw new ArgumentNullException("transactionProcessorPackage is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.TransactionProcessorPackage = transactionProcessorPackage;
            // to ensure "metadataModulePackage" is required (not null)
            if (metadataModulePackage == null)
            {
                throw new ArgumentNullException("metadataModulePackage is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.MetadataModulePackage = metadataModulePackage;
            // to ensure "royaltyModulePackage" is required (not null)
            if (royaltyModulePackage == null)
            {
                throw new ArgumentNullException("royaltyModulePackage is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.RoyaltyModulePackage = royaltyModulePackage;
            // to ensure "roleAssignmentModulePackage" is required (not null)
            if (roleAssignmentModulePackage == null)
            {
                throw new ArgumentNullException("roleAssignmentModulePackage is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.RoleAssignmentModulePackage = roleAssignmentModulePackage;
            // to ensure "genesisHelperPackage" is required (not null)
            if (genesisHelperPackage == null)
            {
                throw new ArgumentNullException("genesisHelperPackage is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.GenesisHelperPackage = genesisHelperPackage;
            // to ensure "faucetPackage" is required (not null)
            if (faucetPackage == null)
            {
                throw new ArgumentNullException("faucetPackage is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.FaucetPackage = faucetPackage;
            // to ensure "poolPackage" is required (not null)
            if (poolPackage == null)
            {
                throw new ArgumentNullException("poolPackage is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.PoolPackage = poolPackage;
            // to ensure "consensusManager" is required (not null)
            if (consensusManager == null)
            {
                throw new ArgumentNullException("consensusManager is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.ConsensusManager = consensusManager;
            // to ensure "genesisHelper" is required (not null)
            if (genesisHelper == null)
            {
                throw new ArgumentNullException("genesisHelper is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.GenesisHelper = genesisHelper;
            // to ensure "faucet" is required (not null)
            if (faucet == null)
            {
                throw new ArgumentNullException("faucet is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.Faucet = faucet;
            // to ensure "transactionTracker" is required (not null)
            if (transactionTracker == null)
            {
                throw new ArgumentNullException("transactionTracker is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.TransactionTracker = transactionTracker;
            this.LockerPackage = lockerPackage;
        }

        /// <summary>
        /// Gets or Sets Xrd
        /// </summary>
        [DataMember(Name = "xrd", IsRequired = true, EmitDefaultValue = true)]
        public string Xrd { get; set; }

        /// <summary>
        /// Gets or Sets Secp256k1SignatureVirtualBadge
        /// </summary>
        [DataMember(Name = "secp256k1_signature_virtual_badge", IsRequired = true, EmitDefaultValue = true)]
        public string Secp256k1SignatureVirtualBadge { get; set; }

        /// <summary>
        /// Gets or Sets Ed25519SignatureVirtualBadge
        /// </summary>
        [DataMember(Name = "ed25519_signature_virtual_badge", IsRequired = true, EmitDefaultValue = true)]
        public string Ed25519SignatureVirtualBadge { get; set; }

        /// <summary>
        /// Gets or Sets PackageOfDirectCallerVirtualBadge
        /// </summary>
        [DataMember(Name = "package_of_direct_caller_virtual_badge", IsRequired = true, EmitDefaultValue = true)]
        public string PackageOfDirectCallerVirtualBadge { get; set; }

        /// <summary>
        /// Gets or Sets GlobalCallerVirtualBadge
        /// </summary>
        [DataMember(Name = "global_caller_virtual_badge", IsRequired = true, EmitDefaultValue = true)]
        public string GlobalCallerVirtualBadge { get; set; }

        /// <summary>
        /// Gets or Sets SystemTransactionBadge
        /// </summary>
        [DataMember(Name = "system_transaction_badge", IsRequired = true, EmitDefaultValue = true)]
        public string SystemTransactionBadge { get; set; }

        /// <summary>
        /// Gets or Sets PackageOwnerBadge
        /// </summary>
        [DataMember(Name = "package_owner_badge", IsRequired = true, EmitDefaultValue = true)]
        public string PackageOwnerBadge { get; set; }

        /// <summary>
        /// Gets or Sets ValidatorOwnerBadge
        /// </summary>
        [DataMember(Name = "validator_owner_badge", IsRequired = true, EmitDefaultValue = true)]
        public string ValidatorOwnerBadge { get; set; }

        /// <summary>
        /// Gets or Sets AccountOwnerBadge
        /// </summary>
        [DataMember(Name = "account_owner_badge", IsRequired = true, EmitDefaultValue = true)]
        public string AccountOwnerBadge { get; set; }

        /// <summary>
        /// Gets or Sets IdentityOwnerBadge
        /// </summary>
        [DataMember(Name = "identity_owner_badge", IsRequired = true, EmitDefaultValue = true)]
        public string IdentityOwnerBadge { get; set; }

        /// <summary>
        /// Gets or Sets PackagePackage
        /// </summary>
        [DataMember(Name = "package_package", IsRequired = true, EmitDefaultValue = true)]
        public string PackagePackage { get; set; }

        /// <summary>
        /// Gets or Sets ResourcePackage
        /// </summary>
        [DataMember(Name = "resource_package", IsRequired = true, EmitDefaultValue = true)]
        public string ResourcePackage { get; set; }

        /// <summary>
        /// Gets or Sets AccountPackage
        /// </summary>
        [DataMember(Name = "account_package", IsRequired = true, EmitDefaultValue = true)]
        public string AccountPackage { get; set; }

        /// <summary>
        /// Gets or Sets IdentityPackage
        /// </summary>
        [DataMember(Name = "identity_package", IsRequired = true, EmitDefaultValue = true)]
        public string IdentityPackage { get; set; }

        /// <summary>
        /// Gets or Sets ConsensusManagerPackage
        /// </summary>
        [DataMember(Name = "consensus_manager_package", IsRequired = true, EmitDefaultValue = true)]
        public string ConsensusManagerPackage { get; set; }

        /// <summary>
        /// Gets or Sets AccessControllerPackage
        /// </summary>
        [DataMember(Name = "access_controller_package", IsRequired = true, EmitDefaultValue = true)]
        public string AccessControllerPackage { get; set; }

        /// <summary>
        /// Gets or Sets TransactionProcessorPackage
        /// </summary>
        [DataMember(Name = "transaction_processor_package", IsRequired = true, EmitDefaultValue = true)]
        public string TransactionProcessorPackage { get; set; }

        /// <summary>
        /// Gets or Sets MetadataModulePackage
        /// </summary>
        [DataMember(Name = "metadata_module_package", IsRequired = true, EmitDefaultValue = true)]
        public string MetadataModulePackage { get; set; }

        /// <summary>
        /// Gets or Sets RoyaltyModulePackage
        /// </summary>
        [DataMember(Name = "royalty_module_package", IsRequired = true, EmitDefaultValue = true)]
        public string RoyaltyModulePackage { get; set; }

        /// <summary>
        /// Gets or Sets RoleAssignmentModulePackage
        /// </summary>
        [DataMember(Name = "role_assignment_module_package", IsRequired = true, EmitDefaultValue = true)]
        public string RoleAssignmentModulePackage { get; set; }

        /// <summary>
        /// Gets or Sets GenesisHelperPackage
        /// </summary>
        [DataMember(Name = "genesis_helper_package", IsRequired = true, EmitDefaultValue = true)]
        public string GenesisHelperPackage { get; set; }

        /// <summary>
        /// Gets or Sets FaucetPackage
        /// </summary>
        [DataMember(Name = "faucet_package", IsRequired = true, EmitDefaultValue = true)]
        public string FaucetPackage { get; set; }

        /// <summary>
        /// Gets or Sets PoolPackage
        /// </summary>
        [DataMember(Name = "pool_package", IsRequired = true, EmitDefaultValue = true)]
        public string PoolPackage { get; set; }

        /// <summary>
        /// Gets or Sets LockerPackage
        /// </summary>
        [DataMember(Name = "locker_package", EmitDefaultValue = true)]
        public string LockerPackage { get; set; }

        /// <summary>
        /// Gets or Sets ConsensusManager
        /// </summary>
        [DataMember(Name = "consensus_manager", IsRequired = true, EmitDefaultValue = true)]
        public string ConsensusManager { get; set; }

        /// <summary>
        /// Gets or Sets GenesisHelper
        /// </summary>
        [DataMember(Name = "genesis_helper", IsRequired = true, EmitDefaultValue = true)]
        public string GenesisHelper { get; set; }

        /// <summary>
        /// Gets or Sets Faucet
        /// </summary>
        [DataMember(Name = "faucet", IsRequired = true, EmitDefaultValue = true)]
        public string Faucet { get; set; }

        /// <summary>
        /// Gets or Sets TransactionTracker
        /// </summary>
        [DataMember(Name = "transaction_tracker", IsRequired = true, EmitDefaultValue = true)]
        public string TransactionTracker { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class NetworkConfigurationResponseWellKnownAddresses {\n");
            sb.Append("  Xrd: ").Append(Xrd).Append("\n");
            sb.Append("  Secp256k1SignatureVirtualBadge: ").Append(Secp256k1SignatureVirtualBadge).Append("\n");
            sb.Append("  Ed25519SignatureVirtualBadge: ").Append(Ed25519SignatureVirtualBadge).Append("\n");
            sb.Append("  PackageOfDirectCallerVirtualBadge: ").Append(PackageOfDirectCallerVirtualBadge).Append("\n");
            sb.Append("  GlobalCallerVirtualBadge: ").Append(GlobalCallerVirtualBadge).Append("\n");
            sb.Append("  SystemTransactionBadge: ").Append(SystemTransactionBadge).Append("\n");
            sb.Append("  PackageOwnerBadge: ").Append(PackageOwnerBadge).Append("\n");
            sb.Append("  ValidatorOwnerBadge: ").Append(ValidatorOwnerBadge).Append("\n");
            sb.Append("  AccountOwnerBadge: ").Append(AccountOwnerBadge).Append("\n");
            sb.Append("  IdentityOwnerBadge: ").Append(IdentityOwnerBadge).Append("\n");
            sb.Append("  PackagePackage: ").Append(PackagePackage).Append("\n");
            sb.Append("  ResourcePackage: ").Append(ResourcePackage).Append("\n");
            sb.Append("  AccountPackage: ").Append(AccountPackage).Append("\n");
            sb.Append("  IdentityPackage: ").Append(IdentityPackage).Append("\n");
            sb.Append("  ConsensusManagerPackage: ").Append(ConsensusManagerPackage).Append("\n");
            sb.Append("  AccessControllerPackage: ").Append(AccessControllerPackage).Append("\n");
            sb.Append("  TransactionProcessorPackage: ").Append(TransactionProcessorPackage).Append("\n");
            sb.Append("  MetadataModulePackage: ").Append(MetadataModulePackage).Append("\n");
            sb.Append("  RoyaltyModulePackage: ").Append(RoyaltyModulePackage).Append("\n");
            sb.Append("  RoleAssignmentModulePackage: ").Append(RoleAssignmentModulePackage).Append("\n");
            sb.Append("  GenesisHelperPackage: ").Append(GenesisHelperPackage).Append("\n");
            sb.Append("  FaucetPackage: ").Append(FaucetPackage).Append("\n");
            sb.Append("  PoolPackage: ").Append(PoolPackage).Append("\n");
            sb.Append("  LockerPackage: ").Append(LockerPackage).Append("\n");
            sb.Append("  ConsensusManager: ").Append(ConsensusManager).Append("\n");
            sb.Append("  GenesisHelper: ").Append(GenesisHelper).Append("\n");
            sb.Append("  Faucet: ").Append(Faucet).Append("\n");
            sb.Append("  TransactionTracker: ").Append(TransactionTracker).Append("\n");
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
                    this.Xrd == input.Xrd ||
                    (this.Xrd != null &&
                    this.Xrd.Equals(input.Xrd))
                ) && 
                (
                    this.Secp256k1SignatureVirtualBadge == input.Secp256k1SignatureVirtualBadge ||
                    (this.Secp256k1SignatureVirtualBadge != null &&
                    this.Secp256k1SignatureVirtualBadge.Equals(input.Secp256k1SignatureVirtualBadge))
                ) && 
                (
                    this.Ed25519SignatureVirtualBadge == input.Ed25519SignatureVirtualBadge ||
                    (this.Ed25519SignatureVirtualBadge != null &&
                    this.Ed25519SignatureVirtualBadge.Equals(input.Ed25519SignatureVirtualBadge))
                ) && 
                (
                    this.PackageOfDirectCallerVirtualBadge == input.PackageOfDirectCallerVirtualBadge ||
                    (this.PackageOfDirectCallerVirtualBadge != null &&
                    this.PackageOfDirectCallerVirtualBadge.Equals(input.PackageOfDirectCallerVirtualBadge))
                ) && 
                (
                    this.GlobalCallerVirtualBadge == input.GlobalCallerVirtualBadge ||
                    (this.GlobalCallerVirtualBadge != null &&
                    this.GlobalCallerVirtualBadge.Equals(input.GlobalCallerVirtualBadge))
                ) && 
                (
                    this.SystemTransactionBadge == input.SystemTransactionBadge ||
                    (this.SystemTransactionBadge != null &&
                    this.SystemTransactionBadge.Equals(input.SystemTransactionBadge))
                ) && 
                (
                    this.PackageOwnerBadge == input.PackageOwnerBadge ||
                    (this.PackageOwnerBadge != null &&
                    this.PackageOwnerBadge.Equals(input.PackageOwnerBadge))
                ) && 
                (
                    this.ValidatorOwnerBadge == input.ValidatorOwnerBadge ||
                    (this.ValidatorOwnerBadge != null &&
                    this.ValidatorOwnerBadge.Equals(input.ValidatorOwnerBadge))
                ) && 
                (
                    this.AccountOwnerBadge == input.AccountOwnerBadge ||
                    (this.AccountOwnerBadge != null &&
                    this.AccountOwnerBadge.Equals(input.AccountOwnerBadge))
                ) && 
                (
                    this.IdentityOwnerBadge == input.IdentityOwnerBadge ||
                    (this.IdentityOwnerBadge != null &&
                    this.IdentityOwnerBadge.Equals(input.IdentityOwnerBadge))
                ) && 
                (
                    this.PackagePackage == input.PackagePackage ||
                    (this.PackagePackage != null &&
                    this.PackagePackage.Equals(input.PackagePackage))
                ) && 
                (
                    this.ResourcePackage == input.ResourcePackage ||
                    (this.ResourcePackage != null &&
                    this.ResourcePackage.Equals(input.ResourcePackage))
                ) && 
                (
                    this.AccountPackage == input.AccountPackage ||
                    (this.AccountPackage != null &&
                    this.AccountPackage.Equals(input.AccountPackage))
                ) && 
                (
                    this.IdentityPackage == input.IdentityPackage ||
                    (this.IdentityPackage != null &&
                    this.IdentityPackage.Equals(input.IdentityPackage))
                ) && 
                (
                    this.ConsensusManagerPackage == input.ConsensusManagerPackage ||
                    (this.ConsensusManagerPackage != null &&
                    this.ConsensusManagerPackage.Equals(input.ConsensusManagerPackage))
                ) && 
                (
                    this.AccessControllerPackage == input.AccessControllerPackage ||
                    (this.AccessControllerPackage != null &&
                    this.AccessControllerPackage.Equals(input.AccessControllerPackage))
                ) && 
                (
                    this.TransactionProcessorPackage == input.TransactionProcessorPackage ||
                    (this.TransactionProcessorPackage != null &&
                    this.TransactionProcessorPackage.Equals(input.TransactionProcessorPackage))
                ) && 
                (
                    this.MetadataModulePackage == input.MetadataModulePackage ||
                    (this.MetadataModulePackage != null &&
                    this.MetadataModulePackage.Equals(input.MetadataModulePackage))
                ) && 
                (
                    this.RoyaltyModulePackage == input.RoyaltyModulePackage ||
                    (this.RoyaltyModulePackage != null &&
                    this.RoyaltyModulePackage.Equals(input.RoyaltyModulePackage))
                ) && 
                (
                    this.RoleAssignmentModulePackage == input.RoleAssignmentModulePackage ||
                    (this.RoleAssignmentModulePackage != null &&
                    this.RoleAssignmentModulePackage.Equals(input.RoleAssignmentModulePackage))
                ) && 
                (
                    this.GenesisHelperPackage == input.GenesisHelperPackage ||
                    (this.GenesisHelperPackage != null &&
                    this.GenesisHelperPackage.Equals(input.GenesisHelperPackage))
                ) && 
                (
                    this.FaucetPackage == input.FaucetPackage ||
                    (this.FaucetPackage != null &&
                    this.FaucetPackage.Equals(input.FaucetPackage))
                ) && 
                (
                    this.PoolPackage == input.PoolPackage ||
                    (this.PoolPackage != null &&
                    this.PoolPackage.Equals(input.PoolPackage))
                ) && 
                (
                    this.LockerPackage == input.LockerPackage ||
                    (this.LockerPackage != null &&
                    this.LockerPackage.Equals(input.LockerPackage))
                ) && 
                (
                    this.ConsensusManager == input.ConsensusManager ||
                    (this.ConsensusManager != null &&
                    this.ConsensusManager.Equals(input.ConsensusManager))
                ) && 
                (
                    this.GenesisHelper == input.GenesisHelper ||
                    (this.GenesisHelper != null &&
                    this.GenesisHelper.Equals(input.GenesisHelper))
                ) && 
                (
                    this.Faucet == input.Faucet ||
                    (this.Faucet != null &&
                    this.Faucet.Equals(input.Faucet))
                ) && 
                (
                    this.TransactionTracker == input.TransactionTracker ||
                    (this.TransactionTracker != null &&
                    this.TransactionTracker.Equals(input.TransactionTracker))
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
                if (this.Xrd != null)
                {
                    hashCode = (hashCode * 59) + this.Xrd.GetHashCode();
                }
                if (this.Secp256k1SignatureVirtualBadge != null)
                {
                    hashCode = (hashCode * 59) + this.Secp256k1SignatureVirtualBadge.GetHashCode();
                }
                if (this.Ed25519SignatureVirtualBadge != null)
                {
                    hashCode = (hashCode * 59) + this.Ed25519SignatureVirtualBadge.GetHashCode();
                }
                if (this.PackageOfDirectCallerVirtualBadge != null)
                {
                    hashCode = (hashCode * 59) + this.PackageOfDirectCallerVirtualBadge.GetHashCode();
                }
                if (this.GlobalCallerVirtualBadge != null)
                {
                    hashCode = (hashCode * 59) + this.GlobalCallerVirtualBadge.GetHashCode();
                }
                if (this.SystemTransactionBadge != null)
                {
                    hashCode = (hashCode * 59) + this.SystemTransactionBadge.GetHashCode();
                }
                if (this.PackageOwnerBadge != null)
                {
                    hashCode = (hashCode * 59) + this.PackageOwnerBadge.GetHashCode();
                }
                if (this.ValidatorOwnerBadge != null)
                {
                    hashCode = (hashCode * 59) + this.ValidatorOwnerBadge.GetHashCode();
                }
                if (this.AccountOwnerBadge != null)
                {
                    hashCode = (hashCode * 59) + this.AccountOwnerBadge.GetHashCode();
                }
                if (this.IdentityOwnerBadge != null)
                {
                    hashCode = (hashCode * 59) + this.IdentityOwnerBadge.GetHashCode();
                }
                if (this.PackagePackage != null)
                {
                    hashCode = (hashCode * 59) + this.PackagePackage.GetHashCode();
                }
                if (this.ResourcePackage != null)
                {
                    hashCode = (hashCode * 59) + this.ResourcePackage.GetHashCode();
                }
                if (this.AccountPackage != null)
                {
                    hashCode = (hashCode * 59) + this.AccountPackage.GetHashCode();
                }
                if (this.IdentityPackage != null)
                {
                    hashCode = (hashCode * 59) + this.IdentityPackage.GetHashCode();
                }
                if (this.ConsensusManagerPackage != null)
                {
                    hashCode = (hashCode * 59) + this.ConsensusManagerPackage.GetHashCode();
                }
                if (this.AccessControllerPackage != null)
                {
                    hashCode = (hashCode * 59) + this.AccessControllerPackage.GetHashCode();
                }
                if (this.TransactionProcessorPackage != null)
                {
                    hashCode = (hashCode * 59) + this.TransactionProcessorPackage.GetHashCode();
                }
                if (this.MetadataModulePackage != null)
                {
                    hashCode = (hashCode * 59) + this.MetadataModulePackage.GetHashCode();
                }
                if (this.RoyaltyModulePackage != null)
                {
                    hashCode = (hashCode * 59) + this.RoyaltyModulePackage.GetHashCode();
                }
                if (this.RoleAssignmentModulePackage != null)
                {
                    hashCode = (hashCode * 59) + this.RoleAssignmentModulePackage.GetHashCode();
                }
                if (this.GenesisHelperPackage != null)
                {
                    hashCode = (hashCode * 59) + this.GenesisHelperPackage.GetHashCode();
                }
                if (this.FaucetPackage != null)
                {
                    hashCode = (hashCode * 59) + this.FaucetPackage.GetHashCode();
                }
                if (this.PoolPackage != null)
                {
                    hashCode = (hashCode * 59) + this.PoolPackage.GetHashCode();
                }
                if (this.LockerPackage != null)
                {
                    hashCode = (hashCode * 59) + this.LockerPackage.GetHashCode();
                }
                if (this.ConsensusManager != null)
                {
                    hashCode = (hashCode * 59) + this.ConsensusManager.GetHashCode();
                }
                if (this.GenesisHelper != null)
                {
                    hashCode = (hashCode * 59) + this.GenesisHelper.GetHashCode();
                }
                if (this.Faucet != null)
                {
                    hashCode = (hashCode * 59) + this.Faucet.GetHashCode();
                }
                if (this.TransactionTracker != null)
                {
                    hashCode = (hashCode * 59) + this.TransactionTracker.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
