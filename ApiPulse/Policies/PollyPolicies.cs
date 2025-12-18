using Polly;
using Polly.Extensions.Http;

namespace ApiPulse.Policies;

/// <summary>
/// Статический класс с политиками отказоустойчивости Polly для HTTP-запросов.
/// </summary>
public static class PollyPolicies
{
    /// <summary>
    /// Создаёт политику повторных попыток с экспоненциальной задержкой.
    /// Обрабатывает временные HTTP-ошибки и ответы с кодом 5xx.
    /// </summary>
    /// <param name="maxRetries">Максимальное количество повторных попыток.</param>
    /// <returns>Асинхронная политика повторных попыток.</returns>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int maxRetries)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(
                maxRetries,
                retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100));
    }

    /// <summary>
    /// Создаёт политику таймаута для HTTP-запросов.
    /// </summary>
    /// <param name="timeoutSeconds">Таймаут в секундах.</param>
    /// <returns>Асинхронная политика таймаута.</returns>
    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(int timeoutSeconds)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(timeoutSeconds);
    }
}
