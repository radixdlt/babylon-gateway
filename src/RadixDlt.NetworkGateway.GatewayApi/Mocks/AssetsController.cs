using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.GatewayApi.Mocks;

// 1. ProperAccountId = some proper complex type, definitely not INT
// 2. Should NG care about "associated dApps"? I don't think so.
// 3. Should NG care about some resource grouping? Talking about "Raave Loans" example where two NFTs are combined into one box. I don't think so, this looks like presentation logic. That being said we must ensure both resources are always visible so no pagintation of this resource?
// 4. Not sure if I fully understand concept of "pool shares"

// 1. Type should not be string-based (enum)
// 2. Not sure if Type shouldn't be purely presentation layer concept - then it should be removed
public record AccountResourcesRequest(int ProperAccountId, string? Type);
public record AccountResourcesResponse(int ProperAccountId, ICollection<ResourceAmount> Resources);

// 1. initially I though that only resource-id should be transported but currently proposed type is small enough to remain handy and efficient
// 2. use TokenAmount not decimal
public record ResourceAmount(ResourceSnippet Resource, decimal Amount);
public record ResourceDetailsRequest(string Rri);

/// <summary>
/// Lightweight resource description.
/// </summary>
// 1. Type should not be string-based (enum)
// 2. BehaviorFlags should not be string-based (enum)
public record ResourceSnippet(string Type, string Rri, string Name, ICollection<string> BehaviorFlags);

/// <summary>
/// Detailed (heavyweight) resource description.
/// </summary>
// 1. maybe we need NonFungibleResource[Item|Details], FungibleResource[Item|Details] etc. based on Type (polymorphism)
// 2. Metadata might be [partially] strongly typed (see no. 1) or follow this yet-to-be-invented specs
// 3. Not sure if it's a good idea to inherit from ResourceSnippet
public record ResourceDetails(string Type, string Rri, string Name, ICollection<string> BehaviorFlags) : ResourceSnippet(Type, Rri, Name, BehaviorFlags)
{
    public string? Icon { get; init; }

    public string? Description { get; init; }

    public IDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}

// 1. Type should be enum
public abstract record LedgerFilter(string Type);

public record StateVersionLedgerFilter(long StateVersion) : LedgerFilter("by_state_version");

public record TimestampLedgerFilter(DateTimeOffset Timestamp) : LedgerFilter("by_timestamp");

public record EpochLedgerFilter(long Epoch, long? Round) : LedgerFilter("by_epoch");

public record AccountFilter(int ProperAccountId);

// 1. Type should be enum
// 2. Rename to Transaction[Type]Filter
public record OperationTypeFilter(string Type); // think: deposits, withdrawals, etc.

// 1. Type should be enum
public record AssetTypeFilter(string Type); // think: NFT, token, badge

public record TokenTypeFilter(string Rri); // think: RDR, rBTC, rUSD

public record ApplicationFilter(string WhateverIdentifierApplicationUse); // think: Radaswap, NuBank, Radabridge

[ApiController]
public class AssetsController
{
    // used by Homepage
    [HttpPost("mock/account-resources")]
    public AccountResourcesResponse AccountResources(AccountResourcesRequest request)
    {
        return new AccountResourcesResponse(request.ProperAccountId, new ResourceAmount[]
        {
            new(new ResourceSnippet("fungible", "xrd", "XRD Token", new[] { "fixed-supply" }), 123456),
            new(new ResourceSnippet("fungible", "btcw", "BTC Wrapped Token", Array.Empty<string>()), 32),
            new(new ResourceSnippet("non-fungible", "my_nft", "Some Unique NFT of mine", Array.Empty<string>()), 1),
            new(new ResourceSnippet("non-fungible", "my_badge", "my_badge name", Array.Empty<string>()), 1),
        });
    }

    // used by "single asset view" or "asset details"
    [HttpPost("mock/resource-details")]
    public ResourceDetails ResourceDetails(ResourceDetailsRequest request)
    {
        return new ResourceDetails("fungible", request.Rri, $"Name of {request.Rri}", new[] { "fixed-supply" })
        {
            Description = $"Description of {request.Rri}",
            Metadata =
            {
                ["abc"] = "def",
            },
        };
    }
}



