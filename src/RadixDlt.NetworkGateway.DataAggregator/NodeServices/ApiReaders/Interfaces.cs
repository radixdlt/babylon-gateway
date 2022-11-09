using System;
using System.Threading;
using System.Threading.Tasks;
using CoreApi = RadixDlt.CoreApiSdk.Api;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;

public interface ICoreApiProvider
{
    CoreApi.TransactionApi TransactionsApi { get; }

    CoreApi.StatusApi StatusApi { get; }

    CoreApi.MempoolApi MempoolApi { get; }
}

public interface INetworkConfigurationReader
{
    Task<CoreModel.NetworkConfigurationResponse> GetNetworkConfiguration(CancellationToken token);
}

public interface INetworkStatusReader
{
    Task<CoreModel.NetworkStatusResponse> GetNetworkStatus(CancellationToken token);
}

public interface ITransactionStreamReader
{
    Task<CoreModel.CommittedTransactionsResponse> GetTransactionStream(long fromStateVersion, int count, CancellationToken token);
}

public interface INetworkConfigurationReaderObserver
{
    ValueTask GetNetworkConfigurationFailed(string nodeName, Exception exception);
}

public interface INetworkStatusReaderObserver
{
    ValueTask GetNetworkStatusFailed(string nodeName, Exception exception);
}

public interface ITransactionStreamReaderObserver
{
    ValueTask GetTransactionsFailed(string nodeName, Exception exception);
}
