# Query Conventions

The following conventions make these queries easier to understand:

* Each query will be a static class living in its own file.
* These classes can include record structs inside them for:
  * `internal readonly record struct QueryConfiguration` - various parameters related to the results to return under a given set of query roots. This can include information about cursors, orderings, what content to return, limits etc.
  * `private readonly record struct QueryResultRow` - a model for the returned row, for use with dapper
  * And any models related to the results of the query

Common query types include the following - which should be suffixed onto the end of the query class names:
* `PageQuery` - For reading a cursor-paginated query under one or more roots.
* `LookupQuery` - For reading specific keys under one or more roots.

## Page Query Conventions

Each file has a single method which takes parameters as follows and returns a `Task<Dictionary<EntityAddress, PerEntityResult>>`.

```csharp
    ReadOnlyDbContext dbContext,
    IDapperWrapper dapperWrapper,
    GatewayModel.LedgerState ledgerState,
    List<EntityAddress> rootEntityAddresses,
    PageParameters pageParameters,
    CancellationToken token = default
```

### Page Queries using a definition-based cursor

The `PageParameters` should be a local `record struct` which should include:
* An optional `GatewayModel.IdBoundaryCoursor?` cursor
* A max page size
* A max definition read limit (to bound the work done by the query)
* ... `XYZ.PageParameters` for any internal pages

The method should perform a structured query similar to `NonFungibleIdsInResourcePageQuery` to pull all the pages of data from the database in one round trip.

It should combine this with calls to other Page Query tools for sub-collections; and then unify these together to construct the data model.

How the query works:

* It assumes a structure with two (or three) tables:
  * There is a "XXX_definitions" table, covering all unique keys observed in a collection `entity_id`.
    This should have an index on `(entity_id, from_state_version, definition_id)`.
  * There is a `XXX_entry_history" table, covering the history of each definition over time.
  * (OPTIONAL) there is some kind of totals or aggregate table, such as `XXX_totals_history`.
* In one query, it efficiently returns:
  * Pages of entries for each entity (subject to some bound on work performed)
  * A next cursor for each of these pages
  * Relevant totals associated with that entity
* It supports options to:
  * Have an optional cursor
  * Go ASC or DESC
  * Include the value or not
  * Include deleted entries or not
* In the result set, there will be at least one row for every entity which existed at the given state version.
* The last returned row for each entity is special:
  * It has the cursor for possible further pagination (or `NULL` if none such exists)
  * It includes totals (if they are uncommented)
  * It may have `FilterOut` set to true - in which case, that row should not be returned (but the cursor/totals can still be used).

You will need to take a base query such as the one in `NonFungibleIdsInResourcePageQuery` and change the relevant table/field names for the particular case you're looking at.
