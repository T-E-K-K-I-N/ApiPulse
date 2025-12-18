using ApiPulse.Models;
using ScottPlot;
using Spectre.Console;
using SpectreColor = Spectre.Console.Color;

namespace ApiPulse.Services;

/// <summary>
/// Сервис для отображения графиков в консоли и экспорта в PNG-файлы.
/// Использует Spectre.Console для консольных графиков и ScottPlot для PNG.
/// </summary>
public sealed class ChartService : IChartService
{
    /// <inheritdoc />
    public void DisplayConsoleCharts(ChartData chartData, LoadTestStatistics stats)
    {
        AnsiConsole.WriteLine();

        DisplayResponseTimeDistributionChart(chartData);
        DisplayStatusCodeDistributionChart(chartData);
        DisplayResponseTimeOverTimeChart(chartData);
        DisplayRequestsPerSecondChart(chartData);
    }

    /// <inheritdoc />
    public async Task<List<string>> ExportChartsToPngAsync(ChartData chartData, LoadTestStatistics stats, string outputDirectory)
    {
        var savedFiles = new List<string>();
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        Directory.CreateDirectory(outputDirectory);

        await Task.Run(() =>
        {
            var responseTimePath = Path.Combine(outputDirectory, $"ApiPulse_ResponseTime_{timestamp}.png");
            CreateResponseTimeChart(chartData, stats, responseTimePath);
            savedFiles.Add(responseTimePath);

            var rpsPath = Path.Combine(outputDirectory, $"ApiPulse_RequestsPerSecond_{timestamp}.png");
            CreateRequestsPerSecondChart(chartData, stats, rpsPath);
            savedFiles.Add(rpsPath);

            var distributionPath = Path.Combine(outputDirectory, $"ApiPulse_ResponseTimeDistribution_{timestamp}.png");
            CreateResponseTimeDistributionChart(chartData, stats, distributionPath);
            savedFiles.Add(distributionPath);

            if (chartData.StatusCodeDistribution.Count > 0)
            {
                var statusCodePath = Path.Combine(outputDirectory, $"ApiPulse_StatusCodes_{timestamp}.png");
                CreateStatusCodeChart(chartData, stats, statusCodePath);
                savedFiles.Add(statusCodePath);
            }
        });

        return savedFiles;
    }

    /// <summary>
    /// Отображает гистограмму распределения времени ответа в консоли.
    /// </summary>
    /// <param name="chartData">Данные графика.</param>
    private static void DisplayResponseTimeDistributionChart(ChartData chartData)
    {
        AnsiConsole.Write(new Rule("[cyan]Распределение времени отклика[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();

        var barChart = new BarChart()
            .Width(60)
            .Label("[green bold]Количество запросов по времени отклика[/]")
            .CenterLabel();

        var colors = new[] { SpectreColor.Green, SpectreColor.Lime, SpectreColor.Yellow, SpectreColor.Orange1, SpectreColor.Red };
        var index = 0;

        foreach (var kvp in chartData.ResponseTimeDistribution)
        {
            if (kvp.Value > 0)
            {
                barChart.AddItem(kvp.Key, kvp.Value, colors[index % colors.Length]);
            }
            index++;
        }

        AnsiConsole.Write(barChart);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Отображает гистограмму распределения HTTP статус-кодов в консоли.
    /// </summary>
    /// <param name="chartData">Данные графика.</param>
    private static void DisplayStatusCodeDistributionChart(ChartData chartData)
    {
        if (chartData.StatusCodeDistribution.Count == 0)
            return;

        AnsiConsole.Write(new Rule("[cyan]Распределение HTTP статус кодов[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();

        var barChart = new BarChart()
            .Width(60)
            .Label("[green bold]Количество запросов по статус коду[/]")
            .CenterLabel();

        foreach (var kvp in chartData.StatusCodeDistribution.OrderBy(x => x.Key))
        {
            var color = kvp.Key switch
            {
                >= 200 and < 300 => SpectreColor.Green,
                >= 300 and < 400 => SpectreColor.Yellow,
                >= 400 and < 500 => SpectreColor.Orange1,
                >= 500 => SpectreColor.Red,
                _ => SpectreColor.Grey
            };

            barChart.AddItem($"{kvp.Key}", kvp.Value, color);
        }

        AnsiConsole.Write(barChart);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Отображает график изменения времени ответа во времени в консоли.
    /// </summary>
    /// <param name="chartData">Данные графика.</param>
    private static void DisplayResponseTimeOverTimeChart(ChartData chartData)
    {
        if (chartData.ResponseTimeSeries.Count == 0)
            return;

        AnsiConsole.Write(new Rule("[cyan]Время отклика во времени (сред. мс)[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();

        var maxValue = chartData.ResponseTimeSeries.Max(x => x.Value);
        var step = Math.Max(1, chartData.ResponseTimeSeries.Count / 20);
        var sampledData = chartData.ResponseTimeSeries
            .Where((_, i) => i % step == 0)
            .Take(20)
            .ToList();

        var barChart = new BarChart()
            .Width(60)
            .Label("[green bold]Среднее время отклика (мс) по секундам теста[/]")
            .CenterLabel();

        foreach (var point in sampledData)
        {
            var color = point.Value switch
            {
                < 100 => SpectreColor.Green,
                < 300 => SpectreColor.Yellow,
                < 500 => SpectreColor.Orange1,
                _ => SpectreColor.Red
            };

            barChart.AddItem($"{point.SecondOffset}с", point.Value, color);
        }

        AnsiConsole.Write(barChart);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Отображает график количества запросов в секунду во времени в консоли.
    /// </summary>
    /// <param name="chartData">Данные графика.</param>
    private static void DisplayRequestsPerSecondChart(ChartData chartData)
    {
        if (chartData.RequestsPerSecondSeries.Count == 0)
            return;

        AnsiConsole.Write(new Rule("[cyan]Запросы в секунду во времени[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();

        var step = Math.Max(1, chartData.RequestsPerSecondSeries.Count / 20);
        var sampledData = chartData.RequestsPerSecondSeries
            .Where((_, i) => i % step == 0)
            .Take(20)
            .ToList();

        var barChart = new BarChart()
            .Width(60)
            .Label("[green bold]Количество запросов в секунду[/]")
            .CenterLabel();

        foreach (var point in sampledData)
        {
            barChart.AddItem($"{point.SecondOffset}с", point.Value, SpectreColor.Cyan1);
        }

        AnsiConsole.Write(barChart);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Создаёт PNG-график времени ответа во времени.
    /// </summary>
    /// <param name="chartData">Данные для графика.</param>
    /// <param name="stats">Статистика для заголовка.</param>
    /// <param name="filePath">Путь для сохранения файла.</param>
    private static void CreateResponseTimeChart(ChartData chartData, LoadTestStatistics stats, string filePath)
    {
        var plot = new Plot();

        if (chartData.ResponseTimeSeries.Count > 0)
        {
            var xValues = chartData.ResponseTimeSeries.Select(p => (double)p.SecondOffset).ToArray();
            var yValues = chartData.ResponseTimeSeries.Select(p => p.Value).ToArray();

            var scatter = plot.Add.Scatter(xValues, yValues);
            scatter.Color = Colors.Navy;
            scatter.LineWidth = 2;
            scatter.MarkerSize = 5;
            scatter.LegendText = "Среднее время отклика";
        }

        plot.Title($"Время отклика - {stats.Domain}");
        plot.XLabel("Время (секунды)");
        plot.YLabel("Время отклика (мс)");
        plot.ShowLegend();

        plot.SavePng(filePath, 800, 500);
    }

    /// <summary>
    /// Создаёт PNG-график количества запросов в секунду.
    /// </summary>
    /// <param name="chartData">Данные для графика.</param>
    /// <param name="stats">Статистика для заголовка.</param>
    /// <param name="filePath">Путь для сохранения файла.</param>
    private static void CreateRequestsPerSecondChart(ChartData chartData, LoadTestStatistics stats, string filePath)
    {
        var plot = new Plot();

        if (chartData.RequestsPerSecondSeries.Count > 0)
        {
            var xValues = chartData.RequestsPerSecondSeries.Select(p => (double)p.SecondOffset).ToArray();
            var yValues = chartData.RequestsPerSecondSeries.Select(p => p.Value).ToArray();

            var scatter = plot.Add.Scatter(xValues, yValues);
            scatter.Color = Colors.Green;
            scatter.LineWidth = 2;
            scatter.MarkerSize = 5;
            scatter.LegendText = "Запросы/сек";
        }

        plot.Title($"Пропускная способность - {stats.Domain}");
        plot.XLabel("Время (секунды)");
        plot.YLabel("Запросы в секунду");
        plot.ShowLegend();

        plot.SavePng(filePath, 800, 500);
    }

    /// <summary>
    /// Создаёт PNG-гистограмму распределения времени ответа.
    /// </summary>
    /// <param name="chartData">Данные для графика.</param>
    /// <param name="stats">Статистика для заголовка.</param>
    /// <param name="filePath">Путь для сохранения файла.</param>
    private static void CreateResponseTimeDistributionChart(ChartData chartData, LoadTestStatistics stats, string filePath)
    {
        var plot = new Plot();

        var labels = chartData.ResponseTimeDistribution.Keys.ToArray();
        var values = chartData.ResponseTimeDistribution.Values.Select(v => (double)v).ToArray();

        var bars = plot.Add.Bars(values);

        var barColors = new[] { Colors.Green, Colors.Lime, Colors.Yellow, Colors.Orange, Colors.Red };
        for (var i = 0; i < bars.Bars.Count && i < barColors.Length; i++)
        {
            bars.Bars[i].FillColor = barColors[i];
            bars.Bars[i].Label = labels[i];
        }

        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
            labels.Select((label, index) => new ScottPlot.Tick(index, label)).ToArray()
        );

        plot.Axes.Bottom.TickLabelStyle.Rotation = 15;
        plot.Axes.Margins(bottom: 0);

        plot.Title($"Распределение времени отклика - {stats.Domain}");
        plot.YLabel("Количество запросов");

        plot.SavePng(filePath, 800, 500);
    }

    /// <summary>
    /// Создаёт PNG-гистограмму распределения HTTP статус-кодов.
    /// </summary>
    /// <param name="chartData">Данные для графика.</param>
    /// <param name="stats">Статистика для заголовка.</param>
    /// <param name="filePath">Путь для сохранения файла.</param>
    private static void CreateStatusCodeChart(ChartData chartData, LoadTestStatistics stats, string filePath)
    {
        var plot = new Plot();

        var orderedData = chartData.StatusCodeDistribution.OrderBy(x => x.Key).ToList();
        var labels = orderedData.Select(x => x.Key.ToString()).ToArray();
        var values = orderedData.Select(x => (double)x.Value).ToArray();

        var bars = plot.Add.Bars(values);

        for (var i = 0; i < bars.Bars.Count && i < orderedData.Count; i++)
        {
            var statusCode = orderedData[i].Key;
            bars.Bars[i].FillColor = statusCode switch
            {
                >= 200 and < 300 => Colors.Green,
                >= 300 and < 400 => Colors.Yellow,
                >= 400 and < 500 => Colors.Orange,
                >= 500 => Colors.Red,
                _ => Colors.Gray
            };
            bars.Bars[i].Label = labels[i];
        }

        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
            labels.Select((label, index) => new ScottPlot.Tick(index, label)).ToArray()
        );

        plot.Axes.Margins(bottom: 0);

        plot.Title($"Распределение HTTP статус кодов - {stats.Domain}");
        plot.YLabel("Количество запросов");

        plot.SavePng(filePath, 800, 500);
    }
}
