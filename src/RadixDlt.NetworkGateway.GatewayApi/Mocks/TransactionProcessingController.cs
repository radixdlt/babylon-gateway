using Microsoft.AspNetCore.Mvc;
using RadixDlt.NetworkGateway.Common.Extensions;

namespace RadixDlt.NetworkGateway.GatewayApi.Mocks;

public record TransactionSubmitRequest;

public record TransactionSubmitResponse;

public record TransactionPreviewRequest;

public record TransactionPreviewResponse;

public record TransactionStatusRequest;

public record TransactionStatusResponse;

[ApiController]
public class TransactionProcessingController
{
    [HttpPost("mock/transactions/submit")]
    public TransactionSubmitResponse Submit(TransactionSubmitRequest request)
    {
        return new TransactionSubmitResponse();
    }

    [HttpPost("mock/transactions/preview")]
    public TransactionPreviewResponse Preview(TransactionPreviewRequest request)
    {
        return new TransactionPreviewResponse();
    }

    // TODO: should we split this into 4 different endpoints: byIntentHash, bySignedTransactionHash, byNotarizedTransactionHash, byPayloadHash
    [HttpPost("mock/transactions/status")]
    public TransactionStatusResponse Status(TransactionStatusRequest request)
    {
        return new TransactionStatusResponse();
    }
}
