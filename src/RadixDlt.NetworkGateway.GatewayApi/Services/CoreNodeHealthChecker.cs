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
using Prometheus;
using RadixDlt.NetworkGateway.Common.Extensions;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CoreApiModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public interface ICoreNodeHealthChecker
{
    Task<CoreNodeHealthResult> CheckCoreNodeHealth(CancellationToken cancellationToken);
}

public record CoreNodeHealthResult(Dictionary<CoreNodeStatus, List<Configuration.CoreApiNode>> CoreApiNodesByStatus);

// Using explicit integers for enum values
// because they're used for ordering the nodes (from best to worst).
public enum CoreNodeStatus
{
    HealthyAndSynced = 0,
    HealthyButLagging = 1,
    Unhealthy = 2,
}

/// <summary>
/// This contains references to a HttpClient (which should be temporary, due to caching stuff)
/// and a LedgerStateQuerier (which is a scoped service, as it contains a DataContext) - this service should thus be
/// scoped to a unit of work.
/// </summary>
public class CoreNodeHealthChecker : ICoreNodeHealthChecker
{
    private static readonly Gauge _healthCheckStatusByNode = Metrics
        .CreateGauge(
            "ng_node_gateway_health_check_status",
            "The health check status of an individual node. 1 if healthy and synced, 0.5 if health but lagging, 0 if unhealthy",
            new GaugeConfiguration { LabelNames = new[] { "node" } }
        );

    private static readonly Gauge _healthCheckCountsAcrossAllNodes = Metrics
        .CreateGauge(
            "ng_nodes_gateway_health_check_node_statuses",
            "The health check status of all nodes. Statuses are HEALTHY_AND_SYNCED, HEALTHY_BUT_LAGGING, UNHEALTHY",
            new GaugeConfiguration { LabelNames = new[] { "status" } }
        );

    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly ILedgerStateQuerier _ledgerStateQuerier;
    private readonly IOptionsMonitor<NetworkOptions> _networkOptionsMonitor;

    public CoreNodeHealthChecker(
        ILogger<CoreNodesSelectorService> logger,
        HttpClient httpClient,
        ILedgerStateQuerier ledgerStateQuerier,
        IOptionsMonitor<NetworkOptions> networkOptionsMonitor)
    {
        _logger = logger;
        _httpClient = httpClient;
        _ledgerStateQuerier = ledgerStateQuerier;
        _networkOptionsMonitor = networkOptionsMonitor;
    }

    public async Task<CoreNodeHealthResult> CheckCoreNodeHealth(CancellationToken cancellationToken)
    {
        var coreNodes = _networkOptionsMonitor.CurrentValue.CoreApiNodes;
        var enabledCoreNodes = coreNodes
            .Where(n => n.Enabled && !string.IsNullOrWhiteSpace(n.CoreApiAddress))
            .ToList();

        if (!enabledCoreNodes.Any())
        {
            _logger.LogError("No Core API Nodes have been defined as enabled");
            return new CoreNodeHealthResult(new Dictionary<CoreNodeStatus, List<Configuration.CoreApiNode>>());
        }

        var enabledCoreNodeStateVersionLookupTasks = coreNodes
            .Where(n => n.Enabled && !string.IsNullOrWhiteSpace(n.CoreApiAddress))
            .Select(n => GetCoreNodeStateVersion(n, cancellationToken));

        var topOfLedgerStateVersionTask = _ledgerStateQuerier.GetTopOfLedgerStateVersion();

        var nodesStateVersions = (await Task.WhenAll(enabledCoreNodeStateVersionLookupTasks))
            .ToDictionary(p => p.CoreApiNode, p => (p.CoreApiNode, p.StateVersion, p.Exception));

        var topOfLedgerStateVersion = await topOfLedgerStateVersionTask;

        var coreNodesByStatus = nodesStateVersions
            .Select(kv => (CoreApiNode: kv.Key, Status: DetermineNodeStatus(kv.Value, topOfLedgerStateVersion)))
            .ToLookup(p => p.Status, p => p.CoreApiNode)
            .ToDictionary(p => p.Key, p => p.ToList());

        var healthyAndSyncedCount = coreNodesByStatus.GetValueOrDefault(CoreNodeStatus.HealthyAndSynced)?.Count ?? 0;
        var healthyButLaggingCount = coreNodesByStatus.GetValueOrDefault(CoreNodeStatus.HealthyButLagging)?.Count ?? 0;
        var unhealthyCount = coreNodesByStatus.GetValueOrDefault(CoreNodeStatus.Unhealthy)?.Count ?? 0;

        // If a substantial number of nodes are not up, then report as ERROR
        var reportResultLogLevel = (healthyAndSyncedCount <= enabledCoreNodes.Count / 2)
            ? LogLevel.Error
            : LogLevel.Information;

        _logger.Log(
            reportResultLogLevel,
            "Core API health check count by status: HealthyAndSynced={HealthyAndSyncedCount}, HealthyButLagging={HealthyButLaggingCount}, Unhealthy={UnhealthyCount}",
            healthyAndSyncedCount,
            healthyButLaggingCount,
            unhealthyCount
        );
        _healthCheckCountsAcrossAllNodes.WithLabels("HEALTHY_AND_SYNCED").Set(healthyAndSyncedCount);
        _healthCheckCountsAcrossAllNodes.WithLabels("HEALTHY_BUT_LAGGING").Set(healthyButLaggingCount);
        _healthCheckCountsAcrossAllNodes.WithLabels("UNHEALTHY").Set(unhealthyCount);

        return new CoreNodeHealthResult(coreNodesByStatus);
    }

    private CoreNodeStatus DetermineNodeStatus((Configuration.CoreApiNode CoreApiNode, long? NodeStateVersion, System.Exception? Exception) healthCheckData, long topOfLedgerStateVersion)
    {
        if (healthCheckData.NodeStateVersion == null)
        {
            _logger.LogWarning(
                healthCheckData.Exception,
                "Exception connecting to {CoreNode} ({CoreNodeAddress}), will be marked as unhealthy",
                healthCheckData.CoreApiNode.Name,
                healthCheckData.CoreApiNode.CoreApiAddress
            );
            _healthCheckStatusByNode.WithLabels(healthCheckData.CoreApiNode.Name).Set(0);
            return CoreNodeStatus.Unhealthy;
        }

        var maxAcceptableLag = _networkOptionsMonitor.CurrentValue.MaxAllowedStateVersionLagToBeConsideredSynced;
        var syncedThreshold = topOfLedgerStateVersion - maxAcceptableLag;

        if (healthCheckData.NodeStateVersion < syncedThreshold)
        {
            _logger.LogInformation(
                "{CoreNode} ({CoreNodeAddress}) is at state version {NodeStateVersion}, more then {MaxAcceptableLag} below {DbLedgerStateVersion}, so will be marked as healthy but lagging",
                healthCheckData.CoreApiNode.Name,
                healthCheckData.CoreApiNode.CoreApiAddress,
                healthCheckData.NodeStateVersion,
                maxAcceptableLag,
                topOfLedgerStateVersion
            );
            _healthCheckStatusByNode.WithLabels(healthCheckData.CoreApiNode.Name).Set(0.5);
            return CoreNodeStatus.HealthyButLagging;
        }

        _logger.LogDebug(
            "{CoreNode} ({CoreNodeAddress}) is at state version {NodeStateVersion}, within {MaxAcceptableLag} of the DB's state version {DbLedgerStateVersion}, so will be marked as healthy and synced",
            healthCheckData.CoreApiNode,
            healthCheckData.CoreApiNode.CoreApiAddress,
            healthCheckData.NodeStateVersion,
            maxAcceptableLag,
            topOfLedgerStateVersion
        );
        _healthCheckStatusByNode.WithLabels(healthCheckData.CoreApiNode.Name).Set(1);
        return CoreNodeStatus.HealthyAndSynced;
    }

    private async Task<(CoreApiNode CoreApiNode, long? StateVersion, Exception? Exception)> GetCoreNodeStateVersion(
        CoreApiNode coreApiNode,
        CancellationToken cancellationToken)
    {
        var coreApiProvider = new CoreApiProvider(coreApiNode, _httpClient);
        var timeoutSeconds = 5;
        try
        {
            var timeoutCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            using var sharedCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    timeoutCancellationTokenSource.Token
                );

            var networkStatusResponse =
                await coreApiProvider.NetworkApi.NetworkStatusPostAsync(
                    new CoreApiModel.NetworkStatusRequest(
                        new CoreApiModel.NetworkIdentifier(_networkOptionsMonitor.CurrentValue.NetworkName)),
                    sharedCancellationTokenSource.Token
                );

            return (coreApiNode, networkStatusResponse.CurrentStateIdentifier.StateVersion, null);
        }
        catch (TaskCanceledException)
        {
            // The timeout above expired
            return (coreApiNode, null, new TimeoutException($"Failed to connect or receive response within {timeoutSeconds} seconds"));
        }
        catch (Exception ex)
        {
            if (ex.ShouldBeConsideredAppFatal())
            {
                _logger.LogError(ex, "An app-fatal error occurred connecting to {CoreNode}. Rethrowing...", coreApiNode.Name);
                throw;
            }

            // If, for any reason, the node can't be reached then it's considered Unhealthy
            return (coreApiNode, null, ex);
        }
    }
}
