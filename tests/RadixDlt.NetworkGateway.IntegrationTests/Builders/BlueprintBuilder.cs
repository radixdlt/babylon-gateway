using RadixDlt.CoreApiSdk.Model;
using System;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class BlueprintBuilder : BuilderBase<StateUpdates>
{
    private Type _blueprintType = null!;

    public override StateUpdates Build()
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
