using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Refit;
using System;
using System.Threading.Tasks;
using OVHCredentials;

namespace TestProject1
{
    [TestClass]
    public class IntegrationTest
    {
        [TestMethod]
        public async Task TestAsync()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            var expectedOptions = new OvhCredentialsOption()
            {
                ApplicationKey = nameof(OvhCredentialsOption.ApplicationKey),
                ApplicationSecret = nameof(OvhCredentialsOption.ApplicationSecret),
                ConsumerKey = nameof(OvhCredentialsOption.ConsumerKey)
            };

            using var hostTime = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.Run(async context =>
                        {
                            Assert.AreEqual("/1.0/auth/time", context.Request.Path.Value);
                            await context.Response.WriteAsJsonAsync(now.AddSeconds(42).ToUnixTimeSeconds());
                        });
                    });
            })
            .StartAsync();

            using var hostApi = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.Run(async context =>
                        {
                            Assert.AreEqual("/1.0/echo", context.Request.Path.Value);
                            Assert.AreEqual(expectedOptions.ConsumerKey, (string)context.Request.Headers["X-Ovh-Consumer"]);
                            Assert.AreEqual(expectedOptions.ApplicationKey, (string)context.Request.Headers["X-Ovh-Application"]);
                            Assert.AreEqual((now.ToUnixTimeSeconds() + 42).ToString(), (string)context.Request.Headers["X-Ovh-Timestamp"]);
                            Assert.IsNotNull(context.Request.Headers["X-Ovh-Signature"]);

                            await context.Response.WriteAsJsonAsync(await context.Request.ReadFromJsonAsync<string[]>());
                        });
                    });
            })
            .StartAsync();

            ServiceCollection services = new ServiceCollection();

            services.AddSingleton<ISystemClock>(new TestSystemClock(now));

            services.AddHttpClient("testtimeclient").ConfigurePrimaryHttpMessageHandler(() => hostTime.GetTestServer().CreateHandler());

            services.AddRefitClient<IEchoApi>()
                .ConfigurePrimaryHttpMessageHandler(() => hostApi.GetTestServer().CreateHandler())
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri(hostApi.GetTestServer().BaseAddress, "1.0");
                })
                .AddOvhCredentials(options =>
                {
                    options.ApplicationKey = expectedOptions.ApplicationKey;
                    options.ApplicationSecret = expectedOptions.ApplicationSecret;
                    options.ConsumerKey = expectedOptions.ConsumerKey;
                    options.RemoteTimeHttpClientName = "testtimeclient";
                });

            var provider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            var sut = provider.GetRequiredService<IEchoApi>();

            var result = await sut.PostEchoAsync(new[] { "foo", "bar" });

            CollectionAssert.AreEqual(new[] { "foo", "bar" }, result);
        }

        private class TestSystemClock : ISystemClock
        {
            private readonly DateTimeOffset testNow;

            public TestSystemClock(DateTimeOffset testNow)
            {
                this.testNow = testNow;
            }

            public DateTimeOffset UtcNow => testNow;
        }
    }

    public interface IEchoApi
    {
        [Post("/echo")]
        public Task<string[]> PostEchoAsync([Body(buffered: true)] string[] body);
    }
}