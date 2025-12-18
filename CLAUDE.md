# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run Commands

```bash
# Build the project
dotnet build ApiPulse/ApiPulse.csproj

# Run in interactive mode (prompts for URL, threads, duration)
dotnet run --project ApiPulse/ApiPulse.csproj

# Run in CLI mode
dotnet run --project ApiPulse/ApiPulse.csproj -- <url> <threads> <duration> [options]

# Example CLI usage with options
dotnet run --project ApiPulse/ApiPulse.csproj -- https://api.example.com 10 30 --method=POST --body="{\"name\":\"test\"}" --content-type=application/json --query="key1=value1&key2=value2"
```

## Project Overview

ApiPulse is a .NET 8 console application for load testing REST APIs. It supports both interactive mode (Spectre.Console UI) and CLI mode with command-line arguments.

### Key Technologies
- **Spectre.Console** - Rich terminal UI (FigletText banners, tables, progress bars, prompts)
- **Polly** - Resilience/retry policies for HTTP requests (exponential backoff on transient errors)
- **Microsoft.Extensions.DependencyInjection** - Service registration via `ServiceCollectionExtensions.AddApiPulseServices()`

## Architecture

### Entry Point
`Program.cs` - Top-level statements pattern. Handles both CLI argument parsing and interactive mode routing.

### Core Services (registered in `Extensions/ServiceCollectionExtensions.cs`)
- **ILoadTestService** / `LoadTestService` - Orchestrates load test execution with parallel worker tasks
- **IStatisticsCollector** / `StatisticsCollector` - Thread-safe statistics aggregation using `ConcurrentBag<RequestResult>` and `Interlocked` operations
- **IResultExporter** / `ResultExporter` - Exports test results to text files
- **IUrlHistoryService** / `UrlHistoryService` - Persists recent URLs to `%LOCALAPPDATA%/ApiPulse/url_history.json`

### UI Layer
- **IConsoleUI** / `SpectreConsoleUI` - All Spectre.Console interactions (prompts, progress display, results tables)

### Models
- `LoadTestConfiguration` - Test parameters (URL, threads, duration, HTTP method, body, query params)
- `LoadTestStatistics` - Aggregated results (response times, percentiles, success rates)
- `RequestResult` - Individual request outcome
- `LoadTestProgress` - Real-time progress data for UI updates

### HTTP Configuration
HttpClient named "LoadTest" configured with:
- 30-second timeout
- Polly retry policy (3 retries with exponential backoff for transient errors and 5xx responses)

## Notes

- UI text and output are in Russian
- URL history persists between sessions (max 5 URLs)
- Thread count: 1-1000, Duration: 1-3600 seconds
- Results auto-save in CLI mode, optional save prompt in interactive mode
