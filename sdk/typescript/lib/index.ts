import { RadixNetworkConfigById } from './helpers/networks'
import {
  ConfigurationParameters,
  ExtensionsApi,
  StateApi,
  StatisticsApi,
  StatusApi,
  StreamApi,
  TransactionApi,
} from './generated'
import { Extensions, State, Statistics, Status, Stream, Transaction } from './subapis'
import { RuntimeConfiguration } from './runtime'
import { normalizeBasePath } from './helpers/normalize-base-path'
import { SDK_VERSION } from './constants'

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
   * Maximum number of validator addresses that can be queried when using /statistics/validators/uptime endpoint
   */
  maxValidatorsUptimeCount?: number

  /**
   * Application name required for statistics purposes.
   */
  applicationName: string

  /**
   * Application version which can be used for statistics purposes.
   */
  applicationVersion?: string

  /**
   * Application dApp definition address which can be used for statistics purposes.
   */
  applicationDappDefinitionAddress?: string

  /**
   * Network ID of the network the client is connecting to. It's used to determine the gateway URL.
   * It's discarded if `basePath` is provided.
   */
  networkId?: number
}

export class GatewayApiClient {
  static initialize(settings: GatewayApiClientSettings) {
    const configuration = GatewayApiClient.constructConfiguration(settings)
    return new GatewayApiClient(configuration)
  }

  private static constructConfiguration(settings: GatewayApiClientSettings) {
    const basePath =
      settings.networkId && !settings.basePath
        ? RadixNetworkConfigById[settings.networkId].gatewayUrl
        : normalizeBasePath(settings?.basePath)
    const applicationName = settings?.applicationName ?? 'Unknown'

    return new RuntimeConfiguration({
      ...settings,
      basePath,
      applicationName,
      headers: {
        ...(settings?.headers ?? {}),
        'RDX-Client-Name': '@radixdlt/babylon-gateway-api-sdk',
        'RDX-Client-Version': SDK_VERSION,
        'RDX-App-Name': applicationName,
        'RDX-App-Version': settings.applicationVersion ?? 'Unknown',
        'RDX-App-Dapp-Definition':
          settings.applicationDappDefinitionAddress ?? 'Unknown',
      },
    })
  }

  state: State
  stream: Stream
  status: Status
  transaction: Transaction
  statistics: Statistics
  extensions: Extensions

  private lowLevel: {
    state: StateApi
    stream: StreamApi
    status: StatusApi
    transaction: TransactionApi
    statistics: StatisticsApi
    extensions: ExtensionsApi
  }

  constructor(configuration: RuntimeConfiguration) {
    this.lowLevel = {
      state: new StateApi(configuration),
      stream: new StreamApi(configuration),
      status: new StatusApi(configuration),
      transaction: new TransactionApi(configuration),
      statistics: new StatisticsApi(configuration),
      extensions: new ExtensionsApi(configuration),
    }

    this.state = new State(this.lowLevel.state, configuration)
    this.stream = new Stream(this.lowLevel.stream)
    this.status = new Status(this.lowLevel.status)
    this.transaction = new Transaction(this.lowLevel.transaction)
    this.statistics = new Statistics(this.lowLevel.statistics, configuration)
    this.extensions = new Extensions(this.lowLevel.extensions)
  }
}
