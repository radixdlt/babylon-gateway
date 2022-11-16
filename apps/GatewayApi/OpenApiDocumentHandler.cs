using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;
using RadixDlt.NetworkGateway.GatewayApi;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayApi;

public static class OpenApiDocumentHandler
{
    public static async Task Handle(HttpContext context, CancellationToken token = default)
    {
        var assembly = typeof(GatewayApiBuilder).Assembly;
        var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.gateway-api-spec.yaml");
        var readResult = await new OpenApiStreamReader().ReadAsync(stream);
        var document = readResult.OpenApiDocument;

        document.Servers.Clear();
        document.Servers.Add(new OpenApiServer
        {
            Url = new UriBuilder(context.Request.GetEncodedUrl())
            {
                Path = context.Request.PathBase,
                Query = null,
                Fragment = null,
            }.ToString(),
        });

        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "application/json; charset=utf-8";

        await using var textWriter = new StringWriter(CultureInfo.InvariantCulture);
        var jsonWriter = new OpenApiJsonWriter(textWriter);

        document.SerializeAsV3(jsonWriter);

        await context.Response.WriteAsync(textWriter.ToString(), Encoding.UTF8, token);
    }
}
