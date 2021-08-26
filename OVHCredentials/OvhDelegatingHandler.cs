using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OVHCredentials
{
    internal class OvhDelegatingHandler : DelegatingHandler
    {
        public const string OVH_APPLICATION_HEADER = "X-Ovh-Application";
        public const string OVH_CONSUMER_HEADER = "X-Ovh-Consumer";
        public const string OVH_TIMESTAMP_HEADER = "X-Ovh-Timestamp";
        public const string OVH_SIGNATURE_HEADER = "X-Ovh-Signature";

        public OvhDelegatingHandler(IOptions<OvhCredentialsOption> options, IRemoteTimeProvider timeProvider)
        {
            _options = options;
            _timeProvider = timeProvider;
        }

        private readonly IOptions<OvhCredentialsOption> _options;
        private readonly IRemoteTimeProvider _timeProvider;


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var headers = request.Headers;
            headers.Add(OVH_APPLICATION_HEADER, _options.Value.ApplicationKey);

            if (!request.RequestUri.AbsolutePath.EndsWith("/auth/credential"))
            {
                long currentTimestamp = await _timeProvider.UnixTimeUtcNowAsync(new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority)), cancellationToken);
                if (_options.Value.ConsumerKey != null)
                {
                    headers.Add(OVH_CONSUMER_HEADER, _options.Value.ConsumerKey);

                    // request content must be read
                    // no luck, if you thought you would stream the body for better performance
                    string content = string.Empty;
                    if (request.Content != null)
                    {
                        content = await request.Content.ReadAsStringAsync();
                        request.Content = new StringContent(content, Encoding.UTF8, request.Content.Headers.ContentType.MediaType);
                    }

                    string signature = Sign(
                        applicationSecret: _options.Value.ApplicationSecret,
                        consumerKey: _options.Value.ConsumerKey,
                        currentTimestamp: currentTimestamp,
                        method: request.Method.Method,
                        target: request.RequestUri.ToString(),
                        data: content);
                    headers.Add(OVH_SIGNATURE_HEADER, signature);
                }

                headers.Add(OVH_TIMESTAMP_HEADER, currentTimestamp.ToString());
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private string Sign(string applicationSecret, string consumerKey, long currentTimestamp, string method, string target, string data = null)
        {
            string payload = string.Join("+", applicationSecret, consumerKey, method, target, data, currentTimestamp);

            using var sha1 = new SHA1Managed();
            byte[] binaryHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(payload));

            // despite the replace, still fastest than linq or string builder version
            string signature = BitConverter.ToString(binaryHash).Replace("-", string.Empty).ToLower();
            return $"$1${signature}";
        }
    }
}
