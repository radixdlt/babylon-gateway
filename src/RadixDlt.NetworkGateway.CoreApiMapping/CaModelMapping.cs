using Riok.Mapperly.Abstractions;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.CoreApiMapping;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public static partial class CaModelMapping
{
    [MapDerivedType<GatewayModel.CoreApiEncryptedTransactionMessage, CoreModel.EncryptedTransactionMessage>]
    [MapDerivedType<GatewayModel.CoreApiPlaintextTransactionMessage, CoreModel.PlaintextTransactionMessage>]
    public static partial CoreModel.TransactionMessage? ToCoreModel(this GatewayModel.CoreApiTransactionMessage? source);
}
