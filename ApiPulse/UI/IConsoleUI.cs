using ApiPulse.Models;

namespace ApiPulse.UI;

public interface IConsoleUI
{
    LoadTestConfiguration GetConfigurationFromUser(IReadOnlyList<string> urlHistory);
    Task<LoadTestStatistics> DisplayProgressAsync(
        LoadTestConfiguration config,
        Func<IProgress<LoadTestProgress>, CancellationToken, Task<LoadTestStatistics>> runTest,
        CancellationToken cancellationToken);
    void DisplayResults(LoadTestStatistics stats);
    bool AskToSaveResults();
    string? AskForSavePath();
    void DisplayFileSaved(string filename);
    void DisplayError(string message);
    void DisplayCancelled();
    bool ConfirmStart(LoadTestConfiguration config);
}
