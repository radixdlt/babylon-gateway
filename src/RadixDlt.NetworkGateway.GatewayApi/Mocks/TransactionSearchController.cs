using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.GatewayApi.Mocks;

// 1. Upper and LowerBound correspond to at_state_identifier and from_state_identifier
// 2. AscendingOrder should be enum or complex type as we might end-up with something slightly more sophisticated that ASC/DESC
// 3. Collection types should be considered empty when null (null -> Array.Empty<SomeType>()) and empty collection should be considered "match all"
// 4. Collection types might not need complex types, maybe simple strings (e.g. Applications) or enums (e.g. AssetTypes) will be good enough
public record TransactionsSearchRequest
{
    public LedgerFilter? LedgerUpperBound { get; set; }

    public LedgerFilter? LedgerLowerBound { get; set; }

    // thinking out loud: do we want to support anything more sophisticated in foreseeable future (think: order by transaction amount DESC then by timestamp ASC or something)
    // maybe we should immediatelly start with public SortingOptions Sort { get; set; } // public record SortingOptions(bool? AscOrder) ?
    public bool? AscendingOrder { get; set; }

    public ICollection<OperationTypeFilter>? Operations { get; set; }

    // do we actually need collections for all of those?
    public ICollection<AccountFilter>? Accounts { get; set; }

    public ICollection<AssetTypeFilter>? AssetTypes { get; set; }

    public ICollection<TokenTypeFilter>? TokenTypes { get; set; }

    public ICollection<ApplicationFilter>? Applications { get; set; }
}

public record TransactionsSearchResultItem;

[ApiController]
public class TransactionSearchController
{
    // used by recent transactions, account transactions etc.
    [HttpPost("mock/transactions/search")]
    public ResultSet<TransactionsSearchResultItem> Search(TransactionsSearchRequest request)
    {
        var results = new TransactionsSearchResultItem[]
        {
            new(/* any details omitted right now; polymorphism needed probably; transaction details must be paginated */),
            new(/* any details omitted right now; polymorphism needed probably; transaction details must be paginated */),
            new(/* any details omitted right now; polymorphism needed probably; transaction details must be paginated */),
            new(/* any details omitted right now; polymorphism needed probably; transaction details must be paginated */),
        };

        return new ResultSet<TransactionsSearchResultItem>(results, "some_base64_encoded_cursor", results.Length + 2);
    }
}
