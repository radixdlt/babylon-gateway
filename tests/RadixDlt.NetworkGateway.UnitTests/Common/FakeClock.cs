using RadixDlt.NetworkGateway.Common;
using System;

namespace RadixDlt.NetworkGateway.UnitTests.Common;

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
