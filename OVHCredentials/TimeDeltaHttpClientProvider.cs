namespace OVHCredentials;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

internal class TimeDeltaHttpClientProvider
{
    private readonly HttpClient httpClient;

    public TimeDeltaHttpClientProvider(HttpClient client) => this.httpClient = client;

    public async Task<long> UnixTimeUtcNowAsync(Uri baseAddress, CancellationToken cancellationToken = default) => await this.httpClient.GetFromJsonAsync<long>(new Uri(baseAddress, "/1.0/auth/time"), cancellationToken).ConfigureAwait(false);
}
