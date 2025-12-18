namespace ApiPulse.Models;

public sealed record LoadTestConfiguration
{
    public required Uri TargetUrl { get; init; }
    public required int ThreadCount { get; init; }
    public required int DurationSeconds { get; init; }
    public int TimeoutSeconds { get; init; } = 30;
    public int MaxRetries { get; init; } = 3;
    public HttpMethod HttpMethod { get; init; } = HttpMethod.Get;
    public Dictionary<string, string>? QueryParameters { get; init; }
    public string? RequestBody { get; init; }
    public string ContentType { get; init; } = "application/json";
}
