/**
 * Maximum number of addresses that can be queried at once when using /state/entity/details endpoint
 */
export const MAX_ADDRESSES_COUNT = 20

/**
 * Maximum number of NFT IDs that can be queried at once when using /state/non-fungible/data 
 * or /state/non-fungible/location endpoint
 */
export const MAX_NFT_IDS_COUNT = 29

/**
 * Maximum number of validator addresses that can be queried when using /statistics/validators/uptime endpoint
 */
export const MAX_VALIDATORS_UPTIME_COUNT = 200

/**
 * Gateway API SDK version
 */
export const SDK_VERSION = import.meta.env.VITE_SDK_VERSION || '0.0.0'