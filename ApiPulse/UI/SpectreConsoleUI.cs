using ApiPulse.Models;
using Spectre.Console;

namespace ApiPulse.UI;

public sealed class SpectreConsoleUI : IConsoleUI
{
    private const string NewUrlOption = "[cyan]Ввести новый URL[/]";

    public LoadTestConfiguration GetConfigurationFromUser(IReadOnlyList<string> urlHistory)
    {
        AnsiConsole.Write(new FigletText("ApiPulse").Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[grey]Инструмент нагрузочного тестирования REST API[/]\n");

        var url = GetUrlFromUserOrHistory(urlHistory);

        var httpMethod = GetHttpMethodFromUser();

        var queryParameters = GetQueryParametersFromUser();

        string? requestBody = null;
        var contentType = "application/json";

        if (httpMethod != HttpMethod.Get && httpMethod != HttpMethod.Head && httpMethod != HttpMethod.Delete)
        {
            (requestBody, contentType) = GetRequestBodyFromUser();
        }

        var threads = AnsiConsole.Prompt(
            new TextPrompt<int>("Введите [green]количество параллельных потоков[/] (1-1000):")
                .PromptStyle("cyan")
                .DefaultValue(10)
                .ValidationErrorMessage("[red]Должно быть от 1 до 1000[/]")
                .Validate(n => n is >= 1 and <= 1000
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Должно быть от 1 до 1000[/]")));

        var duration = AnsiConsole.Prompt(
            new TextPrompt<int>("Введите [green]длительность теста (секунды)[/] (1-3600):")
                .PromptStyle("cyan")
                .DefaultValue(30)
                .ValidationErrorMessage("[red]Должно быть от 1 до 3600[/]")
                .Validate(n => n is >= 1 and <= 3600
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Должно быть от 1 до 3600[/]")));

        return new LoadTestConfiguration
        {
            TargetUrl = new Uri(url),
            ThreadCount = threads,
            DurationSeconds = duration,
            HttpMethod = httpMethod,
            QueryParameters = queryParameters,
            RequestBody = requestBody,
            ContentType = contentType
        };
    }

    public bool ConfirmStart(LoadTestConfiguration config)
    {
        AnsiConsole.WriteLine();

        var summary = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("Параметр")
            .AddColumn("Значение");

        summary.AddRow("URL", config.TargetUrl.ToString());
        summary.AddRow("HTTP метод", config.HttpMethod.Method);

        if (config.QueryParameters != null && config.QueryParameters.Count > 0)
        {
            var paramsStr = string.Join(", ", config.QueryParameters.Select(p => $"{p.Key}={p.Value}"));
            summary.AddRow("Query параметры", paramsStr);
        }

        if (!string.IsNullOrEmpty(config.RequestBody))
        {
            var bodyPreview = config.RequestBody.Length > 50
                ? config.RequestBody[..50] + "..."
                : config.RequestBody;
            summary.AddRow("Тело запроса", $"{bodyPreview} ({config.ContentType})");
        }

        summary.AddRow("Потоков", config.ThreadCount.ToString());
        summary.AddRow("Длительность", $"{config.DurationSeconds} сек.");

        AnsiConsole.Write(summary);
        AnsiConsole.WriteLine();

        return AnsiConsole.Confirm("Запустить нагрузочный тест?");
    }

    public async Task<LoadTestStatistics> DisplayProgressAsync(
        LoadTestConfiguration config,
        Func<IProgress<LoadTestProgress>, CancellationToken, Task<LoadTestStatistics>> runTest,
        CancellationToken cancellationToken)
    {
        LoadTestStatistics? result = null;

        await AnsiConsole.Progress()
            .AutoRefresh(true)
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask($"[cyan]Тестирование {config.TargetUrl.Host}[/]", maxValue: config.DurationSeconds);

                var progress = new Progress<LoadTestProgress>(p =>
                {
                    task.Value = p.ElapsedSeconds;
                    task.Description = $"[cyan]Запросов: {p.TotalRequests}[/] | [green]Успешно: {p.SuccessCount}[/] | [red]Ошибок: {p.FailureCount}[/]";
                });

                result = await runTest(progress, cancellationToken);
                task.Value = config.DurationSeconds;
            });

        return result!;
    }

    public void DisplayResults(LoadTestStatistics stats)
    {
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .Title("[bold cyan]Результаты нагрузочного теста[/]")
            .AddColumn(new TableColumn("[yellow]Метрика[/]").Centered())
            .AddColumn(new TableColumn("[yellow]Значение[/]").Centered());

        table.AddRow("URL", $"[white]{stats.TargetUrl}[/]");
        table.AddRow("Потоков", $"[white]{stats.ThreadCount}[/]");
        table.AddRow("Длительность", $"[white]{stats.DurationSeconds} сек.[/]");
        table.AddEmptyRow();

        // Response times with color coding
        table.AddRow("[bold]Время отклика[/]", "");
        table.AddRow("Минимальное", FormatTime(stats.MinResponseTimeMs));
        table.AddRow("Максимальное", FormatTime(stats.MaxResponseTimeMs));
        table.AddRow("Среднее", FormatTime(stats.AverageResponseTimeMs));
        table.AddRow("Медиана", FormatTime(stats.MedianResponseTimeMs));
        table.AddRow("95-й перцентиль", FormatTime(stats.Percentile95Ms));
        table.AddEmptyRow();

        // Request counts
        table.AddRow("[bold]Статистика запросов[/]", "");
        table.AddRow("Всего запросов", $"[white]{stats.TotalRequests}[/]");
        table.AddRow("Успешных", $"[green]{stats.SuccessfulRequests}[/]");
        table.AddRow("Неудачных", stats.FailedRequests > 0
            ? $"[red]{stats.FailedRequests}[/]"
            : $"[green]{stats.FailedRequests}[/]");
        table.AddRow("Успешность", FormatSuccessRate(stats.SuccessRate));
        table.AddRow("Запросов/сек", $"[white]{stats.RequestsPerSecond:F2}[/]");

        AnsiConsole.Write(table);
    }

    public bool AskToSaveResults()
    {
        AnsiConsole.WriteLine();
        return AnsiConsole.Confirm("Сохранить результаты в файл?");
    }

    public string? AskForSavePath()
    {
        var path = AnsiConsole.Prompt(
            new TextPrompt<string>("Введите [green]путь для сохранения файла[/] (или Enter для пути по умолчанию):")
                .PromptStyle("cyan")
                .AllowEmpty());

        return string.IsNullOrWhiteSpace(path) ? null : path;
    }

    public void DisplayFileSaved(string filename)
    {
        AnsiConsole.MarkupLine($"[green]Результаты сохранены в:[/] [cyan]{filename}[/]");
    }

    public void DisplayError(string message)
    {
        AnsiConsole.MarkupLine($"[red]Ошибка: {message}[/]");
    }

    public void DisplayCancelled()
    {
        AnsiConsole.MarkupLine("[yellow]Тест был отменён.[/]");
    }

    private static string FormatTime(double ms) => ms switch
    {
        < 100 => $"[green]{ms:F0} мс[/]",
        < 500 => $"[yellow]{ms:F0} мс[/]",
        _ => $"[red]{ms:F0} мс[/]"
    };

    private static string FormatSuccessRate(double rate) => rate switch
    {
        >= 99 => $"[green]{rate:F2}%[/]",
        >= 95 => $"[yellow]{rate:F2}%[/]",
        _ => $"[red]{rate:F2}%[/]"
    };

    private string GetUrlFromUserOrHistory(IReadOnlyList<string> urlHistory)
    {
        if (urlHistory.Count == 0)
        {
            return PromptForNewUrl();
        }

        var choices = new List<string> { NewUrlOption };
        choices.AddRange(urlHistory);

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Выберите [green]URL[/] из истории или введите новый:")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Cyan1))
                .AddChoices(choices));

        return selection == NewUrlOption ? PromptForNewUrl() : selection;
    }

    private static string PromptForNewUrl()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>("Введите [green]целевой URL[/]:")
                .PromptStyle("cyan")
                .ValidationErrorMessage("[red]Неверный формат URL[/]")
                .Validate(input =>
                {
                    if (Uri.TryCreate(input, UriKind.Absolute, out var uri) &&
                        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    {
                        return ValidationResult.Success();
                    }
                    return ValidationResult.Error("[red]Введите корректный HTTP/HTTPS URL[/]");
                }));
    }

    private static HttpMethod GetHttpMethodFromUser()
    {
        var methodChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Выберите [green]HTTP метод[/]:")
                .HighlightStyle(new Style(Color.Cyan1))
                .AddChoices("GET", "POST", "PUT", "PATCH", "DELETE"));

        return methodChoice switch
        {
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "PATCH" => HttpMethod.Patch,
            "DELETE" => HttpMethod.Delete,
            _ => HttpMethod.Get
        };
    }

    private static Dictionary<string, string>? GetQueryParametersFromUser()
    {
        var addParams = AnsiConsole.Confirm("Добавить [green]query параметры[/]?", false);

        if (!addParams)
        {
            return null;
        }

        var parameters = new Dictionary<string, string>();

        while (true)
        {
            var key = AnsiConsole.Prompt(
                new TextPrompt<string>("Введите [green]имя параметра[/] (или пустую строку для завершения):")
                    .PromptStyle("cyan")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(key))
            {
                break;
            }

            var value = AnsiConsole.Prompt(
                new TextPrompt<string>($"Введите [green]значение[/] для '{key}':")
                    .PromptStyle("cyan")
                    .AllowEmpty());

            parameters[key] = value;

            AnsiConsole.MarkupLine($"[grey]Добавлен параметр: {key}={value}[/]");
        }

        return parameters.Count > 0 ? parameters : null;
    }

    private static (string? Body, string ContentType) GetRequestBodyFromUser()
    {
        var addBody = AnsiConsole.Confirm("Добавить [green]тело запроса[/]?", false);

        if (!addBody)
        {
            return (null, "application/json");
        }

        var contentType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Выберите [green]Content-Type[/]:")
                .HighlightStyle(new Style(Color.Cyan1))
                .AddChoices(
                    "application/json",
                    "application/x-www-form-urlencoded",
                    "text/plain",
                    "application/xml"));

        AnsiConsole.MarkupLine("[grey]Введите тело запроса (для завершения введите пустую строку дважды):[/]");

        var lines = new List<string>();
        var emptyLineCount = 0;

        while (emptyLineCount < 2)
        {
            var line = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrEmpty(line))
            {
                emptyLineCount++;
                if (emptyLineCount < 2)
                {
                    lines.Add(line);
                }
            }
            else
            {
                emptyLineCount = 0;
                lines.Add(line);
            }
        }

        var body = string.Join(Environment.NewLine, lines).TrimEnd();

        if (string.IsNullOrWhiteSpace(body))
        {
            return (null, contentType);
        }

        AnsiConsole.MarkupLine($"[grey]Тело запроса ({body.Length} символов)[/]");

        return (body, contentType);
    }
}
