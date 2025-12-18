namespace ApiPulse.Models;

public sealed record RequestResult
{
    public required long ResponseTimeMs { get; init; }
    public required bool IsSuccess { get; init; }
    public required int StatusCode { get; init; }
    public string? ErrorMessage { get; init; }
    public required DateTime Timestamp { get; init; }
}
