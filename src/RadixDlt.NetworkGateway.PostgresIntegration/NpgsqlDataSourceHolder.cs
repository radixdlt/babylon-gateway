using Npgsql;
using System;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

// ReSharper disable once UnusedTypeParameter
internal class NpgsqlDataSourceHolder<T> : IAsyncDisposable
{
    public NpgsqlDataSourceHolder(NpgsqlDataSource npgsqlDataSource)
    {
        NpgsqlDataSource = npgsqlDataSource;
    }

    public NpgsqlDataSource NpgsqlDataSource { get; }

    public ValueTask DisposeAsync()
    {
        return NpgsqlDataSource.DisposeAsync();
    }
}
