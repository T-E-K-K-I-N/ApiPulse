namespace ApiPulse.Models;

public sealed record LoadTestProgress
{
    public required double ElapsedSeconds { get; init; }
    public required int TotalRequests { get; init; }
    public required int SuccessCount { get; init; }
    public required int FailureCount { get; init; }
}
