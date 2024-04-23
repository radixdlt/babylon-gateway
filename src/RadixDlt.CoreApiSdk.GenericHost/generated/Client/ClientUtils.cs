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
 * Radix Core API - Babylon
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  The default configuration is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function. The node exposes a configuration flag which allows disabling certain endpoints which may be problematic, but monitoring is advised. This configuration parameter is `api.core.flags.enable_unbounded_endpoints` / `RADIXDLT_CORE_API_FLAGS_ENABLE_UNBOUNDED_ENDPOINTS`.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` have high guarantees of forward compatibility in future node versions. We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  Other endpoints may be changed with new node versions carrying protocol-updates, although any breaking changes will be flagged clearly in the corresponding release notes.  All responses may have additional fields added, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects. 
 *
 * The version of the OpenAPI document: v1.0.4
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */

#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using RadixDlt.CoreApiSdk.GenericHost.Model;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RadixDlt.CoreApiSdk.GenericHost.Test")]

namespace RadixDlt.CoreApiSdk.GenericHost.Client
{
    /// <summary>
    /// Utility functions providing some benefit to API client consumers.
    /// </summary>
    public static class ClientUtils
    {

        /// <summary>
        /// A delegate for events.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public delegate void EventHandler<T>(object sender, T e) where T : EventArgs;

        /// <summary>
        /// Returns true when deserialization succeeds.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="options"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryDeserialize<T>(string json, JsonSerializerOptions options, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? result)
        {
            try
            {
                result = JsonSerializer.Deserialize<T>(json, options);
                return result != null;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Returns true when deserialization succeeds.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="options"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryDeserialize<T>(ref Utf8JsonReader reader, JsonSerializerOptions options, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? result)
        {
            try
            {
                result = JsonSerializer.Deserialize<T>(ref reader, options);
                return result != null;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Sanitize filename by removing the path
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>Filename</returns>
        public static string SanitizeFilename(string filename)
        {
            Match match = Regex.Match(filename, @".*[/\\](.*)$");
            return match.Success ? match.Groups[1].Value : filename;
        }

        /// <summary>
        /// If parameter is DateTime, output in a formatted string (default ISO 8601), customizable with Configuration.DateTime.
        /// If parameter is a list, join the list with ",".
        /// Otherwise just return the string.
        /// </summary>
        /// <param name="obj">The parameter (header, path, query, form).</param>
        /// <param name="format">The DateTime serialization format.</param>
        /// <returns>Formatted string.</returns>
        public static string? ParameterToString(object? obj, string? format = ISO8601_DATETIME_FORMAT)
        {
            if (obj is DateTime dateTime)
                // Return a formatted date string - Can be customized with Configuration.DateTimeFormat
                // Defaults to an ISO 8601, using the known as a Round-trip date/time pattern ("o")
                // https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx#Anchor_8
                // For example: 2009-06-15T13:45:30.0000000
                return dateTime.ToString(format);
            if (obj is DateTimeOffset dateTimeOffset)
                // Return a formatted date string - Can be customized with Configuration.DateTimeFormat
                // Defaults to an ISO 8601, using the known as a Round-trip date/time pattern ("o")
                // https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx#Anchor_8
                // For example: 2009-06-15T13:45:30.0000000
                return dateTimeOffset.ToString(format);
            if (obj is bool boolean)
                return boolean
                    ? "true"
                    : "false";
            if (obj is AccessRuleNodeType accessRuleNodeType)
                return AccessRuleNodeTypeValueConverter.ToJsonValue(accessRuleNodeType);
            if (obj is AccessRuleType accessRuleType)
                return AccessRuleTypeValueConverter.ToJsonValue(accessRuleType);
            if (obj is AttachedModuleId attachedModuleId)
                return AttachedModuleIdValueConverter.ToJsonValue(attachedModuleId);
            if (obj is AuthorizedDepositorBadgeType authorizedDepositorBadgeType)
                return AuthorizedDepositorBadgeTypeValueConverter.ToJsonValue(authorizedDepositorBadgeType);
            if (obj is BlueprintCollectionSchemaType blueprintCollectionSchemaType)
                return BlueprintCollectionSchemaTypeValueConverter.ToJsonValue(blueprintCollectionSchemaType);
            if (obj is BlueprintPayloadDefType blueprintPayloadDefType)
                return BlueprintPayloadDefTypeValueConverter.ToJsonValue(blueprintPayloadDefType);
            if (obj is BlueprintTypeReferenceKind blueprintTypeReferenceKind)
                return BlueprintTypeReferenceKindValueConverter.ToJsonValue(blueprintTypeReferenceKind);
            if (obj is DefaultDepositRule defaultDepositRule)
                return DefaultDepositRuleValueConverter.ToJsonValue(defaultDepositRule);
            if (obj is EntityModule entityModule)
                return EntityModuleValueConverter.ToJsonValue(entityModule);
            if (obj is EntityType entityType)
                return EntityTypeValueConverter.ToJsonValue(entityType);
            if (obj is ErrorResponseType errorResponseType)
                return ErrorResponseTypeValueConverter.ToJsonValue(errorResponseType);
            if (obj is EventEmitterIdentifierType eventEmitterIdentifierType)
                return EventEmitterIdentifierTypeValueConverter.ToJsonValue(eventEmitterIdentifierType);
            if (obj is FieldSchemaFeatureConditionType fieldSchemaFeatureConditionType)
                return FieldSchemaFeatureConditionTypeValueConverter.ToJsonValue(fieldSchemaFeatureConditionType);
            if (obj is FunctionAuthType functionAuthType)
                return FunctionAuthTypeValueConverter.ToJsonValue(functionAuthType);
            if (obj is GenericSubstitutionType genericSubstitutionType)
                return GenericSubstitutionTypeValueConverter.ToJsonValue(genericSubstitutionType);
            if (obj is GenericTypeParameterConstraints genericTypeParameterConstraints)
                return GenericTypeParameterConstraintsValueConverter.ToJsonValue(genericTypeParameterConstraints);
            if (obj is LedgerProofOriginType ledgerProofOriginType)
                return LedgerProofOriginTypeValueConverter.ToJsonValue(ledgerProofOriginType);
            if (obj is LedgerTransactionType ledgerTransactionType)
                return LedgerTransactionTypeValueConverter.ToJsonValue(ledgerTransactionType);
            if (obj is LocalTypeId.KindEnum localTypeIdKindEnum)
                return LocalTypeId.KindEnumToJsonValue(localTypeIdKindEnum);
            if (obj is LtsCommittedTransactionStatus ltsCommittedTransactionStatus)
                return LtsCommittedTransactionStatusValueConverter.ToJsonValue(ltsCommittedTransactionStatus);
            if (obj is LtsFeeFungibleResourceBalanceChangeType ltsFeeFungibleResourceBalanceChangeType)
                return LtsFeeFungibleResourceBalanceChangeTypeValueConverter.ToJsonValue(ltsFeeFungibleResourceBalanceChangeType);
            if (obj is LtsTransactionIntentStatus ltsTransactionIntentStatus)
                return LtsTransactionIntentStatusValueConverter.ToJsonValue(ltsTransactionIntentStatus);
            if (obj is LtsTransactionPayloadStatus ltsTransactionPayloadStatus)
                return LtsTransactionPayloadStatusValueConverter.ToJsonValue(ltsTransactionPayloadStatus);
            if (obj is LtsTransactionSubmitErrorDetailsType ltsTransactionSubmitErrorDetailsType)
                return LtsTransactionSubmitErrorDetailsTypeValueConverter.ToJsonValue(ltsTransactionSubmitErrorDetailsType);
            if (obj is MethodAccessibilityType methodAccessibilityType)
                return MethodAccessibilityTypeValueConverter.ToJsonValue(methodAccessibilityType);
            if (obj is MethodAuthType methodAuthType)
                return MethodAuthTypeValueConverter.ToJsonValue(methodAuthType);
            if (obj is ModuleId moduleId)
                return ModuleIdValueConverter.ToJsonValue(moduleId);
            if (obj is NonFungibleIdType nonFungibleIdType)
                return NonFungibleIdTypeValueConverter.ToJsonValue(nonFungibleIdType);
            if (obj is ObjectHook objectHook)
                return ObjectHookValueConverter.ToJsonValue(objectHook);
            if (obj is ObjectSubstateTypeReferenceType objectSubstateTypeReferenceType)
                return ObjectSubstateTypeReferenceTypeValueConverter.ToJsonValue(objectSubstateTypeReferenceType);
            if (obj is OwnerRoleUpdater ownerRoleUpdater)
                return OwnerRoleUpdaterValueConverter.ToJsonValue(ownerRoleUpdater);
            if (obj is ParsedTransactionType parsedTransactionType)
                return ParsedTransactionTypeValueConverter.ToJsonValue(parsedTransactionType);
            if (obj is PartitionDescriptionType partitionDescriptionType)
                return PartitionDescriptionTypeValueConverter.ToJsonValue(partitionDescriptionType);
            if (obj is PartitionKind partitionKind)
                return PartitionKindValueConverter.ToJsonValue(partitionKind);
            if (obj is PlaintextMessageContentType plaintextMessageContentType)
                return PlaintextMessageContentTypeValueConverter.ToJsonValue(plaintextMessageContentType);
            if (obj is PresentedBadgeType presentedBadgeType)
                return PresentedBadgeTypeValueConverter.ToJsonValue(presentedBadgeType);
            if (obj is ProofRuleType proofRuleType)
                return ProofRuleTypeValueConverter.ToJsonValue(proofRuleType);
            if (obj is PublicKeyType publicKeyType)
                return PublicKeyTypeValueConverter.ToJsonValue(publicKeyType);
            if (obj is ReceiverInfo.ReceiverEnum receiverInfoReceiverEnum)
                return ReceiverInfo.ReceiverEnumToJsonValue(receiverInfoReceiverEnum);
            if (obj is RequirementType requirementType)
                return RequirementTypeValueConverter.ToJsonValue(requirementType);
            if (obj is ResourcePreference resourcePreference)
                return ResourcePreferenceValueConverter.ToJsonValue(resourcePreference);
            if (obj is ResourceType resourceType)
                return ResourceTypeValueConverter.ToJsonValue(resourceType);
            if (obj is RoleSpecification roleSpecification)
                return RoleSpecificationValueConverter.ToJsonValue(roleSpecification);
            if (obj is RoyaltyAmount.UnitEnum royaltyAmountUnitEnum)
                return RoyaltyAmount.UnitEnumToJsonValue(royaltyAmountUnitEnum);
            if (obj is StreamTransactionsErrorDetailsType streamTransactionsErrorDetailsType)
                return StreamTransactionsErrorDetailsTypeValueConverter.ToJsonValue(streamTransactionsErrorDetailsType);
            if (obj is SubstateKeyType substateKeyType)
                return SubstateKeyTypeValueConverter.ToJsonValue(substateKeyType);
            if (obj is SubstateSystemStructureType substateSystemStructureType)
                return SubstateSystemStructureTypeValueConverter.ToJsonValue(substateSystemStructureType);
            if (obj is SubstateType substateType)
                return SubstateTypeValueConverter.ToJsonValue(substateType);
            if (obj is SystemFieldKind systemFieldKind)
                return SystemFieldKindValueConverter.ToJsonValue(systemFieldKind);
            if (obj is TargetIdentifierType targetIdentifierType)
                return TargetIdentifierTypeValueConverter.ToJsonValue(targetIdentifierType);
            if (obj is TransactionIntentStatus transactionIntentStatus)
                return TransactionIntentStatusValueConverter.ToJsonValue(transactionIntentStatus);
            if (obj is TransactionMessageType transactionMessageType)
                return TransactionMessageTypeValueConverter.ToJsonValue(transactionMessageType);
            if (obj is TransactionParseRequest.ParseModeEnum transactionParseRequestParseModeEnum)
                return TransactionParseRequest.ParseModeEnumToJsonValue(transactionParseRequestParseModeEnum);
            if (obj is TransactionParseRequest.ResponseModeEnum transactionParseRequestResponseModeEnum)
                return TransactionParseRequest.ResponseModeEnumToJsonValue(transactionParseRequestResponseModeEnum);
            if (obj is TransactionParseRequest.ValidationModeEnum transactionParseRequestValidationModeEnum)
                return TransactionParseRequest.ValidationModeEnumToJsonValue(transactionParseRequestValidationModeEnum);
            if (obj is TransactionPayloadStatus transactionPayloadStatus)
                return TransactionPayloadStatusValueConverter.ToJsonValue(transactionPayloadStatus);
            if (obj is TransactionStatus transactionStatus)
                return TransactionStatusValueConverter.ToJsonValue(transactionStatus);
            if (obj is TransactionSubmitErrorDetailsType transactionSubmitErrorDetailsType)
                return TransactionSubmitErrorDetailsTypeValueConverter.ToJsonValue(transactionSubmitErrorDetailsType);
            if (obj is TransactionTrackerTransactionStatus transactionTrackerTransactionStatus)
                return TransactionTrackerTransactionStatusValueConverter.ToJsonValue(transactionTrackerTransactionStatus);
            if (obj is TypeInfoType typeInfoType)
                return TypeInfoTypeValueConverter.ToJsonValue(typeInfoType);
            if (obj is VmType vmType)
                return VmTypeValueConverter.ToJsonValue(vmType);
            if (obj is ICollection collection)
            {
                List<string?> entries = new();
                foreach (var entry in collection)
                    entries.Add(ParameterToString(entry));
                return string.Join(",", entries);
            }

            return Convert.ToString(obj, System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// URL encode a string
        /// Credit/Ref: https://github.com/restsharp/RestSharp/blob/master/RestSharp/Extensions/StringExtensions.cs#L50
        /// </summary>
        /// <param name="input">string to be URL encoded</param>
        /// <returns>Byte array</returns>
        public static string UrlEncode(string input)
        {
            const int maxLength = 32766;

            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (input.Length <= maxLength)
            {
                return Uri.EscapeDataString(input);
            }

            StringBuilder sb = new StringBuilder(input.Length * 2);
            int index = 0;

            while (index < input.Length)
            {
                int length = Math.Min(input.Length - index, maxLength);
                string subString = input.Substring(index, length);

                sb.Append(Uri.EscapeDataString(subString));
                index += subString.Length;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Encode string in base64 format.
        /// </summary>
        /// <param name="text">string to be encoded.</param>
        /// <returns>Encoded string.</returns>
        public static string Base64Encode(string text)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        /// Convert stream to byte array
        /// </summary>
        /// <param name="inputStream">Input stream to be converted</param>
        /// <returns>Byte array</returns>
        public static byte[] ReadAsBytes(Stream inputStream)
        {
            using (var ms = new MemoryStream())
            {
                inputStream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Select the Content-Type header's value from the given content-type array:
        /// if JSON type exists in the given array, use it;
        /// otherwise use the first one defined in 'consumes'
        /// </summary>
        /// <param name="contentTypes">The Content-Type array to select from.</param>
        /// <returns>The Content-Type header to use.</returns>
        public static string? SelectHeaderContentType(string[] contentTypes)
        {
            if (contentTypes.Length == 0)
                return null;

            foreach (var contentType in contentTypes)
            {
                if (IsJsonMime(contentType))
                    return contentType;
            }

            return contentTypes[0]; // use the first content type specified in 'consumes'
        }

        /// <summary>
        /// Select the Accept header's value from the given accepts array:
        /// if JSON exists in the given array, use it;
        /// otherwise use all of them (joining into a string)
        /// </summary>
        /// <param name="accepts">The accepts array to select from.</param>
        /// <returns>The Accept header to use.</returns>
        public static string? SelectHeaderAccept(string[] accepts)
        {
            if (accepts.Length == 0)
                return null;

            if (accepts.Contains("application/json", StringComparer.OrdinalIgnoreCase))
                return "application/json";

            return string.Join(",", accepts);
        }

        /// <summary>
        /// Provides a case-insensitive check that a provided content type is a known JSON-like content type.
        /// </summary>
        public static readonly Regex JsonRegex = new Regex("(?i)^(application/json|[^;/ \t]+/[^;/ \t]+[+]json)[ \t]*(;.*)?$");

        /// <summary>
        /// Check if the given MIME is a JSON MIME.
        /// JSON MIME examples:
        ///    application/json
        ///    application/json; charset=UTF8
        ///    APPLICATION/JSON
        ///    application/vnd.company+json
        /// </summary>
        /// <param name="mime">MIME</param>
        /// <returns>Returns True if MIME type is json.</returns>
        public static bool IsJsonMime(string mime)
        {
            if (string.IsNullOrWhiteSpace(mime)) return false;

            return JsonRegex.IsMatch(mime) || mime.Equals("application/json-patch+json");
        }

        /// <summary>
        /// The base path of the API
        /// </summary>
        public const string BASE_ADDRESS = "http://localhost:3333/core";

        /// <summary>
        /// The scheme of the API
        /// </summary>
        public const string SCHEME = "http";

        /// <summary>
        /// The context path of the API
        /// </summary>
        public const string CONTEXT_PATH = "/core";

        /// <summary>
        /// The host of the API
        /// </summary>
        public const string HOST = "localhost";

        /// <summary>
        /// The format to use for DateTime serialization
        /// </summary>
        public const string ISO8601_DATETIME_FORMAT = "o";
    }
}
