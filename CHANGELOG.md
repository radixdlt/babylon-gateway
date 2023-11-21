## 1.2.2
Release Date: _unreleased_

- Fixed invalid foreign key between `pending_transactions` and `pending_transaction_payloads` tables.
- Fixed package detail lookups to return all the blueprints and schemas.
- Optimized transaction balance changes fetch time (parallelized).

## 1.2.1 
Release Date: 06.11.2023

- Fixed local development environment setup.
- Fixed missing `state` property on non-global entity state details.

## 1.2.0
Release Date: 27.10.2023

- Added more strongly-typed OAS definitions for `programmatic_json` and types derived from the Core API.
- Added `resource_address` to fungible and non-fungible vault entity details in the `/state/entity/details` endpoint.
- Fixed `epoch [+ round]` based ledger state lookups.
- Fixed vault collection ordering for newly ingested data. A database wipe might be required, see information below.
- Fixed non-persisted identity/account lookups.

*Warning* This release contains a fix for a non-critical bug in the data ingestion mechanism where resource vaults were not ordered correctly by last change descending. Already ingested data will remain with the old ordering, i.e. the bugfix will only affect newly ingested data. If this matters to you, you will need to resync your database from scratch.

## 1.1.0
Release Date: ~20.10.2023~ (scrapped)

- Fixed invalid HTTP status code on input validation failure.
- Changed default configuration value of MaxPageSize for endpoints to 100. Validate if max page size is higher than DefaultPageSize.
- Added new opt-in `balance_changes` to `/transaction/committed-details` returning resource balance changes for a given transaction.
- Added new opt-in `receipt_output` to `/stream/transactions`, and `/transaction/committed-details` endpoints. Temporarily set by default to true, to allow client's migration.
- Added vault-related details to lookups in `/state/entity/details` endpoint.  
- Optimized `TransactionQuerier.GetTransactions` not to fetch unnecessary data from underlying database.
- Added strongly-typed OAS definition for `programmatic_json`.
- Tuned documentation and constraints of various OAS type definitions.

## 1.0.1 
Release Date: 10.10.2023

- Fixed missing `RecordTopOfDbLedger` observer call in `LedgerTransactionsProcessor`.
- Fixed invalid response model for HTTP 400 Bad Request responses on input parameter validation failure.
- Return 400 with validation error instead of 500 if `from_ledger_state` `state_version` is beyond known ledger tip.

## 1.0.0 - Babylon Launch
Release Date: 28.09.2023

### What’s new?
- log warning if sql query takes longer than configured threshold (default to 250ms) for both entity framework and dapper queries.
- gather execution time metrics for all sql queries (both entity framework and dapper).

## 1.0.0-rc3 - Babylon Launch (Release Candidate 3)
Release Date: Friday 22nd September 2023

### Breaking changes
- Instead of returning only the event data payload from `/stream/transactions` and `/transaction/committed-details`, the event data is now a complex object, wrapping the data payload, but also containing the emitter and event name. This allows you to properly determine which entity emitted the event.

### What’s new?
- Fixed `epoch` in `from_state_version` forward querying for migrated environments where lowest epoch number isn't 1. 
- Fixed the `validator_active_set_history` table which contains data about validator active set history. It was wrongly attached to future epoch not current one.
- Pending transaction handling has been reworked, and `/transaction/status` returns some additional fields with a lot more information regarding the status of the intent and submitted payloads. Check out the `intent_status` and `payload_status` fields. Each status is also associated with a description to help developers understand the meaning of the returned status.

## 0.5.5 RCNet v3.1 revision 4
Release Date: 18.09.2023

- fix `/state/entity/details` endpoint when querying for multiple components with same schema.

## 0.5.4 RCNet v3.1 revision 3
Release Date: 15.09.2023

- Fix event schema lookup in `/stream/transactions` and `/transaction/committed-details`.
- Add `non_fungible_id_location_history` table to improve NFID lookup performance.
- Add missing index to `entity_vault_history` table to improve royalty vault lookup performance.

## 0.5.3 RCNet v3.1 revision 2
Release Date: 13.09.2023

- Fix incomplete entity type mapping.
- Fix non-fungible resource aggregation.
- Add `key_json` property to `StateKeyValueStoreDataRequestKeyItem` enabling JSON-based KVStore lookup.
- support remote schema assignment for generic (key value store, non fungible data) substitution.

## 0.5.2 RCNet v3.1
Release Date: 07.09.2023

### What’s New?
- Add `from_state_version` to `validator_emission_statistics`.
- Returning all possible role assignment keys in `main` module for all entity types (previously only for fungible and non fungible resources).
- Fixed broken pagination for NFIDs under `/state/entity/details` and `/state/entity/page/non-fungible-vaults` endpoints.
- Fixed invalid `index_in_epoch` and `index_in_round` for genesis TX.
- Fixed virtual identity and account details.
- Fixed how TX total fee paid is calculated.

## 0.5.1 RCNet v3 revision 2
Release Date: 01.09.2023

- Fix data aggregator processing custom events.

# 0.5.0 - RCNet v3
Release Date: 31.08.2023

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
Release Date: 26.07.2023

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
Release Date: 31.03.2023

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
Release Date: 14.02.2023

- Added `/state/validators/list` endpoint: returns paginable collection of all validators ever observed; validators that are part of so called active-set (i.e. participate in consensus for current/requested epoch) are decorated with some extra information like their public_key and stake.
- `NonFungibleIdType`s `u32` and `u64` have been replaced with single `Number`.
- `TransactionStatusRequest` no longer supports `at_state_version` property.
- `EntityDetailsResponse.details` property is now optional.
- `/entity/details` endpoint supports all entities with global address now.

Fixes for the following issues:
- [#96](https://github.com/radixdlt/babylon-gateway/issues/96) - "Paging through the NonFungibleIds of a resource does not work properly" thank you `backyard-coder`!
- Incorrect calculation of balances when there are multiple vaults in a component. (Note - NFIds are still incorrectly tracked if spread across multiple vaults - this will be fixed as part of a schema overhaul)
- Fungible resource metadata is updated properly

