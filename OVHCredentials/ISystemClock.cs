namespace OVHCredentials;

using System;

/// <summary>
/// Used for testing purpose.
/// </summary>
public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}

internal class SystemClock : ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
