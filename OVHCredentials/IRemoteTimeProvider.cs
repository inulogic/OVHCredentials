using System;
using System.Threading;
using System.Threading.Tasks;

namespace OVHCredentials
{
    internal interface IRemoteTimeProvider
    {
        Task<long> UnixTimeUtcNowAsync(Uri baseAddress, CancellationToken cancellationToken);
    }
}
