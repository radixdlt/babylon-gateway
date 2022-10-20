using RadixDlt.CoreApiSdk.Api;
using RadixDlt.CoreApiSdk.Client;
using RadixDlt.CoreApiSdk.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

public class MempoolApiStub : IDisposable, IMempoolApi
{
    private readonly MempoolListResponse _mempoolListResponse;

    private MempoolApi _mempoolApi = new();

    public MempoolApiStub(MempoolListResponse mempoolListResponse)
    {
        _mempoolListResponse = mempoolListResponse;
    }

    public void Dispose()
    {
        _mempoolApi.Dispose();
    }

    public IReadableConfiguration Configuration
    {
        get
        {
            return _mempoolApi.Configuration;
        }

        set
        {
            _mempoolApi.Configuration = value;
        }
    }

    public string GetBasePath()
    {
        return _mempoolApi.GetBasePath();
    }

    public ExceptionFactory ExceptionFactory
    {
        get
        {
            return _mempoolApi.ExceptionFactory;
        }

        set
        {
            _mempoolApi.ExceptionFactory = value;
        }
    }

    public MempoolListResponse MempoolListPost(MempoolListRequest mempoolListRequest)
    {
        return _mempoolListResponse;
    }

    public ApiResponse<MempoolListResponse> MempoolListPostWithHttpInfo(MempoolListRequest mempoolListRequest)
    {
        throw new NotImplementedException();
    }

    public MempoolTransactionResponse MempoolTransactionPost(MempoolTransactionRequest mempoolTransactionRequest)
    {
        throw new NotImplementedException();
    }

    public ApiResponse<MempoolTransactionResponse> MempoolTransactionPostWithHttpInfo(MempoolTransactionRequest mempoolTransactionRequest)
    {
        throw new NotImplementedException();
    }

    public Task<MempoolListResponse> MempoolListPostAsync(MempoolListRequest mempoolListRequest, CancellationToken cancellationToken = default(CancellationToken))
    {
        return Task.FromResult(_mempoolListResponse);
    }

    public Task<ApiResponse<MempoolListResponse>> MempoolListPostWithHttpInfoAsync(MempoolListRequest mempoolListRequest, CancellationToken cancellationToken = default(CancellationToken))
    {
        throw new NotImplementedException();
    }

    public Task<MempoolTransactionResponse> MempoolTransactionPostAsync(MempoolTransactionRequest mempoolTransactionRequest, CancellationToken cancellationToken = default(CancellationToken))
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<MempoolTransactionResponse>> MempoolTransactionPostWithHttpInfoAsync(MempoolTransactionRequest mempoolTransactionRequest, CancellationToken cancellationToken = default(CancellationToken))
    {
        throw new NotImplementedException();
    }
}
