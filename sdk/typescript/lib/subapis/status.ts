import { GatewayStatusResponse, NetworkConfigurationResponse, StatusApi } from "../generated";

export class Status {
  constructor(public innerClient: StatusApi) {}

  /**
   * Get Gateway API version and current ledger state
   */
  getCurrent(): Promise<GatewayStatusResponse> {
    return this.innerClient.gatewayStatus()
  }

  /**
   * Get network identifier, network name and well-known network addresses
   */
  getNetworkConfiguration(): Promise<NetworkConfigurationResponse> {
    return this.innerClient.networkConfiguration()
  }
}