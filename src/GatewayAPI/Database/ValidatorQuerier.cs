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

using Common.Database;
using Common.Database.Models.Ledger.History;
using Common.Database.Models.Ledger.Substates;
using GatewayAPI.ApiSurface;
using GatewayAPI.Exceptions;
using GatewayAPI.Services;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization;
using Db = Common.Database.Models.Ledger.Normalization;
using Gateway = RadixGatewayApi.Generated.Model;
using TokenAmount = Common.Numerics.TokenAmount;

namespace GatewayAPI.Database;

public interface IValidatorQuerier
{
    Task<Gateway.Validator> GetValidatorAtState(ValidatedValidatorAddress validatorAddress, Gateway.LedgerState ledgerState);

    Task<List<Gateway.Validator>> GetValidatorsAtState(Gateway.LedgerState ledgerState);

    Task<ValidatorStakesPage> GetValidatorStakesPage(ValidatorStakesPageRequest request, Gateway.LedgerState ledgerState);
}

[DataContract]
public record ValidatorStakesPaginationCursor(long? FixedStateVersion, long? NextPageStartsWithAccountId)
{
    [DataMember(EmitDefaultValue = false, Name = "v")]
    public long? FixedStateVersion { get; set; } = FixedStateVersion;

    [DataMember(EmitDefaultValue = false, Name = "a")]
    public long? NextPageStartsWithAccountId { get; set; } = NextPageStartsWithAccountId;

    public static ValidatorStakesPaginationCursor FromCursorString(string? cursorString)
    {
        return Serializations.FromBase64JsonOrDefault<ValidatorStakesPaginationCursor>(cursorString)
            ?? new ValidatorStakesPaginationCursor(null, null);
    }

    public string ToCursorString()
    {
        return Serializations.AsBase64Json(this);
    }

    public Gateway.PartialLedgerStateIdentifier? GetPartialStateIdentifier()
    {
        return FixedStateVersion.HasValue
            ? new Gateway.PartialLedgerStateIdentifier(version: FixedStateVersion.Value)
            : null;
    }

    public void AssertLedgerStateIsConsistent(Gateway.LedgerState ledgerState)
    {
        if (FixedStateVersion.HasValue && FixedStateVersion.Value != ledgerState._Version)
        {
            throw InvalidRequestException.FromOtherError("The provided partial state identifier can't change when making requests for later pages. You may leave the partial state identifier off requests for later pages, as it's captured in the cursor.");
        }
    }
}

public record ValidatorStakesPage(
    long TotalRecords,
    ValidatorStakesPaginationCursor? NextPageCursor,
    List<Gateway.ValidatorAccountStake> ValidatorAccountStakes
);

public record ValidatorStakesPageRequest(
    ValidatedValidatorAddress ValidatorAddress,
    ValidatorStakesPaginationCursor? Cursor,
    int PageSize
);

public class ValidatorQuerier : IValidatorQuerier
{
    private const int UptimeDefaultEpochRange = 500; // 500 Epochs is approx 2 weeks

    private readonly GatewayReadOnlyDbContext _dbContext;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;

    public ValidatorQuerier(GatewayReadOnlyDbContext dbContext, INetworkConfigurationProvider networkConfigurationProvider)
    {
        _dbContext = dbContext;
        _networkConfigurationProvider = networkConfigurationProvider;
    }

    public async Task<Gateway.Validator> GetValidatorAtState(ValidatedValidatorAddress validatorAddress, Gateway.LedgerState ledgerState)
    {
        var validator = await GetDbValidatorAtState(validatorAddress.Address, ledgerState);

        if (validator == null)
        {
            return new Gateway.Validator(
                validatorAddress.Address.AsGatewayValidatorIdentifier(),
                TokenAmount.Zero.AsGatewayTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier()),
                new Gateway.ValidatorInfo(
                    TokenAmount.Zero.AsGatewayTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier()),
                    GetDefaultValidatorUptime(ledgerState)
                ),
                GetDefaultValidatorProperties(validatorAddress.ByteValidatorAddress.CompressedPublicKey)
            );
        }

        return (await CreateApiValidatorsAtState(new List<Db.Validator> { validator }, ledgerState)).Single();
    }

    public async Task<List<Gateway.Validator>> GetValidatorsAtState(Gateway.LedgerState ledgerState)
    {
        var validators = await GetDbValidatorsAtState(ledgerState);

        return await CreateApiValidatorsAtState(validators, ledgerState);
    }

    public async Task<ValidatorStakesPage> GetValidatorStakesPage(ValidatorStakesPageRequest request, Gateway.LedgerState ledgerState)
    {
        var validator = await GetDbValidatorAtState(request.ValidatorAddress.Address, ledgerState);
        if (validator == null)
        {
            return new ValidatorStakesPage(0, null, new List<Gateway.ValidatorAccountStake>());
        }

        var validatorId = validator.Id;
        var validatorStakeSnapshot = (await GetValidatorStakes(new List<long> { validatorId }, ledgerState))[validatorId];

        var totalCount = await CountValidatorAccountStakes(validator.Id, ledgerState);
        var pagePlusOne = await GetValidatorAccountStakesPagePlusOne(validator, request, ledgerState);

        var nextCursor = pagePlusOne.Count == request.PageSize + 1
            ? new ValidatorStakesPaginationCursor(ledgerState._Version, pagePlusOne.Last().AccountId)
            : null;

        var pageContents = pagePlusOne
            .Take(request.PageSize)
            .Select(h => new Gateway.ValidatorAccountStake(
                account: h.Account.Address.AsGatewayAccountIdentifier(),
                validator: validator.AsGatewayValidatorIdentifier(),
                totalPendingStake: h.StakeSnapshot.TotalPreparedXrdStake
                    .AsGatewayTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier()),
                totalStake: validatorStakeSnapshot.EstimateXrdConversion(h.StakeSnapshot.TotalStakeUnits)
                    .AsGatewayTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier()),
                totalPendingUnstake: validatorStakeSnapshot.EstimateXrdConversion(h.StakeSnapshot.TotalPreparedUnStakeUnits)
                    .AsGatewayTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier()),
                totalUnstaking: h.StakeSnapshot.TotalExitingXrdStake
                    .AsGatewayTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier())
            ))
            .ToList();

        return new ValidatorStakesPage(totalCount, nextCursor, pageContents);
    }

    private async Task<long> CountValidatorAccountStakes(long validatorId, Gateway.LedgerState ledgerState)
    {
        return await _dbContext.NonZeroAccountValidatorStakeHistoryForValidatorIdAtVersion(validatorId, ledgerState._Version)
            .CountAsync();
    }

    private async Task<List<AccountValidatorStakeHistory>> GetValidatorAccountStakesPagePlusOne(
        Db.Validator validator,
        ValidatorStakesPageRequest request,
        Gateway.LedgerState ledgerState
    )
    {
        var accountIdMinBound = request.Cursor?.NextPageStartsWithAccountId ?? 0;

        return await _dbContext.NonZeroAccountValidatorStakeHistoryForValidatorIdAtVersion(validator.Id, ledgerState._Version)
            .Where(h => h.AccountId >= accountIdMinBound)
            .Include(h => h.Account)
            .OrderBy(h => h.AccountId)
            .Take(request.PageSize + 1)
            .ToListAsync();
    }

    private async Task<List<Gateway.Validator>> CreateApiValidatorsAtState(List<Db.Validator> validators, Gateway.LedgerState ledgerState)
    {
        var validatorIds = validators.Select(v => v.Id).ToList();

        var validatorStakeSnapshots = await GetValidatorStakes(validatorIds, ledgerState);
        var validatorProperties = await GetValidatorPropertiesByValidatorIdAtState(validators, ledgerState);
        var validatorUptimes = await GetUptimeByValidatorIdAtState(validatorIds, ledgerState);

        var validatorAndOwnerIds = validatorIds
            .Where(id => validatorProperties[id].OwnerId.HasValue)
            .Select(id => new DbQueryExtensions.AccountValidatorIds(validatorProperties[id].OwnerId!.Value, id))
            .ToList();

        var validatorOwnerStakeSnapshots = await GetOwnerStakesByValidatorIdAtState(validatorAndOwnerIds, ledgerState);

        return validators.Select(validator =>
        {
            var validatorTotalStake = validatorStakeSnapshots.GetValueOrDefault(validator.Id) ??
                                      ValidatorStakeSnapshot.GetDefault();
            var validatorOwnerStake = validatorOwnerStakeSnapshots.GetValueOrDefault(validator.Id) ??
                                      AccountValidatorStakeSnapshot.GetDefault();
            var validatorUptime = validatorUptimes.GetValueOrDefault(validator.Id) ?? GetDefaultValidatorUptime(ledgerState);
            var properties = validatorProperties.GetValueOrDefault(validator.Id)?.Properties ??
                             GetDefaultValidatorProperties(validator.PublicKey);

            var validatorOwnerStakeXrd = validatorTotalStake.EstimateXrdConversion(validatorOwnerStake.TotalStakeUnits);

            return (validator, validatorTotalStake, validatorOwnerStakeXrd, validatorUptime, properties);
        })
        .OrderByDescending(x => x.validatorTotalStake.TotalXrdStake)
        .ThenBy(x => x.validator.Id) // Oldest validators first if they have equal stake
        .Select(x =>
        {
            var (validator, validatorTotalStake, validatorOwnerStakeXrd, validatorUptime, properties) = x;

            return new Gateway.Validator(
                validator.Address.AsGatewayValidatorIdentifier(),
                validatorTotalStake.TotalXrdStake.AsGatewayTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier()),
                new Gateway.ValidatorInfo(
                    validatorOwnerStakeXrd.AsGatewayTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier()),
                    validatorUptime
                ),
                properties
            );
        })
        .ToList();
    }

    private async Task<Db.Validator?> GetDbValidatorAtState(string validatorAddress, Gateway.LedgerState ledgerState)
    {
        var stateVersion = ledgerState._Version;
        return await _dbContext.Validator(validatorAddress, stateVersion).SingleOrDefaultAsync();
    }

    private async Task<List<Db.Validator>> GetDbValidatorsAtState(Gateway.LedgerState ledgerState)
    {
        var stateVersion = ledgerState._Version;
        return await _dbContext.Validators
            .Where(v => v.FromStateVersion <= stateVersion)
            .ToListAsync();
    }

    private async Task<Dictionary<long, ValidatorStakeSnapshot>> GetValidatorStakes(List<long> validatorIds, Gateway.LedgerState ledgerState)
    {
        var stateVersion = ledgerState._Version;

        return await _dbContext.ValidatorStakeHistoryAtVersionForValidatorIds(validatorIds, stateVersion)
            .ToDictionaryAsync(
                v => v.ValidatorId,
                v => v.StakeSnapshot
            );
    }

    private record PropertiesAndOwner(Gateway.ValidatorProperties Properties, long? OwnerId);

    private async Task<Dictionary<long, PropertiesAndOwner>> GetValidatorPropertiesByValidatorIdAtState(List<Db.Validator> validators, Gateway.LedgerState ledgerState)
    {
        var validatorIds = validators.Select(v => v.Id).ToList();
        var validatorsById = validators.ToDictionary(v => v.Id);
        var stateVersion = ledgerState._Version;

        var validatorDataSubstatesByValidatorId = (await (
                from data in _dbContext.ValidatorDataSubstates.UpAtVersion(stateVersion)
                where validatorIds.Contains(data.ValidatorId)
                    && (
                        data.EffectiveEpoch == null // Active substates
                        || data.EffectiveEpoch > 0 // Non-default prepared substates
                    )
                orderby data.UpStateVersion descending
                group data by new { data.ValidatorId, data.Type }
                into g
                select new
                    {
                        ValidatorId = g.Key.ValidatorId,
                        Type = g.Key.Type,
                        Data = g.First(),
                        Count = g.Count(),
                    }
            )
            .ToListAsync())
            .GroupBy(v => v.ValidatorId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(x => x.Type, x => x)
            );

        var validatorOwnerIds = validatorDataSubstatesByValidatorId
            .Where(v => v.Value.ContainsKey(ValidatorDataSubstateType.ValidatorData))
            .Select(v => v.Value[ValidatorDataSubstateType.ValidatorData].Data.ValidatorData!.OwnerId)
            .ToList();

        var validatorPreparedOwnerIds = validatorDataSubstatesByValidatorId
            .Where(v => v.Value.ContainsKey(ValidatorDataSubstateType.PreparedValidatorOwner))
            .Select(v => v.Value[ValidatorDataSubstateType.PreparedValidatorOwner].Data.PreparedValidatorOwner!.PreparedOwnerId)
            .ToList();

        var validatorOwnerAndPreparedOwnerAddresses = await _dbContext.Accounts
            .Where(a => validatorOwnerIds.Contains(a.Id) || validatorPreparedOwnerIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Address);

        var validatorSelfDefaultOwnerAddresses = validatorIds
            .Where(validatorId =>
            {
                var dataSubstates = validatorDataSubstatesByValidatorId.GetValueOrDefault(validatorId);
                return dataSubstates == null || !dataSubstates.ContainsKey(ValidatorDataSubstateType.ValidatorData);
            })
            .Select(validatorId => ValidatorData.GetDefaultOutputData(
                _networkConfigurationProvider.GetAddressHrps(),
                validatorsById[validatorId].PublicKey).OwnerAddress
            )
            .ToList();

        var validatorSelfOwners = await _dbContext.Accounts
            .Where(a => validatorSelfDefaultOwnerAddresses.Contains(a.Address))
            .ToDictionaryAsync(a => a.Address, a => a.Id);

        var outDictionary = new Dictionary<long, PropertiesAndOwner>();

        foreach (var validatorId in validatorIds)
        {
            var validatorDataSubstates = validatorDataSubstatesByValidatorId.GetValueOrDefault(validatorId);
            var validatorData = validatorDataSubstates?.GetValueOrDefault(ValidatorDataSubstateType.ValidatorData)?.Data.ValidatorData;
            var validatorOutputData = validatorData?.ToOutputData(ownerId => validatorOwnerAndPreparedOwnerAddresses[ownerId])
                                      ?? ValidatorData.GetDefaultOutputData(_networkConfigurationProvider.GetAddressHrps(), validatorsById[validatorId].PublicKey);
            var validatorMetadata = validatorDataSubstates?.GetValueOrDefault(ValidatorDataSubstateType.ValidatorMetaData)?.Data.ValidatorMetaData
                                    ?? ValidatorMetadata.GetDefault();
            var validatorAllowDelegation = validatorDataSubstates?.GetValueOrDefault(ValidatorDataSubstateType.ValidatorAllowDelegation)?.Data.ValidatorAllowDelegation
                                           ?? ValidatorAllowDelegation.GetDefault();

            var preparedOwner = PreparedValidatorOwner.GetIfActive(
                validatorDataSubstates?.GetValueOrDefault(ValidatorDataSubstateType.PreparedValidatorOwner)?.Data,
                ownerId => validatorOwnerAndPreparedOwnerAddresses[ownerId]
            );
            var preparedFee = PreparedValidatorFee.GetIfActive(
                validatorDataSubstates?.GetValueOrDefault(ValidatorDataSubstateType.PreparedValidatorFee)?.Data
            );
            var preparedRegistered = PreparedValidatorRegistered.GetIfActive(
                validatorDataSubstates?.GetValueOrDefault(ValidatorDataSubstateType.PreparedValidatorRegistered)?.Data
            );

            var validatorProperties = GetValidatorPropertiesFromStates(
                validatorOutputData,
                validatorMetadata,
                validatorAllowDelegation,
                preparedOwner,
                preparedFee,
                preparedRegistered
            );

            var ownerId = validatorData?.OwnerId;

            if (ownerId == null)
            {
                // In this case, the ValidatorData is missing - so the validator is owned by itself.
                // Let's try to look up its Account Id - if it's been seen on ledger as an account.
                var validatorSelfOwnerAddress = validatorOutputData.OwnerAddress;
                if (validatorSelfOwners.ContainsKey(validatorSelfOwnerAddress))
                {
                    ownerId = validatorSelfOwners[validatorSelfOwnerAddress];
                }
            }

            outDictionary.Add(validatorId, new PropertiesAndOwner(validatorProperties, ownerId));
        }

        return outDictionary;
    }

    private Gateway.ValidatorProperties GetDefaultValidatorProperties(byte[] validatorPublicKey)
    {
        return GetValidatorPropertiesFromStates(
            ValidatorData.GetDefaultOutputData(_networkConfigurationProvider.GetAddressHrps(), validatorPublicKey),
            ValidatorMetadata.GetDefault(),
            ValidatorAllowDelegation.GetDefault()
        );
    }

    private Gateway.ValidatorProperties GetValidatorPropertiesFromStates(
        OutputValidatorData validatorOutputData,
        ValidatorMetadata validatorMetadata,
        ValidatorAllowDelegation validatorAllowDelegation,
        OutputPreparedValidatorOwner? preparedOwner = null,
        OutputPreparedValidatorFee? preparedFee = null,
        OutputPreparedValidatorRegistered? preparedRegistered = null
    )
    {
        // TODO NG-100 - revert the overrides once NG-97 is implemented and used by wallets
        return new Gateway.ValidatorProperties(
            url: validatorMetadata.Url,
            validatorFeePercentage: preparedFee?.FeePercentage ?? validatorOutputData.FeePercentage,
            name: validatorMetadata.Name,
            registered: preparedRegistered?.IsRegistered ?? validatorOutputData.IsRegistered,
            ownerAccountIdentifier: (preparedOwner?.OwnerAddress ?? validatorOutputData.OwnerAddress).AsGatewayAccountIdentifier(),
            externalStakeAccepted: validatorAllowDelegation.AllowDelegation
        );
    }

    private async Task<Dictionary<long, AccountValidatorStakeSnapshot>> GetOwnerStakesByValidatorIdAtState(
        List<DbQueryExtensions.AccountValidatorIds> validatorOwnerIds, Gateway.LedgerState ledgerState
    )
    {
        var stateVersion = ledgerState._Version;

        return await _dbContext.BulkAccountValidatorStakeHistoryAtVersion(validatorOwnerIds, stateVersion)
                .ToDictionaryAsync(
                    a => a.ValidatorId,
                    a => a.StakeSnapshot
                );
    }

    /// <summary>
    /// Because of how we store the uptime, we can't be specific to a give state version, but we can be specific
    /// to a given epoch.
    /// In particular, if the ledger state points into the current epoch, the uptime will change as the epoch progresses.
    /// </summary>
    private async Task<Dictionary<long, Gateway.ValidatorUptime>> GetUptimeByValidatorIdAtState(List<long> validatorIds, Gateway.LedgerState ledgerState)
    {
        // These are inclusive endpoints
        var fromEpoch = Math.Max(1, ledgerState.Epoch - UptimeDefaultEpochRange);
        var toEpoch = ledgerState.Epoch;

        var proposalCountsQuery =
            from proposalHistory in _dbContext.ValidatorProposalRecords
            where validatorIds.Contains(proposalHistory.ValidatorId)
                  && fromEpoch <= proposalHistory.Epoch
                  && proposalHistory.Epoch <= toEpoch
            group proposalHistory.ProposalRecord by proposalHistory.ValidatorId
            into g
            select new
            {
                ValidatorId = g.Key,
                ProposalsCompleted = g.Sum(p => p.ProposalsCompleted),
                ProposalsMissed = g.Sum(p => p.ProposalsMissed),
            }
        ;

        var proposalCounts = await proposalCountsQuery.ToDictionaryAsync(
            g => g.ValidatorId,
            g => g
        );

        var outDictionary = new Dictionary<long, Gateway.ValidatorUptime>();

        foreach (var (validatorId, results) in proposalCounts)
        {
            var proposalsCompleted = results.ProposalsCompleted;
            var proposalsMissed = results.ProposalsMissed;

            var proportion = 100 * (decimal)proposalsCompleted / Math.Max(1, proposalsCompleted + proposalsMissed);

            var uptime = new Gateway.ValidatorUptime(
                new Gateway.EpochRange(fromEpoch, toEpoch),
                Math.Round(proportion, 2),
                proposalsMissed: proposalsMissed,
                proposalsCompleted: proposalsCompleted
            );
            outDictionary.Add(validatorId, uptime);
        }

        return outDictionary;
    }

    private Gateway.ValidatorUptime GetDefaultValidatorUptime(Gateway.LedgerState ledgerState)
    {
        var fromEpoch = Math.Max(1, ledgerState.Epoch - UptimeDefaultEpochRange);
        var toEpoch = ledgerState.Epoch;

        return new Gateway.ValidatorUptime(
            new Gateway.EpochRange(fromEpoch, toEpoch),
            uptimePercentage: 0,
            proposalsMissed: 0,
            proposalsCompleted: 0
        );
    }
}
