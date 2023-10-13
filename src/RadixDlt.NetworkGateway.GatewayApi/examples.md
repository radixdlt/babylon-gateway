# How to use Gateway

## State information in response
All endpoints return `ledger_state` data, it defines ledger state which was used by gateway to generate response.

Because state of entities constantly changes that data makes it clear which state was used to generate response.

## Browsing historical data
All endpoints with `at_ledger_state` parameter allows you to browse historical state and get how it looked like at given point of time.

You can specify one of:
- `state_version`
- `timestamp` (it'll basically find first state version before that timestamp and query against it)
- `epoch` (it'll query against first tx in that epoch)
- pair of (`epoch` and `round` pair) it'll query against first tx in that epoch and round

i.e: 
```
/state/entity/details

{
  "addresses": [
    "resource_rdx1tknxxxxxxxxxradxrdxxxxxxxxx009923554798xxxxxxxxxradxrd"
  ]
}
```
Will return current state (for current tip of the ledger) for entity with address: `resource_rdx1tknxxxxxxxxxradxrdxxxxxxxxx009923554798xxxxxxxxxradxrd`

```
/state/entity/details

{
  "addresses": [
    "resource_rdx1tknxxxxxxxxxradxrdxxxxxxxxx009923554798xxxxxxxxxradxrd"
  ],
  "at_ledger_state": {
    "state_version": 1000
  }
}
```
Will return state as it was at state version 1000 for entity with address: `resource_rdx1tknxxxxxxxxxradxrdxxxxxxxxx009923554798xxxxxxxxxradxrd`



## Querying for entity data, including fungible and non fungible resources.
Gateway API has different entry points which can be used to fetch entity data.

* `/state/entity/details` &rarr; returns basic entity details, which differs per entity type. Check OpenAPI specs for exact details.

* `/state/entity/metadata` &rarr; returns entity metadata, can be used as entry point or for further pagination with cursor returned from `/state/entity/details`

* `/state/entity/page/fungibles/` &rarr; can be used as entry point to fetch fungible resources for given entity or for further pagination with cursor returned from `/state/entity/details`

* `/state/entity/page/fungible-vaults/` &rarr; can be used as entry point to fetch fungible vaults for given entity and resource type or for further pagination with cursor returned from `/state/entity/details`

* `/state/entity/page/non-fungibles/` &rarr; can be used as entry point to fetch non fungible resources for given entity or for further pagination with cursor returned from `/state/entity/details`

* `/state/entity/page/non-fungible-vaults/` &rarr; can be used as entry point to fetch non fungible vaults for given entity and resource type or for further pagination with cursor returned from `/state/entity/details`

* `/state/entity/page/non-fungible-vault/ids` &rarr; can be used as entry point to fetch non fungible ids for given entity, vault and resource type or for further pagination with cursor returned from `/state/entity/details`

### Metadata
`/state/entity/details` endpoint returns only first page of metadata. If queried entity contains more items `next_cursor` will be returned, which can be used as `cursor` in `/state/entity/page/metadata` endpoint to fetch next pages.

You can also specify explicitly list of metadata keys in which you're interested in response. You can do that by filling `explicit_metadata` opt-in.

i.e:
```
/state/entity/details

{
  "addresses": [
    "resource_rdx1tknxxxxxxxxxradxrdxxxxxxxxx009923554798xxxxxxxxxradxrd"
  ],
  "opt_ins": {
    "explicit_metadata": [
      "name",
      "description"
    ]
  }
}
```

Will return regular `metadata` property in response but will also contain `explicit_metadata` with all queried items `name`, `description` (if they are defined for entity)

```
...
"explicit_metadata": {
  "total_count": 2,
  "items": [
    {
      "key": "name",
      "value": {
        "raw_hex": "5c2200010c055261646978",
        "programmatic_json": {
          "kind": "Enum",
          "variant_id": "0",
          "fields": [
            {
              "kind": "String",
              "value": "Radix"
            }
          ]
        },
        "typed": {
          "value": "Radix",
          "type": "String"
        }
      },
      "is_locked": true,
      "last_updated_at_state_version": 2
    },
    {
      "key": "description",
      "value": {
        "raw_hex": "5c2200010c9e01546865205261646978205075626c6963204e6574776f726b2773206e617469766520746f6b656e2c207573656420746f2070617920746865206e6574776f726b2773207265717569726564207472616e73616374696f6e206665657320616e6420746f2073656375726520746865206e6574776f726b207468726f756768207374616b696e6720746f206974732076616c696461746f72206e6f6465732e",
        "programmatic_json": {
          "kind": "Enum",
          "variant_id": "0",
          "fields": [
            {
              "kind": "String",
              "value": "The Radix Public Network's native token, used to pay the network's required transaction fees and to secure the network through staking to its validator nodes."
            }
          ]
        },
        "typed": {
          "value": "The Radix Public Network's native token, used to pay the network's required transaction fees and to secure the network through staking to its validator nodes.",
          "type": "String"
        }
      },
      "is_locked": true,
      "last_updated_at_state_version": 2
    }
  ]
},
...
```

### Resource aggregation
Both fungible and non fungible resources can be aggregated globally or per vault.
It's controlled by `aggregation_level` request field.

`Global` - if entity contains multiple vaults of same resource, or nested components contains vaults of same resources they will be sumed and returned as total balance.

`Vault` - each vault is returned separately, regardless if they are of same resource.

### Fungible resources

Endpoint returns only first page of fungible resources. If queried entity contains more items `next_cursor` will be returned, which can be used as `cursor` in `/state/entity/page/fungibles/` endpoint.

If you aggregated per vault. Endpoint returns only first page of vaults for each resource. If queried entity contains more items `next_cursor` will be returned, which can be used as `cursor` in `/state/entity/page/fungible-vaults/` endpoint.

### Non Fungible Resources

Endpoint returns only first page of non fungible resources. If queried entity contains more items `next_cursor` will be returned, which can be used as `cursor` in `/state/entity/page/non-fungibles/` endpoint.

If you aggregated per vault. Endpoint returns only first page of vaults for each resource. If queried entity contains more items `next_cursor` will be returned, which can be used as `cursor` in `/state/entity/page/non-fungible-vaults/` endpoint.

If `non_fungible_include_nfids` opt-in was set to true, response will contain first page of non fungible ids for each vault. If queried entity contains more items `next_cursor` will be returned, which can be used as `cursor` in `/state/entity/page/non-fungible-vault/ids` endpoint.


## Using endpoints with OptIns feature
To reduce bandwith some properties in certain endpoints are optional. You can ask for them explicitly by setting opt-in property to true.


Endpoints that currently support OptIns feature:

```
/transaction/committed-details
/stream/transactions
/state/entity/details
/state/entity/page/fungibles/
/state/entity/page/non-fungibles/
/state/entity/page/non-fungible-vaults/
```

### Example usage:
#### Request without opt-ins:

```
/transaction/committed-details

{
  "intent_hash": "txid_rdx1t44y7lrqtrmn0pq4gxgsmn035lh5glws273h0lsff37jnzj2ylls3aeumn"
}
```

Will return simple, short response with optins disabled.
```
{
  "ledger_state": {
    "network": "mainnet",
    "state_version": 5151578,
    "proposer_round_timestamp": "2023-10-11T07:21:03.167Z",
    "epoch": 36452,
    "round": 1060
  },
  "transaction": {
    "transaction_status": "CommittedSuccess",
    "state_version": 5150877,
    "epoch": 36452,
    "round": 362,
    "round_timestamp": "2023-10-11T07:18:30.417Z",
    "payload_hash": "notarizedtransaction_rdx1hjlstgpadgpyeulp5yahrcz56ymkmsukmrjv42nwzvygfl9nyfhsxccm42",
    "intent_hash": "txid_rdx1t44y7lrqtrmn0pq4gxgsmn035lh5glws273h0lsff37jnzj2ylls3aeumn",
    "fee_paid": "0.25417642453",
    "confirmed_at": "2023-10-11T07:18:30.417Z",
    "receipt": {
      "status": "CommittedSuccess",
      "output": [
        {
          "hex": "5c2100",
          "programmatic_json": null
        },
        {
          "hex": "5c90f8ef824eb480c16bbceebcc36d0e263b9ebf287cdcab710332344104f11c",
          "programmatic_json": null
        },
        {
          "hex": "5c2100",
          "programmatic_json": null
        },
        {
          "hex": "5c2100",
          "programmatic_json": null
        }
      ]
    }
  }
}
```

#### Request with opt-ins:
Let's say we're interested in transaction raw hex. We can enable it in response like that:

```
/transaction/committed-details
{
  "intent_hash": "txid_rdx1t44y7lrqtrmn0pq4gxgsmn035lh5glws273h0lsff37jnzj2ylls3aeumn",
  "opt_ins": {
    "raw_hex": true
  }
}
```

As you can see in response raw hex is now returned:
```
{
  "ledger_state": {
    "network": "mainnet",
    "state_version": 5152002,
    "proposer_round_timestamp": "2023-10-11T07:22:45.886Z",
    "epoch": 36453,
    "round": 152
  },
  "transaction": {
    "transaction_status": "CommittedSuccess",
    "state_version": 5150877,
    "epoch": 36452,
    "round": 362,
    "round_timestamp": "2023-10-11T07:18:30.417Z",
    "payload_hash": "notarizedtransaction_rdx1hjlstgpadgpyeulp5yahrcz56ymkmsukmrjv42nwzvygfl9nyfhsxccm42",
    "intent_hash": "txid_rdx1t44y7lrqtrmn0pq4gxgsmn035lh5glws273h0lsff37jnzj2ylls3aeumn",
    "fee_paid": "0.25417642453",
    "confirmed_at": "2023-10-11T07:18:30.417Z",
    "raw_hex": "4d22030221022104210707010a648e0000000000000a6e8e000000000000090b86dca62201012007208df9fdf4b8325fffdf300b0c68492ebc0fbe9f17c7fc811c68e6cb16a6eaf5f9010008000020220441038000d1f20f6eaff22df7090ddc21cf738ba70cd700c6e6854ea349ebec4530e80c086c6f636b5f666565210185e08f4fb23e2e21050000000000000000000000000000000041038000d1f20f6eaff22df7090ddc21cf738ba70cd700c6e6854ea349ebec4530e80c087769746864726177210280005da66318c6318c61f5a61b4c6318c6318cf794aa8d295f14e6318c6318c68500003468c609860bac140000000000000000000000000000000280005da66318c6318c61f5a61b4c6318c6318cf794aa8d295f14e6318c6318c68500003468c609860bac14000000000000000000000000000041038000d1f742847eb59027497d466b7404f6b6e3c3f0458c5a7da3eb54858c49ed0c147472795f6465706f7369745f6f725f61626f72742102810000000022000020200022000020220100012101200741017c16c54cf8ba3e3d3487172424b40502404812a7a9ceeda5aa544baa1c2d0f0c1e6cfbb3dd6af5b6aa047231d918e5288e1139d397064326ac4f63283da6686f2201012101200740e82f9e9a002a64cb3dbb2154e7912ef25720c92e0b86a7b0d090ff4c9d9992ef435b5c6d08e577f50423f8a9b831b3fbb9faecf16399a386b8412d2a53f3450f",
    "receipt": {
      "status": "CommittedSuccess",
      "output": [
        {
          "hex": "5c2100",
          "programmatic_json": null
        },
        {
          "hex": "5c90f8ef824eb480c16bbceebcc36d0e263b9ebf287cdcab710332344104f11c",
          "programmatic_json": null
        },
        {
          "hex": "5c2100",
          "programmatic_json": null
        },
        {
          "hex": "5c2100",
          "programmatic_json": null
        }
      ]
    }
  }
}
```

## Using `/transaction/stream` endpoint

### State version

You can narrow range of transactions by specifying ledger state boundaries with `at_ledger_state` and `from_ledger_state` filters.

`at_ledger_state` let's you specify under which state version you want to query. It's same for almost all endpoints where you can basically travel in time on ledger. Let's say it's currently state version 347 062 but for any reason you'd like to check how ledger looked like at state version `100`  If not specified it'll always query against current tip of the ledger.

i.e 
```
/stream/transactions
{
  "at_ledger_state": {"state_version": 100 }
}
```
 
will return transaction stream as it was for state version `100`  (by default all user transactions in desceding order)

`from_ledger_state` let's you specify lower boundary of state versions.

i.e
```
/stream/transactions

{
  "from_ledger_state": {"state_version": 100 },
}
```


Will return transaction stream starting from tip of the ledger till state version `300` (by default all user transactions in desceding order)

You can combine these two
I.e
```
/stream/transactions

{
  "from_ledger_state": {"state_version": 100 },
  "at_ledger_state": {"state_version": 300 }
}

```

Will return all user transactions transactions between state version `100` and `300` in desceding order.


### Supported filters

Endpoint allows to get transaction stream based on specified filters. Transaction has to satisfy all filters in order to be returned. 

Currently supported filters:

`kind_filter` - one of `User`, `EpochChange`, `All`. You can use that filter to query for certain kinds of transactions. Defaults to `User`.

`manifest_accounts_withdrawn_from_filter` - allows to specify array of addresses. If specified, response will contain only transactions with manifest containing withdraw from given addresses.

`manifest_accounts_deposited_into_filter` - similar to withdrawn, but will return only transactions with manifest containing deposit to given addresses. 

`manifest_resources_filter` - allows to specify array of addresses. If specified, response will contain only transactions containing given resources in manifest. Regardless of their usage.

`affected_global_entities_filter` - allows to specify array of addresses. If specified response will contain transactions that affected all of given global entities. Affected global entities are defined by substates published as result of transaction. Each substate references entity, if that entity is global it's considered affected global entity, if it's not global then it's global ancestor is treated as affected global entity.

`events_filter` - allows to filter transaction stream based on published events. Currently only deposit and withdrawal events are supported. 

It's complex object where you can specify:
- `event` - type of event (required)  i.e `{"event": "withdrawal"}` 
- `emitter_address` - will return all transactions where given entity emitted event. i.e `{"emitter_address" : "consensusmanager_rdx1scxxxxxxxxxxcnsmgrxxxxxxxxx000999665565xxxxxxxxxcnsmgr"}` 
- `resource_address` - only transactions deposit/withdrawal events for given resource will be returned i.e `{"resource_address" : "resource_rdx1tknxxxxxxxxxradxrdxxxxxxxxx009923554798xxxxxxxxxradxrd"}` 

You can combine multiple event filters, it'll look for transactions that satisfies all conditions. i.e you can query for all transactions, where given entity emitted deposit event for specified resource:

```
{
  "events_filter": [
    {
    "event": "Deposit",
    "emitter_address": "consensusmanager_rdx1scxxxxxxxxxxcnsmgrxxxxxxxxx000999665565xxxxxxxxxcnsmgr",
    "resource_address": "resource_rdx1tknxxxxxxxxxradxrdxxxxxxxxx009923554798xxxxxxxxxradxrd"
    }
  ]
}
```



### Pagination

If `limit_per_page` request parameter is lower than number of transactions to be returned, `next_cursor` will be returned. Use it in next request as `cursor` parameter to fetch further pages.

i.e:
```
{
  "kind_filter":"All",
  "from_ledger_state": {"state_version": 100 },
  "at_ledger_state": {"state_version": 300 },
  "limit_per_page":1
}
```
will return 
```
{
...
"next_cursor": "eyJ2IjoyOTl9",
...
}
```

use that in next query to fetch next pages:
```
{
  "kind_filter":"All",
  "from_ledger_state": {"state_version": 100 },
  "at_ledger_state": {"state_version": 300 },
  "limit_per_page":1,
  "cursor": "eyJ2IjoyOTl9"
}
```

### Ordering
By default ordering in that endpoint is **descending** (highest state version first), so when using cursor you'll fetch newest first and with each page you'll fetch older transactions.

You can change default behaviour by seting `order` parameter to `Asc`

```
{ 
  ...
  "order": "Asc" 
  ...
}
```
Keep in mind that if order is ascending you'll start fetching transactions from oldest first and with each page you'll fetch newer transactions.

### Reading events to find out what happened in certain dApp.
### TODO PP:

Let's imagine real life scenario that we have created some dApp and we want to get list of all transactions and events 

To do that we can

## How role assignment works
Global components define list of different roles which controlls access to their features. To get list of role assignment definition use `/state/entity/details` endpoint.

i.e let's take a look at XRD resource.
```
/state/entity/details

{
  "addresses": [
    "resource_rdx1tknxxxxxxxxxradxrdxxxxxxxxx009923554798xxxxxxxxxradxrd"
  ],
}
```

Response will contain two main parts. Definition of owner role and list of all role assignments for that component:

```
"role_assignments": {
  "owner": {
    ...
  },
  "entries": [
    ...
  ]
}
```



### Owner
```
  "owner": {
    "rule": {
      "type": "Protected",
      "access_rule": {
        "type": "ProofRule",
        "proof_rule": {
          "type": "Require",
          "requirement": {
            "type": "NonFungible",
            "non_fungible": {
              "local_id": {
                "id_type": "Integer",
                "sbor_hex": "5cc0010000000000000000",
                "simple_rep": "#0#"
              },
              "resource_address": "resource_rdx1nfxxxxxxxxxxsystxnxxxxxxxxx002683325037xxxxxxxxxsystxn"
            }
          }
        }
      }
    },
    "updater": "None"
  },
```
`rule` - defines what are the requirements to be treated as owner of given component. In below example you need to present certain non fungible id.

`updater` - defines updater role for certain role. In short words what role can update `Owner` role. In above example `None` which means it can't be changed.

### Entries 
list of all role assignment keys. 

Let's take a look at one of the items:
```
"role_key": {
  "module": "Main",
  "name": "burner"
},
"assignment": {
  "resolution": "Explicit",
  "explicit_rule": {
    "type": "Protected",
    "access_rule": {
      "type": "ProofRule",
      "proof_rule": {
        "type": "Require",
        "requirement": {
          "type": "NonFungible",
          "non_fungible": {
            "local_id": {
              "id_type": "Bytes",
              "sbor_hex": "5cc0022068c44e9d432e71c51e2a2ac285897b372328d8b31374ff29131bc6b25b6bd070",
              "simple_rep": "[68c44e9d432e71c51e2a2ac285897b372328d8b31374ff29131bc6b25b6bd070]"
            },
            "resource_address": "resource_rdx1nfxxxxxxxxxxglcllrxxxxxxxxx002350006550xxxxxxxxxglcllr"
          }
        }
      }
    }
  }
},
"updater_roles": [
  {
    "module": "Main",
    "name": "burner_updater"
  }
]
```

`role_key` - complex key consisting of module name and name of the role inside module.

`assignment` - role assignment definition. It could be either `Explicit` as in above example and contain rule or `Owner` which means it's same as Owner role.

`updater_roles` - similar as for owner role, it defines list of `role_key`'s which protect updating role. 


## How to query content of KeyValueStore inside component

### Creating component
Imagine a component with two XRD vaults - parse state and determine value of each vault
Let's say we created MultiVault which holds multiple vaults inside internal `KeyValueStore`. To make it clear, let's say we instantiated component that using below blueprint:
```
use scrypto::prelude::*;

#[blueprint]
mod multi_vault {
    use scrypto::prelude::Vault;

    struct MultiVault {
        token_vaults: KeyValueStore<String, Vault>
    }

    impl MultiVault {
        pub fn instantiate_multivault() -> Global<MultiVault> {
            Self {
                token_vaults: KeyValueStore::new()
            }
            .instantiate()
            .prepare_to_globalize(OwnerRole::None)
            .globalize()
        }

        pub fn deposit_to_vault(&mut self, vault_id: String, deposit: Bucket) {
            let tmp_token_vaults = self.token_vaults.get_mut(&vault_id);
            match tmp_token_vaults {
                Some(mut vault) => vault.put(deposit),
                None => {
                    drop(tmp_token_vaults);
                    self.token_vaults.insert(vault_id, Vault::with_bucket(deposit))
                }
            }
        }
    }
}

```

### Querying for `KeyValueStore` address

Let's say that after creating it we created 3 vaults "1", "2", "3" and transfered some amount of resource to them.

If you'd like to get content of `KeyValueStore` you need to firstly get it's address. You can do that i.e by calling `/state/entity/details` with instantiated component:

```
/state/entity/details

{
  "addresses": [
    "component_tdx_2_1crmcapqnz2sex9u6tppnagps64lgpfmkupmyrwazeg5qe3x2z3trcr"
  ]
}
```

In response you'll receive state of that component which will contain address of that `KeyValueStore`

```
      "details": {
      ...
        "state": {
          "kind": "Tuple",
          "type_name": "MultiVault",
          "fields": [
            {
              "kind": "Own",
              "type_name": "KeyValueStore",
              "field_name": "token_vaults",
              "value": "internal_keyvaluestore_tdx_2_1kzjd929eqlzd9n02uuj8jd48705vcrpvhv4mgxnaltrgystnca3qxk"
            }
          ]
        },
      ...
```

### Querying for each Key content
After that you can use `/state/key-value-store/data` endpoint to query each key content. To do that you can either use key in form of `json` or `hex`.

Assuming keys in our `KeyValueStore` are strings, each key is identified as simple JSON with two properties, `kind` and `value`:

```      
"key_json": {
    "kind": "String",
    "value": "{Id}"
}
```

And we can use that json in `/state/key-value-store/data` simply replacing {Id} with our vault id. i.e for "1":

```
/state/key-value-store/data 

{
"key_value_store_address": "internal_keyvaluestore_tdx_2_1kzjd929eqlzd9n02uuj8jd48705vcrpvhv4mgxnaltrgystnca3qxk",
  "keys": [
    {
      "key_json": {
        "kind": "Tuple",
        "fields": [
          {
            "kind": "string",
            "value": "1"
          }
        ]
      }
    }
  ]
}
```

it'll respond with content which is held under given key:

```
{
  ...
  "key_value_store_address": "internal_keyvaluestore_tdx_2_1kzjd929eqlzd9n02uuj8jd48705vcrpvhv4mgxnaltrgystnca3qxk",
  "entries": [
    {
      "key": {
        "raw_hex": "5c0c0131",
        "programmatic_json": {
          "value": "1",
          "kind": "String"
        }
      },
      "value": {
        "raw_hex": "5c90588c6d59227f64bd7fc68e38bf7c7013cf179a78d5562ce9378b1378e2fa",
        "programmatic_json": {
          "value": "internal_vault_tdx_2_1tzxx6kfz0ajt6l7x3cut7lrsz0830xnc64tze6fh3vfh3ch6587c5d",
          "kind": "Own",
          "type_name": "Vault"
        }
      },
    ...
    }
  ]
}
```


