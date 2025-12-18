namespace ApiPulse.Models;

/// <summary>
/// Полный результат нагрузочного тестирования, включающий статистику и данные для графиков.
/// </summary>
public sealed class LoadTestResult
{
    /// <summary>
    /// Агрегированная статистика тестирования.
    /// </summary>
    public required LoadTestStatistics Statistics { get; init; }

    /// <summary>
    /// Данные для построения графиков.
    /// </summary>
    public required ChartData ChartData { get; init; }
}
