/**
 * Maximum number of addresses that can be queried at once when using /state/entity/details endpoint
 */
export const MAX_ADDRESSES_COUNT = 20

/**
 * Maximum number of NFT IDs that can be queried at once when using /state/non-fungible/data endpoint
 */
export const MAX_NFT_IDS_COUNT = 29

/**
 * Gateway API SDK version
 */
export const SDK_VERSION = import.meta.env.VITE_SDK_VERSION || '0.0.0'