using ApiPulse.Models;

namespace ApiPulse.Services;

/// <summary>
/// Интерфейс для потокобезопасного сбора и агрегации статистики нагрузочного тестирования.
/// </summary>
public interface IStatisticsCollector
{
    /// <summary>
    /// Записывает результат выполнения одного запроса.
    /// </summary>
    /// <param name="result">Результат выполнения запроса.</param>
    void RecordResult(RequestResult result);

    /// <summary>
    /// Вычисляет и возвращает агрегированную статистику тестирования.
    /// </summary>
    /// <param name="config">Конфигурация теста.</param>
    /// <param name="startTime">Время начала теста.</param>
    /// <param name="endTime">Время окончания теста.</param>
    /// <returns>Агрегированная статистика тестирования.</returns>
    LoadTestStatistics GetStatistics(LoadTestConfiguration config, DateTime startTime, DateTime endTime);

    /// <summary>
    /// Формирует данные для построения графиков на основе собранных результатов.
    /// </summary>
    /// <param name="startTime">Время начала теста для вычисления временных смещений.</param>
    /// <returns>Данные для построения графиков.</returns>
    ChartData GetChartData(DateTime startTime);

    /// <summary>
    /// Возвращает текущее общее количество выполненных запросов.
    /// </summary>
    /// <returns>Количество запросов.</returns>
    int GetCurrentRequestCount();

    /// <summary>
    /// Возвращает текущее количество успешных запросов.
    /// </summary>
    /// <returns>Количество успешных запросов.</returns>
    int GetSuccessCount();

    /// <summary>
    /// Возвращает текущее количество неудачных запросов.
    /// </summary>
    /// <returns>Количество неудачных запросов.</returns>
    int GetFailureCount();

    /// <summary>
    /// Сбрасывает все накопленные результаты и счётчики.
    /// </summary>
    void Reset();
}
