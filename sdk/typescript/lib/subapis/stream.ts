import { StreamApi, StreamTransactionsResponse } from '../generated'

export class Stream {
  constructor(public innerClient: StreamApi) {}

  getTransactionsList(affectedEntities?: string[], cursor?: string): Promise<StreamTransactionsResponse> {
    return this.innerClient.streamTransactions({
      streamTransactionsRequest: {
        cursor,
        affected_global_entities_filter: affectedEntities,
      },
    })
  }
}
