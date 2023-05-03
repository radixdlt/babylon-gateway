import {
  TransactionApi,
  TransactionCommittedDetailsResponse,
  TransactionStatusResponse,
} from '../generated'

export class Transaction {
  constructor(public innerClient: TransactionApi) {}

  /**
   * Get transaction status for given transaction id. Possible transaction statuses are: Unknown, CommittedSuccess, CommittedFailure, Pending, Rejected
   *
   * @example
   * const txStatusResponse = await gatewayApi.transaction.getStatus('266cdfe0a28a761909d04761cdbfe33555ee5fdcf1db37fcf71c9a644b53e60b')
   * console.log(txStatusResponse.status)
   */
  getStatus(
    transactionIntentHashHex: string
  ): Promise<TransactionStatusResponse> {
    return this.innerClient.transactionStatus({
      transactionStatusRequest: {
        intent_hash_hex: transactionIntentHashHex,
      },
    })
  }

  /**
   * Get details of committed transaction together with ledger state at a time of commit
   */
  getCommittedDetails(
    transactionIntentHashHex: string
  ): Promise<TransactionCommittedDetailsResponse> {
    return this.innerClient
      .transactionCommittedDetails({
        transactionCommittedDetailsRequest: {
          intent_hash_hex: transactionIntentHashHex,
        },
      })
  }
}
