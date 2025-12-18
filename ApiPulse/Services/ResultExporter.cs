using System.Text;
using ApiPulse.Models;

namespace ApiPulse.Services;

/// <summary>
/// Сервис для экспорта результатов нагрузочного тестирования в текстовые файлы.
/// </summary>
public sealed class ResultExporter : IResultExporter
{
    /// <inheritdoc />
    public async Task<string> ExportToFileAsync(LoadTestStatistics stats, string? customPath = null)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var defaultFileName = $"ApiPulse_Results_{timestamp}.txt";

        string filename;
        if (string.IsNullOrWhiteSpace(customPath))
        {
            filename = defaultFileName;
        }
        else if (Directory.Exists(customPath))
        {
            filename = Path.Combine(customPath, defaultFileName);
        }
        else
        {
            filename = customPath;
        }

        var sb = new StringBuilder();
        sb.AppendLine(new string('=', 60));
        sb.AppendLine("      API PULSE - РЕЗУЛЬТАТЫ НАГРУЗОЧНОГО ТЕСТА");
        sb.AppendLine(new string('=', 60));
        sb.AppendLine();
        sb.AppendLine($"Дата теста:      {stats.TestStartTime:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"Целевой URL:     {stats.TargetUrl}");
        sb.AppendLine($"Кол-во потоков:  {stats.ThreadCount}");
        sb.AppendLine($"Длительность:    {stats.DurationSeconds} сек.");
        sb.AppendLine();
        sb.AppendLine(new string('-', 60));
        sb.AppendLine("ВРЕМЯ ОТКЛИКА");
        sb.AppendLine(new string('-', 60));
        sb.AppendLine($"Минимальное:     {stats.MinResponseTimeMs} мс");
        sb.AppendLine($"Максимальное:    {stats.MaxResponseTimeMs} мс");
        sb.AppendLine($"Среднее:         {stats.AverageResponseTimeMs:F2} мс");
        sb.AppendLine($"Медиана:         {stats.MedianResponseTimeMs:F2} мс");
        sb.AppendLine($"95-й перцентиль: {stats.Percentile95Ms:F2} мс");
        sb.AppendLine();
        sb.AppendLine(new string('-', 60));
        sb.AppendLine("СТАТИСТИКА ЗАПРОСОВ");
        sb.AppendLine(new string('-', 60));
        sb.AppendLine($"Всего запросов:  {stats.TotalRequests}");
        sb.AppendLine($"Успешных:        {stats.SuccessfulRequests}");
        sb.AppendLine($"Неудачных:       {stats.FailedRequests}");
        sb.AppendLine($"Успешность:      {stats.SuccessRate:F2}%");
        sb.AppendLine($"Пропускная сп.:  {stats.RequestsPerSecond:F2} зап./сек.");
        sb.AppendLine();
        sb.AppendLine(new string('=', 60));

        await File.WriteAllTextAsync(filename, sb.ToString());
        return filename;
    }
}
