using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RadixDlt.NetworkGateway.IntegrationTests.GatewayApi
{
    public static class TestJsonExtensions
    {
        public static async Task<TResponse?> ParseToObjectAndAssert<TResponse>(this HttpResponseMessage responseMessage)
        {
            responseMessage.EnsureSuccessStatusCode(); // Status Code 200-299

            MediaTypeHeaderValue.TryParse(responseMessage.Content.Headers.ContentType?.ToString(), out var mediaTypeHeader);

            Assert.NotNull(mediaTypeHeader);

            Assert.Equal("application/json", mediaTypeHeader?.MediaType);

            Assert.Equal("utf-8", mediaTypeHeader?.CharSet);

            string json = await responseMessage.Content.ReadAsStringAsync();

            var payload = JsonConvert.DeserializeObject<TResponse>(json);

            payload.ShouldNotBeNull();

            return payload;
        }
    }
}
