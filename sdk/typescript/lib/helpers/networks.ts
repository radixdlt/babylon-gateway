export const RadixNetwork = {
  Mainnet: 0x01,
  Stokenet: 0x02,
  Alphanet: 0x0a,
  Betanet: 0x0b,
  Kisharnet: 0x0c,
  Ansharnet: 0x0d,
  Gilganet: 0x20,
  Enkinet: 0x21,
  Hammunet: 0x22,
  Nergalnet: 0x23,
  Mardunet: 0x24,
  LocalNet: 0xf0,
  InternalTestNet: 0xf1,
  Simulator: 0xf2,

  RCnetV1: 0x0c,
  RCnetV2: 0x0d,
} as const

export const RadixNetworkConfig: Partial<
  Record<
    | 'RCnetV1'
    | 'RCnetV2'
    | 'Kisharnet'
    | 'Ansharnet'
    | 'Gilganet'
    | 'Enkinet'
    | 'Hammunet',
    {
      networkId: (typeof RadixNetwork)[keyof typeof RadixNetwork]
      gatewayUrl: string
      dashboardUrl?: string
    }
  >
> = {
  RCnetV1: {
    networkId: RadixNetwork.RCnetV1,
    gatewayUrl: 'https://rcnet.radixdlt.com/',
    dashboardUrl: 'https://rcnet-dashboard.radixdlt.com/',
  },
  RCnetV2: {
    networkId: RadixNetwork.RCnetV2,
    gatewayUrl: 'https://ansharnet-gateway.radixdlt.com',
    dashboardUrl: 'https://rcnet-v2-dashboard.radixdlt.com/',
  },
  Kisharnet: {
    networkId: RadixNetwork.Kisharnet,
    gatewayUrl: 'https://kisharnet-gateway.radixdlt.com/',
    dashboardUrl: 'https://kisharnet-dashboard.radixdlt.com/',
  },
  Ansharnet: {
    networkId: RadixNetwork.Ansharnet,
    gatewayUrl: 'https://ansharnet-gateway.radixdlt.com/',
    dashboardUrl: 'https://ansharnet-dashboard.radixdlt.com/',
  },
  Gilganet: {
    networkId: RadixNetwork.Gilganet,
    gatewayUrl: 'https://gilganet-gateway.radixdlt.com/',
  },
  Enkinet: {
    networkId: RadixNetwork.Enkinet,
    gatewayUrl: 'https://enkinet-gateway.radixdlt.com/',
    dashboardUrl: 'https://enkinet-dashboard.rdx-works-main.extratools.works/',
  },
  Hammunet: {
    networkId: RadixNetwork.Hammunet,
    gatewayUrl: 'https://hammunet-gateway.radixdlt.com:443/',
    dashboardUrl: 'https://hammunet-dashboard.rdx-works-main.extratools.works/',
  },
}