using ApiPulse.Policies;
using ApiPulse.Services;
using ApiPulse.UI;
using Microsoft.Extensions.DependencyInjection;

namespace ApiPulse.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiPulseServices(this IServiceCollection services)
    {
        // Register HttpClient with Polly policies
        services.AddHttpClient("LoadTest", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "ApiPulse/1.0");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(PollyPolicies.GetRetryPolicy(3));

        // Register services
        services.AddSingleton<IStatisticsCollector, StatisticsCollector>();
        services.AddSingleton<IUrlHistoryService, UrlHistoryService>();
        services.AddTransient<ILoadTestService, LoadTestService>();
        services.AddTransient<IResultExporter, ResultExporter>();
        services.AddTransient<IConsoleUI, SpectreConsoleUI>();

        return services;
    }
}
