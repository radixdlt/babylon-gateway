using Common.Addressing;
using Common.Extensions;
using RadixCoreApi.GeneratedClient.Model;
using System.Diagnostics.CodeAnalysis;

namespace DataAggregator.GlobalServices;

public record Entity(
    EntityType EntityType,
    string? AccountAddress = null,
    string? ValidatorAddress = null, // This may be provided along with the AccountAddress for stake entities
    string? ResourceAddress = null,
    long? EpochUnlock = null
);

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The underscore identifies the split between identity and sub-entity")]
public enum EntityType
{
    System,
    Account,
    Account_PreparedStake,
    Account_PreparedUnstake,
    Account_ExitingStake,
    Validator,
    Validator_System,
    Resource,
}

public interface IEntityDeterminer
{
    Entity? DetermineEntity(EntityIdentifier entityIdentifier);

    bool IsXrd(string rri);
}

public class EntityDeterminer : IEntityDeterminer
{
    private static readonly byte[] _xrdRadixEngineAddress = { 1 };

    private readonly ILogger<EntityDeterminer> _logger;
    private readonly INetworkDetailsProvider _networkDetailsProvider;

    public EntityDeterminer(ILogger<EntityDeterminer> logger, INetworkDetailsProvider networkDetailsProvider)
    {
        _logger = logger;
        _networkDetailsProvider = networkDetailsProvider;
    }

    public Entity? DetermineEntity(EntityIdentifier entityIdentifier)
    {
        var primaryEntityAddress = entityIdentifier.Address;
        if (primaryEntityAddress == "system")
        {
            return new Entity(EntityType.System);
        }

        if (!RadixAddressParser.TryParse(
                _networkDetailsProvider.GetNetworkDetails().AddressHrps,
                primaryEntityAddress,
                out var primaryEntityRadixAddress,
                out var errorMessage
            ))
        {
            _logger.LogWarning(
                "Entity address [{Address}] didn't parse correctly: {ErrorMessage}",
                primaryEntityAddress,
                errorMessage
            );
            return null;
        }

        var subEntity = entityIdentifier.SubEntity;

        switch (primaryEntityRadixAddress.Type)
        {
            case RadixAddressType.Account when subEntity == null:
                return new Entity(EntityType.Account, AccountAddress: primaryEntityAddress);
            case RadixAddressType.Account when subEntity.Address == "prepared_stake":
                return new Entity(EntityType.Account_PreparedStake, AccountAddress: primaryEntityAddress, ValidatorAddress: subEntity.Metadata.Validator);
            case RadixAddressType.Account when subEntity.Address == "prepared_unstake":
                return new Entity(EntityType.Account_PreparedUnstake, AccountAddress: primaryEntityAddress, ValidatorAddress: subEntity.Metadata.Validator);
            case RadixAddressType.Account when subEntity.Address == "exiting_stake":
                return new Entity(EntityType.Account_ExitingStake, AccountAddress: primaryEntityAddress, ValidatorAddress: subEntity.Metadata.Validator, EpochUnlock: subEntity.Metadata.EpochUnlock);
            case RadixAddressType.Account:
                _logger.LogWarning("Unknown account sub-entity address: {SubEntityAddress}", subEntity.Address);
                return null;
            case RadixAddressType.Validator when subEntity == null:
                return new Entity(EntityType.Validator, ValidatorAddress: primaryEntityAddress);
            case RadixAddressType.Validator when subEntity.Address == "system":
                return new Entity(EntityType.Validator_System, ValidatorAddress: primaryEntityAddress);
            case RadixAddressType.Validator:
                _logger.LogWarning("Unknown validator sub-entity address: {SubEntityAddress}", subEntity.Address);
                return null;
            case RadixAddressType.Resource when subEntity == null:
                return new Entity(EntityType.Resource, ResourceAddress: primaryEntityAddress);
            case RadixAddressType.Resource:
                _logger.LogWarning("Unknown resource sub-entity address: {SubEntityAddress}", subEntity.Address);
                return null;
            case RadixAddressType.Node: // A Node address here should not be possible
            default:
                _logger.LogWarning("Unhandled radix address type: {RadixAddressType}", primaryEntityRadixAddress.Type);
                return null;
        }
    }

    public bool IsXrd(string rri)
    {
        // ReSharper disable once InvertIf - it's clearer to clear out failure cases first
        if (!RadixAddressParser.TryParse(
                _networkDetailsProvider.GetNetworkDetails().AddressHrps,
                rri,
                out var primaryEntityRadixAddress,
                out var errorMessage
            ))
        {
            _logger.LogWarning(
                "Presumed rri [{Address}] didn't parse correctly: {ErrorMessage}",
                rri,
                errorMessage
            );
            return false;
        }

        return
            primaryEntityRadixAddress.Type == RadixAddressType.Resource
            && primaryEntityRadixAddress.AddressData.BytesAreEqual(_xrdRadixEngineAddress);
    }
}
