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
using FileParameter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Model
{
    /// <summary>
    /// NetworkConfigurationResponseWellKnownAddresses
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
        /// <param name="xrd">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="secp256k1SignatureVirtualBadge">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="ed25519SignatureVirtualBadge">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="packageOfDirectCallerVirtualBadge">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="globalCallerVirtualBadge">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="systemTransactionBadge">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="packageOwnerBadge">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="validatorOwnerBadge">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="accountOwnerBadge">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="identityOwnerBadge">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="packagePackage">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="resourcePackage">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="accountPackage">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="identityPackage">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="consensusManagerPackage">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="accessControllerPackage">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="transactionProcessorPackage">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="metadataModulePackage">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="royaltyModulePackage">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="accessRulesPackage">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="genesisHelperPackage">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="faucetPackage">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="consensusManager">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="genesisHelper">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="faucet">Bech32m-encoded human readable version of the address. (required).</param>
        /// <param name="poolPackage">Bech32m-encoded human readable version of the address. (required).</param>
        public NetworkConfigurationResponseWellKnownAddresses(string xrd = default(string), string secp256k1SignatureVirtualBadge = default(string), string ed25519SignatureVirtualBadge = default(string), string packageOfDirectCallerVirtualBadge = default(string), string globalCallerVirtualBadge = default(string), string systemTransactionBadge = default(string), string packageOwnerBadge = default(string), string validatorOwnerBadge = default(string), string accountOwnerBadge = default(string), string identityOwnerBadge = default(string), string packagePackage = default(string), string resourcePackage = default(string), string accountPackage = default(string), string identityPackage = default(string), string consensusManagerPackage = default(string), string accessControllerPackage = default(string), string transactionProcessorPackage = default(string), string metadataModulePackage = default(string), string royaltyModulePackage = default(string), string accessRulesPackage = default(string), string genesisHelperPackage = default(string), string faucetPackage = default(string), string consensusManager = default(string), string genesisHelper = default(string), string faucet = default(string), string poolPackage = default(string))
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
            // to ensure "accessRulesPackage" is required (not null)
            if (accessRulesPackage == null)
            {
                throw new ArgumentNullException("accessRulesPackage is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.AccessRulesPackage = accessRulesPackage;
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
            // to ensure "poolPackage" is required (not null)
            if (poolPackage == null)
            {
                throw new ArgumentNullException("poolPackage is a required property for NetworkConfigurationResponseWellKnownAddresses and cannot be null");
            }
            this.PoolPackage = poolPackage;
        }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "xrd", IsRequired = true, EmitDefaultValue = true)]
        public string Xrd { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "secp256k1_signature_virtual_badge", IsRequired = true, EmitDefaultValue = true)]
        public string Secp256k1SignatureVirtualBadge { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "ed25519_signature_virtual_badge", IsRequired = true, EmitDefaultValue = true)]
        public string Ed25519SignatureVirtualBadge { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "package_of_direct_caller_virtual_badge", IsRequired = true, EmitDefaultValue = true)]
        public string PackageOfDirectCallerVirtualBadge { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "global_caller_virtual_badge", IsRequired = true, EmitDefaultValue = true)]
        public string GlobalCallerVirtualBadge { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "system_transaction_badge", IsRequired = true, EmitDefaultValue = true)]
        public string SystemTransactionBadge { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "package_owner_badge", IsRequired = true, EmitDefaultValue = true)]
        public string PackageOwnerBadge { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "validator_owner_badge", IsRequired = true, EmitDefaultValue = true)]
        public string ValidatorOwnerBadge { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "account_owner_badge", IsRequired = true, EmitDefaultValue = true)]
        public string AccountOwnerBadge { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "identity_owner_badge", IsRequired = true, EmitDefaultValue = true)]
        public string IdentityOwnerBadge { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "package_package", IsRequired = true, EmitDefaultValue = true)]
        public string PackagePackage { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "resource_package", IsRequired = true, EmitDefaultValue = true)]
        public string ResourcePackage { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "account_package", IsRequired = true, EmitDefaultValue = true)]
        public string AccountPackage { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "identity_package", IsRequired = true, EmitDefaultValue = true)]
        public string IdentityPackage { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "consensus_manager_package", IsRequired = true, EmitDefaultValue = true)]
        public string ConsensusManagerPackage { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "access_controller_package", IsRequired = true, EmitDefaultValue = true)]
        public string AccessControllerPackage { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "transaction_processor_package", IsRequired = true, EmitDefaultValue = true)]
        public string TransactionProcessorPackage { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "metadata_module_package", IsRequired = true, EmitDefaultValue = true)]
        public string MetadataModulePackage { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "royalty_module_package", IsRequired = true, EmitDefaultValue = true)]
        public string RoyaltyModulePackage { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "access_rules_package", IsRequired = true, EmitDefaultValue = true)]
        public string AccessRulesPackage { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "genesis_helper_package", IsRequired = true, EmitDefaultValue = true)]
        public string GenesisHelperPackage { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "faucet_package", IsRequired = true, EmitDefaultValue = true)]
        public string FaucetPackage { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "consensus_manager", IsRequired = true, EmitDefaultValue = true)]
        public string ConsensusManager { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "genesis_helper", IsRequired = true, EmitDefaultValue = true)]
        public string GenesisHelper { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "faucet", IsRequired = true, EmitDefaultValue = true)]
        public string Faucet { get; set; }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "pool_package", IsRequired = true, EmitDefaultValue = true)]
        public string PoolPackage { get; set; }

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
            sb.Append("  AccessRulesPackage: ").Append(AccessRulesPackage).Append("\n");
            sb.Append("  GenesisHelperPackage: ").Append(GenesisHelperPackage).Append("\n");
            sb.Append("  FaucetPackage: ").Append(FaucetPackage).Append("\n");
            sb.Append("  ConsensusManager: ").Append(ConsensusManager).Append("\n");
            sb.Append("  GenesisHelper: ").Append(GenesisHelper).Append("\n");
            sb.Append("  Faucet: ").Append(Faucet).Append("\n");
            sb.Append("  PoolPackage: ").Append(PoolPackage).Append("\n");
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
                    this.AccessRulesPackage == input.AccessRulesPackage ||
                    (this.AccessRulesPackage != null &&
                    this.AccessRulesPackage.Equals(input.AccessRulesPackage))
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
                    this.PoolPackage == input.PoolPackage ||
                    (this.PoolPackage != null &&
                    this.PoolPackage.Equals(input.PoolPackage))
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
                if (this.AccessRulesPackage != null)
                {
                    hashCode = (hashCode * 59) + this.AccessRulesPackage.GetHashCode();
                }
                if (this.GenesisHelperPackage != null)
                {
                    hashCode = (hashCode * 59) + this.GenesisHelperPackage.GetHashCode();
                }
                if (this.FaucetPackage != null)
                {
                    hashCode = (hashCode * 59) + this.FaucetPackage.GetHashCode();
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
                if (this.PoolPackage != null)
                {
                    hashCode = (hashCode * 59) + this.PoolPackage.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
