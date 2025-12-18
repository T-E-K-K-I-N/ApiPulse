namespace ApiPulse.Services;

/// <summary>
/// Интерфейс для управления историей использованных URL-адресов.
/// </summary>
public interface IUrlHistoryService
{
    /// <summary>
    /// Возвращает список недавно использованных URL-адресов.
    /// </summary>
    /// <returns>Список URL-адресов, отсортированный по времени использования (новые первыми).</returns>
    IReadOnlyList<string> GetRecentUrls();

    /// <summary>
    /// Добавляет URL-адрес в историю. Если URL уже существует, он перемещается в начало списка.
    /// </summary>
    /// <param name="url">URL-адрес для добавления.</param>
    void AddUrl(string url);

    /// <summary>
    /// Асинхронно загружает историю URL-адресов из файла.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Асинхронно сохраняет историю URL-адресов в файл.
    /// </summary>
    Task SaveAsync();
}
