using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System.Reflection;
using Xunit;

namespace RadixDlt.NetworkGateway.IntegrationTests
{
    internal static class GatewayApiSpecValidator
    {
        private static readonly string _openApiFileName = "../../../../../gateway-api-spec.yaml";

        private static OpenApiDocument? _openApiDocument = null;

        internal static void ValidateController(Type controller, string controllerRoute)
        {
            if (_openApiDocument == null)
            {
                string stringYml = File.ReadAllText(_openApiFileName);

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

            // extract controller endpoints from the openApi specification
            var openApiPathItems = _openApiDocument.Paths.Where(p => p.Key.Contains(controllerRoute));

            // verify that the number of endpoints matches (first parameter is 'expected', second is 'actual')
            Assert.True(
                openApiPathItems.Count() == methodInfos.Count(),
                $"The number of endpoints in openApi ({openApiPathItems.Count()}) does not match the number of implemented endpoints ({methodInfos.Count()}).");

            // loop through openApi items
            foreach (var openApiPathItem in openApiPathItems)
            {
                // main route
                var openApiItemUrl = openApiPathItem.Key;

                // loop through CRUD ops
                foreach (var op in openApiPathItem.Value.Operations)
                {
                    // GET/POST/PUT/DELETE
                    var openApiOpCode = op.Key.ToString();

                    // check if the route exists in the code
                    var endpoint = methodInfos.Where(m => m.CustomAttributes
                        .FirstOrDefault(ca =>
                                ca.AttributeType.Name == $"Http{openApiOpCode}Attribute" &&
                                ca.ConstructorArguments[0].ArgumentType == typeof(string) &&
                                openApiItemUrl.EndsWith(ca.ConstructorArguments[0].Value.ToString())) != null).ToList();

                    // Assert.True(endpoint.Count() == 1, $"The endpoint '{openApiUrl}' is not implemented.");

                    if (endpoint.Any())
                    {
                        // verify the response type
                        string openApiResponseParameter = openApiPathItem.Value.Operations[op.Key].Responses["200"].Content["application/json"].Schema.Reference.Id;

                        string codeResponseParameter = endpoint[0].ReturnType.GenericTypeArguments[0].Name;
                        Assert.True(
                            openApiResponseParameter.Equals(codeResponseParameter),
                            $"Error in '{openApiItemUrl}'. The return parameter '{openApiResponseParameter}' is not equal to '{codeResponseParameter}'.");

                        // verify request parameters

                        // code request parameters
                        var codeRequestParameters = endpoint[0].GetParameters().Select(p => p.ParameterType.Name);

                        // schema request parameters
                        var openApiRequestParameters = openApiPathItem.Value.Operations[op.Key].RequestBody.Content.Select(c => c.Value.Schema.Reference.Id);

                        // compare with the schema request parameters including the order
                        Assert.True(
                            Enumerable.SequenceEqual(openApiRequestParameters, codeRequestParameters),
                            $"Error in '{openApiItemUrl}'. The request parameters don't match.");
                    }
                }
            }
        }
    }
}
