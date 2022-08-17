using NodaTime;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public interface ILedgerStateQuerierObserver
{
    ValueTask LedgerRoundTimestampClockSkew(Duration difference);
}
