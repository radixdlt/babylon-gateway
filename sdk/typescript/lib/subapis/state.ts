import { chunk } from '../helpers/chunk'
import {
  exhaustPagination,
  exhaustPaginationWithLedgerState,
} from '../helpers/exhaust-pagination'
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
  StateEntityDetailsResponseItem,
  StateEntityMetadataPageResponse,
  StateNonFungibleDetailsResponseItem,
  StateNonFungibleIdsResponse,
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
   * If any of the requests fail, the whole operation will fail.
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

    const { items } = await this.innerClient.stateEntityDetails({
      stateEntityDetailsRequest: {
        addresses: isArray ? addresses : [addresses],
        aggregation_level: ResourceAggregationLevel.Vault,
        opt_ins: {
          ancestor_identities: options?.ancestorIdentities ?? false,
          component_royalty_vault_balance:
            options?.componentRoyaltyVaultBalance ?? false,
          package_royalty_vault_balance:
            options?.packageRoyaltyVaultBalance ?? false,
          non_fungible_include_nfids: options?.nonFungibleIncludeNfids ?? true,
          explicit_metadata: options?.explicitMetadata ?? [],
        },
        at_ledger_state: ledgerState,
      },
    })
    return isArray
      ? (items as StateEntityDetailsVaultResponseItem[])
      : (items[0] as StateEntityDetailsVaultResponseItem)
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
    return exhaustPagination(
      this.getEntityMetadata.bind(this, address),
      startCursor
    )
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
    return exhaustPagination(this.getValidators.bind(this), start)
  }

  /**
   * Get paged list of validators
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
}
