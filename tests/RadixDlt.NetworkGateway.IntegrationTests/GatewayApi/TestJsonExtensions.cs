using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RadixDlt.NetworkGateway.IntegrationTests.GatewayApi
{
    public static class TestJsonExtensions
    {
        public static async Task<TResponse> ParseToObjectAndAssert<TResponse>(this HttpResponseMessage responseMessage)
        {
            responseMessage.EnsureSuccessStatusCode(); // Status Code 200-299

            Assert.Equal("application/json; charset=utf-8", responseMessage.Content.Headers.ContentType.ToString());

            string json = await responseMessage.Content.ReadAsStringAsync();

            var payload = JsonConvert.DeserializeObject<TResponse>(json);

            payload.Should().NotBeNull();

            return payload;
        }
    }
}
