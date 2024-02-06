/* eslint-disable max-nested-callbacks */
import { GatewayApiClient } from './../lib/index'

const fetchRequestFactory = (body: any) => ({
  body: JSON.stringify(body),
  credentials: undefined,
  headers: expect.anything(),
  method: 'POST',
})

const fetchResponseFactory = (response: any) => () => ({
  status: 200,
  json: () => response,
})

describe('State Subapi', () => {
  describe('getEntityDetails', () => {
    it('should split requests into chunks of 1 addresses', async () => {
      // Arrange
      const spy = jest.fn().mockImplementation(
        fetchResponseFactory({
          items: [],
        })
      )
      const gatewayApi = GatewayApiClient.initialize({
        fetchApi: spy,
        basePath: 'https://just-for-test.com',
        maxAddressesCount: 1,
      })

      // Act
      await gatewayApi.state.getEntityDetailsVaultAggregated(['a', 'b'])

      // Assert
      expect(spy).toHaveBeenCalledTimes(2)
      expect(spy).toHaveBeenCalledWith(
        'https://just-for-test.com/state/entity/details',
        fetchRequestFactory({
          opt_ins: {
            ancestor_identities: false,
            component_royalty_vault_balance: false,
            package_royalty_vault_balance: false,
            non_fungible_include_nfids: true,
            explicit_metadata: [],
          },
          addresses: ['a'],
          aggregation_level: 'Vault',
        })
      )
      expect(spy).toHaveBeenCalledWith(
        'https://just-for-test.com/state/entity/details',
        fetchRequestFactory({
          opt_ins: {
            ancestor_identities: false,
            component_royalty_vault_balance: false,
            package_royalty_vault_balance: false,
            non_fungible_include_nfids: true,
            explicit_metadata: [],
          },
          addresses: ['b'],
          aggregation_level: 'Vault',
        })
      )
    })

    it('should default fungible_resources and non_fungible_resources', async () => {
      // Arrange
      const spy = jest.fn().mockImplementation(
        fetchResponseFactory({
          items: [
            {
              address: 'address',
            },
          ],
          ledger_state: {
            state_version: 1,
          },
        })
      )
      const gatewayApi = GatewayApiClient.initialize({
        fetchApi: spy,
        basePath: 'https://just-for-test.com',
        maxAddressesCount: 1,
      })

      // Act
      const response = await gatewayApi.state.getEntityDetailsVaultAggregated([
        'address',
      ])

      // Assert
      expect(response).toEqual([
        {
          address: 'address',
          fungible_resources: {
            items: [],
            total_count: 0,
          },
          non_fungible_resources: {
            items: [],
            total_count: 0,
          },
        },
      ])
    })

    it('should not override exisintg fungible_resources and non_fungible_resources', async () => {
      // Arrange
      const spy = jest.fn().mockImplementation(
        fetchResponseFactory({
          items: [
            {
              address: 'address',
              fungible_resources: {
                items: [{ aggregation_level: 'Vault' }],
                total_count: 1,
              },
              non_fungible_resources: {
                items: [{ aggregation_level: 'Vault' }],
                total_count: 1,
              },
            },
          ],
          ledger_state: {
            state_version: 1,
          },
        })
      )
      const gatewayApi = GatewayApiClient.initialize({
        fetchApi: spy,
        basePath: 'https://just-for-test.com',
        maxAddressesCount: 1,
      })

      // Act
      const response = await gatewayApi.state.getEntityDetailsVaultAggregated([
        'address',
      ])

      // Assert
      expect(response).toEqual([
        {
          address: 'address',
          fungible_resources: {
            items: [{ aggregation_level: 'Vault' }],
            total_count: 1,
          },
          non_fungible_resources: {
            items: [{ aggregation_level: 'Vault' }],
            total_count: 1,
          },
        },
      ])
    })

    it('should propagate explicit_metadata to fungbile paging requests', async () => {
      // Arrange
      const spy = jest.fn().mockImplementation((a) => {
        if (a.includes('/state/entity/details')) {
          return fetchResponseFactory({
            items: [
              {
                address: 'address',
                fungible_resources: {
                  items: [{ aggregation_level: 'Vault' }],
                  next_cursor: 'eyJvIjoxMDB9',
                  total_count: 2,
                },
                non_fungible_resources: {
                  items: [{ aggregation_level: 'Vault' }],
                  total_count: 1,
                },
              },
            ],
            ledger_state: {
              state_version: 1,
            },
          })()
        } else {
          return fetchResponseFactory({
            items: [],
            ledger_state: {
              state_version: 1,
            },
          })()
        }
      })
      const gatewayApi = GatewayApiClient.initialize({
        fetchApi: spy,
        basePath: 'https://just-for-test.com',
        maxAddressesCount: 1,
      })

      // Act
      const response = await gatewayApi.state.getEntityDetailsVaultAggregated(
        ['address'],
        {
          explicitMetadata: ['name'],
        }
      )

      // Assert
      expect(spy).toHaveBeenCalledTimes(2)
      expect(spy.mock.calls).toEqual([
        [
          'https://just-for-test.com/state/entity/details',
          {
            body: '{"opt_ins":{"ancestor_identities":false,"component_royalty_vault_balance":false,"package_royalty_vault_balance":false,"non_fungible_include_nfids":true,"explicit_metadata":["name"]},"addresses":["address"],"aggregation_level":"Vault"}',
            credentials: undefined,
            headers: expect.anything(),
            method: 'POST',
          },
        ],
        [
          'https://just-for-test.com/state/entity/page/fungibles/',
          {
            body: '{"at_ledger_state":{"state_version":1},"cursor":"eyJvIjoxMDB9","address":"address","aggregation_level":"Vault","opt_ins":{"explicit_metadata":["name"]}}',
            credentials: undefined,
            headers: expect.anything(),
            method: 'POST',
          },
        ],
      ])
    })

    it('should propagate explicit_metadata & nonFungibleIncludeNfids to non-fungbile paging requests', async () => {
      // Arrange
      const spy = jest.fn().mockImplementation((a) => {
        if (a.includes('/state/entity/details')) {
          return fetchResponseFactory({
            items: [
              {
                address: 'address',
                fungible_resources: {
                  items: [{ aggregation_level: 'Vault' }],
                  total_count: 2,
                },
                non_fungible_resources: {
                  items: [{ aggregation_level: 'Vault' }],
                  next_cursor: 'eyJvIjoxMDB9',
                  total_count: 2,
                },
              },
            ],
            ledger_state: {
              state_version: 1,
            },
          })()
        } else {
          return fetchResponseFactory({
            items: [],
            ledger_state: {
              state_version: 1,
            },
          })()
        }
      })
      const gatewayApi = GatewayApiClient.initialize({
        fetchApi: spy,
        basePath: 'https://just-for-test.com',
        maxAddressesCount: 1,
      })

      // Act
      await gatewayApi.state.getEntityDetailsVaultAggregated(['address'], {
        explicitMetadata: ['name'],
      })

      // Assert
      expect(spy).toHaveBeenCalledTimes(2)
      expect(spy.mock.calls).toEqual([
        [
          'https://just-for-test.com/state/entity/details',
          {
            body: '{"opt_ins":{"ancestor_identities":false,"component_royalty_vault_balance":false,"package_royalty_vault_balance":false,"non_fungible_include_nfids":true,"explicit_metadata":["name"]},"addresses":["address"],"aggregation_level":"Vault"}',
            credentials: undefined,
            headers: expect.anything(),
            method: 'POST',
          },
        ],
        [
          'https://just-for-test.com/state/entity/page/non-fungibles/',
          {
            body: '{"at_ledger_state":{"state_version":1},"cursor":"eyJvIjoxMDB9","address":"address","aggregation_level":"Vault","opt_ins":{"non_fungible_include_nfids":true,"explicit_metadata":["name"]}}',
            credentials: undefined,
            headers: expect.anything(),
            method: 'POST',
          },
        ],
      ])
    })

    it('should propagate nonFungibleIncludeNfids=false to non-fungbile paging requests', async () => {
      // Arrange
      const spy = jest.fn().mockImplementation((a) => {
        if (a.includes('/state/entity/details')) {
          return fetchResponseFactory({
            items: [
              {
                address: 'address',
                fungible_resources: {
                  items: [{ aggregation_level: 'Vault' }],
                  total_count: 2,
                },
                non_fungible_resources: {
                  items: [{ aggregation_level: 'Vault' }],
                  next_cursor: 'eyJvIjoxMDB9',
                  total_count: 2,
                },
              },
            ],
            ledger_state: {
              state_version: 1,
            },
          })()
        } else {
          return fetchResponseFactory({
            items: [],
            ledger_state: {
              state_version: 1,
            },
          })()
        }
      })
      const gatewayApi = GatewayApiClient.initialize({
        fetchApi: spy,
        basePath: 'https://just-for-test.com',
        maxAddressesCount: 1,
      })

      // Act
      await gatewayApi.state.getEntityDetailsVaultAggregated(['address'], {
        explicitMetadata: ['name'],
        nonFungibleIncludeNfids: false,
      })

      // Assert
      expect(spy).toHaveBeenCalledTimes(2)
      expect(spy.mock.calls).toEqual([
        [
          'https://just-for-test.com/state/entity/details',
          {
            body: '{"opt_ins":{"ancestor_identities":false,"component_royalty_vault_balance":false,"package_royalty_vault_balance":false,"non_fungible_include_nfids":false,"explicit_metadata":["name"]},"addresses":["address"],"aggregation_level":"Vault"}',
            credentials: undefined,
            headers: expect.anything(),
            method: 'POST',
          },
        ],
        [
          'https://just-for-test.com/state/entity/page/non-fungibles/',
          {
            body: '{"at_ledger_state":{"state_version":1},"cursor":"eyJvIjoxMDB9","address":"address","aggregation_level":"Vault","opt_ins":{"non_fungible_include_nfids":false,"explicit_metadata":["name"]}}',
            credentials: undefined,
            headers: expect.anything(),
            method: 'POST',
          },
        ],
      ])
    })
  })

  describe('getNonFungibleData', () => {
    it('should split requests into chunks of 2 addresses', async () => {
      // Arrange
      const spy = jest.fn().mockImplementation(
        fetchResponseFactory({
          non_fungible_ids: [],
        })
      )
      const gatewayApi = GatewayApiClient.initialize({
        fetchApi: spy,
        basePath: 'https://just-for-test.com',
        maxNftIdsCount: 2,
      })

      // Act
      await gatewayApi.state.getNonFungibleData('addr', ['a', 'b', 'c'])

      // Assert
      expect(spy).toHaveBeenCalledTimes(2)
      expect(spy).toHaveBeenCalledWith(
        'https://just-for-test.com/state/non-fungible/data',
        fetchRequestFactory({
          resource_address: 'addr',
          non_fungible_ids: ['a', 'b'],
        })
      )
      expect(spy).toHaveBeenCalledWith(
        'https://just-for-test.com/state/non-fungible/data',
        fetchRequestFactory({
          resource_address: 'addr',
          non_fungible_ids: ['c'],
        })
      )
    })
  })

  describe('getAllEntityMetadata', () => {
    it('should iterate over cursors', async () => {
      // Arrange
      const entityMetadataItemFactory = (key: string) => ({
        key,
        last_updated_at_state_version: 1,
        value: {
          raw_hex: '',
          programmatic_json: undefined,
          typed: undefined,
        },
      })
      const spy = jest
        .fn()
        .mockImplementationOnce(
          fetchResponseFactory({
            items: [
              entityMetadataItemFactory('a'),
              entityMetadataItemFactory('b'),
            ],
            next_cursor: 'pointer',
          })
        )
        .mockImplementationOnce(
          fetchResponseFactory({
            items: [entityMetadataItemFactory('c')],
            next_cursor: 'pointer2',
          })
        )
        .mockImplementationOnce(
          fetchResponseFactory({
            items: [
              entityMetadataItemFactory('d'),
              entityMetadataItemFactory('e'),
            ],
          })
        )

      const gatewayApi = GatewayApiClient.initialize({
        fetchApi: spy,
        basePath: 'https://just-for-test.com',
      })

      // Act
      const response = await gatewayApi.state.getAllEntityMetadata('addr')

      // Assert
      expect(response).toEqual([
        entityMetadataItemFactory('a'),
        entityMetadataItemFactory('b'),
        entityMetadataItemFactory('c'),
        entityMetadataItemFactory('d'),
        entityMetadataItemFactory('e'),
      ])
      expect(spy).toHaveBeenCalledTimes(3)
      expect(spy).toHaveBeenCalledWith(
        'https://just-for-test.com/state/entity/page/metadata',
        fetchRequestFactory({
          address: 'addr',
        })
      )
      expect(spy).toHaveBeenCalledWith(
        'https://just-for-test.com/state/entity/page/metadata',
        fetchRequestFactory({
          cursor: 'pointer',
          address: 'addr',
        })
      )
      expect(spy).toHaveBeenCalledWith(
        'https://just-for-test.com/state/entity/page/metadata',
        fetchRequestFactory({
          cursor: 'pointer2',
          address: 'addr',
        })
      )
    })
  })

  describe('getAllValidators', () => {
    it('should iterate over cursors', async () => {
      const validatorFactory = (address: string) => ({
        address,
        active_in_epoch: undefined,
        current_stake: undefined,
        metadata: undefined,
        state: undefined,
      })
      // Arrange
      const spy = jest
        .fn()
        .mockImplementationOnce(
          fetchResponseFactory({
            validators: {
              items: [validatorFactory('a'), validatorFactory('b')],
              next_cursor: 'next_cursor',
            },
          })
        )
        .mockImplementationOnce(
          fetchResponseFactory({
            validators: {
              items: [validatorFactory('c'), validatorFactory('d')],
              next_cursor: 'next_cursor2',
            },
          })
        )
        .mockImplementationOnce(
          fetchResponseFactory({
            validators: {
              items: [validatorFactory('e')],
            },
          })
        )

      const gatewayApi = GatewayApiClient.initialize({
        fetchApi: spy,
      })

      // Act
      const response = await gatewayApi.state.getAllValidators()

      // Assert
      expect(response).toEqual([
        validatorFactory('a'),
        validatorFactory('b'),
        validatorFactory('c'),
        validatorFactory('d'),
        validatorFactory('e'),
      ])
      expect(spy).toHaveBeenCalledTimes(3)
    })
  })
})
