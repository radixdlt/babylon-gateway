import { StreamApi } from './generated/apis/StreamApi'
import {
  Configuration,
  ConfigurationParameters,
  StateApi,
  StatusApi,
  TransactionApi,
} from './generated'
import { State } from './subapis/state'
import { Status, Stream } from './subapis'
import { Transaction } from './subapis/transaction'

export * from './generated'
export * from './subapis'

export type GatewayApiClientSettings = ConfigurationParameters

export class GatewayApiClient {
  static initialize(settings?: GatewayApiClientSettings) {
    const configuration = GatewayApiClient.constructConfiguration(settings)
    return new GatewayApiClient(configuration)
  }

  private static constructConfiguration(settings?: GatewayApiClientSettings) {
    return new Configuration(settings)
  }

  lowLevel: {
    transaction: TransactionApi
    state: StateApi
    stream: StreamApi
    status: StatusApi
  }

  state: State
  stream: Stream
  status: Status
  transaction: Transaction

  constructor(configuration: Configuration) {
    this.lowLevel = {
      state: new StateApi(configuration),
      stream: new StreamApi(configuration),
      status: new StatusApi(configuration),
      transaction: new TransactionApi(configuration),
    }

    this.state = new State(this.lowLevel.state)
    this.stream = new Stream(this.lowLevel.stream)
    this.status = new Status(this.lowLevel.status)
    this.transaction = new Transaction(this.lowLevel.transaction)
  }
}
