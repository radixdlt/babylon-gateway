using System;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public interface ISqlQueryObserver
{
    void OnSqlQueryExecuted(string queryName, TimeSpan duration);
}
