namespace ApiPulse.Models;

public sealed class LoadTestStatistics
{
    public required string TargetUrl { get; init; }
    public required string Domain { get; init; }
    public required int ThreadCount { get; init; }
    public required int DurationSeconds { get; init; }
    public required long MinResponseTimeMs { get; init; }
    public required long MaxResponseTimeMs { get; init; }
    public required double AverageResponseTimeMs { get; init; }
    public required double MedianResponseTimeMs { get; init; }
    public required double Percentile95Ms { get; init; }
    public required int TotalRequests { get; init; }
    public required int SuccessfulRequests { get; init; }
    public required int FailedRequests { get; init; }
    public required double RequestsPerSecond { get; init; }
    public required double SuccessRate { get; init; }
    public required DateTime TestStartTime { get; init; }
    public required DateTime TestEndTime { get; init; }
}
