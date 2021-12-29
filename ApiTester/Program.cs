namespace ApiTester;

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OVHCredentials;
using Polly;
using Refit;

internal class Program
{
    private const string ApplicationKey = "<application_key>";
    private const string ApplicationSecret = "<application_secret>";
    private const string BaseAddress = "https://eu.api.ovh.com/1.0";

    private static async Task Main(string[] _)
    {
        var consumerKey = await GetConsumerKey();
        await CallMe(consumerKey);
    }

    private static async Task<string> GetConsumerKey()
    {
        var provider = new ServiceCollection()
        .AddRefitClient<ICredentialRequestOvhApi>(new RefitSettings()
        {
            ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
        })
        .ConfigureHttpClient(c => c.BaseAddress = new Uri(BaseAddress))
        .Services.BuildServiceProvider();

        var client = provider.GetRequiredService<ICredentialRequestOvhApi>();

        var requestPayload = new CredentialRequest
        {
            Redirection = "https://redirect.url",
            AccessRules = { new AccessRight { Method = "GET", Path = "/*" } }
        };

        var credentialRequestResult = await client.RequestConsumerKeyAsync(requestPayload, ApplicationKey);

        Console.Write($"Please visit {credentialRequestResult.ValidationUrl} to authenticate and press enter to continue");
        Console.ReadLine();

        return credentialRequestResult.ConsumerKey;
    }

    private static async Task CallMe(string consumerKey)
    {
        var provider = new ServiceCollection()
        .AddRefitClient<IMe>(new RefitSettings()
        {
            ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
        })
        .ConfigureHttpClient(c => c.BaseAddress = new Uri(BaseAddress))
        // as credentials are time based, polly (if used) should be placed before credentials
        .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.RetryAsync(2))
        // configure the client with ovh credentials
        .AddOvhCredentials(options =>
        {
            options.ApplicationKey = ApplicationKey;
            options.ApplicationSecret = ApplicationSecret;
            options.ConsumerKey = consumerKey;
        }).Services.BuildServiceProvider();

        var client = provider.GetRequiredService<IMe>();

        var me = await client.Me();

        Console.WriteLine($"Welcome, {me.Firstname}");
    }

    public interface IMe
    {
        [Get("/me")]
        public Task<PartialMe> Me();
    }

    public class PartialMe
    {
        public string Firstname { get; set; }
        public string Name { get; set; }
    }
}
