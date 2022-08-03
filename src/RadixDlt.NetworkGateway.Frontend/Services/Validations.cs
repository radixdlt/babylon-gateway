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

using Common.Addressing;
using NodaTime;
using RadixDlt.NetworkGateway.Frontend.Exceptions;
using RadixDlt.NetworkGateway.FrontendSdk.Model;

namespace RadixDlt.NetworkGateway.Frontend.Services;

public interface IValidations
{
    ValidatedAccountAddress ExtractValidAccountAddress(AccountIdentifier accountIdentifier);

    ValidatedValidatorAddress ExtractValidValidatorAddress(ValidatorIdentifier validatorIdentifier);

    ValidatedTransactionIdentifier ExtractValidTransactionIdentifier(TransactionIdentifier requestTransactionIdentifier);

    ValidatedPublicKey ExtractValidPublicKey(PublicKey publicKey);

    ValidatedHex ExtractValidHex(string capitalizedErrorMessageFieldName, string hexString);

    ValidatedHex? ExtractOptionalValidHexOrNull(string capitalizedErrorMessageFieldName, string hexString);

    ValidatedResourceAddress ExtractValidResourceAddress(TokenIdentifier tokenIdentifier);

    ValidatedTokenAmount ExtractValidTokenAmount(TokenAmount actionAmount);

    ValidatedTokenAmount ExtractValidPositiveTokenAmount(TokenAmount actionAmount);

    ValidatedTokenAmount ExtractValidPositiveXrdTokenAmount(TokenAmount actionAmount);

    ValidatedSymbol ExtractValidTokenSymbol(string symbol);

    Instant ExtractValidTimestamp(string capitalizedFieldDescriptor, string timestampString);

    int ExtractValidIntInBoundInclusive(string capitalizedFieldDescriptor, int input, int lowerBound, int upperBound);
}

public record ValidatedTokenAmount(string Rri, Common.Numerics.TokenAmount Amount, ResourceAddress ResourceAddress);
public record ValidatedResourceAddress(string Rri, ResourceAddress ResourceAddress);
public record ValidatedAccountAddress(string Address, AccountAddress ByteAccountAddress);
public record ValidatedValidatorAddress(string Address, ValidatorAddress ByteValidatorAddress);
public record ValidatedTransactionIdentifier(string AsString, byte[] Bytes);
public record ValidatedPublicKey(string AsString, byte[] Bytes);
public record ValidatedHex(string AsString, byte[] Bytes);
public record ValidatedSymbol(string AsString);

public class Validations : IValidations
{
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;

    public Validations(INetworkConfigurationProvider networkConfigurationProvider)
    {
        _networkConfigurationProvider = networkConfigurationProvider;
    }

    public ValidatedAccountAddress ExtractValidAccountAddress(AccountIdentifier accountIdentifier)
    {
        if (!RadixAddressParser.TryParseAccountAddress(
                _networkConfigurationProvider.GetAddressHrps(),
                accountIdentifier.Address,
                out var accountAddress,
                out var errorMessage
            ))
        {
            throw new InvalidAccountAddressException(accountIdentifier.Address, "Account address is invalid", errorMessage);
        }

        return new ValidatedAccountAddress(accountIdentifier.Address, accountAddress);
    }

    public ValidatedValidatorAddress ExtractValidValidatorAddress(ValidatorIdentifier validatorIdentifier)
    {
        if (!RadixAddressParser.TryParseValidatorAddress(
                _networkConfigurationProvider.GetAddressHrps(),
                validatorIdentifier.Address,
                out var validatorAddress,
                out var errorMessage
            ))
        {
            throw new InvalidValidatorAddressException(validatorIdentifier.Address, "Validator address is invalid", errorMessage);
        }

        return new ValidatedValidatorAddress(validatorIdentifier.Address, validatorAddress);
    }

    public ValidatedTransactionIdentifier ExtractValidTransactionIdentifier(TransactionIdentifier transactionIdentifier)
    {
        const int TransactionIdentifierByteLength = 32;
        try
        {
            var bytes = Convert.FromHexString(transactionIdentifier.Hash);
            if (bytes.Length != TransactionIdentifierByteLength)
            {
                throw InvalidRequestException.FromOtherError($"Transaction identifier hash is not {TransactionIdentifierByteLength} bytes long");
            }

            return new ValidatedTransactionIdentifier(transactionIdentifier.Hash.ToLowerInvariant(), bytes);
        }
        catch (FormatException exception)
        {
            throw InvalidRequestException.FromOtherError("Transaction identifier hash is not valid hex", exception.Message);
        }
    }

    public ValidatedPublicKey ExtractValidPublicKey(PublicKey publicKey)
    {
        try
        {
            var bytes = Convert.FromHexString(publicKey.Hex);
            if (bytes.Length != RadixBech32.CompressedPublicKeyBytesLength)
            {
                throw new InvalidPublicKeyException(publicKey, $"Public key is not {RadixBech32.CompressedPublicKeyBytesLength} bytes long");
            }

            return new ValidatedPublicKey(publicKey.Hex.ToLowerInvariant(), bytes);
        }
        catch (FormatException exception)
        {
            throw new InvalidPublicKeyException(publicKey, "Public key is not valid hex", exception.Message);
        }
    }

    public ValidatedHex ExtractValidHex(string capitalizedErrorMessageFieldName, string hexString)
    {
        try
        {
            return new ValidatedHex(hexString.ToLowerInvariant(), Convert.FromHexString(hexString));
        }
        catch (FormatException exception)
        {
            throw InvalidRequestException.FromOtherError($"{capitalizedErrorMessageFieldName} is not valid hex", exception.Message);
        }
    }

    public ValidatedHex? ExtractOptionalValidHexOrNull(string capitalizedErrorMessageFieldName, string? hexString)
    {
        try
        {
            return string.IsNullOrWhiteSpace(hexString) ? null : new ValidatedHex(hexString.ToLowerInvariant(), Convert.FromHexString(hexString));
        }
        catch (FormatException exception)
        {
            throw InvalidRequestException.FromOtherError($"{capitalizedErrorMessageFieldName} is not valid hex", exception.Message);
        }
    }

    public ValidatedResourceAddress ExtractValidResourceAddress(TokenIdentifier tokenIdentifier)
    {
        if (!RadixAddressParser.TryParseResourceAddress(
                _networkConfigurationProvider.GetAddressHrps(),
                tokenIdentifier.Rri,
                out var resourceAddress,
                out var errorMessage
            ))
        {
            throw new InvalidTokenRRIException(tokenIdentifier.Rri, "Token rri is invalid", errorMessage);
        }

        return new ValidatedResourceAddress(tokenIdentifier.Rri, resourceAddress);
    }

    public ValidatedTokenAmount ExtractValidTokenAmount(TokenAmount actionAmount)
    {
        var validatedResourceAddress = ExtractValidResourceAddress(actionAmount.TokenIdentifier);
        var tokenAmount = Common.Numerics.TokenAmount.FromSubUnitsString(actionAmount.Value);
        if (tokenAmount.IsNaN())
        {
            if (actionAmount.Value.Contains('.'))
            {
                throw InvalidRequestException.FromOtherError(
                    "Token amount value could not be parsed. " +
                    "Note, that the value is denominated in attos (smallest unit of XRD, 1 XRD = 10^18 attos). " +
                    "Attos are indivisible and therefore the value can't contain a decimal point.");
            }

            throw InvalidRequestException.FromOtherError("Token amount value could not be parsed");
        }

        return new ValidatedTokenAmount(validatedResourceAddress.Rri, tokenAmount, validatedResourceAddress.ResourceAddress);
    }

    public ValidatedTokenAmount ExtractValidPositiveTokenAmount(TokenAmount actionAmount)
    {
        var tokenAmount = ExtractValidTokenAmount(actionAmount);

        if (!tokenAmount.Amount.IsPositive())
        {
            InvalidRequestException.FromOtherError("Token amount value is not positive");
        }

        return tokenAmount;
    }

    public ValidatedTokenAmount ExtractValidPositiveXrdTokenAmount(TokenAmount actionAmount)
    {
        var tokenAmount = ExtractValidPositiveTokenAmount(actionAmount);

        if (tokenAmount.Rri != _networkConfigurationProvider.GetXrdAddress())
        {
            InvalidRequestException.FromOtherError("Token amount is not xrd where it needs to be");
        }

        return tokenAmount;
    }

    public ValidatedSymbol ExtractValidTokenSymbol(string symbol)
    {
        if (!RadixBech32.ValidResourceSymbolRegex.IsMatch(symbol))
        {
            throw new InvalidTokenSymbolException(symbol, "Symbol must be between 1 and 35 lower case alpha-numeric characters");
        }

        return new ValidatedSymbol(symbol);
    }

    public Instant ExtractValidTimestamp(string capitalizedFieldDescriptor, string timestampString)
    {
        if (!DateTimeOffset.TryParse(timestampString, out var dateTimeOffset))
        {
            throw InvalidRequestException.FromOtherError($"{capitalizedFieldDescriptor} DateTime could not be parsed");
        }

        return Instant.FromDateTimeOffset(dateTimeOffset);
    }

    public int ExtractValidIntInBoundInclusive(string capitalizedFieldDescriptor, int input, int lowerBound, int upperBound)
    {
        if (input < lowerBound)
        {
            throw InvalidRequestException.FromOtherError($"{capitalizedFieldDescriptor} must be >= {lowerBound}");
        }

        if (input > upperBound)
        {
            throw InvalidRequestException.FromOtherError($"{capitalizedFieldDescriptor} must be <= {upperBound}");
        }

        return input;
    }
}
