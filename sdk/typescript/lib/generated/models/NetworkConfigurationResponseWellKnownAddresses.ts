/* tslint:disable */
/* eslint-disable */
/**
 * Radix Gateway API - Babylon
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers, and for light queries from front-end dApps. For exchange/asset integrations, back-end dApp integrations, or simple use cases, you should consider using the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  All responses may have additional fields added at any release, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects.  When the Radix protocol is updated, new functionality may be added, and so discriminated unions returned by the API may need to be updated to have new variants added, corresponding to the updated data. Clients may need to update in advance to be able to handle these new variants when a protocol update comes out.  On the very rare occasions we need to make breaking changes to the API, these will be warned in advance with deprecation notices on previous versions. These deprecation notices will include a safe migration path. Deprecation notes or breaking changes will be flagged clearly in release notes for new versions of the Gateway.  The Gateway DB schema is not subject to any compatibility guarantees, and may be changed at any release. DB changes will be flagged in the release notes so clients doing custom DB integrations can prepare. 
 *
 * The version of the OpenAPI document: v1.2.0
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */

import { exists, mapValues } from '../runtime';
/**
 * 
 * @export
 * @interface NetworkConfigurationResponseWellKnownAddresses
 */
export interface NetworkConfigurationResponseWellKnownAddresses {
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    xrd: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    secp256k1_signature_virtual_badge: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    ed25519_signature_virtual_badge: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    package_of_direct_caller_virtual_badge: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    global_caller_virtual_badge: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    system_transaction_badge: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    package_owner_badge: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    validator_owner_badge: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    account_owner_badge: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    identity_owner_badge: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    package_package: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    resource_package: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    account_package: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    identity_package: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    consensus_manager_package: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    access_controller_package: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    transaction_processor_package: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    metadata_module_package: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    royalty_module_package: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    access_rules_package: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    genesis_helper_package: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    faucet_package: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    consensus_manager: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    genesis_helper: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    faucet: string;
    /**
     * Bech32m-encoded human readable version of the address.
     * @type {string}
     * @memberof NetworkConfigurationResponseWellKnownAddresses
     */
    pool_package: string;
}

/**
 * Check if a given object implements the NetworkConfigurationResponseWellKnownAddresses interface.
 */
export function instanceOfNetworkConfigurationResponseWellKnownAddresses(value: object): boolean {
    let isInstance = true;
    isInstance = isInstance && "xrd" in value;
    isInstance = isInstance && "secp256k1_signature_virtual_badge" in value;
    isInstance = isInstance && "ed25519_signature_virtual_badge" in value;
    isInstance = isInstance && "package_of_direct_caller_virtual_badge" in value;
    isInstance = isInstance && "global_caller_virtual_badge" in value;
    isInstance = isInstance && "system_transaction_badge" in value;
    isInstance = isInstance && "package_owner_badge" in value;
    isInstance = isInstance && "validator_owner_badge" in value;
    isInstance = isInstance && "account_owner_badge" in value;
    isInstance = isInstance && "identity_owner_badge" in value;
    isInstance = isInstance && "package_package" in value;
    isInstance = isInstance && "resource_package" in value;
    isInstance = isInstance && "account_package" in value;
    isInstance = isInstance && "identity_package" in value;
    isInstance = isInstance && "consensus_manager_package" in value;
    isInstance = isInstance && "access_controller_package" in value;
    isInstance = isInstance && "transaction_processor_package" in value;
    isInstance = isInstance && "metadata_module_package" in value;
    isInstance = isInstance && "royalty_module_package" in value;
    isInstance = isInstance && "access_rules_package" in value;
    isInstance = isInstance && "genesis_helper_package" in value;
    isInstance = isInstance && "faucet_package" in value;
    isInstance = isInstance && "consensus_manager" in value;
    isInstance = isInstance && "genesis_helper" in value;
    isInstance = isInstance && "faucet" in value;
    isInstance = isInstance && "pool_package" in value;

    return isInstance;
}

export function NetworkConfigurationResponseWellKnownAddressesFromJSON(json: any): NetworkConfigurationResponseWellKnownAddresses {
    return NetworkConfigurationResponseWellKnownAddressesFromJSONTyped(json, false);
}

export function NetworkConfigurationResponseWellKnownAddressesFromJSONTyped(json: any, ignoreDiscriminator: boolean): NetworkConfigurationResponseWellKnownAddresses {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'xrd': json['xrd'],
        'secp256k1_signature_virtual_badge': json['secp256k1_signature_virtual_badge'],
        'ed25519_signature_virtual_badge': json['ed25519_signature_virtual_badge'],
        'package_of_direct_caller_virtual_badge': json['package_of_direct_caller_virtual_badge'],
        'global_caller_virtual_badge': json['global_caller_virtual_badge'],
        'system_transaction_badge': json['system_transaction_badge'],
        'package_owner_badge': json['package_owner_badge'],
        'validator_owner_badge': json['validator_owner_badge'],
        'account_owner_badge': json['account_owner_badge'],
        'identity_owner_badge': json['identity_owner_badge'],
        'package_package': json['package_package'],
        'resource_package': json['resource_package'],
        'account_package': json['account_package'],
        'identity_package': json['identity_package'],
        'consensus_manager_package': json['consensus_manager_package'],
        'access_controller_package': json['access_controller_package'],
        'transaction_processor_package': json['transaction_processor_package'],
        'metadata_module_package': json['metadata_module_package'],
        'royalty_module_package': json['royalty_module_package'],
        'access_rules_package': json['access_rules_package'],
        'genesis_helper_package': json['genesis_helper_package'],
        'faucet_package': json['faucet_package'],
        'consensus_manager': json['consensus_manager'],
        'genesis_helper': json['genesis_helper'],
        'faucet': json['faucet'],
        'pool_package': json['pool_package'],
    };
}

export function NetworkConfigurationResponseWellKnownAddressesToJSON(value?: NetworkConfigurationResponseWellKnownAddresses | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'xrd': value.xrd,
        'secp256k1_signature_virtual_badge': value.secp256k1_signature_virtual_badge,
        'ed25519_signature_virtual_badge': value.ed25519_signature_virtual_badge,
        'package_of_direct_caller_virtual_badge': value.package_of_direct_caller_virtual_badge,
        'global_caller_virtual_badge': value.global_caller_virtual_badge,
        'system_transaction_badge': value.system_transaction_badge,
        'package_owner_badge': value.package_owner_badge,
        'validator_owner_badge': value.validator_owner_badge,
        'account_owner_badge': value.account_owner_badge,
        'identity_owner_badge': value.identity_owner_badge,
        'package_package': value.package_package,
        'resource_package': value.resource_package,
        'account_package': value.account_package,
        'identity_package': value.identity_package,
        'consensus_manager_package': value.consensus_manager_package,
        'access_controller_package': value.access_controller_package,
        'transaction_processor_package': value.transaction_processor_package,
        'metadata_module_package': value.metadata_module_package,
        'royalty_module_package': value.royalty_module_package,
        'access_rules_package': value.access_rules_package,
        'genesis_helper_package': value.genesis_helper_package,
        'faucet_package': value.faucet_package,
        'consensus_manager': value.consensus_manager,
        'genesis_helper': value.genesis_helper,
        'faucet': value.faucet,
        'pool_package': value.pool_package,
    };
}

