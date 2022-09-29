using RadixDlt.CoreApiSdk.Model;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public interface IBuilder<T>
{
    public T Build();
}
