import { TransactionApi } from '../generated'

export class Transaction {
  constructor(public innerClient: TransactionApi) {}

  getStatus(transactionIntentHashHex: string) {
    return this.innerClient.transactionStatus({
      transactionStatusRequest: {
        intent_hash_hex: transactionIntentHashHex,
      },
    })
  }

  getCommitedDetails(
    transactionIntentHashHex: string,
    stateVersion?: number
  ) {
    return this.innerClient.transactionCommittedDetails({
      transactionCommittedDetailsRequest: {
        intent_hash_hex: transactionIntentHashHex,
        ...(stateVersion
          ? {
              at_ledger_state: {
                state_version: stateVersion,
              },
            }
          : {}),
      },
    })
  }
}
