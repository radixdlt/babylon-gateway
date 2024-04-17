export const RadixNetwork = {
  Mainnet: 0x01,
  Stokenet: 0x02,
  Alphanet: 0x0a,
  Betanet: 0x0b,
  Kisharnet: 0x0c,
  RCnetV1: 0x0c,
  Zabanet: 0x0e,
  RCnetV3: 0x0e,
  Gilganet: 0x20,
  Enkinet: 0x21,
  Hammunet: 0x22,
  Nergalnet: 0x23,
  Mardunet: 0x24,
  Dumunet: 0x25,
  LocalNet: 0xf0,
  InternalTestNet: 0xf1,
  Simulator: 0xf2,
} as const

export type NetworkConfig = {
  networkName: string
  networkId: (typeof RadixNetwork)[keyof typeof RadixNetwork]
  gatewayUrl: string
  dashboardUrl: string
}

export const RadixNetworkConfig: Record<string, NetworkConfig> = {
  Mainnet: {
    networkName: 'Mainnet',
    networkId: RadixNetwork.Mainnet,
    gatewayUrl: 'https://mainnet.radixdlt.com',
    dashboardUrl: 'https://dashboard.radixdlt.com',
  },
  Stokenet: {
    networkName: 'Stokenet',
    networkId: RadixNetwork.Stokenet,
    gatewayUrl: 'https://stokenet.radixdlt.com',
    dashboardUrl: 'https://stokenet-dashboard.radixdlt.com',
  },
  Kisharnet: {
    networkName: 'Kisharnet',
    networkId: RadixNetwork.Kisharnet,
    gatewayUrl: 'https://kisharnet-gateway.radixdlt.com',
    dashboardUrl: 'https://kisharnet-dashboard.radixdlt.com',
  },
  RCnetV1: {
    networkName: 'RCnetV1',
    networkId: RadixNetwork.RCnetV1,
    gatewayUrl: 'https://rcnet.radixdlt.com',
    dashboardUrl: 'https://rcnet-dashboard.radixdlt.com',
  },
  Mardunet: {
    networkName: 'Mardunet',
    networkId: RadixNetwork.Mardunet,
    gatewayUrl: 'https://mardunet-gateway.radixdlt.com',
    dashboardUrl: 'https://mardunet-dashboard.rdx-works-main.extratools.works',
  },
  Zabanet: {
    networkName: 'Zabanet',
    networkId: RadixNetwork.Zabanet,
    gatewayUrl: 'https://zabanet-gateway.radixdlt.com',
    dashboardUrl: 'https://rcnet-v3-dashboard.radixdlt.com',
  },
  RCnetV3: {
    networkName: 'RCNetV3',
    networkId: RadixNetwork.RCnetV3,
    gatewayUrl: 'https://zabanet-gateway.radixdlt.com',
    dashboardUrl: 'https://rcnet-v3-dashboard.radixdlt.com',
  },
  Gilganet: {
    networkName: 'Gilganet',
    networkId: RadixNetwork.Gilganet,
    gatewayUrl: 'https://gilganet-gateway.radixdlt.com',
    dashboardUrl: 'https://gilganet-dashboard.rdx-works-main.extratools.works',
  },
  Enkinet: {
    networkName: 'Enkinet',
    networkId: RadixNetwork.Enkinet,
    gatewayUrl: 'https://enkinet-gateway.radixdlt.com',
    dashboardUrl: 'https://enkinet-dashboard.rdx-works-main.extratools.works',
  },
  Hammunet: {
    networkName: 'Hammunet',
    networkId: RadixNetwork.Hammunet,
    gatewayUrl: 'https://hammunet-gateway.radixdlt.com',
    dashboardUrl: 'https://hammunet-dashboard.rdx-works-main.extratools.works',
  },
  Dumunet: {
    networkName: 'Dumunet',
    networkId: RadixNetwork.Dumunet,
    gatewayUrl: 'https://dumunet-gateway.radixdlt.com',
    dashboardUrl: 'https://dumunet-dashboard.rdx-works-main.extratools.works',
  },
}

export const RadixNetworkConfigById = Object.values(RadixNetworkConfig).reduce(
  (prev: Record<number, NetworkConfig>, config) => {
    prev[config.networkId] = config
    return prev
  },
  {}
)
