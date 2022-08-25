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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RadixDlt.NetworkGateway.Commons.Coordination;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

internal class PostgresDistributedLockFactory : IDistributedLockFactory
{
    internal const int InitialLockCounter = 1;

    private static readonly TimeSpan _defaultTtl = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan _defaultRefreshRate = TimeSpan.FromSeconds(2);

    private readonly IDbContextFactory<ReadWriteDbContext> _dbContextFactory;
    private readonly ILogger _logger;

    public PostgresDistributedLockFactory(IDbContextFactory<ReadWriteDbContext> dbContextFactory, ILogger<PostgresDistributedLockFactory> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public Task<AcquireResult> TryAcquire(string name, CancellationToken token = default)
    {
        return TryAcquire(name, _defaultTtl, _defaultRefreshRate, token);
    }

    public async Task<AcquireResult> TryAcquire(string name, TimeSpan ttl, TimeSpan refreshRate, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (ttl < TimeSpan.Zero)
        {
            throw new ArgumentException("Must be positive", nameof(ttl));
        }

        if (refreshRate < TimeSpan.Zero)
        {
            throw new ArgumentException("Must be positive", nameof(refreshRate));
        }

        // if running in non-primary aws region
        if (new Random().Next(1, 2) > 5)
        {
            return new AcquireResult(TmpResult.Impossible);
        }

        var baseValue = CreateBaseValue();
        var value = CreateLockValue(baseValue, InitialLockCounter);

        _logger.LogDebug("Attempt to acquire lock {Name} ({Value})", name, value);

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        var rowsAffected = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $@"
INSERT INTO distributed_locks (name, value, created_timestamp, expiration_timestamp)
VALUES ({name}, {value}, now() at time zone 'utc', now() at time zone 'utc' + {ttl})
ON CONFLICT (name)
    DO UPDATE SET
        value = {value},
        created_timestamp = now() at time zone 'utc',
        expiration_timestamp = now() at time zone 'utc' + {ttl}
    WHERE
        distributed_locks.expiration_timestamp < now() at time zone 'utc'
RETURNING *
",
            token);

        return rowsAffected == 0
            ? new AcquireResult(TmpResult.Failed)
            : new AcquireResult(TmpResult.Ok, new PostgresDistributedLock(name, baseValue, ttl, refreshRate, this));
    }

    internal string CreateLockValue(string baseValue, int counter) => $"{baseValue}_{counter}";

    internal async Task<bool> TryRefresh(string name, string oldValue, string newValue, TimeSpan ttl, CancellationToken token = default)
    {
        _logger.LogDebug("Attempt to refresh lock {Name} ({OldValue} => {NewValue})", name, oldValue, newValue);

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        var rowsAffected = await dbContext.Database
            .ExecuteSqlInterpolatedAsync(
                $@"
UPDATE distributed_locks SET
    value = {newValue},
    expiration_timestamp = now() at time zone 'utc' + {ttl}
WHERE
    name = {name} AND value = {oldValue} AND expiration_timestamp > now() at time zone 'utc'
RETURNING *
",
                token);

        return rowsAffected > 0;
    }

    internal async Task<bool> TryRelease(string name, string value, CancellationToken token = default)
    {
        _logger.LogDebug("Attempt to release lock {Name} ({Value})", name, value);

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        var rowsAffected = await dbContext.Database
            .ExecuteSqlInterpolatedAsync(
                $@"
DELETE FROM distributed_locks
WHERE name = {name} AND value = {value} AND expiration_timestamp > now() at time zone 'utc'
",
                token);

        return rowsAffected > 0;
    }

    private static string CreateBaseValue()
    {
        var rndBytes = new byte[8];
        var rnd = new Random();
        rnd.NextBytes(rndBytes);

        return Convert.ToHexString(rndBytes);
    }
}

internal class PostgresDistributedLock : IDistributedLock
{
    private readonly string _name;
    private readonly string _baseValue;
    private readonly TimeSpan _ttl;
    private readonly PostgresDistributedLockFactory _factory;
    private readonly CancellationTokenSource _cts;
    private readonly PeriodicTimer _timer;

    private int _refreshCounter = PostgresDistributedLockFactory.InitialLockCounter;

    public PostgresDistributedLock(string name, string baseValue, TimeSpan ttl, TimeSpan refreshRate, PostgresDistributedLockFactory factory)
    {
        _name = name;
        _baseValue = baseValue;
        _ttl = ttl;
        _factory = factory;
        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(refreshRate);

        Task.Run(() => RefreshLoop(_cts.Token), _cts.Token);
    }

    public CancellationToken LostToken => _cts.Token;

    public async ValueTask DisposeAsync()
    {
        _cts.Dispose();
        _timer.Dispose();

        await _factory.TryRelease(_name, _factory.CreateLockValue(_baseValue, _refreshCounter), CancellationToken.None);
    }

    private async Task RefreshLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested && await _timer.WaitForNextTickAsync(token))
        {
            var oldValue = _factory.CreateLockValue(_baseValue, _refreshCounter++);
            var newValue = _factory.CreateLockValue(_baseValue, _refreshCounter);

            if (await _factory.TryRefresh(_name, oldValue, newValue, _ttl, token) == false)
            {
                _cts.Cancel();
            }
        }
    }
}
