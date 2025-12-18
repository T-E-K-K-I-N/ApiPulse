using ApiPulse.Extensions;
using ApiPulse.Models;
using ApiPulse.Services;
using ApiPulse.UI;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

var services = new ServiceCollection();
services.AddApiPulseServices();

await using var serviceProvider = services.BuildServiceProvider();

var consoleUI = serviceProvider.GetRequiredService<IConsoleUI>();
var loadTestService = serviceProvider.GetRequiredService<ILoadTestService>();
var resultExporter = serviceProvider.GetRequiredService<IResultExporter>();
var urlHistoryService = serviceProvider.GetRequiredService<IUrlHistoryService>();

// Load URL history
await urlHistoryService.LoadAsync();

try
{
    LoadTestConfiguration config;
    bool isInteractive = args.Length == 0;

    // Check for command-line arguments or interactive mode
    if (args.Length >= 3)
    {
        // Command-line mode: ApiPulse <url> <threads> <duration> [method] [body] [content-type] [query-params]
        if (!Uri.TryCreate(args[0], UriKind.Absolute, out var uri))
        {
            AnsiConsole.MarkupLine("[red]Неверный формат URL[/]");
            return 1;
        }
        if (!int.TryParse(args[1], out var threads) || threads < 1 || threads > 1000)
        {
            AnsiConsole.MarkupLine("[red]Количество потоков должно быть от 1 до 1000[/]");
            return 1;
        }
        if (!int.TryParse(args[2], out var duration) || duration < 1 || duration > 3600)
        {
            AnsiConsole.MarkupLine("[red]Длительность должна быть от 1 до 3600 секунд[/]");
            return 1;
        }

        // Parse optional parameters
        var httpMethod = HttpMethod.Get;
        string? requestBody = null;
        var contentType = "application/json";
        Dictionary<string, string>? queryParameters = null;

        for (var i = 3; i < args.Length; i++)
        {
            if (args[i].StartsWith("--method=", StringComparison.OrdinalIgnoreCase))
            {
                var methodStr = args[i]["--method=".Length..].ToUpperInvariant();
                httpMethod = methodStr switch
                {
                    "POST" => HttpMethod.Post,
                    "PUT" => HttpMethod.Put,
                    "PATCH" => HttpMethod.Patch,
                    "DELETE" => HttpMethod.Delete,
                    _ => HttpMethod.Get
                };
            }
            else if (args[i].StartsWith("--body=", StringComparison.OrdinalIgnoreCase))
            {
                requestBody = args[i]["--body=".Length..];
            }
            else if (args[i].StartsWith("--content-type=", StringComparison.OrdinalIgnoreCase))
            {
                contentType = args[i]["--content-type=".Length..];
            }
            else if (args[i].StartsWith("--query=", StringComparison.OrdinalIgnoreCase))
            {
                var queryStr = args[i]["--query=".Length..];
                queryParameters = ParseQueryParameters(queryStr);
            }
        }

        config = new LoadTestConfiguration
        {
            TargetUrl = uri,
            ThreadCount = threads,
            DurationSeconds = duration,
            HttpMethod = httpMethod,
            RequestBody = requestBody,
            ContentType = contentType,
            QueryParameters = queryParameters
        };

        // Save URL to history (CLI mode)
        urlHistoryService.AddUrl(config.TargetUrl.ToString());
        await urlHistoryService.SaveAsync();

        AnsiConsole.Write(new FigletText("ApiPulse").Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[grey]Инструмент нагрузочного тестирования REST API[/]\n");
        AnsiConsole.MarkupLine($"[cyan]URL:[/] {config.TargetUrl}");
        AnsiConsole.MarkupLine($"[cyan]HTTP метод:[/] {config.HttpMethod.Method}");
        if (config.QueryParameters != null && config.QueryParameters.Count > 0)
        {
            AnsiConsole.MarkupLine($"[cyan]Query параметры:[/] {string.Join(", ", config.QueryParameters.Select(p => $"{p.Key}={p.Value}"))}");
        }
        if (!string.IsNullOrEmpty(config.RequestBody))
        {
            AnsiConsole.MarkupLine($"[cyan]Тело запроса:[/] {(config.RequestBody.Length > 50 ? config.RequestBody[..50] + "..." : config.RequestBody)}");
            AnsiConsole.MarkupLine($"[cyan]Content-Type:[/] {config.ContentType}");
        }
        AnsiConsole.MarkupLine($"[cyan]Потоков:[/] {config.ThreadCount}");
        AnsiConsole.MarkupLine($"[cyan]Длительность:[/] {config.DurationSeconds} сек.\n");
    }
    else if (args.Length > 0 && args.Length < 3)
    {
        AnsiConsole.MarkupLine("[yellow]Использование:[/] ApiPulse <url> <потоки> <длительность> [опции]");
        AnsiConsole.MarkupLine("[yellow]Опции:[/]");
        AnsiConsole.MarkupLine("  --method=GET|POST|PUT|PATCH|DELETE  HTTP метод (по умолчанию: GET)");
        AnsiConsole.MarkupLine("  --body=\"{...}\"                      Тело запроса");
        AnsiConsole.MarkupLine("  --content-type=application/json     Content-Type заголовок");
        AnsiConsole.MarkupLine("  --query=\"key1=value1&key2=value2\"   Query параметры");
        AnsiConsole.MarkupLine("[yellow]Пример:[/] ApiPulse https://api.example.com 10 30 --method=POST --body=\"{\\\"name\\\":\\\"test\\\"}\"");
        return 1;
    }
    else
    {
        // Interactive mode
        config = consoleUI.GetConfigurationFromUser(urlHistoryService.GetRecentUrls());

        // Save URL to history
        urlHistoryService.AddUrl(config.TargetUrl.ToString());
        await urlHistoryService.SaveAsync();

        // Confirmation in interactive mode
        if (!consoleUI.ConfirmStart(config))
        {
            AnsiConsole.MarkupLine("[yellow]Тест отменён.[/]");
            return 0;
        }
    }

    // Setup cancellation
    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
        AnsiConsole.MarkupLine("\n[yellow]Запрошена отмена...[/]");
    };

    // Run the test with progress display
    var stats = await consoleUI.DisplayProgressAsync(
        config,
        (progress, ct) => loadTestService.RunLoadTestAsync(config, progress, ct),
        cts.Token);

    // Display results
    consoleUI.DisplayResults(stats);

    // Ask about file export (only in interactive mode) or auto-save in CLI mode
    if (isInteractive)
    {
        if (consoleUI.AskToSaveResults())
        {
            var customPath = consoleUI.AskForSavePath();
            var filename = await resultExporter.ExportToFileAsync(stats, customPath);
            consoleUI.DisplayFileSaved(filename);
        }
    }
    else
    {
        // Auto-save in CLI mode
        var filename = await resultExporter.ExportToFileAsync(stats);
        consoleUI.DisplayFileSaved(filename);
    }

    return 0;
}
catch (OperationCanceledException)
{
    consoleUI.DisplayCancelled();
    return 1;
}
catch (Exception ex)
{
    consoleUI.DisplayError(ex.Message);
    return 1;
}

static Dictionary<string, string>? ParseQueryParameters(string queryStr)
{
    if (string.IsNullOrWhiteSpace(queryStr))
    {
        return null;
    }

    var result = new Dictionary<string, string>();
    var pairs = queryStr.Split('&', StringSplitOptions.RemoveEmptyEntries);

    foreach (var pair in pairs)
    {
        var keyValue = pair.Split('=', 2);
        if (keyValue.Length == 2)
        {
            result[keyValue[0]] = keyValue[1];
        }
        else if (keyValue.Length == 1)
        {
            result[keyValue[0]] = string.Empty;
        }
    }

    return result.Count > 0 ? result : null;
}
