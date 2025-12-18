namespace ApiPulse.Models;

/// <summary>
/// Конфигурация параметров нагрузочного тестирования.
/// </summary>
public sealed record LoadTestConfiguration
{
    /// <summary>
    /// Целевой URL для тестирования.
    /// </summary>
    public required Uri TargetUrl { get; init; }

    /// <summary>
    /// Количество параллельных потоков для выполнения запросов.
    /// </summary>
    public required int ThreadCount { get; init; }

    /// <summary>
    /// Продолжительность теста в секундах.
    /// </summary>
    public required int DurationSeconds { get; init; }

    /// <summary>
    /// Таймаут для одного HTTP-запроса в секундах. По умолчанию: 15.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 15;

    /// <summary>
    /// Максимальное количество повторных попыток при ошибке. По умолчанию: 3.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// HTTP-метод для запросов. По умолчанию: GET.
    /// </summary>
    public HttpMethod HttpMethod { get; init; } = HttpMethod.Get;

    /// <summary>
    /// Параметры строки запроса (query string).
    /// </summary>
    public Dictionary<string, string>? QueryParameters { get; init; }

    /// <summary>
    /// Тело HTTP-запроса для методов POST, PUT, PATCH.
    /// </summary>
    public string? RequestBody { get; init; }

    /// <summary>
    /// Тип содержимого (Content-Type) для тела запроса. По умолчанию: application/json.
    /// </summary>
    public string ContentType { get; init; } = "application/json";
}
