namespace OVHCredentials;

using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

internal class OvhDelegatingHandler : DelegatingHandler
{
    public const string OVHAPPLICATIONHEADER = "X-Ovh-Application";
    public const string OVHCONSUMERHEADER = "X-Ovh-Consumer";
    public const string OVHTIMESTAMPHEADER = "X-Ovh-Timestamp";
    public const string OVHSIGNATUREHEADER = "X-Ovh-Signature";

    private readonly IOptions<OvhCredentialsOption> options;
    private readonly IRemoteTimeProvider timeProvider;

    public OvhDelegatingHandler(IOptions<OvhCredentialsOption> options, IRemoteTimeProvider timeProvider)
    {
        this.options = options;
        this.timeProvider = timeProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var headers = request.Headers;
        headers.Add(OVHAPPLICATIONHEADER, this.options.Value.ApplicationKey);

        if (!request.RequestUri.AbsolutePath.EndsWith("/auth/credential"))
        {
            var currentTimestamp = await this.timeProvider.UnixTimeUtcNowAsync(new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority)), cancellationToken);
            if (this.options.Value.ConsumerKey != null)
            {
                headers.Add(OVHCONSUMERHEADER, this.options.Value.ConsumerKey);

                // request content must be read
                // no luck, if you thought you would stream the body for better performance
                var content = string.Empty;
                if (request.Content != null)
                {
                    content = await request.Content.ReadAsStringAsync();
                    request.Content = new StringContent(content, Encoding.UTF8, request.Content.Headers.ContentType.MediaType);
                }

                var signature = this.Sign(
                    applicationSecret: this.options.Value.ApplicationSecret,
                    consumerKey: this.options.Value.ConsumerKey,
                    currentTimestamp: currentTimestamp,
                    method: request.Method.Method,
                    target: request.RequestUri.ToString(),
                    data: content);
                headers.Add(OVHSIGNATUREHEADER, signature);
            }

            headers.Add(OVHTIMESTAMPHEADER, currentTimestamp.ToString());
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private string Sign(string applicationSecret, string consumerKey, long currentTimestamp, string method, string target, string data = null)
    {
        var payload = string.Join("+", applicationSecret, consumerKey, method, target, data, currentTimestamp);

        using var sha1 = new SHA1Managed();
        var binaryHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(payload));

        // despite the replace, still fastest than linq or string builder version
        var signature = BitConverter.ToString(binaryHash).Replace("-", string.Empty).ToLower();
        return $"$1${signature}";
    }
}
