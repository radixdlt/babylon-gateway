using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System.Reflection;
using Xunit;

namespace RadixDlt.NetworkGateway.IntegrationTests
{
    internal static class GatewayApiSpecValidator
    {
        private static readonly string _openApispecFileName = "../../../../../gateway-api-spec.yaml";

        private static OpenApiDocument? _openApiDocument = null;

        internal static void ValidateController(Type controller, string controllerRoute)
        {
            if (_openApiDocument == null)
            {
                string stringYml = File.ReadAllText(_openApispecFileName);

                // Read V3 as YAML
                _openApiDocument = new OpenApiStringReader().Read(stringYml, out var diagnostic);
            }

            // enumerate controller endpoints
            var methodInfos = controller
                               .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                               .Where(m => m.CustomAttributes.Any() &&
                                      m.CustomAttributes.FirstOrDefault(ca =>
                                                        ca.AttributeType.Name == "HttpPostAttribute" ||
                                                        ca.AttributeType.Name == "HttpGetAttribute" ||
                                                        ca.AttributeType.Name == "HtttpPutAttribute" ||
                                                        ca.AttributeType.Name == "HttpDeleteAttribute") != null);

            // extract /transaction endpoints from the specification
            var apiPathItems = _openApiDocument.Paths.Where(p => p.Key.Contains(controllerRoute));

            // verify that the number of endpoints matches (first parameter is 'expected', second is 'actual'
            Assert.True(
                apiPathItems.Count() == methodInfos.Count(),
                $"The number of endpoints in spec ({apiPathItems.Count()}) does not match the number of implemented endpoints ({methodInfos.Count()}).");

            // loop through the spec
            foreach (var specApiItem in apiPathItems)
            {
                var specUrl = specApiItem.Key;

                foreach (var op in specApiItem.Value.Operations)
                {
                    var specOpCode = op.Key.ToString();
                    string specResponseParameter = specApiItem.Value.Operations[op.Key].Responses["200"].Content["application/json"].Schema.Reference.Id;

                    // check if the endpont exists url + opCode
                    var endpoint = methodInfos.Where(m => m.CustomAttributes
                        .FirstOrDefault(ca =>
                                ca.AttributeType.Name == $"Http{specOpCode}Attribute" &&
                                ca.ConstructorArguments[0].ArgumentType == typeof(string) &&
                                specUrl.EndsWith(ca.ConstructorArguments[0].Value.ToString())) != null).ToList();

                    // Assert.True(endpoint.Count() == 1, $"The endpoint '{specUrl}' is not implemented.");

                    // verify the return type
                    if (endpoint.Any())
                    {
                        string codeResponseParameter = endpoint[0].ReturnType.GenericTypeArguments[0].Name;
                        Assert.True(
                            specResponseParameter.Equals(codeResponseParameter),
                            $"Error in '{specUrl}'. The return parameter '{specResponseParameter}' is not equal to '{codeResponseParameter}'.");
                    }
                }
            }
        }
    }
}
