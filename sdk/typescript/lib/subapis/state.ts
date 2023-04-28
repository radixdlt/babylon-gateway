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

  async getVaultEntityDetails(
    addresses: string
  ): Promise<StateEntityDetailsVaultResponseItem>
  async getVaultEntityDetails(
    addresses: string[]
  ): Promise<StateEntityDetailsVaultResponseItem[]>
  async getVaultEntityDetails(
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
