import { MAX_VALIDATORS_UPTIME_COUNT } from '../constants'
import {
  StatisticsApi,
  ValidatorUptimeCollectionItem,
  ValidatorsUptimeResponse,
} from '../generated'
import { chunk as chunkFn } from '../helpers'
import { RuntimeConfiguration } from '../runtime'

export class Statistics {
  constructor(
    public innerClient: StatisticsApi,
    public configuration?: RuntimeConfiguration
  ) {}

  /**
   * Returns uptime statistics for given validators. Request is split into multiple requests if there are more addresses than configured limit.
   * You can change limit by passing `maxValidatorsUptimeCount` during gateway instantiation.
   *
   * @param addresses list of validator addresses
   * @param from optional starting date (timestamp) or state verson from which uptime should be calculated
   * @param to optional end date (timestamp) or state verson from which uptime should be calculated
   * @returns list of validator uptime collection items
   */
  getValidatorsUptimeFromTo(
    addresses: string[],
    from?: Date | number,
    to?: Date | number
  ): Promise<ValidatorUptimeCollectionItem[]> {
    if (
      addresses.length >
      (this.configuration?.maxValidatorsUptimeCount ||
        MAX_VALIDATORS_UPTIME_COUNT)
    ) {
      const chunks = chunkFn(
        addresses,
        this.configuration?.maxValidatorsUptimeCount ||
          MAX_VALIDATORS_UPTIME_COUNT
      )
      return Promise.all(
        chunks.map((chunk) => this.getValidatorsUptimeFromTo(chunk, from, to))
      ).then((results) => results.flat())
    }

    return this.innerClient
      .validatorsUptime({
        validatorsUptimeRequest: {
          validator_addresses: addresses,
          from_ledger_state:
            from !== undefined
              ? from instanceof Date
                ? { timestamp: from }
                : { state_version: from }
              : undefined,
          at_ledger_state:
            to !== undefined
              ? to instanceof Date
                ? { timestamp: to }
                : { state_version: to }
              : undefined,
        },
      })
      .then((response) => response.validators.items)
  }

  getValidatorsUptime(
    validator_addresses: string[]
  ): Promise<ValidatorsUptimeResponse> {
    return this.innerClient.validatorsUptime({
      validatorsUptimeRequest: {
        validator_addresses,
      },
    })
  }
}
