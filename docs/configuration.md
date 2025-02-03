# Configuration

## How to configure

The Network Gateway services can be configured in line with the [configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0) paradigm, with a few notable additions:

* Environment variables must be prefixed by application-specific prefix (`GatewayApi__` or `DataAggregator__`) to disambiguate from other environment variables.
* There's support for defining an environment variable `CustomJsonConfigurationFilePath` which is configured with a path to a JSON file, in ASP.NET JSON format.
  Note that the configuration configured there takes priority over environment variables.

## Configuration options

### Gateway API

#### Endpoint behavior
- `GatewayApi__Endpoint__MaxHeavyCollectionsPageSize` (type: `int`, default value: `20`) - maximum page size for heavy collections. Affected endpoints - `/state/package/page/codes`, `/state/package/page/blueprints`, and `/state/entity/page/schemas`.
- `GatewayApi__Endpoint__MaxPageSize` (type: `int`, default value: `100`) - maximum page size for all endpoints except these controlled by `GatewayApi__Endpoint__MaxHeavyCollectionsPageSize`.

- `GatewayApi__Endpoint__DefaultTransactionsStreamPageSize` (type: `int`, default value: `10`) - default page size for `/stream/transactions` endpoint.
- `GatewayApi__Endpoint__DefaultNonFungibleIdsPageSize` (type: `int`, default value: `100`) - default page size for returning nonfungible ids from `/state/non-fungible/ids` endpoint.
- `GatewayApi__Endpoint__DefaultHeavyCollectionsPageSize` (type: `int`, default value: `10`) - default page size for heavy collections endpoint. Affected endpoints - `/state/package/page/codes`, `/state/package/page/blueprints`, and `/state/entity/page/schemas`.
- `GatewayApi__Endpoint__DefaultPageSize` (type: `int`, default value: `100`) - default page size for all endpoints that do not have explicit configuration.

- `GatewayApi__Endpoint__EntitiesByRoleRequirementLookupMaxRequestedRequirementsCount` - (type: `int`, default value: `50`) - maximum number of requirements that can be passed to the `/extensions/entities-by-role-requirement/lookup` endpoint. 
- `GatewayApi__Endpoint__StateEntityDetailsPageSize` (type: `int`, default value: `20`) - maximum number of addresses that can be passed to `/state/entity/details` endpoint.
- `GatewayApi__Endpoint__TransactionAccountDepositPreValidationMaxResourceItems` (type: `int`, default value: `20`) - maximum number of resources that can be passed to `/transaction/account-deposit-pre-validation` endpoint.
- `GatewayApi__Endpoint__ExplicitMetadataMaxItems` (type: `int`, default value: `20`) - maximum number of metadata keys that can be passed as explicit metadata parameter.
- `GatewayApi__Endpoint__TransactionStreamMaxFilterCount` (type: `int`, default value: `10`) - maximum number of filters that can be applied to `/stream/transactions` endpoint. For details on how each filter is counted please visit the documentation https://radix-babylon-gateway-api.redoc.ly/

- `GatewayApi__Endpoint__ValidatorsPageSize` (type: `int`, default value: `1000`) - fixed page size for `/state/validators/list` endpoint.
- `GatewayApi__Endpoint__ValidatorsUptimePageSize` (type: `int`, default value: `200`) -  fixed page size for `/statistics/validators/uptime` endpoint.

- `GatewayApi__Endpoint__RequestTimeout` - (type: `timespan in string format [d.]hh:mm:ss[.fffffff]`, default value: `10s`) - the amount of time after which request gets canceled and timeout is returned from API.
- `GatewayApi__Endpoint__MaxDefinitionsLookupLimit` - (type: `int`, default value: `50 000`) - Max number of definitions to scan when searching for non-deleted entries.


#### Ledger Lag
- `GatewayApi__AcceptableLedgerLag__PreventReadRequestsIfDbLedgerIsBehind` (type: `bool`, default value: `true`) - controls if API will return a response if observed ledger state by the gateway is behind.
- `GatewayApi__AcceptableLedgerLag__ReadRequestAcceptableDbLedgerLagSeconds` (type: `int`, default value: `30` ) - allowed difference between observed ledger state and actual one.
- `GatewayApi__AcceptableLedgerLag__PreventConstructionRequestsIfDbLedgerIsBehind` (type: `bool`, default value: `true`) - controls if `/transaction/construction` will return response if observed ledger state by the gateway is behind.
- `GatewayApi__AcceptableLedgerLag__ConstructionRequestsAcceptableDbLedgerLagSeconds` (type: `int`, default value: `30`) - allowed difference between observed ledger state and actual one for `/transaction/construction` endpoint.

#### CoreAPI Integration
- `GatewayApi__CoreApiIntegration__SubmitTransactionNodeRequestTimeoutMilliseconds` (type: `int`, default value: `4000`) - timeout after which transaction submission will be canceled.
- `GatewayApi__CoreApiIntegration__StopResubmittingAfterSeconds` (type: `int`, default value: `300`) - the number of seconds after which the gateway will resign from submitting the certain transaction.
- `GatewayApi__CoreApiIntegration__MaxSubmissionAttempts` (type: `int`, default value: `5`) - maximum number of submission attempts for the transaction.
- `GatewayApi__CoreApiIntegration__BaseDelayBetweenResubmissionsSeconds` (type: `int`, default value: `10`) - factor used to calculate the delay between resubmission attempts. Formula: BaseDelayBetweenResubmissionsSeconds + Pow(ResubmissionDelayBackoffExponent, AttemptCounter).
- `GatewayApi__CoreApiIntegration__ResubmissionDelayBackoffExponent` (type: `double`, default value: `2`) - factor used to calculate the delay between resubmission attempts. Formula: BaseDelayBetweenResubmissionsSeconds + Pow(ResubmissionDelayBackoffExponent, AttemptCounter).
- `GatewayApi__CoreApiIntegration__MaxTransientErrorRetryCount` (type: `int`, default value: `3`) - maximum number of retries for transient errors when calling CoreAPI.

#### Postgres integration
- `GatewayApi__PostgresIntegration__MaxTransientErrorRetryCount` (type: `int`, default value: `3`) - maximum number of retries for transient errors on connection to Postgres database.

#### Slow Query logging
- `GatewayApi__SlowQueryLogging__SlowQueryThreshold` (type: `timespan in string format [d.]hh:mm:ss[.fffffff]`, default value: `250ms`) - threshold after which query is considered slow and will be logged.

#### Network
- `GatewayApi__Network__NetworkName` (type: `string`, default value: `empty`) - the name of the network Gateway API is operating on.
- `GatewayApi__Network__CoreApiNodes` - (type: `complex object`) - list of core API nodes to connect to.
- `GatewayApi__Network__DisableCoreApiHttpsCertificateChecks` (type: `bool`, default value: `false`) - controls if the HTTPS certificate check should be disabled. To be used only in development environments.
- `GatewayApi__Network__CoreApiHttpProxyAddress` (type: `string`, default value: `empty`) - URL to be used as a proxy when calling core API.
- `GatewayApi__Network__MaxAllowedStateVersionLagToBeConsideredSynced` (type: `int`, default value: `100`) - maximum allowed state version lag for core API node to be considered synced.
- `GatewayApi__Network__IgnoreNonSyncedNodes` (type: `bool`, default value: `true`) - controls if nodes with a different status than healthy and synced should be used by gateway API.

### Data Aggregator

#### Slow Query logging
- `DataAggregator__SlowQueryLogging__SlowQueryThreshold` (type: `timespan in string format [d.]hh:mm:ss[.fffffff]`, default value: `250ms`) - threshold after which query is considered slow and will be logged.

#### Processor related configuration
- `DataAggregator__LedgerProcessors__EntitiesByRoleAssignmentsPerStateVersionWarningThreashold` (type: `int`, default value: `25`) - Specifies the threshold for the number of entries added to the entities_by_role_requirement table per transaction that triggers a warning.

#### Network
- `DataAggregator__Network__NetworkName` (type: `string`, default value: `empty`) - the name of the network data aggregator is operating on.
- `DataAggregator__Network__CoreApiNodes` - (type: `complex object`) - list of core API nodes to connect to.
- `DataAggregator__Network__DisableCoreApiHttpsCertificateChecks` (type: `bool`, default value: `false`) - controls if HTTPS certificate check should be disabled. To be used only in development environments.
- `DataAggregator__Network__CoreApiHttpProxyAddress` (type: `string`, default value: `empty`) - URL to be used as a proxy when calling core API.
- `DataAggregator__Network__MaxAllowedStateVersionLagToBeConsideredSynced` (type: `int`, default value: `100`) - maximum allowed state version lag for CoreAPI node to be considered synced.
- `DataAggregator__Network__IgnoreNonSyncedNodes` (type: `bool`, default value: `true`) - controls if nodes with a different status than healthy and synced should be used by the Data Aggregator.

#### Storage
- `DataAggregator__Storage__StoreTransactionReceiptEvents` (type: `enum`, default value: `StoreForAllTransactions`) - controls if data aggregator should store transaction receipt events in database.
  - Possible values:
     - `StoreForAllTransactions` (default) - will store data for all transactions.
     - `StoryOnlyForUserTransactionsAndEpochChanges` - will store data for user transactions and transactions that resulted in epoch change.
     - `StoreOnlyForUserTransactions` - will store data only for user transactions.
     - `DoNotStore` - will not store any data.
- `DataAggregator__Storage__StoreReceiptStateUpdates` (type: `enum`, default value: `StoreForAllTransactions`) - controls if data aggregator should store transaction receipt state updates in database.
  - Possible values:
      - `StoreForAllTransactions` (default) - will store data for all transactions.
      - `StoryOnlyForUserTransactionsAndEpochChanges` - will store data for user transactions and transactions that resulted in epoch change.
      - `StoreOnlyForUserTransactions` - will store data only for user transactions.
      - `DoNotStore` - will not store any data.

#### Monitoring
`DataAggregator__Monitoring__StartupGracePeriodSeconds` (type: `int`, default value: `10`) - duration (seconds) of start-up grace period for Data Aggregator.
`DataAggregator__Monitoring__UnhealthyCommitmentGapSeconds` (type: `int`, default value: `20`) - time window since the last committed transaction (seconds) in which Data Aggregator is considered healthy if does not commit new transactions.

#### Mempool
`DataAggregator__Mempool__ResubmissionNodeRequestTimeoutMilliseconds` (type: `int`, default value: `4000`) - time after resubmission is canceled.
`DataAggregator__Mempool__BaseDelayBetweenResubmissionsSeconds` (type: `int`, default value: `10`) - delay (seconds) between resubmission attempts.
`DataAggregator__Mempool__ResubmissionDelayBackoffExponent` (type: `double`, default value: `2`) - resubmission delay backoff exponent.
`DataAggregator__Mempool__ResubmissionBatchSize` (type: `int`, default value: `30`) - number of transactions in the resubmission batch.
`DataAggregator__Mempool__StopResubmittingAfterSeconds` (type: `int`, default value: `360`) - the amount of time (seconds) after which transaction will no longer be resubmitted.
`DataAggregator__Mempool__MaxSubmissionAttempts` (type: `int`, default value: `5`) - maximum resubmission attempts per transaction.
`DataAggregator__Mempool__PruneMissingTransactionsAfterTimeSinceLastGatewaySubmissionSeconds` (type: `int`, default value: `7 * 24 * 60 * 60 - 1 week`) - the amount of time after the last submission attempt, when transactions will be removed from the database.

#### NodeWorkers
`DataAggregator__NodeWorkers__ErrorStartupBlockTimeSeconds` (type: `int`, default value: `20`) - seconds to delay after error when initializing node runners.
`DataAggregator__NodeWorkers__GraceSecondsBeforeMarkingStalled` (type: `int`, default value: `10`) - seconds after which node worker is considered stalled if there is no activity reported.

#### LedgerConfirmation
`DataAggregator__LedgerConfirmation__MaxCommitBatchSize` (type: `int`, default value: `1000`) - maximum number of transactions to commit/process.
`DataAggregator__LedgerConfirmation__MinCommitBatchSize` (type: `int`, default value: `1`) - minimal number of transactions to commit. If there are fewer transactions, DataAggregator will wait for more before processing them.
`DataAggregator__LedgerConfirmation__LargeBatchSizeToAddDelay` (type: `int`, default value: `500`) - batch size to be considered large.
`DataAggregator__LedgerConfirmation__DelayBetweenLargeBatchesMilliseconds` (type: `int`, default value: `0`) - milliseconds of delay to add between large batches.
`DataAggregator__LedgerConfirmation__MaxTransactionPipelineSizePerNode` (type: `int`, default value: `3000`) - maximum number of transactions to be prefetched from CoreAPI and held in memory before processing by Data Aggregator.
`DataAggregator__LedgerConfirmation__MaxEstimatedTransactionPipelineByteSizePerNode` (type: `int`, default value: `50 * 1024 * 1024 = 50mb`) - maximum size per node for prefetched transactions.
`DataAggregator__LedgerConfirmation__MaxCoreApiTransactionBatchSize` (type: `int`, default value: `1000`) - maximum number of transactions fetched from CoreAPI.
