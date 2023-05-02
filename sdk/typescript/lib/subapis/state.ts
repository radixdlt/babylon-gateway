import {
  FungibleResourcesCollection,
  FungibleResourcesCollectionItemVaultAggregated,
  NonFungibleResourcesCollection,
  NonFungibleResourcesCollectionItemVaultAggregated,
  ResourceAggregationLevel,
  StateApi,
  StateEntityDetailsResponseItem,
} from '../generated'

type ReplaceItems<T, U> = Omit<T, 'items'> & { items: U[] }

export type FungibleResourcesVaultCollection = ReplaceItems<
  FungibleResourcesCollection,
  FungibleResourcesCollectionItemVaultAggregated
>
export type NonFungibleResourcesVaultCollection = ReplaceItems<
  NonFungibleResourcesCollection,
  NonFungibleResourcesCollectionItemVaultAggregated
>

export type StateEntityDetailsVaultResponseItem =
  StateEntityDetailsResponseItem & {
    fungible_resources: FungibleResourcesVaultCollection
    non_fungible_resources: NonFungibleResourcesVaultCollection
  }

export class State {
  constructor(public innerClient: StateApi) {}

  /**
   * Get detailed information about entities together with vault aggregated fungible and non-fungible resources.
   * Returns array or single item depending on input value
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
    addresses: string
  ): Promise<StateEntityDetailsVaultResponseItem>
  async getEntityDetailsVaultAggregated(
    addresses: string[]
  ): Promise<StateEntityDetailsVaultResponseItem[]>
  async getEntityDetailsVaultAggregated(
    addresses: string[] | string
  ): Promise<
    StateEntityDetailsVaultResponseItem[] | StateEntityDetailsVaultResponseItem
  > {
    const isArray = Array.isArray(addresses)
    const { items } = await this.innerClient.stateEntityDetails({
      stateEntityDetailsRequest: {
        addresses: isArray ? addresses : [addresses],
        aggregation_level: ResourceAggregationLevel.Vault,
      },
    })
    return isArray
      ? (items as StateEntityDetailsVaultResponseItem[])
      : (items[0] as StateEntityDetailsVaultResponseItem)
  }
}
