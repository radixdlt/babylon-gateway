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

using Newtonsoft.Json;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.StandardMetadata;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class StandardMetadataResolver
{
    private record PartiallyValidatedTwoWayLink(long FromStateVersion, StandardMetadataKey Discriminator, bool IsLocked, string EntityAddress, string TargetValue, string ValidationResult);

    private readonly ReadOnlyDbContext _dbContext;
    private readonly IDapperWrapper _dapperWrapper;

    public StandardMetadataResolver(ReadOnlyDbContext dbContext, IDapperWrapper dapperWrapper)
    {
        _dbContext = dbContext;
        _dapperWrapper = dapperWrapper;
    }

    public async Task<IDictionary<EntityAddress, ICollection<ResolvedTwoWayLink>>> ResolveTwoWayLinks(
        ICollection<Entity> entities,
        bool resolveValidOnly,
        bool validateOnLedgerOnly,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var entityIds = entities.Select(e => e.Id).ToList();

        if (!entityIds.Any())
        {
            return ImmutableDictionary<EntityAddress, ICollection<ResolvedTwoWayLink>>.Empty;
        }

        var partiallyValidatedEntries = await _dapperWrapper.ToList<PartiallyValidatedTwoWayLink>(
            _dbContext,
            @"
WITH
    variables (entity_id) AS (SELECT UNNEST(@entityIds)),
    aggregate_history AS (
        SELECT ah.*
        FROM variables var
        INNER JOIN LATERAL (
            SELECT *
            FROM unverified_standard_metadata_aggregate_history
            WHERE entity_id = var.entity_id AND from_state_version <= @stateVersion
            ORDER BY from_state_version DESC
            LIMIT 1
        ) ah ON TRUE
    ),
    entry_history_expanded AS (
        SELECT eh.from_state_version, eh.entity_id, eh.is_deleted, eh.is_locked, eh.discriminator, UNNEST(eh.entity_ids) AS target_entity_id, UNNEST(eh.values) AS target_value
        FROM aggregate_history ah
        INNER JOIN LATERAL UNNEST(ah.entry_ids) AS entry_id ON TRUE
        INNER JOIN unverified_standard_metadata_entry_history eh ON eh.id = entry_id
    ),
    candidates AS (
        SELECT
            entry.from_state_version,
            entry.is_locked,
            entry.discriminator,
            source_entity.id AS source_id,
            source_entity.address AS source_address,
            source_entity.discriminator AS source_discriminator,
            entry.target_value AS target_value,
            target_entity.id AS target_entity_id,
            target_entity.address AS target_entity_address,
            target_entity.discriminator AS target_entity_discriminator,
            dapp_marker_check.valid AS dapp_marker_check_valid,
            target_dapp_marker_check.valid AS target_dapp_marker_check_valid,
            target_entity_check.valid AS target_entity_check_valid
        FROM entry_history_expanded entry
        INNER JOIN entities source_entity ON source_entity.id = entry.entity_id
        LEFT JOIN entities target_entity ON target_entity.id = entry.target_entity_id
        LEFT JOIN LATERAL (
            SELECT is_deleted = FALSE AND @dappAccountTypeDappDefinition = ANY(values) AS valid
            FROM unverified_standard_metadata_entry_history
            WHERE
                entity_id = source_entity.id
              AND discriminator = 'dapp_account_type'
              AND from_state_version <= @stateVersion
            ORDER BY from_state_version DESC
            LIMIT 1
        ) dapp_marker_check ON source_entity.discriminator = 'global_account_component'
        LEFT JOIN LATERAL (
            SELECT is_deleted = FALSE AND @dappAccountTypeDappDefinition = ANY(values) AS valid
            FROM unverified_standard_metadata_entry_history
            WHERE
                entity_id = entry.target_entity_id
              AND discriminator = 'dapp_account_type'
              AND from_state_version <= @stateVersion
            ORDER BY from_state_version DESC
            LIMIT 1
        ) target_dapp_marker_check ON entry.target_entity_id IS NOT NULL
        LEFT JOIN LATERAL (
            SELECT is_deleted = FALSE AND source_entity.id = ANY(entity_ids) AS valid
            FROM unverified_standard_metadata_entry_history
            WHERE
                entity_id = entry.target_entity_id
              AND discriminator = CASE
                -- dApp account
                WHEN entry.discriminator = 'dapp_claimed_entities' AND source_entity.discriminator = 'global_account_component' THEN
                  CASE
                    WHEN target_entity.discriminator = 'global_fungible_resource' OR target_entity.discriminator = 'global_non_fungible_resource' THEN
                        'dapp_definitions'::standard_metadata_key
                    ELSE
                        'dapp_definition'::standard_metadata_key
                    END
                WHEN entry.discriminator = 'dapp_definitions' AND source_entity.discriminator = 'global_account_component' THEN
                  'dapp_definitions'::standard_metadata_key
                WHEN entry.discriminator = 'dapp_account_locker' AND source_entity.discriminator = 'global_account_component' THEN
                  'dapp_definition'::standard_metadata_key
                -- dApp resources
                WHEN entry.discriminator = 'dapp_definitions' AND (source_entity.discriminator = 'global_fungible_resource' OR source_entity.discriminator = 'global_non_fungible_resource') THEN
                    'dapp_claimed_entities'::standard_metadata_key
                -- other dApp components
                WHEN entry.discriminator = 'dapp_definition' THEN
                    'dapp_claimed_entities'::standard_metadata_key
                END
              AND from_state_version <= @stateVersion
            ORDER BY from_state_version DESC
            LIMIT 1
        ) target_entity_check ON entry.target_entity_id IS NOT NULL
        WHERE entry.is_deleted = FALSE
    ),
    resolved AS (
        SELECT
            -- return values:
            -- 'unknown' - unexpected, something went terribly wrong,
            -- 'on-ok' - on-ledger, successfully validated,
            -- 'on-app-check' - on-ledger, partially validated,
            -- 'off-app-check' - off-ledger, partially validated,
            -- <non-null string value> - validation failure
            coalesce(CASE c.discriminator
                WHEN 'dapp_account_type' THEN
                    CASE FALSE
                        WHEN source_discriminator = 'global_account_component' THEN 'account expected'
                        WHEN dapp_marker_check_valid = TRUE THEN 'dapp marker invalid'
                        ELSE @validationOnLedgerSucceeded
                    END
                WHEN 'dapp_definition' THEN
                    CASE FALSE
                        WHEN source_discriminator != 'global_fungible_resource' THEN 'invalid on fungible resource'
                        WHEN source_discriminator != 'global_non_fungible_resource' THEN 'invalid on non-fungible resource'
                        WHEN target_entity_discriminator = 'global_account_component' THEN 'target account expected'
                        WHEN target_dapp_marker_check_valid = TRUE THEN 'target dapp marker invalid'
                        WHEN target_entity_check_valid = TRUE THEN 'target link broken'
                        ELSE @validationOnLedgerSucceeded
                    END
                WHEN 'dapp_definitions' THEN
                    CASE source_discriminator
                        WHEN 'global_account_component' THEN
                            CASE FALSE
                                WHEN dapp_marker_check_valid = TRUE THEN 'dapp marker invalid'
                                WHEN target_entity_discriminator = 'global_account_component' THEN 'target account expected'
                                WHEN target_dapp_marker_check_valid = TRUE THEN 'target dapp marker invalid'
                                WHEN target_entity_check_valid = TRUE THEN 'target link broken'
                                ELSE @validationOnLedgerSucceeded
                            END
                        WHEN 'global_fungible_resource' THEN
                            CASE FALSE
                                WHEN target_entity_discriminator = 'global_account_component' THEN 'target account expected'
                                WHEN target_dapp_marker_check_valid = TRUE THEN 'target dapp marker invalid'
                                WHEN target_entity_check_valid = TRUE THEN 'target link broken'
                                ELSE @validationOnLedgerSucceeded
                            END
                        WHEN 'global_non_fungible_resource' THEN
                            CASE FALSE
                                WHEN target_entity_discriminator = 'global_account_component' THEN 'target account expected'
                                WHEN target_dapp_marker_check_valid = TRUE THEN 'target dapp marker invalid'
                                WHEN target_entity_check_valid = TRUE THEN 'target link broken'
                                ELSE @validationOnLedgerSucceeded
                            END
                        ELSE 'account or resource expected'
                    END
                WHEN 'dapp_claimed_entities' THEN
                    CASE FALSE
                        WHEN source_discriminator = 'global_account_component' THEN 'account expected'
                        WHEN dapp_marker_check_valid = TRUE THEN 'dapp marker invalid'
                        WHEN target_entity_check_valid = TRUE THEN 'target link broken'
                        ELSE @validationOnLedgerSucceeded
                    END
                WHEN 'dapp_account_locker' THEN
                    CASE FALSE
                        WHEN source_discriminator = 'global_account_component' THEN 'account expected'
                        WHEN dapp_marker_check_valid = TRUE THEN 'dapp marker invalid'
                        WHEN target_entity_check_valid = TRUE THEN 'target link broken'
                        WHEN target_entity_discriminator = 'global_account_locker' THEN 'target locker expected'
                        ELSE @validationOnLedgerAppCheck
                    END
                WHEN 'dapp_claimed_websites' THEN
                    @validationOffLedgerAppCheck
                END, @validationUnknown) AS validation_result,
            c.*
        FROM candidates c
    )
SELECT
    from_state_version AS FromStateVersion,
    discriminator AS Discriminator,
    is_locked AS IsLocked,
    source_address AS EntityAddress,
    coalesce(target_value, target_entity_address) AS TargetValue,
    validation_result AS ValidationResult
FROM resolved",
            new
            {
                dappAccountTypeDappDefinition = StandardMetadataConstants.DappAccountTypeDappDefinition,
                validationUnknown = StandardMetadataConstants.ValidationUnknown,
                validationOnLedgerSucceeded = StandardMetadataConstants.ValidationOnLedgerSucceeded,
                validationOnLedgerAppCheck = StandardMetadataConstants.ValidationOnLedgerAppCheck,
                validationOffLedgerAppCheck = StandardMetadataConstants.ValidationOffLedgerAppCheck,
                stateVersion = ledgerState.StateVersion,
                entityIds = entityIds,
            },
            token);

        var result = new ConcurrentDictionary<EntityAddress, ConcurrentQueue<ResolvedTwoWayLink>>();
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 4,
            CancellationToken = token,
        };

        await Parallel.ForEachAsync(partiallyValidatedEntries, options, async (pv, innerToken) =>
        {
            var resolved = await ResolveTwoWayLink(pv, validateOnLedgerOnly, partiallyValidatedEntries, innerToken);

            if (resolved == null)
            {
                return;
            }

            if (resolveValidOnly && !resolved.IsValid)
            {
                return;
            }

            result.GetOrAdd((EntityAddress)pv.EntityAddress, _ => new ConcurrentQueue<ResolvedTwoWayLink>()).Enqueue(resolved);
        });

        return result.ToDictionary(e => e.Key, e => (ICollection<ResolvedTwoWayLink>)e.Value.ToList());
    }

    private async ValueTask<ResolvedTwoWayLink?> ResolveTwoWayLink(PartiallyValidatedTwoWayLink entry, bool validateOnLedgerOnly, ICollection<PartiallyValidatedTwoWayLink> allEntries, CancellationToken token)
    {
        if (entry.ValidationResult == StandardMetadataConstants.ValidationUnknown)
        {
            throw CreateException(entry, "unknown validation result");
        }

        if (entry.Discriminator == StandardMetadataKey.DappClaimedWebsites)
        {
            if (entry.ValidationResult == StandardMetadataConstants.ValidationOffLedgerAppCheck)
            {
                return await ResolveDappClaimedWebsite((EntityAddress)entry.EntityAddress, entry.TargetValue, validateOnLedgerOnly, token);
            }

            if (!Uri.TryCreate(entry.TargetValue, UriKind.Absolute, out var uri))
            {
                uri = new Uri("http://example.com");
            }

            return new DappClaimedWebsiteResolvedTwoWayLink(uri, "expected off-ledger app-check validation result, got: " + entry.ValidationResult);
        }

        if (entry.Discriminator == StandardMetadataKey.DappAccountLocker)
        {
            if (entry.ValidationResult == StandardMetadataConstants.ValidationOnLedgerAppCheck)
            {
                return ResolveDappAccountLocker((EntityAddress)entry.EntityAddress, (EntityAddress)entry.TargetValue, allEntries);
            }

            return new DappAccountLockerResolvedTwoWayLink((EntityAddress)entry.TargetValue, "expected on-ledger app-check validation result, got: " + entry.ValidationResult);
        }

        var invalidReason = entry.ValidationResult == StandardMetadataConstants.ValidationOnLedgerSucceeded ? null : entry.ValidationResult;

        return entry.Discriminator switch
        {
            StandardMetadataKey.DappAccountType => null,
            StandardMetadataKey.DappDefinition => new DappDefinitionResolvedTwoWayLink((EntityAddress)entry.TargetValue, invalidReason),
            StandardMetadataKey.DappDefinitions => new DappDefinitionsResolvedTwoWayLink((EntityAddress)entry.TargetValue, invalidReason),
            StandardMetadataKey.DappClaimedEntities => new DappClaimedEntityResolvedTwoWayLink((EntityAddress)entry.TargetValue, invalidReason),
            _ => throw CreateException(entry, "unsupported entry discriminator"),
        };
    }

    private async ValueTask<DappClaimedWebsiteResolvedTwoWayLink> ResolveDappClaimedWebsite(EntityAddress entityAddress, string claimedWebsite, bool validateOnLedgerOnly, CancellationToken token)
    {
        async Task<string?> Validate()
        {
            if (validateOnLedgerOnly)
            {
                return "off-ledger validation disabled";
            }

            if (token.IsCancellationRequested)
            {
                return "validation aborted";
            }

            if (!Uri.TryCreate(claimedWebsite, UriKind.Absolute, out var origin))
            {
                return "invalid origin URI: " + claimedWebsite;
            }

            if (!Uri.TryCreate(origin, StandardMetadataConstants.RadixWellKnownPath, out var radixJsonUrl))
            {
                return "unable to construct radix JSON URI for origin: " + origin;
            }

            await Task.CompletedTask;

            throw new NotImplementedException($"unsupported; URL = {radixJsonUrl}, entity_addr = {entityAddress}");
        }

        var invalidReason = await Validate();

        return new DappClaimedWebsiteResolvedTwoWayLink(new Uri(claimedWebsite), invalidReason);
    }

    private DappAccountLockerResolvedTwoWayLink ResolveDappAccountLocker(EntityAddress entityAddress, EntityAddress lockerAddress, ICollection<PartiallyValidatedTwoWayLink> allEntries)
    {
        var valid = allEntries.Any(x => x.EntityAddress == entityAddress && x.Discriminator == StandardMetadataKey.DappClaimedEntities && x.TargetValue == lockerAddress);

        return new DappAccountLockerResolvedTwoWayLink(lockerAddress, valid ? null : "claimed_entities entry with the locker address missing");
    }

    private Exception CreateException(PartiallyValidatedTwoWayLink entry, string details)
    {
        var dump = JsonConvert.SerializeObject(entry);

        return new InvalidOperationException($"Unable to resolve standard metadata. Details: {details}. Entry dump: {dump}");
    }
}
