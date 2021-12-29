namespace OVHCredentials;

using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

internal class CacheTimeDeltaProvider : IRemoteTimeProvider
{
    private readonly ConcurrentDictionary<Uri, Task<long>> cache = new();
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ISystemClock clock;
    private readonly OvhCredentialsOption options;

    public CacheTimeDeltaProvider(IHttpClientFactory httpClientFactory, IOptions<OvhCredentialsOption> options, ISystemClock clock)
    {
        this.httpClientFactory = httpClientFactory;
        this.clock = clock;
        this.options = options.Value;
    }

    public async Task<long> UnixTimeUtcNowAsync(Uri baseAddress, CancellationToken cancellationToken = default) => this.clock.UtcNow.ToUnixTimeSeconds() + await this.GetTimeDeltaAsync(baseAddress, cancellationToken).ConfigureAwait(false);

    private async Task<long> GetTimeDeltaAsync(Uri baseAddress, CancellationToken cancellationToken = default) => await this.cache.GetOrAdd(baseAddress, key => this.ComputeTimeDeltaAsync(key, cancellationToken));

    private async Task<long> ComputeTimeDeltaAsync(Uri baseAddress, CancellationToken cancellationToken = default)
    {
        var client = this.options.RemoteTimeHttpClientName == null ? this.httpClientFactory.CreateClient() : this.httpClientFactory.CreateClient(this.options.RemoteTimeHttpClientName);

        var innerProvider = new TimeDeltaHttpClientProvider(client);

        var serverUnixTimestamp = await innerProvider.UnixTimeUtcNowAsync(baseAddress, cancellationToken).ConfigureAwait(false);

        var currentUnixTimestamp = this.clock.UtcNow.ToUnixTimeSeconds();
        return serverUnixTimestamp - currentUnixTimestamp;
    }
}
