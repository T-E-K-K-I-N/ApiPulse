namespace ApiPulse.Services;

public interface IUrlHistoryService
{
    IReadOnlyList<string> GetRecentUrls();
    void AddUrl(string url);
    Task LoadAsync();
    Task SaveAsync();
}
