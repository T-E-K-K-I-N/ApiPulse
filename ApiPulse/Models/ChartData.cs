namespace ApiPulse.Models;

/// <summary>
/// Данные для построения графиков результатов нагрузочного тестирования.
/// </summary>
public sealed class ChartData
{
    /// <summary>
    /// Временной ряд значений времени ответа по секундам теста.
    /// </summary>
    public required List<TimeSeriesPoint> ResponseTimeSeries { get; init; }

    /// <summary>
    /// Временной ряд количества запросов в секунду.
    /// </summary>
    public required List<TimeSeriesPoint> RequestsPerSecondSeries { get; init; }

    /// <summary>
    /// Распределение HTTP-кодов статуса (код -> количество).
    /// </summary>
    public required Dictionary<int, int> StatusCodeDistribution { get; init; }

    /// <summary>
    /// Распределение времени ответа по диапазонам (диапазон -> количество).
    /// </summary>
    public required Dictionary<string, int> ResponseTimeDistribution { get; init; }
}

/// <summary>
/// Точка временного ряда для графиков.
/// </summary>
/// <param name="SecondOffset">Смещение в секундах от начала теста.</param>
/// <param name="Value">Значение метрики в данный момент времени.</param>
public sealed record TimeSeriesPoint(int SecondOffset, double Value);
