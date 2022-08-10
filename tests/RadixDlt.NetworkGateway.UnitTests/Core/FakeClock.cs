using RadixDlt.NetworkGateway.Core;
using System;

namespace RadixDlt.NetworkGateway.UnitTests.Core;

public class FakeClock : ISystemClock
{
    private DateTimeOffset _clock;

    public FakeClock()
    {
        _clock = DateTimeOffset.UtcNow;
    }

    public FakeClock(DateTimeOffset clock)
    {
        _clock = clock;
    }

    public DateTimeOffset UtcNow => _clock;

    public void Advance(TimeSpan timeSpan)
    {
        _clock += timeSpan;
    }
}
