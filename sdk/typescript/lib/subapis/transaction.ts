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
   * Get details of committed transaction including all opt-ins by default.
   * Particular opt-ins can be skipped by passing `false` to corresponding keys
   * inside `options` configuration object
   *
   * @example <caption>Get committed transaction details without raw hex transaction</caption>
   * const details = await gatewayApi.transaction.getCommittedDetails('266cdfe0a28a761909d04761cdbfe33555ee5fdcf1db37fcf71c9a644b53e60b', { rawHex: false })
   * console.log(details.transaction)
   */
  getCommittedDetails(
    transactionIntentHashHex: string,
    options?: {
      rawHex: false
      receiptEvents: false
      receiptFeeSummary: false
      receiptStateChanges: false
    }
  ): Promise<TransactionCommittedDetailsResponse> {
    return this.innerClient.transactionCommittedDetails({
      transactionCommittedDetailsRequest: {
        intent_hash_hex: transactionIntentHashHex,
        opt_ins: {
          raw_hex: options?.rawHex ?? true,
          receipt_events: options?.receiptEvents ?? true,
          receipt_fee_summary: options?.receiptFeeSummary ?? true,
          receipt_state_changes: options?.receiptStateChanges ?? true,
        },
      },
    })
  }
}
