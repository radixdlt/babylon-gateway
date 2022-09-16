using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.Commons.Addressing;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

internal class StateQuerier : IStateQuerier
{
    private readonly ReadOnlyDbContext _dbContext;

    public StateQuerier(ReadOnlyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TmpSomeResult> TmpAccountResourcesSnapshot(RadixAddress radixAddress, LedgerState ledgerState, CancellationToken token = default)
    {
        // TODO just some quick and naive implementation

        var account = await _dbContext.TmpEntities.FirstOrDefaultAsync(e => e.GlobalAddress == radixAddress.AddressData, token);

        if (account == null)
        {
            throw new Exception("zzz zzz zzz x1");
        }

        // // all fungible vaults
        // var allResources = _dbContext.TmpEntities.Where(e => e.)

        return new TmpSomeResult();
    }
}
