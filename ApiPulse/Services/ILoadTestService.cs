using ApiPulse.Models;

namespace ApiPulse.Services;

public interface ILoadTestService
{
    Task<LoadTestStatistics> RunLoadTestAsync(
        LoadTestConfiguration config,
        IProgress<LoadTestProgress> progress,
        CancellationToken cancellationToken);
}
