## 0.4.0 - RCNet-V2
_Release Date: Unreleased_

- renamed `mutable_data` property to `data` in `/state/non-fungible/data` endpoint.
- added `image_tag` with currently deployed image tag to release information.
- opt-in properties added to `/transaction/committed-details`,`/state/entity/details` user can specify additional properties in response.
- added opt-in royalty vault balance to `/state/entity/details` if queried entity is component or package.
- added possibility to query for `explicit_metadata` in  `/state/entity/details`, ` /state/entity/page/fungibles`, `/state/entity/page/non-fungibles`. If given metadata keys exist, they will be returned for top level entity and all returned resources.

### 0.3.1

- Fix `/state/non-fungible/data` to return data for all NFIDs

## 0.3.0 - RCNet-V1
_Release Date: 31.03.2023_

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


## 0.1.1 - Betanet V2
_Release Date: 14.02.2023_

- Added `/state/validators/list` endpoint: returns paginable collection of all validators ever observed; validators that are part of so called active-set (i.e. participate in consensus for current/requested epoch) are decorated with some extra information like their public_key and stake.
- `NonFungibleIdType`s `u32` and `u64` have been replaced with single `Number`.
- `TransactionStatusRequest` no longer supports `at_state_version` property.
- `EntityDetailsResponse.details` property is now optional.
- `/entity/details` endpoint supports all entities with global address now.

Fixes for the following issues:
- [#96](https://github.com/radixdlt/babylon-gateway/issues/96) - "Paging through the NonFungibleIds of a resource does not work properly" thank you `backyard-coder`!
- Incorrect calculation of balances when there are multiple vaults in a component. (Note - NFIds are still incorrectly tracked if spread across multiple vaults - this will be fixed as part of a schema overhaul)
- Fungible resource metadata is updated properly


## 0.1.0 - Betanet V1
_Release Date: 20.12.2022_

## 0.0.3
_Release Date: 17.11.2022_

## 0.0.2
_Release Date: 14.11.2022_

## 0.0.1 - Alphanet
_Release Date: 24.10.2022_
