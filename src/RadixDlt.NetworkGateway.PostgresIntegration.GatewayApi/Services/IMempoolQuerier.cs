using RadixDlt.NetworkGateway.Common.Database.Models.Mempool;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public interface IMempoolQuerier
{
    Task<MempoolTransaction?> GetMempoolTransaction(byte[] transactionIdentifierHash);
}
