using ApiPulse.Models;

namespace ApiPulse.Services;

/// <summary>
/// Интерфейс сервиса для выполнения нагрузочного тестирования API.
/// </summary>
public interface ILoadTestService
{
    /// <summary>
    /// Запускает нагрузочное тестирование с заданной конфигурацией.
    /// </summary>
    /// <param name="config">Конфигурация нагрузочного теста.</param>
    /// <param name="progress">Интерфейс для отчёта о прогрессе выполнения.</param>
    /// <param name="cancellationToken">Токен отмены для прерывания теста.</param>
    /// <returns>Результат нагрузочного тестирования, включающий статистику и данные для графиков.</returns>
    Task<LoadTestResult> RunLoadTestAsync(
        LoadTestConfiguration config,
        IProgress<LoadTestProgress> progress,
        CancellationToken cancellationToken);
}
