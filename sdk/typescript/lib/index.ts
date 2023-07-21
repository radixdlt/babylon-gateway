import { StreamApi } from './generated/apis/StreamApi'
import {
  ConfigurationParameters,
  StateApi,
  StatusApi,
  TransactionApi,
} from './generated'
import { State } from './subapis/state'
import { Status, Stream } from './subapis'
import { Transaction } from './subapis/transaction'
import { RuntimeConfiguration } from './runtime'
import { normalizeBasePath } from './helpers/normalize-base-path'

export * from './generated'
export * from './subapis'
export * from './helpers'

export type GatewayApiClientSettings = ConfigurationParameters & {
  /**
   * Maximum number of addresses that can be queried at once when using /state/entity/details endpoint
   */
  maxAddressesCount?: number
  /**
   * Maximum number of NFT IDs that can be queried at once when using /state/non-fungible/data endpoint
   */
  maxNftIdsCount?: number
  /**
   * This is only used for statistics purposes. It is not required for any functionality.
   */
  dAppDefinitionAddress?: string
}

export class GatewayApiClient {
  static initialize(settings?: GatewayApiClientSettings) {
    const configuration = GatewayApiClient.constructConfiguration(settings)
    return new GatewayApiClient(configuration)
  }

  private static constructConfiguration(settings?: GatewayApiClientSettings) {
    const basePath = normalizeBasePath(settings?.basePath)
    return new RuntimeConfiguration({
      ...settings,
      basePath,
      headers: {
        ...(settings?.headers ?? {}),
        ...(settings?.dAppDefinitionAddress
          ? { 'dapp-definition-address': settings.dAppDefinitionAddress }
          : {}),
      },
    })
  }

  state: State
  stream: Stream
  status: Status
  transaction: Transaction

  private lowLevel: {
    state: StateApi
    stream: StreamApi
    status: StatusApi
    transaction: TransactionApi
  }

  constructor(configuration: RuntimeConfiguration) {
    this.lowLevel = {
      state: new StateApi(configuration),
      stream: new StreamApi(configuration),
      status: new StatusApi(configuration),
      transaction: new TransactionApi(configuration),
    }

    this.state = new State(this.lowLevel.state, configuration)
    this.stream = new Stream(this.lowLevel.stream)
    this.status = new Status(this.lowLevel.status)
    this.transaction = new Transaction(this.lowLevel.transaction)
  }
}
