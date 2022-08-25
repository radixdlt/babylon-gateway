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

using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.NewWorkers;

public class DefaultExecutionPolicyBuilder
{
    public static IAsyncPolicy CreateExecutionPolicy()
    {
        // TODO circuitBreakerPolicy should be per node,
        // TODO policies should be dynamically configurable (think IOptions<XxxWorkerExecutionPolicy> or IOptions<WorkerExecutionPolicy>("xxx"))

        var retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(DefaultRetryDurations(), OnRetryAsync);
        var circuitBreakerPolicy = Policy.Handle<Exception>().AdvancedCircuitBreakerAsync(failureThreshold: 0.25, samplingDuration: TimeSpan.FromMinutes(1), minimumThroughput: 5, durationOfBreak: TimeSpan.FromSeconds(5), OnBreak, OnReset, OnHalfOpen);
        var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(5), TimeoutStrategy.Optimistic, OnTimeoutAsync);

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
    }

    // returns duration of 1, 3, 7, 15, 31 seconds
    private static IEnumerable<TimeSpan> DefaultRetryDurations()
    {
        for (var i = 1; i <= 5; i++)
        {
            yield return TimeSpan.FromSeconds(Math.Pow(2, i) - 1);
        }
    }

    private static Task OnRetryAsync(Exception exception, TimeSpan waitDuration, int retryCount, Context context)
    {
        context.GetLogger().LogError(exception, "bla bla bla worker failed, node '{NodeName}', attempt {RetryCount}, wait duration {WaitDuration}", context.GetNodeName(), retryCount, waitDuration);

        return Task.CompletedTask;
    }

    private static void OnHalfOpen()
    {
        // TODO why there's no Context here?!
    }

    private static void OnReset(Context context)
    {
        context.GetLogger().LogInformation("bla bla bla circuit closed, node '{NodeName}', restoring processing", context.GetNodeName());
    }

    private static void OnBreak(Exception exception, CircuitState circuitState, TimeSpan waitDuration, Context context)
    {
        context.GetLogger().LogError(exception, "bla bla bla circuit opened, node '{NodeName}', processing stopped", context.GetNodeName());
    }

    private static Task OnTimeoutAsync(Context context, TimeSpan timeout, Task task, Exception exception)
    {
        context.GetLogger().LogError(exception, "bla bla bla timed out, node '{NodeName}'", context.GetNodeName());

        // do NOT await this one
        task.ContinueWith(_ =>
        {
            // any clean-up if needed
        });

        return Task.CompletedTask;
    }
}
