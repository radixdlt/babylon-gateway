using RadixDlt.NetworkGateway.Contracts.Api.Model;

namespace RadixDlt.NetworkGateway.Endpoints;

public class TransactionEndpoint
{
    public async Task<TransactionBuildResponse> Build(TransactionBuildRequest request, CancellationToken token = default)
    {
        request.DoSth();

        await Task.Delay(1, token);

        var fee = new TokenAmount("123", new TokenIdentifier("some rri"));

        return new TransactionBuildResponse(new TransactionBuild(fee, "unsigned trans", "payload to sign"));
    }
}
