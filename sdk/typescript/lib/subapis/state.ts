import { chunk } from '../helpers/chunk'
import { exhaustPaginationWithLedgerState } from '../helpers/exhaust-pagination'
import {
  EntityMetadataItem,
  FungibleResourcesCollection,
  FungibleResourcesCollectionItemVaultAggregated,
  LedgerStateSelector,
  NonFungibleIdsCollection,
  NonFungibleResourcesCollection,
  NonFungibleResourcesCollectionItemVaultAggregated,
  ResourceAggregationLevel,
  StateApi,
  StateEntityDetailsResponse,
  StateEntityDetailsResponseItem,
  StateEntityFungiblesPageResponse,
  StateEntityMetadataPageResponse,
  StateEntityNonFungiblesPageResponse,
  StateNonFungibleDetailsResponseItem,
  StateNonFungibleIdsResponse,
  StateNonFungibleLocationResponseItem,
  ValidatorCollection,
  ValidatorCollectionItem,
} from '../generated'
import { RuntimeConfiguration } from '../runtime'

export type ReplaceProperty<
  ObjectType,
  Property extends string | number | symbol,
  NewPropertyType
> = Omit<ObjectType, Property> & { [key in Property]: NewPropertyType }

export type FungibleResourcesVaultCollection = ReplaceProperty<
  FungibleResourcesCollection,
  'items',
  FungibleResourcesCollectionItemVaultAggregated[]
>
export type NonFungibleResourcesVaultCollection = ReplaceProperty<
  NonFungibleResourcesCollection,
  'items',
  NonFungibleResourcesCollectionItemVaultAggregated[]
>

export type StateEntityDetailsOptions = {
  explicitMetadata?: string[]
  ancestorIdentities?: true
  nonFungibleIncludeNfids?: false
  packageRoyaltyVaultBalance?: true
  componentRoyaltyVaultBalance?: true
  dappTwoWayLinks?: true
  nativeResourceDetails?: true
}

export type StateEntityDetailsVaultResponseItem = Omit<
  StateEntityDetailsResponseItem,
  'fungible_resources' | 'non_fungible_resources'
> & {
  fungible_resources: FungibleResourcesVaultCollection
  non_fungible_resources: NonFungibleResourcesVaultCollection
}

export class State {
  constructor(
    public innerClient: StateApi,
    public configuration: RuntimeConfiguration
  ) {}

  /**
   * Get detailed information about entities together with vault aggregated fungible and non-fungible resources.
   * Returns an array or single item depending on input value. If array is passed, it will be split into chunks of 20 addresses
   * which will be requested separately and returned only if all requests are successful.
   *
   * Calling this function will exhaust list of all resources for each entity.
   * If any of the requests fail, the whole operation will fail.
   *
   * When requesting details for `internal_vault` entity, `non_fungible_resources` and `fungible_resources` will be defaulted to objects with empty arrays
   * in order to keep backward compatibility. You should look up balances inside `details` object.
   *
   * You can change limit by passing `maxAddressesCount` during gateway instantiation.
   *
   * @example
   * const entityDetails = await gatewayApi.state.getEntityDetailsVaultAggregated('account_tdx_21_1p823h2sq7nsefkdharvvh5')
   * console.log(entityDetails.fungible_resources.items, entityDetails.non_fungible_resources.items)
   *
   * @example
   * const entities = await gatewayApi.state.getEntityDetailsVaultAggregated(['account_tdx_21_1p823h2sq7nsefkdharvvh5'])
   * console.log(entities[0].fungible_resources.items, entities[0].non_fungible_resources.items)
   */
  async getEntityDetailsVaultAggregated(
    addresses: string,
    options?: StateEntityDetailsOptions,
    ledgerState?: LedgerStateSelector
  ): Promise<StateEntityDetailsVaultResponseItem>
  async getEntityDetailsVaultAggregated(
    addresses: string[],
    options?: StateEntityDetailsOptions,
    ledgerState?: LedgerStateSelector
  ): Promise<StateEntityDetailsVaultResponseItem[]>
  async getEntityDetailsVaultAggregated(
    addresses: string[] | string,
    options?: StateEntityDetailsOptions,
    ledgerState?: LedgerStateSelector
  ): Promise<
    StateEntityDetailsVaultResponseItem[] | StateEntityDetailsVaultResponseItem
  > {
    const isArray = Array.isArray(addresses)
    if (isArray && addresses.length === 0) return Promise.resolve([])
    if (isArray && addresses.length > this.configuration.maxAddressesCount) {
      const chunks = chunk(addresses, this.configuration.maxAddressesCount)
      return Promise.all(
        chunks.map((chunk) =>
          this.getEntityDetailsVaultAggregated(chunk, options, ledgerState)
        )
      ).then((results) => results.flat())
    }

    const { items, ledger_state } = await this.innerClient
      .stateEntityDetails({
        stateEntityDetailsRequest: {
          addresses: isArray ? addresses : [addresses],
          aggregation_level: ResourceAggregationLevel.Vault,
          opt_ins: {
            ancestor_identities: options?.ancestorIdentities ?? false,
            component_royalty_vault_balance:
              options?.componentRoyaltyVaultBalance ?? false,
            package_royalty_vault_balance:
              options?.packageRoyaltyVaultBalance ?? false,
            non_fungible_include_nfids:
              options?.nonFungibleIncludeNfids ?? true,
            explicit_metadata: options?.explicitMetadata ?? [],
            dapp_two_way_links: options?.dappTwoWayLinks ?? false,
            native_resource_details: options?.nativeResourceDetails ?? false,
          },
          at_ledger_state: ledgerState,
        },
      })
      .then((response) => this.ensureResourcesProperties(response))

    return isArray
      ? Promise.all(
          (items as StateEntityDetailsVaultResponseItem[]).map((item) =>
            this.queryAllResources(
              item,
              {
                explicitMetadata: options?.explicitMetadata ?? [],
                nonFungibleIncludeNfids:
                  options?.nonFungibleIncludeNfids ?? true,
              },
              ledgerState || {
                state_version: ledger_state.state_version,
              }
            )
          )
        )
      : this.queryAllResources(
          items[0] as StateEntityDetailsVaultResponseItem,
          {
            explicitMetadata: options?.explicitMetadata ?? [],
            nonFungibleIncludeNfids: options?.nonFungibleIncludeNfids ?? true,
          },
          ledgerState || {
            state_version: ledger_state.state_version,
          }
        )
  }

  /**
   * Get paged list of entity metadata
   * @param address
   * @param cursor
   */
  async getEntityMetadata(
    address: string,
    cursor?: string
  ): Promise<StateEntityMetadataPageResponse> {
    return this.innerClient.entityMetadataPage({
      stateEntityMetadataPageRequest: {
        address,
        cursor,
      },
    })
  }

  /**
   * Get list of fungibles location for given resource and ids. If ids array is larger than configured limit, it will be split into chunks and multiple requests will be made.
   * You can change limit by passing `maxNftIdsCount` during gateway instantiation.
   * @param resource - non fungible resource address
   * @param ids - non fungible resource ids to get location for
   * @returns list of non fungible location response items
   */
  async getNonFungibleLocation(
    resource: string,
    ids: string[]
  ): Promise<StateNonFungibleLocationResponseItem[]> {
    if (ids.length > this.configuration.maxNftIdsCount) {
      const chunks = chunk(ids, this.configuration.maxNftIdsCount)
      return Promise.all(
        chunks.map((chunk) => this.getNonFungibleLocation(resource, chunk))
      ).then((results) => results.flat())
    }

    return this.innerClient
      .nonFungibleLocation({
        stateNonFungibleLocationRequest: {
          resource_address: resource,
          non_fungible_ids: ids,
        },
      })
      .then((data) => data.non_fungible_ids)
  }

  /**
   * Get list of all metadata items for given entity. This will iterate over returned cursors and aggregate all responses,
   * which is why multiple API requests can be made.
   *
   * @param address - entity address
   * @param startCursor - optional cursor to start iteration from
   */
  async getAllEntityMetadata(
    address: string,
    startCursor?: string
  ): Promise<EntityMetadataItem[]> {
    return exhaustPaginationWithLedgerState(
      this.getEntityMetadata.bind(this, address),
      startCursor
    ).then((res) => res.aggregatedEntities)
  }

  /**
   * Get paged list of validators
   * @param cursor
   */
  async getValidators(cursor?: string): Promise<ValidatorCollection> {
    return this.innerClient
      .stateValidatorsList({
        stateValidatorsListRequest: {
          cursor: cursor || null,
        },
      })
      .then(({ validators }) => validators)
  }

  /**
   * Get list of all validators. This will iterate over returned cursors and aggregate all responses.
   */
  async getAllValidators(start?: string): Promise<ValidatorCollectionItem[]> {
    return exhaustPaginationWithLedgerState((cursor?: string) => {
      const v = this.getValidatorsWithLedgerState(cursor)
      return v.then((res) => ({
        items: res.validators.items,
        ledger_state: res.ledger_state,
        next_cursor: res.validators.next_cursor,
      }))
    }, start).then((res) => res.aggregatedEntities)
  }

  /**
   * Get paged list of validators with ledger state
   * @param cursor
   */
  async getValidatorsWithLedgerState(cursor?: string) {
    return this.innerClient.stateValidatorsList({
      stateValidatorsListRequest: {
        cursor: cursor || null,
      },
    })
  }

  /**
   * Get list of all validators. This will iterate over returned cursors and aggregate all responses.
   */
  async getAllValidatorsWithLedgerState(start?: string) {
    return exhaustPaginationWithLedgerState(
      (cursor?: string) =>
        this.getValidatorsWithLedgerState(cursor).then((res) => ({
          items: res.validators.items,
          ledger_state: res.ledger_state,
          next_cursor: res.validators.next_cursor,
        })),
      start
    )
  }

  /**
   *  Get paged list of non fungible ids for given non fungible resource address
   * @params address - non fungible resource address
   * @params cursor - optional cursor used for pagination
   */
  async getNonFungibleIds(
    address: string,
    ledgerState?: LedgerStateSelector,
    cursor?: string
  ): Promise<NonFungibleIdsCollection> {
    return this.innerClient
      .nonFungibleIds({
        stateNonFungibleIdsRequest: {
          resource_address: address,
          cursor,
          at_ledger_state: ledgerState,
        },
      })
      .then(({ non_fungible_ids }) => non_fungible_ids)
  }

  /**
   * Get list of non fungible ids for given non fungible resource address. This will iterate over returned cursors and aggregate all responses.
   *
   * @params address - non fungible resource address
   * @params startCursor - optional cursor to start paging from
   */
  async getAllNonFungibleIds(
    address: string,
    startCursor?: string,
    ledgerState?: LedgerStateSelector
  ): Promise<string[]> {
    let atLedgerState = ledgerState
    let next_cursor: string | null | undefined = startCursor
    const aggregatedEntities: string[] = []

    do {
      const queryFunctionResponse: StateNonFungibleIdsResponse =
        await this.innerClient.nonFungibleIds({
          stateNonFungibleIdsRequest: {
            resource_address: address,
            cursor: next_cursor,
            at_ledger_state: atLedgerState,
          },
        })
      aggregatedEntities.push(...queryFunctionResponse.non_fungible_ids.items)
      atLedgerState = atLedgerState || {
        state_version: queryFunctionResponse.ledger_state.state_version,
      }
      next_cursor = queryFunctionResponse.non_fungible_ids.next_cursor
    } while (next_cursor)

    return aggregatedEntities
  }

  async getNonFungibleData(
    address: string,
    ids: string,
    ledgerState?: LedgerStateSelector
  ): Promise<StateNonFungibleDetailsResponseItem>
  async getNonFungibleData(
    address: string,
    ids: string[],
    ledgerState?: LedgerStateSelector
  ): Promise<StateNonFungibleDetailsResponseItem[]>
  async getNonFungibleData(
    address: string,
    ids: string | string[],
    ledgerState?: LedgerStateSelector
  ): Promise<
    StateNonFungibleDetailsResponseItem | StateNonFungibleDetailsResponseItem[]
  > {
    const isArray = Array.isArray(ids)
    if (isArray && ids.length === 0) return Promise.resolve([])
    if (isArray && ids.length > this.configuration.maxNftIdsCount) {
      const chunks = chunk(ids, this.configuration.maxNftIdsCount)
      return Promise.all(
        chunks.map((chunk) =>
          this.getNonFungibleData(address, chunk, ledgerState)
        )
      ).then((results) => results.flat())
    }

    const { non_fungible_ids } = await this.innerClient.nonFungibleData({
      stateNonFungibleDataRequest: {
        resource_address: address,
        non_fungible_ids: isArray ? ids : [ids],
        at_ledger_state: ledgerState,
      },
    })
    return isArray
      ? (non_fungible_ids as StateNonFungibleDetailsResponseItem[])
      : (non_fungible_ids[0] as StateNonFungibleDetailsResponseItem)
  }

  private async getEntityFungibleVaultsPage(
    address: string,
    resourceAddress: string,
    options: {
      ledgerState?: LedgerStateSelector
      cursor?: string
    }
  ) {
    return this.innerClient.entityFungibleResourceVaultPage({
      stateEntityFungibleResourceVaultsPageRequest: {
        address,
        resource_address: resourceAddress,
        at_ledger_state: options.ledgerState,
        cursor: options.cursor,
      },
    })
  }

  private async getEntityNonFungibleVaultsPage(
    address: string,
    resourceAddress: string,
    options: {
      ledgerState?: LedgerStateSelector
      cursor?: string
      includeNfids?: boolean
    }
  ) {
    return this.innerClient.entityNonFungibleResourceVaultPage({
      stateEntityNonFungibleResourceVaultsPageRequest: {
        address,
        resource_address: resourceAddress,
        at_ledger_state: options.ledgerState,
        cursor: options.cursor,
        opt_ins: {
          non_fungible_include_nfids: options.includeNfids,
        },
      },
    })
  }

  private async getEntityFungiblesPageVaultAggregated(
    entity: string,
    options?: {
      nextCursor?: string | undefined
      ledgerState?: LedgerStateSelector
      explicitMetadata?: string[]
    }
  ): Promise<
    ReplaceProperty<
      StateEntityFungiblesPageResponse,
      'items',
      FungibleResourcesCollectionItemVaultAggregated[]
    >
  > {
    return this.innerClient.entityFungiblesPage({
      stateEntityFungiblesPageRequest: {
        address: entity,
        cursor: options?.nextCursor,
        aggregation_level: 'Vault',
        at_ledger_state: options?.ledgerState,
        opt_ins: {
          explicit_metadata: options?.explicitMetadata,
        },
      },
    }) as Promise<
      ReplaceProperty<
        StateEntityFungiblesPageResponse,
        'items',
        FungibleResourcesCollectionItemVaultAggregated[]
      >
    >
  }

  private async getEntityNonFungiblesPageVaultAggregated(
    entity: string,
    options?: {
      cursor: string | undefined
      ledgerState?: LedgerStateSelector
      explicitMetadata?: string[]
      nonFungibleIncludeNfids?: boolean
    }
  ): Promise<
    ReplaceProperty<
      StateEntityNonFungiblesPageResponse,
      'items',
      NonFungibleResourcesCollectionItemVaultAggregated[]
    >
  > {
    return this.innerClient.entityNonFungiblesPage({
      stateEntityNonFungiblesPageRequest: {
        address: entity,
        cursor: options?.cursor,
        aggregation_level: 'Vault',
        at_ledger_state: options?.ledgerState,
        opt_ins: {
          explicit_metadata: options?.explicitMetadata,
          non_fungible_include_nfids: options?.nonFungibleIncludeNfids,
        },
      },
    }) as Promise<
      ReplaceProperty<
        StateEntityNonFungiblesPageResponse,
        'items',
        NonFungibleResourcesCollectionItemVaultAggregated[]
      >
    >
  }

  private ensureResourcesProperties(
    response: StateEntityDetailsResponse
  ): ReplaceProperty<
    StateEntityDetailsResponse,
    'items',
    StateEntityDetailsVaultResponseItem[]
  > {
    return {
      ...response,
      items: response.items.map((item) => ({
        ...item,
        fungible_resources: item.fungible_resources || {
          total_count: 0,
          items: [],
        },
        non_fungible_resources: item.non_fungible_resources || {
          total_count: 0,
          items: [],
        },
      })) as StateEntityDetailsVaultResponseItem[],
    }
  }

  private async queryAllFungibles(
    stateEntityDetails: StateEntityDetailsVaultResponseItem,
    options?: {
      explicitMetadata?: string[]
    },
    ledgerState?: LedgerStateSelector
  ): Promise<StateEntityDetailsVaultResponseItem> {
    const nextCursor = stateEntityDetails?.fungible_resources?.next_cursor

    if (!nextCursor)
      return this.ensureAllFungibleVaults(stateEntityDetails, ledgerState)

    const allFungibles = await exhaustPaginationWithLedgerState(
      (cursor) =>
        this.getEntityFungiblesPageVaultAggregated(stateEntityDetails.address, {
          nextCursor: cursor,
          ledgerState,
          explicitMetadata: options?.explicitMetadata,
        }),
      nextCursor
    )

    return this.ensureAllFungibleVaults(
      {
        ...stateEntityDetails,
        fungible_resources: {
          items: [
            ...(stateEntityDetails?.fungible_resources?.items || []),
            ...allFungibles.aggregatedEntities,
          ],
        },
      },
      ledgerState
    )
  }

  private async ensureAllFungibleVaults(
    stateEntityDetails: StateEntityDetailsVaultResponseItem,
    ledgerState?: LedgerStateSelector
  ): Promise<StateEntityDetailsVaultResponseItem> {
    const fungibleResources = stateEntityDetails.fungible_resources.items

    const ensuredFungibleResourcesItems = await Promise.all(
      fungibleResources.map((item) => {
        const nextCursor = item.vaults.next_cursor

        if (!nextCursor) return Promise.resolve(item)

        return exhaustPaginationWithLedgerState(
          (cursor) =>
            this.getEntityFungibleVaultsPage(
              stateEntityDetails.address,
              item.resource_address,
              {
                ledgerState,
                cursor,
              }
            ),
          nextCursor
        ).then((aggregatedVaults) => ({
          ...item,
          vaults: {
            items: [
              ...item.vaults.items,
              ...aggregatedVaults.aggregatedEntities,
            ],
            total_count: item.vaults.total_count,
          },
        }))
      })
    )
    return Promise.resolve({
      ...stateEntityDetails,
      fungible_resources: {
        ...stateEntityDetails.fungible_resources,
        items: ensuredFungibleResourcesItems,
      },
    })
  }

  private async ensureAllNonFungileVaults(
    stateEntityDetails: StateEntityDetailsVaultResponseItem,
    ledgerState?: LedgerStateSelector
  ) {
    const nonFungibleResources = stateEntityDetails.non_fungible_resources.items

    const ensuredNonFungibleResourcesItems = await Promise.all(
      nonFungibleResources.map((item) => {
        const nextCursor = item.vaults.next_cursor

        if (!nextCursor) return Promise.resolve(item)

        return exhaustPaginationWithLedgerState(
          (cursor) =>
            this.getEntityNonFungibleVaultsPage(
              stateEntityDetails.address,
              item.resource_address,
              {
                ledgerState,
                cursor,
              }
            ),
          nextCursor
        ).then((aggregatedVaults) => ({
          ...item,
          vaults: {
            items: [
              ...item.vaults.items,
              ...aggregatedVaults.aggregatedEntities,
            ],
            total_count: item.vaults.total_count,
          },
        }))
      })
    )
    return Promise.resolve({
      ...stateEntityDetails,
      non_fungible_resources: {
        ...stateEntityDetails.non_fungible_resources,
        items: ensuredNonFungibleResourcesItems,
      },
    })
  }

  private async queryAllNonFungibles(
    stateEntityDetails: StateEntityDetailsVaultResponseItem,
    options?: {
      explicitMetadata?: string[]
      nonFungibleIncludeNfids?: boolean
    },
    ledgerState?: LedgerStateSelector
  ): Promise<StateEntityDetailsVaultResponseItem> {
    const nextCursor = stateEntityDetails.non_fungible_resources.next_cursor

    if (!nextCursor)
      return this.ensureAllNonFungileVaults(stateEntityDetails, ledgerState)

    const allNonFungibles = await exhaustPaginationWithLedgerState(
      (cursor) =>
        this.getEntityNonFungiblesPageVaultAggregated(
          stateEntityDetails.address,
          {
            cursor,
            ledgerState,
            explicitMetadata: options?.explicitMetadata,
            nonFungibleIncludeNfids: options?.nonFungibleIncludeNfids,
          }
        ),
      nextCursor
    )

    return this.ensureAllNonFungileVaults(
      {
        ...stateEntityDetails,
        non_fungible_resources: {
          items: [
            ...stateEntityDetails.non_fungible_resources.items,
            ...allNonFungibles.aggregatedEntities,
          ],
        },
      },
      ledgerState
    )
  }

  private async queryAllResources(
    stateEntityDetails: StateEntityDetailsVaultResponseItem,
    options?: {
      explicitMetadata?: string[]
      nonFungibleIncludeNfids?: boolean
    },
    ledgerState?: LedgerStateSelector
  ): Promise<StateEntityDetailsVaultResponseItem> {
    const itemsWithAllFungibles = this.queryAllFungibles(
      stateEntityDetails,
      options,
      ledgerState
    )
    const itemsWithAllNonFungibles = this.queryAllNonFungibles(
      stateEntityDetails,
      options,
      ledgerState
    )

    return Promise.all([itemsWithAllFungibles, itemsWithAllNonFungibles]).then(
      (results) => ({
        ...stateEntityDetails,
        fungible_resources: {
          ...stateEntityDetails.fungible_resources,
          items: [...results[0].fungible_resources.items],
        },
        non_fungible_resources: {
          ...stateEntityDetails.non_fungible_resources,
          items: [...results[1].non_fungible_resources.items],
        },
      })
    )
  }
}
