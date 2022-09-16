/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

using System.Diagnostics.CodeAnalysis;

namespace RadixDlt.NetworkGateway.Commons.Addressing;

public static class RadixAddressParser
{
    public static TmpAccountHashedAddress TmpParseRadixAccountHashedAddress(AddressHrps hrps, string address)
    {
        var (addressHrp, addressData, _) = RadixBech32.Decode(address);

        if (addressHrp != hrps.AccountHrp)
        {
            throw new AddressException($"Address HRP was {addressHrp} but didn't match account HRP {hrps.AccountHrp}");
        }

        var (reAddressType, publicKey) = RadixBech32.ExtractRadixEngineAddressData(addressData);

        if (reAddressType != RadixEngineAddressType.HASHED_KEY)
        {
            throw new AddressException("Address with account hrp is not of type HASHED_KEY");
        }

        return new TmpAccountHashedAddress(addressData);
    }

    public static bool TryParse(
        AddressHrps hrps,
        string address,
        [NotNullWhen(true)] out RadixAddress? radixAddress,
        [NotNullWhen(false)] out string? errorMessage
    )
    {
        try
        {
            radixAddress = Parse(hrps, address);
            errorMessage = null;
            return true;
        }
        catch (AddressException exception)
        {
            radixAddress = null;
            errorMessage = $"Failed to parse address: {exception.Message}";
            return false;
        }
    }

    public static bool TryParseAccountAddress(
        AddressHrps hrps,
        string address,
        [NotNullWhen(true)] out AccountAddress? accountAddress,
        [NotNullWhen(false)] out string? errorMessage
    )
    {
        try
        {
            accountAddress = ParseAccountAddress(hrps, address);
            errorMessage = null;
            return true;
        }
        catch (AddressException exception)
        {
            accountAddress = null;
            errorMessage = $"Failed to parse account address: {exception.Message}";
            return false;
        }
    }

    public static bool TryParseValidatorAddress(
        AddressHrps hrps,
        string address,
        [NotNullWhen(true)] out ValidatorAddress? validatorAddress,
        [NotNullWhen(false)] out string? errorMessage
    )
    {
        try
        {
            validatorAddress = ParseValidatorAddress(hrps, address);
            errorMessage = null;
            return true;
        }
        catch (AddressException exception)
        {
            validatorAddress = null;
            errorMessage = $"Failed to parse account address: {exception.Message}";
            return false;
        }
    }

    public static bool TryParseResourceAddress(
        AddressHrps hrps,
        string address,
        [NotNullWhen(true)] out ResourceAddress? resourceAddress,
        [NotNullWhen(false)] out string? errorMessage
    )
    {
        try
        {
            resourceAddress = ParseResourceAddress(hrps, address);
            errorMessage = null;
            return true;
        }
        catch (AddressException exception)
        {
            resourceAddress = null;
            errorMessage = $"Failed to parse account address: {exception.Message}";
            return false;
        }
    }

    private static AccountAddress ParseAccountAddress(AddressHrps hrps, string address)
    {
        var (addressHrp, addressData, _) = RadixBech32.Decode(address);

        if (addressHrp != hrps.AccountHrp)
        {
            throw new AddressException($"Address HRP was {addressHrp} but didn't match account HRP {hrps.AccountHrp}");
        }

        return GetAccountAddressFromAddressData(addressData);
    }

    private static ResourceAddress ParseResourceAddress(AddressHrps hrps, string address)
    {
        var (addressHrp, addressData, _) = RadixBech32.Decode(address);

        if (!addressHrp.EndsWith(hrps.ResourceHrpSuffix))
        {
            throw new AddressException($"Address HRP was {addressHrp} but didn't match end with resource HRP suffix {hrps.ResourceHrpSuffix}");
        }

        return GetResourceAddressFromAddressData(addressData);
    }

    private static ValidatorAddress ParseValidatorAddress(AddressHrps hrps, string address)
    {
        var (addressHrp, addressData, _) = RadixBech32.Decode(address);

        if (addressHrp != hrps.ValidatorHrp)
        {
            throw new AddressException($"Address HRP was {addressHrp} but didn't match validator HRP {hrps.ValidatorHrp}");
        }

        return GetValidatorAddressFromAddressData(addressData);
    }

    private static NodeAddress ParseNodeAddress(AddressHrps hrps, string address)
    {
        var (addressHrp, addressData, _) = RadixBech32.Decode(address);

        if (addressHrp != hrps.NodeHrp)
        {
            throw new AddressException($"Address HRP was {addressHrp} but didn't match node HRP {hrps.NodeHrp}");
        }

        return GetNodeAddressFromAddressData(addressData);
    }

    private static RadixAddress Parse(AddressHrps hrps, string address)
    {
        var (addressHrp, addressData, _) = RadixBech32.Decode(address);

        if (addressHrp == hrps.AccountHrp)
        {
            return GetAccountAddressFromAddressData(addressData);
        }

        if (addressHrp.EndsWith(hrps.ResourceHrpSuffix))
        {
            return GetResourceAddressFromAddressData(addressData);
        }

        if (addressHrp == hrps.ValidatorHrp)
        {
            return GetValidatorAddressFromAddressData(addressData);
        }

        if (addressHrp == hrps.NodeHrp)
        {
            return GetNodeAddressFromAddressData(addressData);
        }

        throw new AddressException($"Address HRP was {addressHrp} but didn't match any known types of address");
    }

    private static AccountAddress GetAccountAddressFromAddressData(byte[] addressData)
    {
        var (reAddressType, publicKey) = RadixBech32.ExtractRadixEngineAddressData(addressData);
        if (reAddressType != RadixEngineAddressType.PUB_KEY)
        {
            throw new AddressException("Address with account hrp is not of type PUB_KEY");
        }

        return new AccountAddress(addressData, publicKey);
    }

    private static ResourceAddress GetResourceAddressFromAddressData(byte[] addressData)
    {
        var (reAddressType, _) = RadixBech32.ExtractRadixEngineAddressData(addressData);
        if (reAddressType is not (RadixEngineAddressType.NATIVE_TOKEN or RadixEngineAddressType.HASHED_KEY))
        {
            throw new AddressException("Address with resource hrp suffix is not of type NATIVE_TOKEN or HASHED_KEY");
        }

        return new ResourceAddress(addressData, addressData);
    }

    private static ValidatorAddress GetValidatorAddressFromAddressData(byte[] addressData)
    {
        // Validator Addresses aren't Radix Engine Addresses
        RadixBech32.ValidatePublicKeyLength(addressData);
        return new ValidatorAddress(addressData, addressData);
    }

    private static NodeAddress GetNodeAddressFromAddressData(byte[] addressData)
    {
        // Node Addresses aren't Radix Engine Addresses
        RadixBech32.ValidatePublicKeyLength(addressData);
        return new NodeAddress(addressData, addressData);
    }
}
