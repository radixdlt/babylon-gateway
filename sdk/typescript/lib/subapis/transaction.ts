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
   * const txStatusResponse = await gatewayApi.transaction.getStatus('txid_tdx_21_18g0pfaxkprvz3c5tee8aydhujmm74yeul7v824fvaye2n7fvlzfqvpn2kz')
   * console.log(txStatusResponse.status)
   */
  getStatus(transactionIntentHash: string): Promise<TransactionStatusResponse> {
    return this.innerClient.transactionStatus({
      transactionStatusRequest: {
        intent_hash: transactionIntentHash,
      },
    })
  }

  /**
   * Get details of committed transaction including all opt-ins by default.
   * Particular opt-ins can be skipped by passing `false` to corresponding keys
   * inside `options` configuration object
   *
   * @example <caption>Get committed transaction details without raw hex transaction</caption>
   * const details = await gatewayApi.transaction.getCommittedDetails('txid_tdx_21_18g0pfaxkprvz3c5tee8aydhujmm74yeul7v824fvaye2n7fvlzfqvpn2kz', { rawHex: false })
   * console.log(details.transaction)
   */
  getCommittedDetails(
    transactionIntentHash: string,
    options?: {
      rawHex: false
      receiptEvents: false
      receiptFeeSource: false
      receiptFeeSummary: false
      receiptFeeDestination: false
      receiptCostingParameters: false
      receiptStateChanges: false
      affectedGlobalEntities: false
      balanceChanges: false
      receiptOutput: false
    }
  ): Promise<TransactionCommittedDetailsResponse> {
    return this.innerClient.transactionCommittedDetails({
      transactionCommittedDetailsRequest: {
        intent_hash: transactionIntentHash,
        opt_ins: {
          raw_hex: options?.rawHex ?? true,
          receipt_events: options?.receiptEvents ?? true,
          receipt_fee_source: options?.receiptFeeSource ?? true,
          receipt_fee_destination: options?.receiptFeeDestination ?? true,
          receipt_costing_parameters: options?.receiptCostingParameters ?? true,
          receipt_fee_summary: options?.receiptFeeSummary ?? true,
          receipt_state_changes: options?.receiptStateChanges ?? true,
          affected_global_entities: options?.affectedGlobalEntities ?? true,
          balance_changes: options?.balanceChanges ?? true,
          receipt_output: options?.receiptOutput ?? true,
        },
      },
    })
  }
}
