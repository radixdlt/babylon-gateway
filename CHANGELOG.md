### 0.1.1
_Release Date: ???_

- Added `/state/validators/list` endpoint: returns paginable collection of all validators ever observed; validators that are part of so called active-set (i.e. participate in consensus for current/requested epoch) are decorated with some extra information like their public_key and stake.
- `NonFungibleIdType`s `u32` and `u64` have been replaced with single `Number`.
- `TransactionStatusRequest` no longer supports `at_state_version` property.
- `EntityDetailsResponse.details` property is now optional.
- `/entity/details` endpoint supports all entities with global address now.

## 0.1.0 - Betanet
_Release Date: 20.12.2022_

### 0.0.3
_Release Date: 17.11.2022_

### 0.0.2
_Release Date: 14.11.2022_

## 0.0.1 - Alphanet
_Release Date: 24.10.2022_
