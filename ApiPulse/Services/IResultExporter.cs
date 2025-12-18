using ApiPulse.Models;

namespace ApiPulse.Services;

public interface IResultExporter
{
    Task<string> ExportToFileAsync(LoadTestStatistics stats, string? customPath = null);
}
