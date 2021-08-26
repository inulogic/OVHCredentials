using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OVHCredentials
{
    internal class TimeDeltaHttpClientProvider
    {
        private readonly HttpClient _httpClient;
        public TimeDeltaHttpClientProvider(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<long> UnixTimeUtcNowAsync(Uri baseAddress, CancellationToken cancellationToken = default)
        {
            return await _httpClient.GetFromJsonAsync<long>(new Uri(baseAddress, "/1.0/auth/time"), cancellationToken).ConfigureAwait(false);
        }
    }

    internal class CacheTimeDeltaProvider : IRemoteTimeProvider
    {
        private readonly ConcurrentDictionary<Uri, Task<long>> _cache = new ConcurrentDictionary<Uri, Task<long>>();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISystemClock _clock;
        private readonly OvhCredentialsOption _options;

        public CacheTimeDeltaProvider(IHttpClientFactory httpClientFactory, IOptions<OvhCredentialsOption> options, ISystemClock clock)
        {
            _httpClientFactory = httpClientFactory;
            _clock = clock;
            _options = options.Value;
        }

        private async Task<long> GetTimeDeltaAsync(Uri baseAddress, CancellationToken cancellationToken = default)
        {
            return await _cache.GetOrAdd(baseAddress, key => ComputeTimeDeltaAsync(key, cancellationToken));
        }

        private async Task<long> ComputeTimeDeltaAsync(Uri baseAddress, CancellationToken cancellationToken = default)
        {
            var client = _options.RemoteTimeHttpClientName == null ? _httpClientFactory.CreateClient() : _httpClientFactory.CreateClient(_options.RemoteTimeHttpClientName);

            var innerProvider = new TimeDeltaHttpClientProvider(client);

            long serverUnixTimestamp = await innerProvider.UnixTimeUtcNowAsync(baseAddress, cancellationToken).ConfigureAwait(false);

            long currentUnixTimestamp = _clock.UtcNow.ToUnixTimeSeconds();
            return serverUnixTimestamp - currentUnixTimestamp;
        }

        public async Task<long> UnixTimeUtcNowAsync(Uri baseAddress, CancellationToken cancellationToken = default)
        {
            return _clock.UtcNow.ToUnixTimeSeconds() + await GetTimeDeltaAsync(baseAddress, cancellationToken).ConfigureAwait(false);
        }
    }
}
