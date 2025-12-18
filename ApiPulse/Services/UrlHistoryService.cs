using System.Text.Json;

namespace ApiPulse.Services;

/// <summary>
/// Сервис для сохранения и загрузки истории использованных URL-адресов.
/// История хранится в JSON-файле в папке LocalApplicationData.
/// </summary>
public sealed class UrlHistoryService : IUrlHistoryService
{
    /// <summary>
    /// Максимальное количество URL-адресов в истории.
    /// </summary>
    private const int MaxHistorySize = 5;

    private readonly string _historyFilePath;
    private readonly List<string> _urls = new();
    private readonly object _lock = new();

    /// <summary>
    /// Инициализирует новый экземпляр сервиса и создаёт директорию для хранения истории при необходимости.
    /// </summary>
    public UrlHistoryService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var apiPulsePath = Path.Combine(appDataPath, "ApiPulse");
        Directory.CreateDirectory(apiPulsePath);
        _historyFilePath = Path.Combine(apiPulsePath, "url_history.json");
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetRecentUrls()
    {
        lock (_lock)
        {
            return _urls.ToList().AsReadOnly();
        }
    }

    /// <inheritdoc />
    public void AddUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        lock (_lock)
        {
            _urls.Remove(url);
            _urls.Insert(0, url);

            if (_urls.Count > MaxHistorySize)
            {
                _urls.RemoveAt(_urls.Count - 1);
            }
        }
    }

    /// <inheritdoc />
    public async Task LoadAsync()
    {
        if (!File.Exists(_historyFilePath))
            return;

        try
        {
            var json = await File.ReadAllTextAsync(_historyFilePath);
            var urls = JsonSerializer.Deserialize<List<string>>(json);

            if (urls != null)
            {
                lock (_lock)
                {
                    _urls.Clear();
                    _urls.AddRange(urls.Take(MaxHistorySize));
                }
            }
        }
        catch (JsonException)
        {
            // Ignore corrupted history file
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync()
    {
        List<string> urlsCopy;
        lock (_lock)
        {
            urlsCopy = _urls.ToList();
        }

        var json = JsonSerializer.Serialize(urlsCopy, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_historyFilePath, json);
    }
}
