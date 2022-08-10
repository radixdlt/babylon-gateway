using System;

namespace RadixDlt.NetworkGateway.Core;

public interface ISystemClock
{
    public DateTimeOffset UtcNow { get; }
}

public class TmpSystemClock : ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
