using ApiPulse.Policies;
using ApiPulse.Services;
using ApiPulse.UI;
using Microsoft.Extensions.DependencyInjection;

namespace ApiPulse.Extensions;

/// <summary>
/// Методы расширения для регистрации сервисов ApiPulse в контейнере зависимостей.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует все сервисы ApiPulse в контейнере зависимостей.
    /// Включает HttpClient с политиками Polly, сервисы тестирования, UI и экспорта.
    /// </summary>
    /// <param name="services">Коллекция сервисов для регистрации.</param>
    /// <returns>Коллекция сервисов для цепочки вызовов.</returns>
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
        services.AddTransient<IChartService, ChartService>();

        return services;
    }
}
