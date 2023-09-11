import { StatisticsApi, ValidatorsUptimeResponse } from '../generated'

export class Statistics {
  constructor(public innerClient: StatisticsApi) {}

  getValidatorsUptime(validator_addresses: string[]): Promise<ValidatorsUptimeResponse> {
    return this.innerClient.validatorsUptime({
      validatorsUptimeRequest: {
        validator_addresses,
      },
    })
  }
}
