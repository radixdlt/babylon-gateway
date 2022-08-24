/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

using FluentAssertions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace RadixDlt.NetworkGateway.IntegrationTests
{
    internal static class GatewayApiSpecValidator
    {
        private static readonly string _openApiFileName = "../../../../../src/RadixDlt.NetworkGateway.GatewayApi/gateway-api-spec.yaml";

        private static OpenApiDocument? _openApiDocument = null;

        internal static void ValidateController(Type controller, string controllerRoute)
        {
            if (_openApiDocument == null)
            {
                string stringYml = File.ReadAllText(_openApiFileName);

                // Read the schema
                _openApiDocument = new OpenApiStringReader().Read(stringYml, out var diagnostic);
            }

            // Enumerate controller endpoints
            var methodInfos = controller
                               .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                               .Where(m => m.CustomAttributes.Any() &&
                                      m.CustomAttributes.FirstOrDefault(ca =>
                                                        ca.AttributeType.Name == "HttpPostAttribute" ||
                                                        ca.AttributeType.Name == "HttpGetAttribute" ||
                                                        ca.AttributeType.Name == "HtttpPutAttribute" ||
                                                        ca.AttributeType.Name == "HttpDeleteAttribute") != null);

            // Extract controller endpoints from the openApi specification
            var openApiPathItems = _openApiDocument.Paths.Where(p => p.Key.Contains(controllerRoute));

            // Verify that the number of endpoints matches (first parameter is 'expected', second is 'actual')
            openApiPathItems.Count().Should().Be(methodInfos.Count());

            // Loop through openApi items
            foreach (var openApiPathItem in openApiPathItems)
            {
                // Main route
                var openApiItemUrl = openApiPathItem.Key;

                // Loop through CRUD ops
                foreach (var op in openApiPathItem.Value.Operations)
                {
                    // CRUD op codes
                    var openApiOpCode = op.Key.ToString();

                    // Check if the route exists in the code
                    var endpoint = methodInfos.Where(m => m.CustomAttributes
                        .FirstOrDefault(
                            ca =>
                                ca.AttributeType.Name == $"Http{openApiOpCode}Attribute" &&
                                ca.ConstructorArguments[0].ArgumentType == typeof(string) &&
                                openApiItemUrl.EndsWith(ca.ConstructorArguments[0].Value.ToString())) != null).ToList();

                    // Verify the endpoint is implemented.");
                    endpoint.Count().Should().Be(1);

                    if (endpoint.Any())
                    {
                        // Verify the response type
                        string openApiResponseParameter = openApiPathItem.Value.Operations[op.Key].Responses["200"].Content["application/json"].Schema.Reference.Id;

                        string codeResponseParameter = endpoint[0].ReturnType.GenericTypeArguments[0].Name;

                        openApiResponseParameter.Should().BeEquivalentTo(codeResponseParameter);

                        // Verify request parameters

                        // Code request parameters
                        var codeRequestParameters = endpoint[0].GetParameters().Select(p => p.ParameterType.Name);

                        // Schema request parameters
                        var openApiRequestParameters = openApiPathItem.Value.Operations[op.Key].RequestBody.Content.Select(c => c.Value.Schema.Reference.Id);

                        // Compare with the schema request parameters including the order
                        openApiRequestParameters.Should().BeEquivalentTo(codeRequestParameters);
                    }
                }
            }
        }
    }
}
