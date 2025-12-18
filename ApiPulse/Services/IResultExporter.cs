using ApiPulse.Models;

namespace ApiPulse.Services;

/// <summary>
/// Интерфейс для экспорта результатов нагрузочного тестирования в файл.
/// </summary>
public interface IResultExporter
{
    /// <summary>
    /// Экспортирует статистику тестирования в текстовый файл.
    /// </summary>
    /// <param name="stats">Статистика для экспорта.</param>
    /// <param name="customPath">Пользовательский путь для сохранения файла. Если не указан, используется имя по умолчанию в текущей директории.</param>
    /// <returns>Путь к созданному файлу.</returns>
    Task<string> ExportToFileAsync(LoadTestStatistics stats, string? customPath = null);
}
