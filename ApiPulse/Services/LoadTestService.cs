using System.Diagnostics;
using ApiPulse.Models;

namespace ApiPulse.Services;

/// <summary>
/// Сервис для выполнения нагрузочного тестирования API.
/// Управляет параллельными рабочими потоками и сбором результатов.
/// </summary>
public sealed class LoadTestService : ILoadTestService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IStatisticsCollector _statisticsCollector;

    /// <summary>
    /// Инициализирует новый экземпляр сервиса нагрузочного тестирования.
    /// </summary>
    /// <param name="httpClientFactory">Фабрика для создания HTTP-клиентов.</param>
    /// <param name="statisticsCollector">Коллектор для сбора статистики запросов.</param>
    public LoadTestService(IHttpClientFactory httpClientFactory, IStatisticsCollector statisticsCollector)
    {
        _httpClientFactory = httpClientFactory;
        _statisticsCollector = statisticsCollector;
    }

    /// <inheritdoc />
    public async Task<LoadTestResult> RunLoadTestAsync(
        LoadTestConfiguration config,
        IProgress<LoadTestProgress> progress,
        CancellationToken cancellationToken)
    {
        _statisticsCollector.Reset();

        var startTime = DateTime.UtcNow;
        using var testCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        testCts.CancelAfter(TimeSpan.FromSeconds(config.DurationSeconds));

        var overallStopwatch = Stopwatch.StartNew();

        // Start progress reporting task
        var progressTask = ReportProgressAsync(config, overallStopwatch, progress, testCts.Token);

        // Create and start worker tasks
        var tasks = Enumerable.Range(0, config.ThreadCount)
            .Select(_ => ExecuteWorkerAsync(config, testCts.Token))
            .ToList();

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            // Expected - test duration completed
        }

        overallStopwatch.Stop();
        var endTime = DateTime.UtcNow;

        // Wait for progress task to complete
        try
        {
            await progressTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Report final progress
        progress.Report(new LoadTestProgress
        {
            ElapsedSeconds = config.DurationSeconds,
            TotalRequests = _statisticsCollector.GetCurrentRequestCount(),
            SuccessCount = _statisticsCollector.GetSuccessCount(),
            FailureCount = _statisticsCollector.GetFailureCount()
        });

        return new LoadTestResult
        {
            Statistics = _statisticsCollector.GetStatistics(config, startTime, endTime),
            ChartData = _statisticsCollector.GetChartData(startTime)
        };
    }

    /// <summary>
    /// Асинхронно отправляет отчёты о прогрессе выполнения теста.
    /// </summary>
    /// <param name="config">Конфигурация теста.</param>
    /// <param name="stopwatch">Секундомер для измерения прошедшего времени.</param>
    /// <param name="progress">Интерфейс для отчёта о прогрессе.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    private async Task ReportProgressAsync(
        LoadTestConfiguration config,
        Stopwatch stopwatch,
        IProgress<LoadTestProgress> progress,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(100, cancellationToken);

                var elapsed = stopwatch.Elapsed.TotalSeconds;
                progress.Report(new LoadTestProgress
                {
                    ElapsedSeconds = Math.Min(elapsed, config.DurationSeconds),
                    TotalRequests = _statisticsCollector.GetCurrentRequestCount(),
                    SuccessCount = _statisticsCollector.GetSuccessCount(),
                    FailureCount = _statisticsCollector.GetFailureCount()
                });
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Выполняет рабочий цикл отправки HTTP-запросов до отмены токена.
    /// </summary>
    /// <param name="config">Конфигурация теста.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    private async Task ExecuteWorkerAsync(LoadTestConfiguration config, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("LoadTest");
        var targetUri = BuildUriWithQueryParameters(config);

        while (!cancellationToken.IsCancellationRequested)
        {
            var stopwatch = Stopwatch.StartNew();
            RequestResult result;

            try
            {
                using var request = CreateHttpRequest(config, targetUri);
                using var response = await client.SendAsync(request, cancellationToken);
                stopwatch.Stop();

                result = new RequestResult
                {
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    IsSuccess = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                result = new RequestResult
                {
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    IsSuccess = false,
                    StatusCode = 0,
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Test ended, exit gracefully
                break;
            }
            catch (TaskCanceledException ex)
            {
                // Timeout
                stopwatch.Stop();
                result = new RequestResult
                {
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    IsSuccess = false,
                    StatusCode = 0,
                    ErrorMessage = "Request timeout: " + ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }

            _statisticsCollector.RecordResult(result);
        }
    }

    /// <summary>
    /// Строит URI с добавлением параметров строки запроса из конфигурации.
    /// </summary>
    /// <param name="config">Конфигурация теста, содержащая URL и параметры запроса.</param>
    /// <returns>Сформированный URI с параметрами запроса.</returns>
    private static Uri BuildUriWithQueryParameters(LoadTestConfiguration config)
    {
        if (config.QueryParameters == null || config.QueryParameters.Count == 0)
        {
            return config.TargetUrl;
        }

        var uriBuilder = new UriBuilder(config.TargetUrl);
        var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach (var param in config.QueryParameters)
        {
            query[param.Key] = param.Value;
        }

        uriBuilder.Query = query.ToString();
        return uriBuilder.Uri;
    }

    /// <summary>
    /// Создаёт HTTP-запрос на основе конфигурации теста.
    /// </summary>
    /// <param name="config">Конфигурация теста.</param>
    /// <param name="targetUri">Целевой URI для запроса.</param>
    /// <returns>Сконфигурированный HTTP-запрос.</returns>
    private static HttpRequestMessage CreateHttpRequest(LoadTestConfiguration config, Uri targetUri)
    {
        var request = new HttpRequestMessage(config.HttpMethod, targetUri);

        if (!string.IsNullOrEmpty(config.RequestBody) &&
            config.HttpMethod != HttpMethod.Get &&
            config.HttpMethod != HttpMethod.Head)
        {
            request.Content = new StringContent(config.RequestBody, System.Text.Encoding.UTF8, config.ContentType);
        }

        return request;
    }
}
