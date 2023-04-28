import { StatusApi } from "../generated";

export class Status {
    constructor(public innerClient: StatusApi) {}

    getGatewayStatus() {
        return this.innerClient.gatewayStatus()
    }

    getNetworkConfiguration() {
        return this.innerClient.networkConfiguration()
    }
}