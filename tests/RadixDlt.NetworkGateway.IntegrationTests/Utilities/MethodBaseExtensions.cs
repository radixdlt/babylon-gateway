using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RadixDlt.NetworkGateway.IntegrationTests.Utilities;

public static class MethodBaseExtensions
{
    public static string NameFromAsync(this MethodBase asyncMethod)
    {
        var generatedType = asyncMethod.DeclaringType;
        var originalType = generatedType!.DeclaringType;
        var matchingMethods =
            from methodInfo in originalType!.GetMethods()
            let attr = methodInfo.GetCustomAttribute<AsyncStateMachineAttribute>()
            where attr != null && attr.StateMachineType == generatedType
            select methodInfo;

        // If this throws, the async method scanning failed.
        var foundMethod = matchingMethods.Single();

        return foundMethod.Name;
    }
}
