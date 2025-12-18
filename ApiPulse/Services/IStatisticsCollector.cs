using ApiPulse.Models;

namespace ApiPulse.Services;

public interface IStatisticsCollector
{
    void RecordResult(RequestResult result);
    LoadTestStatistics GetStatistics(LoadTestConfiguration config, DateTime startTime, DateTime endTime);
    int GetCurrentRequestCount();
    int GetSuccessCount();
    int GetFailureCount();
    void Reset();
}
