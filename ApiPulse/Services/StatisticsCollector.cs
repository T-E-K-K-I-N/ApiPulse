using System.Collections.Concurrent;
using ApiPulse.Models;

namespace ApiPulse.Services;

/// <summary>
/// Потокобезопасный коллектор статистики для нагрузочного тестирования.
/// Использует <see cref="ConcurrentBag{T}"/> и атомарные операции для безопасного доступа из нескольких потоков.
/// </summary>
public sealed class StatisticsCollector : IStatisticsCollector
{
    private readonly ConcurrentBag<RequestResult> _results = new();
    private int _successCount;
    private int _failureCount;

    /// <inheritdoc />
    public void RecordResult(RequestResult result)
    {
        _results.Add(result);
        if (result.IsSuccess)
            Interlocked.Increment(ref _successCount);
        else
            Interlocked.Increment(ref _failureCount);
    }

    /// <inheritdoc />
    public int GetCurrentRequestCount() => _results.Count;

    /// <inheritdoc />
    public int GetSuccessCount() => Volatile.Read(ref _successCount);

    /// <inheritdoc />
    public int GetFailureCount() => Volatile.Read(ref _failureCount);

    /// <inheritdoc />
    public void Reset()
    {
        _results.Clear();
        Interlocked.Exchange(ref _successCount, 0);
        Interlocked.Exchange(ref _failureCount, 0);
    }

    /// <inheritdoc />
    public ChartData GetChartData(DateTime startTime)
    {
        var results = _results.ToArray();

        var responseTimeSeries = results
            .GroupBy(r => (int)(r.Timestamp - startTime).TotalSeconds)
            .OrderBy(g => g.Key)
            .Select(g => new TimeSeriesPoint(g.Key, g.Average(r => r.ResponseTimeMs)))
            .ToList();

        var requestsPerSecondSeries = results
            .GroupBy(r => (int)(r.Timestamp - startTime).TotalSeconds)
            .OrderBy(g => g.Key)
            .Select(g => new TimeSeriesPoint(g.Key, g.Count()))
            .ToList();

        var statusCodeDistribution = results
            .GroupBy(r => r.StatusCode)
            .ToDictionary(g => g.Key, g => g.Count());

        var responseTimeDistribution = new Dictionary<string, int>
        {
            ["0-100 мс"] = results.Count(r => r.ResponseTimeMs <= 100),
            ["100-300 мс"] = results.Count(r => r.ResponseTimeMs > 100 && r.ResponseTimeMs <= 300),
            ["300-500 мс"] = results.Count(r => r.ResponseTimeMs > 300 && r.ResponseTimeMs <= 500),
            ["500-1000 мс"] = results.Count(r => r.ResponseTimeMs > 500 && r.ResponseTimeMs <= 1000),
            [">1000 мс"] = results.Count(r => r.ResponseTimeMs > 1000)
        };

        return new ChartData
        {
            ResponseTimeSeries = responseTimeSeries,
            RequestsPerSecondSeries = requestsPerSecondSeries,
            StatusCodeDistribution = statusCodeDistribution,
            ResponseTimeDistribution = responseTimeDistribution
        };
    }

    /// <inheritdoc />
    public LoadTestStatistics GetStatistics(LoadTestConfiguration config, DateTime startTime, DateTime endTime)
    {
        var results = _results.ToArray();
        var responseTimes = results.Select(r => r.ResponseTimeMs).OrderBy(t => t).ToArray();

        var totalRequests = results.Length;
        var successfulRequests = Volatile.Read(ref _successCount);
        var failedRequests = Volatile.Read(ref _failureCount);
        var duration = (endTime - startTime).TotalSeconds;

        if (responseTimes.Length == 0)
        {
            return new LoadTestStatistics
            {
                TargetUrl = config.TargetUrl.ToString(),
                Domain = config.TargetUrl.Host,
                ThreadCount = config.ThreadCount,
                DurationSeconds = config.DurationSeconds,
                MinResponseTimeMs = 0,
                MaxResponseTimeMs = 0,
                AverageResponseTimeMs = 0,
                MedianResponseTimeMs = 0,
                Percentile95Ms = 0,
                TotalRequests = 0,
                SuccessfulRequests = 0,
                FailedRequests = 0,
                RequestsPerSecond = 0,
                SuccessRate = 0,
                TestStartTime = startTime,
                TestEndTime = endTime
            };
        }

        return new LoadTestStatistics
        {
            TargetUrl = config.TargetUrl.ToString(),
            Domain = config.TargetUrl.Host,
            ThreadCount = config.ThreadCount,
            DurationSeconds = config.DurationSeconds,
            MinResponseTimeMs = responseTimes.Min(),
            MaxResponseTimeMs = responseTimes.Max(),
            AverageResponseTimeMs = responseTimes.Average(),
            MedianResponseTimeMs = CalculatePercentile(responseTimes, 50),
            Percentile95Ms = CalculatePercentile(responseTimes, 95),
            TotalRequests = totalRequests,
            SuccessfulRequests = successfulRequests,
            FailedRequests = failedRequests,
            RequestsPerSecond = duration > 0 ? totalRequests / duration : 0,
            SuccessRate = totalRequests > 0 ? (double)successfulRequests / totalRequests * 100 : 0,
            TestStartTime = startTime,
            TestEndTime = endTime
        };
    }

    /// <summary>
    /// Вычисляет значение перцентиля для отсортированного массива значений.
    /// </summary>
    /// <param name="sortedValues">Отсортированный массив значений времени ответа.</param>
    /// <param name="percentile">Номер перцентиля (0-100).</param>
    /// <returns>Значение указанного перцентиля.</returns>
    private static double CalculatePercentile(long[] sortedValues, int percentile)
    {
        if (sortedValues.Length == 0)
            return 0;

        var index = (percentile / 100.0) * (sortedValues.Length - 1);
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);

        if (lower == upper)
            return sortedValues[lower];

        var fraction = index - lower;
        return sortedValues[lower] + (sortedValues[upper] - sortedValues[lower]) * fraction;
    }
}
