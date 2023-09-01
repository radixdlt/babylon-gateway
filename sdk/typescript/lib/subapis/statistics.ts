import { LedgerStateSelector, StatisticsApi, ValidatorsUptimeResponse } from '../generated'

export class Statistics {
  constructor(public innerClient: StatisticsApi) { }

  getValidatorsUptime(validator_addresses: string[], from_ledger_state?: LedgerStateSelector, at_ledger_state?: LedgerStateSelector): Promise<ValidatorsUptimeResponse> {
    return this.innerClient.validatorsUptime({
      validatorsUptimeRequest: {
        validator_addresses,
        from_ledger_state,
        at_ledger_state
      },
    })
  }
}
