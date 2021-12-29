namespace OVHCredentials;

using System;
using System.Threading;
using System.Threading.Tasks;

internal interface IRemoteTimeProvider
{
    Task<long> UnixTimeUtcNowAsync(Uri baseAddress, CancellationToken cancellationToken);
}
