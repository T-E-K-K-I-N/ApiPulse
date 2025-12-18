using ApiPulse.Models;

namespace ApiPulse.Services;

/// <summary>
/// Интерфейс сервиса для отображения и экспорта графиков результатов тестирования.
/// </summary>
public interface IChartService
{
    /// <summary>
    /// Отображает графики результатов тестирования в консоли с использованием Spectre.Console.
    /// </summary>
    /// <param name="chartData">Данные для построения графиков.</param>
    /// <param name="stats">Статистика тестирования для заголовков.</param>
    void DisplayConsoleCharts(ChartData chartData, LoadTestStatistics stats);

    /// <summary>
    /// Экспортирует графики результатов тестирования в PNG-файлы.
    /// </summary>
    /// <param name="chartData">Данные для построения графиков.</param>
    /// <param name="stats">Статистика тестирования для заголовков.</param>
    /// <param name="outputDirectory">Директория для сохранения файлов.</param>
    /// <returns>Список путей к созданным файлам.</returns>
    Task<List<string>> ExportChartsToPngAsync(ChartData chartData, LoadTestStatistics stats, string outputDirectory);
}
