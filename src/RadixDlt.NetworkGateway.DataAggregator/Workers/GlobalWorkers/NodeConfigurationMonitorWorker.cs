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
using Microsoft.Extensions.Options;
using Nito.Disposables;
using RadixDlt.NetworkGateway.Commons;
using RadixDlt.NetworkGateway.Commons.Coordination;
using RadixDlt.NetworkGateway.Commons.Workers;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Workers.GlobalWorkers;

/// <summary>
/// Responsible for reading the config, and ensuring workers are running for each node.
/// </summary>
public sealed class NodeConfigurationMonitorWorker : GlobalWorker
{
    private const string DistributedLockName = nameof(NodeConfigurationMonitorWorker);

    private static readonly IDelayBetweenLoopsStrategy _delayBetweenLoopsStrategy =
        IDelayBetweenLoopsStrategy.ConstantDelayStrategy(
            TimeSpan.FromMilliseconds(1000),
            TimeSpan.FromMilliseconds(3000));

    private readonly IDistributedLockFactory _distributedLockFactory;
    private readonly ILogger<NodeConfigurationMonitorWorker> _logger;
    private readonly INodeWorkersRunnerRegistry _nodeWorkersRunnerRegistry;
    private readonly IOptionsMonitor<NetworkOptions> _networkOptions;

    public NodeConfigurationMonitorWorker(
        IDistributedLockFactory distributedLockFactory,
        ILogger<NodeConfigurationMonitorWorker> logger,
        INodeWorkersRunnerRegistry nodeWorkersRunnerRegistry,
        IOptionsMonitor<NetworkOptions> networkOptions,
        IEnumerable<IGlobalWorkerObserver> observers,
        IClock clock)
        : base(logger, _delayBetweenLoopsStrategy, TimeSpan.FromSeconds(60), observers, clock)
    {
        _distributedLockFactory = distributedLockFactory;
        _logger = logger;
        _nodeWorkersRunnerRegistry = nodeWorkersRunnerRegistry;
        _networkOptions = networkOptions;
    }

    private IDistributedLock? _distributedLock;

    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        if (_distributedLock == null)
        {
            var lockResult = await _distributedLockFactory.TryAcquire(DistributedLockName, cancellationToken);

            if (lockResult.Succeeded)
            {
                _distributedLock = lockResult.Lock;
            }
            else
            {
                // TODO we probably want to wait bit longer before another DoWork runs as if it was a failure
                switch (lockResult.Result)
                {
                    case TmpResult.Failed:
                        _logger.LogError("bla bla bla, unable to acquire lock");
                        break;
                    case TmpResult.Impossible:
                        _logger.LogInformation("bla bla bla, not running as a primary data agg");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await _nodeWorkersRunnerRegistry.StopAllWorkers(cancellationToken);

                return;
            }
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _distributedLock.LostToken);
        await using var reg = cts.Token.Register(DistributedLockLost);

        cts.Token.ThrowIfCancellationRequested();

        await HandleNodeConfiguration(cts.Token);
    }

    protected override async Task OnStoppedSuccessfully()
    {
        _logger.LogInformation("Service execution has stopped - now instructing all node workers to stop");

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(1000));
        DistributedLockLost();
        await _nodeWorkersRunnerRegistry.StopAllWorkers(cancellationTokenSource.Token);

        _logger.LogInformation("All node workers have been stopped");

        await base.OnStoppedSuccessfully();
    }

    private async Task HandleNodeConfiguration(CancellationToken token)
    {
        var nodeConfiguration = _networkOptions.CurrentValue.CoreApiNodes;

        var enabledNodes = nodeConfiguration
            .Where(n => n.Enabled)
            .ToList();

        await _nodeWorkersRunnerRegistry.EnsureCorrectNodeServicesRunning(enabledNodes, token);
    }

    private async void DistributedLockLost()
    {
        if (_distributedLock != null)
        {
            await _distributedLock.DisposeAsync();
        }

        _distributedLock = null;
    }
}
