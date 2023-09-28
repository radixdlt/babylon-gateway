import { GatewayApiClientSettings } from '.'
import {
  MAX_ADDRESSES_COUNT,
  MAX_NFT_IDS_COUNT,
  MAX_VALIDATORS_UPTIME_COUNT,
} from './constants'
import { Configuration as GeneratedConfiguration } from './generated'

export class RuntimeConfiguration extends GeneratedConfiguration {
  protected extendedConfiguration: GatewayApiClientSettings

  constructor(params: GatewayApiClientSettings) {
    super(params)
    this.extendedConfiguration = params
  }

  get maxAddressesCount() {
    return this.extendedConfiguration.maxAddressesCount || MAX_ADDRESSES_COUNT
  }

  get maxNftIdsCount() {
    return this.extendedConfiguration.maxNftIdsCount || MAX_NFT_IDS_COUNT
  }

  get maxValidatorsUptimeCount() {
    return (
      this.extendedConfiguration.maxValidatorsUptimeCount ||
      MAX_VALIDATORS_UPTIME_COUNT
    )
  }
}
