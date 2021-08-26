using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace OVHCredentials
{
    public static class OvhExtensions
    {
        /// <summary>
        /// Adds a message handler which will set OVH credentials header on the request
        /// </summary>
        /// <param name="builder">The Microsoft.Extensions.DependencyInjection.IHttpClientBuilder.</param>
        /// <param name="configure"></param>
        /// <returns>An Microsoft.Extensions.DependencyInjection.IHttpClientBuilder that can be used to configure the client.</returns>
        public static IHttpClientBuilder AddOvhCredentials(this IHttpClientBuilder builder, Action<OvhCredentialsOption> configure)
        {
            builder.Services
                .AddCore()
                .AddOptions<OvhCredentialsOption>().Configure(configure).ValidateDataAnnotations();

            builder.AddHttpMessageHandler<OvhDelegatingHandler>();

            return builder;
        }

        private static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddTransient<OvhDelegatingHandler>();
            services.TryAddSingleton<ISystemClock, SystemClock>();

            services.AddSingleton<IRemoteTimeProvider, CacheTimeDeltaProvider>();

            return services;
        }
    }
}
