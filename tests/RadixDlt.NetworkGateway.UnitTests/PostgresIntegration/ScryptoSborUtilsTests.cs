using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RadixDlt.NetworkGateway.PostgresIntegration;
using System;
using System.Collections.Generic;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.PostgresIntegration;

public class ScryptoSborUtilsTests
{
    [Fact]
    public void ParseStringScalarMetadata()
    {
        const string Input = "5c2200010c124d79206d6574616461746120737472696e67";
        var result = ScryptoSborUtils.MetadataValueToGatewayMetadataItemValue(NullLogger<ScryptoSborUtilsTests>.Instance, Convert.FromHexString(Input), 1);

        result.ShouldNotBeNull();
        result.RawHex.Should().Be(Input);
        result.RawJson.ShouldNotBeNull();
        result.AsString.Should().Be("My metadata string");
        result.AsStringCollection.Should().BeNull();
    }

    [Fact]
    public void ParseStringArrayMetadata()
    {
        const string Input = "5c228001200c021a4d79206d6574616461746120617272617920737472696e6720311a4d79206d6574616461746120617272617920737472696e672032";
        var result = ScryptoSborUtils.MetadataValueToGatewayMetadataItemValue(NullLogger<ScryptoSborUtilsTests>.Instance, Convert.FromHexString(Input), 1);

        result.ShouldNotBeNull();
        result.RawHex.Should().Be(Input);
        result.RawJson.ShouldNotBeNull();
        result.AsString.Should().BeNull();
        result.AsStringCollection.Should().BeEquivalentTo(new List<string>
        {
            "My metadata array string 1",
            "My metadata array string 2",
        });
    }
}
