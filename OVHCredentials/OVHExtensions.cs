namespace OVHCredentials;

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class OVHExtensions
{
    /// <summary>
    /// Adds a message handler which will set OVH credentials header on the request.
    /// </summary>
    /// <param name="builder">The Microsoft.Extensions.DependencyInjection.IHttpClientBuilder.</param>
    /// <param name="configure"></param>
    /// <returns>An Microsoft.Extensions.DependencyInjection.IHttpClientBuilder that can be used to configure the client.</returns>
    public static IHttpClientBuilder AddOVHCredentials(this IHttpClientBuilder builder, Action<OVHCredentialsOption> configure)
    {
        builder.Services
            .AddCore()
            .AddOptions<OVHCredentialsOption>().Configure(configure).ValidateDataAnnotations();

        builder.AddHttpMessageHandler<OVHDelegatingHandler>();

        return builder;
    }

    private static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddTransient<OVHDelegatingHandler>();
        services.TryAddSingleton<ISystemClock, SystemClock>();

        services.AddSingleton<IRemoteTimeProvider, CacheTimeDeltaProvider>();

        return services;
    }
}
