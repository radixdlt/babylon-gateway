## 0.2.0 - RCNet
_Release Date: ???_

- `/transaction/recent` endpoint replaced with `/stream/transactions` returning user and non-user transactions.

### 0.1.1 - Betanet V2
_Release Date: 14.02.2022_

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

### 0.0.3
_Release Date: 17.11.2022_

### 0.0.2
_Release Date: 14.11.2022_

## 0.0.1 - Alphanet
_Release Date: 24.10.2022_
