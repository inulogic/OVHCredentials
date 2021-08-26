# OVHCredentials

.NETCore HttpClient DelegatingHandler for [OVH API](https://api.ovh.com/) Credentials.

This delegating handler decouples the proprietary authentication mechanism from a given api implementation. It allows to use any HttpClient/IHttpClientFactory rest client to consume OVH API. Doing so, unit tests are more easy to do than with the official implementation.

This project does not aim at providing a strongly-typed rest client. OVH API changes over the time, is not the same on all regions and the specification of the api is hidden in a yet another proprietary api specification format which make difficult to maitain an up to date package.

## Usage

You only need to add the delegating handler by calling `AddOvhCredentials`.

To get your credentials, follow the [user guide](https://docs.ovh.com/gb/en/api/first-steps-with-ovh-api/#advanced-usage-pair-ovhcloud-apis-with-an-application_2). 

```csharp
var provider = new ServiceCollection()
    .AddHttpClient("ovh", c => c.BaseAddress = new Uri("https://eu.api.ovh.com/1.0"))
    .AddOvhCredentials(options =>
    {
        // you are free to use whatever you are used to to configure the handler
        options.ApplicationKey = "";
        options.ApplicationSecret = "";
        options.ConsumerKey = "";
    }).Services.BuildServiceProvider();

var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
var httpClient = httpClientFactory.CreateClient("ovh");

await httpClient.GetAsync("/me");
```

## Refit based implementation

This sample uses [Refit](https://reactiveui.github.io/refit/) library.

```csharp
class Program
{
    static async Task Main(string[] args)
    {
        var provider = new ServiceCollection()
            .AddRefitClient<IMe>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://eu.api.ovh.com/1.0"))
            // as credentials are time based, polly (if used) should be placed before credentials
            .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.RetryAsync(2))
            // configure the client with ovh credentials
            .AddOvhCredentials(options =>
            {
                // you are free to use whatever you are used to to configure the handler
                options.ApplicationKey = "";
                options.ApplicationSecret = "";
                options.ConsumerKey = "";
            }).Services.BuildServiceProvider();

        var client = provider.GetRequiredService<IMe>();

        var me = await client.Me();

        Console.WriteLine($"Welcome, {me.firstname}");
    }

    public interface IMe
    {
        [Get("/me")]
        public Task<PartialMe> Me();
    }

    public class PartialMe
    {
        public string firstname { get; set; }
        public string name { get; set; }
    }
}
```

## ApiTester

The ApiTester sample application shows how to retrieve the consumer key, if needed.
