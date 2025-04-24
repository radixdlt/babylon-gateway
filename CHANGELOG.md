## 1.11.0
Release built: _not released yet_

### Database changes
- Fixed an invalid `index_in_epoch` value for transactions processed in a batch when they belonged to different epochs.

## 1.10.2
Release built: 23.04.2025

### Bug fixes
- Fixed the 500 status code returned from `/transaction/committed-details` or `/stream/transactions` when the `receipt_events` or `detailed_events` opt-in was set to `true`, and an event with a recursive type of at least depth 3 was returned (e.g., a tuple nested inside a tuple nested inside another tuple).

## 1.10.1
Release built: 6.03.2025

### Bug fixes
- Fixed the 500 status code returned from `/state/entity/details` when querying multiple pre-allocated, non-persisted entities in a single request.

## 1.10.0
Release built: 5.03.2025

### Bug fixes
- Fixed two bugs in two-way links returned from `/state/entity/details` (If you are affected and your two-way link is not working, the Radix Console tool https://console.radixdlt.com/configure-metadata can help fix it. You may also consider using the blueprint link feature introduced in that release, though it is not yet supported by the tool and must be set manually). 
  - If one end of the link pointed to an entity without a corresponding metadata key, it was incorrectly considered a valid two-way link.
  - Fixed invalidation after removing a metadata entry on one end. Previously, the link was still considered valid even after the metadata entry was removed.

### API Changes
- New filters are supported on the `/stream/transactions` endpoint:
    - `transaction_status_filter` - Allows filtering by the transaction commit status (`Success`, `Failure`, `All`). Defaults to `All`.
    - `balance_change_resources_filter` - Allows filtering to transactions which included non-fee related balance changes for all provided resources. Defaults to `[]`. We recommend integrators use this instead of the `manifest_resources_filter` in most instances.
- Improved the performance of the `/extensions/resource-holders/page` endpoint.
- Added a new, detailed events model that provides more in-depth insights and additional context, allowing you to work with events more effectively. It is returned when the `detailed_events` opt-in is enabled for the `/stream/transactions` and `/stream/transactions` endpoints. The existing `events` property is now deprecated, and we advise switching to the new detailed events model.
- Added two new endpoints that allow querying for entities that have ever used a requirement (resource or non-fungible global ID) in their access rules (blueprint authentication templates, owner roles, or role assignments).
  - `/extensions/entities-by-role-requirement/lookup` – allows querying by multiple requirements.
  - `/extensions/entities-by-role-requirement/page` – allows querying and paginating by a single requirement.
- The `manifest_classes` of the transaction manifest in the `/stream/transactions` endpoint have been adjusted slightly. Notably:
  - The `General` classification has been expanded to permit validator stake/unstake/claim actions and pool contribute and redeem actions. 
- Added a new endpoint `/extensions/implicit-requirements/lookup` for resolving implicit access rule requirements (https://docs.radixdlt.com/docs/advanced-accessrules#implicit-requirements).
- Added [blueprint link](https://docs.radixdlt.com/docs/metadata-for-verification#blueprint-Link) support
  - dApp details
    - `auto_link_blueprints` property added to `two_way_linked_dapp_details.entities.items` if entity is package and has any auto link blueprint defined.
  - Linked entity changes
    - Update to `two_way_linked_dapp_address` previously it returned only direct links, right now it resolves to `direct_linked_dapp_address` if it exists; otherwise, it falls back to `blueprint_linked_dapp_address`.
    - New property `direct_linked_dapp_address` returns verified direct two-way link to the dApp address, if available.
    - New property `blueprint_linked_dapp_address` returns verified blueprint two-way link to the dApp address, if available.

### Database changes
- New entries added to the `ledger_transaction_markers` table for each resource whose balance (excluding fee-related changes) was modified in a transaction. Each resource balance change will be represented by an entry with the `resource_balance_change` discriminator and the resource's `entity_id`.
- Removed `transaction_type.round_update` from the `ledger_transaction_markers` table. This should reduce database size and slightly improve the performance of the `/stream/transactions` endpoint.
- A new index `IX_ledger_transaction_markers_resource_balance_change` has been added to the `ledger_transaction_markers` table.
- A new index `IX_ledger_transactions_receipt_status_state_version` has been added to the `ledger_transactions` table.
- Replaced the `IX_resource_holders_entity_id_resource_entity_id_balance` index with the `IX_resource_holders_resource_entity_id_balance_entity_id` index on the `resource_holders` table.
- New `outer_object_entity_id` column in the `entities` table, which holds the outer object entity id (e.g resource entity id for vaults and consensus manager entity id for validators).
- New `receipt_event_emitter_entity_ids` column in the `ledger_transaction_events` table, which holds the emitter entity ids for transaction events.
- Added a new `entities_by_role_requirement_entry_definition` table that stores information about entities that have ever used a requirement (resource or non-fungible global ID) in their access rules.
- Added a new `implicit_requirements` table to store data necessary for resolving implicit access rule requirements.

### What’s new?
- Added a new configuration parameter, `GatewayApi__Endpoint__EntitiesByRoleRequirementLookupMaxRequestedRequirementsCount`, which sets the limit (default `50`) on the number of requirements that can be queried using the `/extensions/entities-by-role-requirement/lookup` endpoint.
- Added a new configuration parameter, `GatewayApi__Endpoint__ImplicitRequirementsLookupMaxRequestedRequirementsCount`, which sets the limit (default `100`) on the number of implicit requirements that can be queried using the `/extensions/implicit-requirements/lookup` endpoint.

## 1.9.2
Release built: 9.12.2024

### Bug fixes
- Fixed a HTTP 500 response issue when querying the `/state/entity/details` endpoint with the `native_resource_details: true` opt-in for:
    - a validator's LSU resource address when the validator was never active.
    - a pool's unit resource when the pool had no contributions.
- Added support for pre-allocated, non-persisted accounts in the `/state/account/page/resource-preferences` and `/state/account/page/authorized-depositors` endpoints.
- Fixed a typo in the value `StoryOnlyForUserTransactionsAndEpochChanges` (replacing `Story` with `Store`) for the configuration entries `DataAggregator__Storage__StoreTransactionReceiptEvents` and `DataAggregator__Storage__StoreReceiptStateUpdates`. It now supports both values:
  - `StoreOnlyForUserTransactionsAndEpochChanges`
  - `StoryOnlyForUserTransactionsAndEpochChanges`

## 1.9.1
Release built: 20.11.2024

### What’s new?
- Added support for the `cuttlefish-part2` protocol version.

## 1.9.0
Release built: 18.11.2024

### What’s new?
- Added support for the `cuttlefish` protocol version.

### API Changes
- Added a new `/transaction/subintent-status` endpoint to check the status of a transaction subintent.
- Added two new optional fields to the `/transaction/committed-details` endpoint: `subintent_details` and `child_subintent_hashes`, which provide information about transaction subintents if present.
- Added a new `/transaction/preview-v2` endpoint to preview transactions. This supports V2 transactions and beyond. If you still need to preview V1 transactions, use the `/transaction/preview` endpoint instead.

### Database changes
- New `ledger_finalized_subintents` table that stores information about subintent status.
- New `UserV2` ledger transaction type discriminator in the `ledger_transactions` table.
- New `ledger_transaction_subintent_data` table that stores additional information about the transaction's subintents.

## 1.8.2
Release built: 4.11.2024

### Bug fixes
- Fix processing multiple changes to single non fungible id data (`NonFungibleResourceManagerDataEntrySubstate`) in one batch. It might result in
  - Wrong data stored and returned from the `/state/non-fungible/data` endpoint.
  - Not tracking properly that non fungible id got deleted, which might lead to returning an invalid location of non fungible id. Affected endpoints are
    - `/state/non-fungible/location`
    - And all endpoints that return non fungible vault content 
      - `/state/entity/details`
      - `/state/entity/page/non-fungibles/`
      - `/state/entity/page/non-fungible-vaults/`
      - `/state/entity/page/non-fungible-vault/ids`


## 1.8.1
Release built: 23.10.2024

> [!CAUTION]
> **Breaking Changes:**
> - Manifest addresses are no longer indexed in the `/stream/transactions` endpoint for failed transactions. Affected filters:
>   - `manifest_accounts_withdrawn_from_filter`
>   - `manifest_accounts_deposited_into_filter`
>   - `manifest_badges_presented_filter`
>   - `manifest_resources_filter`
>   - `accounts_with_manifest_owner_method_calls`
>   - `accounts_without_manifest_owner_method_calls`
> - Changed ordering of entity metadata. Entries are no longer ordered by their last modification state version but rather by their first appearance on the network, descending. Affected endpoints:
>   - `/state/entity/metadata`
>   - `/state/entity/page/metadata`
> - Changed ordering of fungible and non fungible resources. Entries are no longer ordered by their last modification state version but rather by their first appearance on the network, descending. Affected endpoints:
>   - `/state/entity/details`
>   - `/state/entity/page/fungibles/`
>   - `/state/entity/page/non-fungibles/`
> - Changed ordering of vaults when using vault aggregation level. Entries are no longer ordered by their last modification state version but rather by their first appearance on the network, descending.  Affected endpoints:
>   - `/state/entity/details`
>   - `/state/entity/page/fungibles/`
>   - `/state/entity/page/fungible-vaults/`
>   - `/state/entity/page/non-fungibles/`
>   - `/state/entity/page/non-fungible-vaults/`
> - Changed ordering of non fungible ids. Entries are no longer ordered by their last modification state version but rather by their first appearance on the network, descending. Affected endpoints:
>   - `/state/entity/page/non-fungible-vault/ids`
>   - `/state/entity/details` (when using `non_fungible_include_nfids` opt-in)
>   - `/state/entity/page/non-fungibles/` (when using `non_fungible_include_nfids` opt-in)
>   - `/state/entity/page/non-fungible-vaults/` (when using `non_fungible_include_nfids` opt-in)
> -  Existing non fungible vaults with no items will no longer return `items: null` and will return an empty array `items: []` instead, as we do in all other collections. Affected endpoints:
>    - `/state/entity/page/non-fungible-vault/ids`
>    - `/state/entity/details` (when using `non_fungible_include_nfids` opt-in)
>    - `/state/entity/page/non-fungibles/` (when using `non_fungible_include_nfids` opt-in)
>    - `/state/entity/page/non-fungible-vaults/` (when using `non_fungible_include_nfids` opt-in)
>

### What’s new?
- New configuration options `DataAggregator__Storage__StoreTransactionReceiptEvents`, and `DataAggregator__Storage__StoreReceiptStateUpdates` for the data aggregator to configure if a transaction's receipt events and receipt state updates should be stored in the database. It is meant to be used by gateway runners who want to reduce their database size. Keep in mind that when disabled, the corresponding properties will be missing on a response from both the `/stream/transactions` and the `/transaction/committed-details` endpoints. You can save significant space by using `StoryOnlyForUserTransactionsAndEpochChanges` and only excluding round change transactions, which aren't typically read from the `/stream/transactions` endpoint.
  - Possible values:
    - `StoreForAllTransactions` (default) - will store data for all transactions.
    - `StoryOnlyForUserTransactionsAndEpochChanges` - will store data for user transactions and transactions that resulted in epoch change.
       - NOTE: The configuration option is spelt incorrectly. Please use the prefix `Story`. In a future version, this will be fixed, and both `Story` and `Store` will be accepted, for backwards-compatibility.
    - `StoreOnlyForUserTransactions` - will store data only for user transactions.
    - `DoNotStore` - will not store any data.

### Bug fixes
- Added missing `total_count` property to `/state/validators/list` response.
- Fix `/transaction/account-deposit-pre-validation` for uninstantiated pre-allocated accounts. It no longer returns error with code 404 `Entity not found`. 
- Restored missing round update transactions from the `/stream/transactions` endpoint.

### API Changes
- Restored previously removed `total_count` property to `/state/key-value-store/keys` endpoint.

### Database changes
- Refactored multiple aggregates. Queries follow a similar strategy as key value stores and utilize `_entry_definition`, `_entry_history`, and `_totals_history` tables to return data
    - Metadata
        - Removed `entity_metadata_aggregate_history` table.
        - New `entity_metadata_entry_definition` table, which holds information about all the metadata keys ever created for a given entity.
        - Renamed `entity_metadata_history` to `entity_metadata_entry_history`, replaced `entity_id` and `key` columns with `entity_metadata_entry_definition_id`. Holds history of given metadata key at a given state version.
        - New `entity_metadata_totals_history` table, which holds total counts of metadata per entity.
    - Resource globally aggregated
        - Removed `entity_resource_aggregate_history` table.
        - New `entity_resource_entry_definition` table, which holds information about all resources which have ever been held by a given global entity.
        - New `entity_resource_balance_history` table, which holds the sum of globally aggregated resource held by a global entity at a given state version.
        - New `entity_resource_totals_history` table, which holds total count of different resources under a given global entity at a given state version.
    - Resource vault aggregated
        - Removed `entity_resource_aggregated_vaults_history` and `entity_resource_vault_aggregate_history` tables.
        - New `entity_resource_vault_entry_definition` table, which holds information about vaults of a given resource held under a given global entity.
        - New `entity_resource_vault_totals_history` table, which holds total count of all vaults of a given resource held under a given global entity at a given state version.
    - Vault content
        - New `non_fungible_vault_entry_definition` table, which holds information about non fungible held by a given vault.
        - New `non_fungible_vault_entry_history` table which holds history of given non fungible inside vault.
        - Renamed `entity_vault_history` to `vault_balance_history`. Holds information about vault content (amount of fungibles or count of non fungible ids inside vault) at a given state version.
    - Key value store
        - New `key_value_store_totals_history` table, which holds total count of all keys under a given store at a given state version.
- Changed `receipt_state_updates` in the `ledger_transactions` table to be nullable.
- Moved all `receipt_event_*` columns from the `ledger_transactions` table to a new separate `ledger_transaction_events` table.
- Renamed `origin_type` marker type to `transaction_type` (stored in the `ledger_transaction_markers` table), possible values:
  - `User`
  - `RoundChange`
  - `GenesisFlash`
  - `GenesisTransaction`
  - `ProtocolUpdateFlash`
  - `ProtocolUpdateTransaction`
- New transaction marker type `epoch_change` (stored in the `ledger_transaction_markers` table), entry for this marker indicates that this transaction resulted in an epoch change.

## 1.7.3
Release built: 26.09.2024

### What’s new?
This release fixes Data Aggregator stall on state version: `139553672` on the `mainnet` network.

### Database changes
- Removed unique constraint from `IX_account_resource_preference_rule_entry_history_account_enti~` index.

## 1.7.2
Release built: 17.09.2024

### API Changes
- Added `opt_ins` property to `/transaction/preview` request. Currently, there is only one option to use `radix_engine_toolkit_receipt`, it controls whether the preview response will include a Radix Engine Toolkit serializable
  receipt or not (defaults to `false`).

## 1.7.1
Release built: 29.08.2024

### Database changes
- Added missing index on `validator_cumulative_emission_history`

## 1.7.0
Release built: 23.08.2024

> [!CAUTION]
> **Breaking Changes:**
> - `/stream/transactions` no longer indexes `affected_global_entities` for the transaction tracker and the consensus manager entity types.
> - Changed `variant_id` of `ProgrammaticScryptoSborValueEnum` from numeric (`type: integer`) to string-encoded numeric (`type: string`) to make it compatible with the rest of the ecosystem.

### Bug fixes
- Properly indexes manifest classes. Some transactions might have been previously misclassified as `Transfer` and `AccountDepositSettingsUpdate`, i.e. empty transactions with only `lock_fee` instruction.

### API Changes
- Added support for the missing `message` and `flags.disable_auth_checks` properties in the `/transaction/preview` endpoint request.
- Added list of mutable non fungible data fields `non_fungible_data_mutable_fields` returned from `/state/entity/details` endpoint.
- New `event_global_emitters_filter` filter added to `/stream/transactions` endpoint. It allows filtering transactions by the global ancestor of an event emitter. For events emitted by a global entity it is going to be that entity, for internal entities it is going to be a global ancestor.
- Changed `variant_id` of `ProgrammaticScryptoSborValueEnum` from numeric (`type: integer`) to string-encoded numeric (`type: string`) to make it compatible with the rest of the ecosystem.
- Optimized `/statistics/validators/uptime` endpoint processing time.
- Added support for two-way linked dApps in the `/state/entity/details` endpoint, returned when the `dapp_two_way_links` opt-in is enabled.
  - Brand-new `two_way_linked_*` properties on the `details` property of the Resources, Accounts, Packages and other global entities.
  - See https://docs.radixdlt.com/docs/metadata-for-verification#metadata-standards-for-verification-of-onledger-entities for detailed specification.
- Added support for the Native Resource Details in the `/state/entity/details` endpoint, returned when the `native_resource_details` opt-in is enabled. 
  - Introduced a new `native_resource_details` property on the `details` object when looking up fungible or non-fungible resource entities with the entity details endpoint. This property is present when the resource has a special meaning to native blueprints, and gives extra information about the resource. For example, it identifies pool units with their linked pool, and gives the redemption value for a single unit.
  - Includes **unit** redemption value for the Validator LSU token and the unit tokens of various Pools. 
- Added new endpoint `/extensions/resource-holders/page` which returns information about all holders of the queried resource.

### Database changes
- Replaced relationship-related columns (`*_entity_id`) in the `entities` table with more generic collection implementation using `correlated_entity_*` columns.
- Replaced per-epoch validator emissions (`validator_emission_statistics` table) with their cumulative statistics (`validator_cumulative_emission_history` table).
- Added `non_fungible_data_mutable_fields` to `entities` table. Which contains list of all mutable non fungible data fields for non fungible resource entities.
- New `ledger_transaction_markers` type with the `event_global_emitter` discriminator. It represents the global emitter for each event.
- Added new `unverified_standard_metadata_*` tables. They hold **some** of the metadata entries using db-friendly (normalized) model. See https://docs.radixdlt.com/docs/metadata-standards 
- Extended list of supported entity correlations in the `entities` table.
- Renamed values of the `entity_relationship` enum type. 
- Added new `resource_holders` table. It keeps information about all holders of each fungible and non fungible resource.

## 1.6.3
Release built: 06.08.2024

### Database changes
- Removed the large `non_fungible_id_store_history` aggregate table. Queries for non fungible ids follow a similar strategy as key value stores and utilize `_definition` and `_history` tables to return data. Total supply and total minted/burned can be queried from the `resource_entity_supply_history` table.
- Renamed `non_fungible_id_data` table to `non_fungible_id_definition`.

## 1.6.1
Release built: 21.05.2024

### API Changes
- Added `well_known_addresses.locker_package` address property to the `/status/network-configuration` endpoint response.
- Added a new endpoint `/state/account-locker/page/vaults` which allows to read all resource vaults for a given AccountLocker.
- Added a new endpoint `/state/account-lockers/touched-at` which allows to read last touch state version for a given collection of AccountLockers.

### Database changes
- Added a new set of AccountLocker-related tables: `account_locker_entry_definition`, `account_locker_entry_resource_vault_definition` and `account_locker_entry_touch_history`.
- Added two new columns `account_locker_of_account_entity_id` and `account_locker_of_account_locker_entity_id` to the `entities` table filled for AccountLocker-related Vaults and KeyValueStores.
- Changed `IX_entity_vault_history_vault_entity_id_from_state_version` index to match all existing vaults rather non-fungible ones only.

## 1.5.2
Release built: 14.05.2024

- Fixed broken Core API SDK

## 1.5.1
Release built: 09.05.2024

> [!CAUTION]
> **Breaking Changes:**
> - Changed ordering of the collection returned by the `/state/key-value-store/keys` endpoint. Entries are no longer ordered by their last modification state version but rather by their first appearance on the network, descending.
> - Property `total_count` of the `/state/key-value-store/keys` endpoint is no longer provided.
> - Renamed `state.recovery_role_recovery_attempt` property from `timed_recovery_allowed_after` to `allow_timed_recovery_after` returned from `/state/entity/details` when querying for access controller.

- Fixed broken ledger state lookup (`at_ledger_state`) when using epoch-only constraint and given epoch did not result in any transactions at round `1`.
- Fixed broken (missing) package blueprint & code, and schema pagination in the `/state/entity/details` endpoint.
- Fixed unstable package blueprint and code aggregation where changes could overwrite each other if they applied to the same blueprint/package within the same ingestion batch.
- Fixed validator public key and active set aggregation where unnecessary copy of the key was stored on each epoch change.
- Fixed pagination of the `/state/validators/list` endpoint where incorrect `cursor` was generated previously.
- Fixed invalid date-time format for some entity state properties (most notably access controllers and their `recovery_role_recovery_attempt.allow_timed_recovery_after.date_time` property) that was dependent on OS-level locale setup.
- Added `ng_workers_global_loop_duration_seconds` and `ng_workers_node_loop_duration_seconds` histogram metrics measuring the time it took to process a single iteration of a given worker.
- Changed MVC controller and action names. It has no effect on the API itself, but alters prometheus `controler` and `action` labels.
  - `StateKeyValueStoreController.Items` renamed to `StateKeyValueStoreController.KeysPage`,
  - `StateNonFungibleController.Ids` renamed to `StateNonFungibleController.IdsPage`,
  - `StatisticsController.Uptime` renamed to `StatisticsController.ValidatorsUptime`,
  - `StateController` renamed to `StateEntityController`,
  - `ValidatorStateController` renamed to `StateValidatorsComponent`.
- Upgraded to .NET 8:
  - Upgraded runtime and libraries
  - Dockerfiles no longer specify custom `app` user as it comes built-in with official base images.
  - Removed now-obsolete or no-longer-needed code.
  - Prometheus integration exposes new built-in metric `httpclient_request_duration_seconds_bucket` for all registered HTTP clients.
- Reworked internal data aggregation mechanism to ease up maintenance burden.
- Reworked KVStores storage and changed API surface of this area to improve overall performance. 

### API Changes
- Changed the `MetadataInstantValue` type and its array counterpart `MetadataInstantArrayValue` to clamp the `value` property within the RFC-3339 compatible date-time year range `1583` to `9999`. Added a `unix_timestamp_seconds` property to these types to give the exact unclamped numerical timestamp value.
- Added `role_assignments` property to the `StateEntityDetailsResponsePackageDetails`. All global component details returned by the `/state/entity/details` endpoint contain role assignments now.
- Added `owning_vault_parent_ancestor_address` and `owning_vault_global_ancestor_address` properties to the response of the `/state/non-fungible/location` endpoint.
- Added new filter `manifest_badges_presented_filter` to the `/stream/transactions` endpoint which allows filtering transactions by badges presented. 
- Added new opt-in `component_royalty_config` to the `/state/entity/details` endpoint. When enabled `royalty_config` will be returned for each component.
- Use strong type definition for the `royalty_config` property of package blueprint and general components details. This is a change to OAS definition only and does not impact returned data format.
- Introduced upper limit to the overall number of the filters used in the `/stream/transactions` endpoint, defaults to 10.
- Added new endpoint `/state/account/page/resource-preferences` which allows to read resource preferences for given account.
- Added new endpoint `/state/account/page/authorized-depositors` which allows to read authorized depositors for given account.
- Added new endpoint `/state/package/page/blueprints` returning paginable iterator over package blueprints.
- Added new endpoint `/state/package/page/codes` returning paginable iterator over package codes.
- Added new endpoint `/state/entity/page/schemas` returning paginable iterator over entity schemas.
- Added new endpoint `/transaction/account-deposit-pre-validation` which allows to pre-validate if deposits can succeed based on account deposit settings and badges presented, before submitting the transaction. 
- Fixed wrong request validation logic for maximum number of items in `/state/non-fungible/data`, `/state/non-fungible/data` and `/state/non-fungible/data` endpoints.
- `limit_per_page` request parameter is no longer validated against `*MaxPageSize` API configuration parameters. In case requested limit exceeds API configuration maximum value is used. This change is meant to reduce clients need to understand and honor API configuration. 

### Database changes
- Added new `BadgePresented` to `LedgerTransactionMarkerOperationType` enum and started collecting transaction markers for badges presented in transactions.
- Column `royalty_amount` of `component_method_royalty_entry_history` table contains now the JSON payload representing the royalty amount without wrapping object.
- Changed schema area:
    - renamed `schema_history` table to `schema_entry_definition`,
    - introduced `schema_entry_aggregate_history` table that contains aggregate history of schema entries under given entity.
- Changed KVStore area:
  - dropped `key_value_store_aggregate_history` table altogether as we no longer keep track of aggregated KVStores,
  - introduced `key_value_store_entry_definition` table that defines each and every KVStore entry,
  - table `key_value_store_entry_history` references rows from `key_value_store_entry_definition` rather `key`s themselves.

## 1.4.4
Release built: 27.03.2024

### API Changes
- Improved performance `/stream/transactions` endpoint when using `manifest_class_filter` filter.

### Database changes
- Replaced `IX_ledger_transaction_markers_manifest_class_is_most_specific_~` index with two separate indexes `IX_ledger_transaction_markers_manifest_class_is_most_specific` (indexes `is_most_specific` = true only) and `IX_ledger_transaction_markers_manifest_class` (indexes all manifest classes).

## 1.4.3
Release built: 06.03.2024

### Bug fixes
- Properly indexes key value store keys in `key_value_store_entry_history` and `key_value_store_aggregate_history` tables. Previously it was possible that if the key was updated multiple times in one processed transaction batch some updates might not be indexed properly. This release fixes those issues and makes sure they are properly indexed and each change is properly returned from `/state/key-value-store/keys` and `/state/key-value-store/keys` endpoints.

## 1.4.1
Release built: 27.02.2024

### Bug fixes
- Recreated key value store keys are properly returned from `/state/key-value-store/keys` and `/state/key-value-store/data`. Previously Gateway did not return keys that were deleted and then recreated. This release fixes existing data in the database and makes sure new ingested data is properly stored in the database.

## 1.4.0
Release built: 08.02.2024

- Dropped internal `balance_changes` fallback mechanism. As of right now this information is ingested as part of regular transaction ingestion process.
- Reworked internal mechanism used to fetch network configuration. Is no longer stored in the underlying database and it is shared across all services.
- Reworked (partially) internal mechanism used to ingest ledger data by Data Aggregator to improve maintainability and simplify future extensions.
- Fixed `state_version`-based ledger state `at_ledger_state`/`from_ledger_state` constraints which could result in inaccurate lookups previously. Attempt to read from non-existent state version will result in HTTP 400 Bad Request. Previously the nearest state version would be used.

### API Changes
- Return components effective role assignments only for assigned modules.
- Added new filters for the `/stream/transactions` endpoint: `accounts_with_manifest_owner_method_calls`, `accounts_without_manifest_owner_method_calls` and `manifest_class_filter`.
- Extended response models returned by `/transaction/committed-details` and `/stream/transactions` endpoints:
    - added `manifest_instructions` optional property and a corresponding opt-in for returning original manifest of user transactions,
    - added optional `manifest_classes` property: a collection of zero or more manifest classes ordered from the most specific class to the least specific one.
- Added `permanently_rejects_at_epoch` to `/transaction/status` response for pending transactions.
- Added new endpoint `/state/key-value-store/keys/` that allows iterating over `KeyValueStore` keys.

### Database changes
- Created new `key_value_store_aggregate_history` table which will hold pointers to all key_value_store keys.
- Dropped `network_configuration` table.
- Fixed component's method royalty aggregation, added missing `component_method_royalty_aggregate_history` table.
- Changed `IX_validator_emission_statistics_validator_entity_id_epoch_num~` index to include `proposals_made` and `proposals_missed` columns in order to optimize `/statistics/validators/update` endpoint.

### Deprecations
- Obsoleted incorrectly named `access_rules_package` in favor of `role_assignment_module_package` on `NetworkConfigurationResponse.well_known_addresses`. Obsoleted property will contain effective copy of the new one for backwards compability.

## 1.3.0
Release built: 29.01.2024

Adds support for protocol updates (in general) and the anemone update in particular.

### API Changes

- Adds support for a new transaction type (flash transactions) that occur during protocol updates.
- Extends well known addresses returned from `/status/network-configuration` to include the transaction tracker address.
- **DEPRECATION** - Obsoletes the `vm_type`, `code_hash_hex` and `code_hex` properties of `StateEntityDetailsResponsePackageDetails` in favor of the `codes` collection. With the upcoming protocol upgrade it will be possible to have multiple codes per package. The obsoleted properties will contain an effective copy of the first element of the new collection for backwards compability.

### Database changes

- Moves `vm_type` to `package_code_history` table from package in `entity` table.
- Creates new `package_blueprint_aggregate_history` table which will hold pointers to all package blueprints.
- Creates new `package_code_aggregate_history` table which will hold pointers to all package codes.

## 1.2.5
Release built: 26.01.2024

- Fixed broken (incompatible) Core API SDK

## 1.2.4
Release built: 4.01.2024

- Extended validator's data returned from `/state/validators/list`: added `effective_fee_factor` field which returns `current` fee_factor and optionally `pending` change.
- Enable retries on transient database connectivity issues in gateway api.
- Enable retries on core api calls in gateway api.
- Optimized transaction balance changes; if available they're read from internal database, otherwise they use existing fallback mechanism

## 1.2.3
Release built: 19.12.2023

- Fixed exception thrown on empty validator set in the `/state/validator/list` endpoint.
- `524` status code returned instead of `500` if request takes longer than configured timeout.
- Validate if addresses provided in requests to API belong to network it is running on. 
- Fixed `500` status code returned from `/transaction/submit` when Transaction got committed before Gateway was able to store pending transaction node submission result. It'll return 200 status code from now on and log exception as information.

## 1.2.2
Release built: 22.11.2023

- Fixed invalid foreign key between `pending_transactions` and `pending_transaction_payloads` tables.
- Fixed package detail lookups to return all the blueprints and schemas.
- Optimized transaction balance changes fetch time (parallelized).

## 1.2.1 
Release built: 06.11.2023

- Fixed local development environment setup.
- Fixed missing `state` property on non-global entity state details.

## 1.2.0
Release built: 27.10.2023

- Added more strongly-typed OAS definitions for `programmatic_json` and types derived from the Core API.
- Added `resource_address` to fungible and non-fungible vault entity details in the `/state/entity/details` endpoint.
- Fixed `epoch [+ round]` based ledger state lookups.
- Fixed vault collection ordering for newly ingested data. A database wipe might be required, see information below.
- Fixed non-persisted identity/account lookups.

*Warning* This release contains a fix for a non-critical bug in the data ingestion mechanism where resource vaults were not ordered correctly by last change descending. Already ingested data will remain with the old ordering, i.e. the bugfix will only affect newly ingested data. If this matters to you, you will need to resync your database from scratch.

## 1.1.0
Release built: ~20.10.2023~ (scrapped)

- Fixed invalid HTTP status code on input validation failure.
- Changed default configuration value of MaxPageSize for endpoints to 100. Validate if max page size is higher than DefaultPageSize.
- Added new opt-in `balance_changes` to `/transaction/committed-details` returning resource balance changes for a given transaction.
- Added new opt-in `receipt_output` to `/stream/transactions`, and `/transaction/committed-details` endpoints. Temporarily set by default to true, to allow client's migration.
- Added vault-related details to lookups in `/state/entity/details` endpoint.  
- Optimized `TransactionQuerier.GetTransactions` not to fetch unnecessary data from underlying database.
- Added strongly-typed OAS definition for `programmatic_json`.
- Tuned documentation and constraints of various OAS type definitions.

## 1.0.1 
Release built: 10.10.2023

- Fixed missing `RecordTopOfDbLedger` observer call in `LedgerTransactionsProcessor`.
- Fixed invalid response model for HTTP 400 Bad Request responses on input parameter validation failure.
- Return 400 with validation error instead of 500 if `from_ledger_state` `state_version` is beyond known ledger tip.

## 1.0.0 - Babylon Launch
Release built: 28.09.2023

### What’s new?
- log warning if sql query takes longer than configured threshold (default to 250ms) for both entity framework and dapper queries.
- gather execution time metrics for all sql queries (both entity framework and dapper).

## 1.0.0-rc3 - Babylon Launch (Release Candidate 3)
Release built: Friday 22nd September 2023

### Breaking changes
- Instead of returning only the event data payload from `/stream/transactions` and `/transaction/committed-details`, the event data is now a complex object, wrapping the data payload, but also containing the emitter and event name. This allows you to properly determine which entity emitted the event.

### What’s new?
- Fixed `epoch` in `from_state_version` forward querying for migrated environments where lowest epoch number isn't 1. 
- Fixed the `validator_active_set_history` table which contains data about validator active set history. It was wrongly attached to future epoch not current one.
- Pending transaction handling has been reworked, and `/transaction/status` returns some additional fields with a lot more information regarding the status of the intent and submitted payloads. Check out the `intent_status` and `payload_status` fields. Each status is also associated with a description to help developers understand the meaning of the returned status.

## 0.5.5 RCNet v3.1 revision 4
Release built: 18.09.2023

- fix `/state/entity/details` endpoint when querying for multiple components with same schema.

## 0.5.4 RCNet v3.1 revision 3
Release built: 15.09.2023

- Fix event schema lookup in `/stream/transactions` and `/transaction/committed-details`.
- Add `non_fungible_id_location_history` table to improve NFID lookup performance.
- Add missing index to `entity_vault_history` table to improve royalty vault lookup performance.

## 0.5.3 RCNet v3.1 revision 2
Release built: 13.09.2023

- Fix incomplete entity type mapping.
- Fix non-fungible resource aggregation.
- Add `key_json` property to `StateKeyValueStoreDataRequestKeyItem` enabling JSON-based KVStore lookup.
- support remote schema assignment for generic (key value store, non fungible data) substitution.

## 0.5.2 RCNet v3.1
Release built: 07.09.2023

### What’s New?
- Add `from_state_version` to `validator_emission_statistics`.
- Returning all possible role assignment keys in `main` module for all entity types (previously only for fungible and non fungible resources).
- Fixed broken pagination for NFIDs under `/state/entity/details` and `/state/entity/page/non-fungible-vaults` endpoints.
- Fixed invalid `index_in_epoch` and `index_in_round` for genesis TX.
- Fixed virtual identity and account details.
- Fixed how TX total fee paid is calculated.

## 0.5.1 RCNet v3 revision 2
Release built: 01.09.2023

- Fix data aggregator processing custom events.

# 0.5.0 - RCNet v3
Release built: 31.08.2023

## RCNet v2 to RCNet v3 Migration Guide

Use Bech32m-encoded transaction hashes in `/transaction/committed-details` and `/transaction/status` endpoints.

Previously:
```json
{"intent_hash_hex": "efbbbfe1d0536d2f6e28cbe8f78f9fe519c4c799a9b0384b8d09e9ecdd66fcbb"}
``` 
Now:
```json
{"intent_hash": "txid_loc1lapmrzd6mwfamusjlqjaurmemla2xpx6mqygt74px72vtawjawws5rjtd4"}
```

---

### What’s New?
- state returned for access controller, pool components and account from `/state/entity/details` endpoint.
- access controller access rules returned from `/state/entity/details` endpoint.
- Added `blueprint_version` to `StateEntityDetailsResponseComponentDetails` response
- fixed `total_supply`, `total_burned` and `total_minted` for resources (i.e native XRD).
- new endpoint `/state/non-fungible/location` returns location of given non fungible id.
- Return programmatic json with type names for:
    - key-value key and data in `/state/key-value/data` endpoint
    - non fungible data in `/state/non-fungible/data` endpoint
    - events in `/transaction/committed-details` and `/stream/transactions` endpoints.
    - custom scrypto component state in `/state/entity/details` endpoint.
- New endpoint `/statistics/validators/uptime` returns validator uptime data.
- New endpoint `/state/key-value/data` returns entries of requested KeyValueStore.
- Rework in `role_assignments`. Returning all possible keys for native modules (`AccessRules`, `Metadata`, `Royalty`) and if no role is assigned pointer to owner role is returned. Same functionality applies to `MainModule` for FungibleResource and NonFungibleResource.

### Breaking Changes

- Renamed `access_rules` to `role_assignments`. Included missing `module` to role assignment key.
- Deleted non fungible ids are also returned from `/state/non-fungible/data` with null data, marked as `is_burned` with state version when they got burned.
- Transaction hashes are now exposed as Bech32m hashes instead of hex-encoded binary sequences.
- Dropped `previous_cursor` altogether from all paginable collections.

### Known Issues

- only assigned `role_assignments` keys for `main` module for non resource entities are returned. If key is not assigned it'll not be returned from API.

## Full technical changelog by minor release
### 0.4.1

- Renamed `access_rules` to `role_assignments`. Included missing `module` to role assignment key.
- Added package details to `/satus/entity/details` endpoint.

-------

# 0.4.0 - RCNet v2
Release built: 26.07.2023

## RCNet v1 to RCNet v2 Migration Guide
### What’s New?

* Some properties are now annotated with an `is_locked` field indicating whether the value is locked or is still able to be updated.
* Strongly-typed metadata values. Each metadata property is now represented as raw SBOR value and one of sixteen possible typed values, as per the [Entity Metadata docs](https://docs-babylon.radixdlt.com/main/scrypto/system/entity-metadata.html)
* Additional entity details are returned from the `/state/entity/details` endpoint:
  * Entity Role Assignments "access rules"
    * _NOTE: Expect minor field renames at RCnet v3, to better align with the new role abstractions._
  * Total supply alongside overall number of minted and burned tokens for resources.
    * _WARNING: Total supply is not currently accurate. This will be fixed at RCnet v3._
  * Package code, blueprints, schemas and royalty vaults
* New opt-in flags are supported for `/state/entity/details` endpoint:
  * `explicit_metadata` - allows to specify a collection of metadata keys you’d like to receive in the response,
  * `ancestor_identities` - to include parental entity identifiers,
  * `component_royalty_vault_valance` and `package_royalty_vault_balance` - to include royalty vaults and their respective balances where applicable,
  * `non_fungible_include_nfids` - to include a first chunk of NFIDs of a returned NF vaults.
* New opt-in flags supported for `/stream/transactions` endpoint:
  * `raw_hex` to include raw bytes of a TX,
  * `receipt_state_changes` to include low-level state changes,
  * `receipt_fee_sumary` to include TX fees,
  * `receipt_events` to include low-level events,
  * `affected_global_entities` to include list of entities affected by given TX,
* Additional transaction details in `/stream/transactions` endpoint: epoch, round, round_timestamp.
* New filters in `/stream/transactions` endpoint to limit search results:
  * `manifest_accounts_withdrawn_from_filter` - filter for TXs where given accounts have been withdrawn from,
  * `manifest_accounts_deposited_into_filter` - filter for TXs where given accounts have been deposited into,
  * `manifest_resources_filter` - filter for TXs where given resources have been used,
  * `affected_global_entities_filter` - filter for TXs where given entities have had internal updates,
  * `events_filter` - filter for TXs where the given event occurred (only Vault Deposit/Withdrawal events are currently supported)

* More validator details in `/state/validators/list` endpoint: `state_vault`, `pending_xrd_withdraw_vault`, `locked_owner_stake_unit_vault`, `pending_owner_stake_unit_unlock_vault`

### Breaking Changes

* All addresses (including internal addresses) are now represented with a single Address type.
* No more `as_string` and `as_string_collection` metadata representations - they’ve been replaced with strongly-typed models.
* Minor property renames to reflect changes in upstream engine and Core APIs

## Full technical changelog by minor release
### 0.4.0

- Renamed `mutable_data` property to `data` in `/state/non-fungible/data` endpoint.
- Opt-in properties added to `/transaction/committed-details`,`/state/entity/details` user can specify additional properties in response.
- Added opt-in royalty vault balance to `/state/entity/details` if queried entity is component or package.
- Possibility to configure max number of transaction fetched from CoreAPI in single batch by `MaxCoreApiTransactionBatchSize`. By default it's fetching 1000.
- New opt-in property `non_fungible_include_nfids` in `/state/entity/details`, `/state/entity/page/non-fungibles`, `/state/entity/page/non-fungible-vaults`, when enabled and aggregating per vault first page of non fungible resource ids is going be returned.
- Multiple address-related aliases (`ResourceAddress`, `ComponentAddress` etc.) have been combined into single generic `Address`.
- Added `explicit_metadata` parameter to  `/state/entity/details`, ` /state/entity/page/fungibles` and `/state/entity/page/non-fungibles` - if given metadata keys exist, they will be returned for top level entity and all returned resources.
- Added information about `epoch`, `round`, `round_timestamp` to `/transaction/committed-details` and `/stream/transactions` endpoints.
- Flattened `transaction` and `details` properties of `/transaction/committed-details` endpoint.
- Added all the properties and opt-ins available in `/transaction/committed-details` endpoint to `/stream/transactions` endpoint.
- Dropped `TokenAmount` type (used to represent transaction fee which was always expressed in XRDs) in favor of `BigDecimal` scalar value.
- `/status/network-configuration` endpoint returns several new well-known addresses.
- Added `image_tag` with currently deployed image tag to release information.
- Added multiple new filter options to `/stream/transactions` endpoint.
- Added `total_supply`, `total_minted`, `total_burned` to `/state/entity/details` when querying for fungible or non fungible resources.
- Unwrapped non fungible ids in `/state/non-fungible/ids`, `/state/entity/page/non-fungible-vault/ids` endpoints. They are no longer wrapped in `non_fungible_id` object.
- Dropped `transaction.referenced_entities` from `/transaction/committed-details` endpoint.
- Added `affected_global_entities` to `/transaction/committed-details` and `/stream/transactions` endpoints. To include them in response make sure to include `affected_global_entities` optin.
- New `affected_global_entities_filter` filter in `/stream/transactions`.
- Use strongly-typed metadata models.
- Return `pending_xrd_withdraw_vault`, `locked_owner_stake_unit_vault`, `pending_owner_stake_unit_unlock_vault`, `stake_vault` from `/state/validators/list` endpoint.
- Changed `access_rule_chain` to partially strongly typed `access_rules`.

### 0.3.1

- Fix `/state/non-fungible/data` to return data for all NFIDs

-------

# 0.3.0 - RCNet v1
Release built: 31.03.2023

- `/entity/overview` and `/entity/details` endpoints replaced with single `/state/entity/details`.
- Fungible and non-fungible resources are now ordered by the most recently updated.
- `/transaction/recent` endpoint replaced with `/stream/transactions` returning user and non-user transactions.
- `limit` request parameter has been renamed to `limit_per_page`.
- All enum values use `PascalCase` naming.
- `/gateway/information` replaced with two separate endpoints `/status/gateway-status` and `/status/network-configuration`
- Added new configuration section `CoreApiIntegration` where you can configure integration with core API. For now only `SubmitTransactionTimeout` setting is supported.
- `/stake/validator/list` rework, added current stake value.
- `/entity/resources` merged into `/state/entity/details` endpoint
- new paginable endpoints `/state/entity/page/{x}` for metadata, fungibles, fungible-vaults, non-fungibles, non-fungible-vaults and non-fungible-vault/ids. Cursors returned from `state/entity/details` can be later used to query for next pages in specific endpoints.
- `/transaction/committed-details` endpoint operates on `intent_hash_hex` only now

-------

# 0.1.1 - Betanet v2
Release built: 14.02.2023

- Added `/state/validators/list` endpoint: returns paginable collection of all validators ever observed; validators that are part of so called active-set (i.e. participate in consensus for current/requested epoch) are decorated with some extra information like their public_key and stake.
- `NonFungibleIdType`s `u32` and `u64` have been replaced with single `Number`.
- `TransactionStatusRequest` no longer supports `at_state_version` property.
- `EntityDetailsResponse.details` property is now optional.
- `/entity/details` endpoint supports all entities with global address now.

Fixes for the following issues:
- [#96](https://github.com/radixdlt/babylon-gateway/issues/96) - "Paging through the NonFungibleIds of a resource does not work properly" thank you `backyard-coder`!
- Incorrect calculation of balances when there are multiple vaults in a component. (Note - NFIds are still incorrectly tracked if spread across multiple vaults - this will be fixed as part of a schema overhaul)
- Fungible resource metadata is updated properly

