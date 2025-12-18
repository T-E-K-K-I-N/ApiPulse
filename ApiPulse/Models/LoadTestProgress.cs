namespace ApiPulse.Models;

/// <summary>
/// Представляет текущий прогресс выполнения нагрузочного теста в реальном времени.
/// </summary>
public sealed record LoadTestProgress
{
    /// <summary>
    /// Количество секунд, прошедших с начала теста.
    /// </summary>
    public required double ElapsedSeconds { get; init; }

    /// <summary>
    /// Общее количество выполненных запросов.
    /// </summary>
    public required int TotalRequests { get; init; }

    /// <summary>
    /// Количество успешных запросов.
    /// </summary>
    public required int SuccessCount { get; init; }

    /// <summary>
    /// Количество неудачных запросов.
    /// </summary>
    public required int FailureCount { get; init; }
}
