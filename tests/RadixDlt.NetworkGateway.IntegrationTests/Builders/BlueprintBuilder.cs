using RadixDlt.CoreApiSdk.Model;
using System;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class BlueprintBuilder : IBuilder<StateUpdates>
{
    private Type _blueprintType = null!;

    public StateUpdates Build()
    {
        if (_blueprintType == null)
        {
            throw new NullReferenceException("No Blueprint type was found");
        }

        var bluePrint = (Activator.CreateInstance(_blueprintType) as StateUpdates)!;

        return bluePrint;
    }

    public BlueprintBuilder Of(Type blueprintType)
    {
        _blueprintType = blueprintType;

        return this;
    }
}
