import { ExtensionsApi, ResourceHoldersResponse } from "../generated";

export class Extensions {
  constructor(public innerClient: ExtensionsApi) { }

  /**
   * Get holders of a specific resource
   */
  getResourceHolders(resourceAddress: string, cursor?: string): Promise<ResourceHoldersResponse> {
    return this.innerClient.resourceHoldersPage({
      resourceHoldersRequest: {
        resource_address: resourceAddress,
        cursor
      }
    })
  }
}