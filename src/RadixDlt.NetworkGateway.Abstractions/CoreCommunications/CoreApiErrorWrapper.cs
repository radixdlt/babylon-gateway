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

using RadixDlt.CoreApiSdk.Client;
using RadixDlt.NetworkGateway.Abstractions.Exceptions;
using System;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.Abstractions.CoreCommunications;

public static class CoreApiErrorWrapper
{
    public static string GetDetailedExceptionName(this WrappedCoreApiException wrappedCoreApiException)
    {
        return $"{nameof(WrappedCoreApiException)}<{wrappedCoreApiException.Error.GetType().Name}>";
    }

    public static async Task<T> ExtractCoreApiErrors<T>(Func<Task<T>> requestAction)
    {
        try
        {
            return await requestAction();
        }
        catch (ApiException apiException)
        {
            var wrappedException = ExtractWrappedCoreApiException(apiException);

            if (wrappedException == null)
            {
                throw; // Throw unwrapped ApiException
            }

            throw wrappedException;
        }
    }

    public static WrappedCoreApiException? ExtractWrappedCoreApiException(ApiException apiException)
    {
        return null;

        // TODO commented out as incompatible with current Core API version, not sure if we want to remove it permanently
        // var coreError = ExtractUpstreamGatewayErrorResponse(apiException.ErrorContent?.ToString());
        // if (coreError == null)
        // {
        //     return null;
        // }
        //
        // // We have to handle each separately to capture the right types on the error objects
        // return coreError.Details switch
        // {
        //     AboveMaximumValidatorFeeIncreaseError error => WrappedCoreApiException.Of(apiException, error, new CoreApiErrorProperties { MarksInvalidTransaction = true }),
        //     BelowMinimumStakeError error => WrappedCoreApiException.Of(apiException, error, new CoreApiErrorProperties { MarksInvalidTransaction = true }),
        //     DataObjectNotSupportedByEntityError error => WrappedCoreApiException.Of(apiException, error),
        //     FeeConstructionError error => WrappedCoreApiException.Of(apiException, error, new CoreApiErrorProperties { Transience = Transience.MaybeTransient }),
        //     InternalServerError error => WrappedCoreApiException.Of(apiException, error, new CoreApiErrorProperties { Transience = Transience.MaybeTransient }),
        //     InvalidAddressError error => WrappedCoreApiException.Of(apiException, error),
        //     InvalidDataObjectError error => WrappedCoreApiException.Of(apiException, error),
        //     InvalidFeePayerEntityError error => WrappedCoreApiException.Of(apiException, error),
        //     InvalidHexError error => WrappedCoreApiException.Of(apiException, error),
        //     InvalidJsonError error => WrappedCoreApiException.Of(apiException, error),
        //     InvalidPartialStateIdentifierError error => WrappedCoreApiException.Of(apiException, error),
        //     InvalidPublicKeyError error => WrappedCoreApiException.Of(apiException, error, new CoreApiErrorProperties { MarksInvalidTransaction = true }),
        //     InvalidSignatureError error => WrappedCoreApiException.Of(apiException, error, new CoreApiErrorProperties { MarksInvalidTransaction = true }),
        //     InvalidSubEntityError error => WrappedCoreApiException.Of(apiException, error),
        //     InvalidTransactionError error => WrappedCoreApiException.Of(apiException, error),
        //     InvalidTransactionHashError error => WrappedCoreApiException.Of(apiException, error),
        //     MessageTooLongError error => WrappedCoreApiException.Of(apiException, error, new CoreApiErrorProperties { MarksInvalidTransaction = true }),
        //     MempoolFullError error => WrappedCoreApiException.Of(apiException, error, new CoreApiErrorProperties { Transience = Transience.Transient }),
        //     NetworkNotSupportedError error => WrappedCoreApiException.Of(apiException, error),
        //     NotEnoughNativeTokensForFeesError error => WrappedCoreApiException.Of(apiException, error),
        //     NotEnoughResourcesError error => WrappedCoreApiException.Of(apiException, error),
        //     NotValidatorOwnerError error => WrappedCoreApiException.Of(apiException, error),
        //     PublicKeyNotSupportedError error => WrappedCoreApiException.Of(apiException, error),
        //     ResourceDepositOperationNotSupportedByEntityError error => WrappedCoreApiException.Of(apiException, error),
        //     ResourceWithdrawOperationNotSupportedByEntityError error => WrappedCoreApiException.Of(apiException, error),
        //     StateIdentifierNotFoundError error => WrappedCoreApiException.Of(apiException, error, new CoreApiErrorProperties { MarksInvalidTransaction = true }),
        //     SubstateDependencyNotFoundError error => WrappedCoreApiException.Of(apiException, error, new CoreApiErrorProperties { MarksInvalidTransaction = true }),
        //     TransactionNotFoundError error => WrappedCoreApiException.Of(apiException, error),
        //     _ => null,
        // };
    }
}
