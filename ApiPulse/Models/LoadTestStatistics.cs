namespace ApiPulse.Models;

/// <summary>
/// Содержит агрегированную статистику по результатам нагрузочного тестирования.
/// </summary>
public sealed class LoadTestStatistics
{
    /// <summary>
    /// Целевой URL, который был протестирован.
    /// </summary>
    public required string TargetUrl { get; init; }

    /// <summary>
    /// Доменное имя целевого сервера.
    /// </summary>
    public required string Domain { get; init; }

    /// <summary>
    /// Количество параллельных потоков, использованных в тесте.
    /// </summary>
    public required int ThreadCount { get; init; }

    /// <summary>
    /// Продолжительность теста в секундах.
    /// </summary>
    public required int DurationSeconds { get; init; }

    /// <summary>
    /// Минимальное время ответа в миллисекундах.
    /// </summary>
    public required long MinResponseTimeMs { get; init; }

    /// <summary>
    /// Максимальное время ответа в миллисекундах.
    /// </summary>
    public required long MaxResponseTimeMs { get; init; }

    /// <summary>
    /// Среднее время ответа в миллисекундах.
    /// </summary>
    public required double AverageResponseTimeMs { get; init; }

    /// <summary>
    /// Медианное время ответа в миллисекундах (50-й перцентиль).
    /// </summary>
    public required double MedianResponseTimeMs { get; init; }

    /// <summary>
    /// 95-й перцентиль времени ответа в миллисекундах.
    /// </summary>
    public required double Percentile95Ms { get; init; }

    /// <summary>
    /// Общее количество выполненных запросов.
    /// </summary>
    public required int TotalRequests { get; init; }

    /// <summary>
    /// Количество успешных запросов (HTTP статус 2xx).
    /// </summary>
    public required int SuccessfulRequests { get; init; }

    /// <summary>
    /// Количество неудачных запросов.
    /// </summary>
    public required int FailedRequests { get; init; }

    /// <summary>
    /// Среднее количество запросов в секунду (RPS).
    /// </summary>
    public required double RequestsPerSecond { get; init; }

    /// <summary>
    /// Процент успешных запросов (0-100).
    /// </summary>
    public required double SuccessRate { get; init; }

    /// <summary>
    /// Время начала тестирования.
    /// </summary>
    public required DateTime TestStartTime { get; init; }

    /// <summary>
    /// Время окончания тестирования.
    /// </summary>
    public required DateTime TestEndTime { get; init; }
}
