using ApiPulse.Models;

namespace ApiPulse.UI;

/// <summary>
/// Интерфейс консольного пользовательского интерфейса для взаимодействия с пользователем.
/// </summary>
public interface IConsoleUI
{
    /// <summary>
    /// Получает конфигурацию теста от пользователя через интерактивные диалоги.
    /// </summary>
    /// <param name="urlHistory">История ранее использованных URL-адресов.</param>
    /// <returns>Заполненная конфигурация нагрузочного теста.</returns>
    LoadTestConfiguration GetConfigurationFromUser(IReadOnlyList<string> urlHistory);

    /// <summary>
    /// Отображает прогресс выполнения теста и запускает его.
    /// </summary>
    /// <param name="config">Конфигурация теста.</param>
    /// <param name="runTest">Функция для выполнения теста.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат нагрузочного тестирования.</returns>
    Task<LoadTestResult> DisplayProgressAsync(
        LoadTestConfiguration config,
        Func<IProgress<LoadTestProgress>, CancellationToken, Task<LoadTestResult>> runTest,
        CancellationToken cancellationToken);

    /// <summary>
    /// Отображает результаты тестирования в виде таблицы.
    /// </summary>
    /// <param name="stats">Статистика тестирования для отображения.</param>
    void DisplayResults(LoadTestStatistics stats);

    /// <summary>
    /// Спрашивает пользователя о необходимости сохранения результатов.
    /// </summary>
    /// <returns>true, если пользователь хочет сохранить результаты.</returns>
    bool AskToSaveResults();

    /// <summary>
    /// Запрашивает у пользователя путь для сохранения файла.
    /// </summary>
    /// <returns>Путь для сохранения или null для использования пути по умолчанию.</returns>
    string? AskForSavePath();

    /// <summary>
    /// Отображает сообщение об успешном сохранении файла результатов.
    /// </summary>
    /// <param name="filename">Путь к сохранённому файлу.</param>
    void DisplayFileSaved(string filename);

    /// <summary>
    /// Отображает сообщение об успешном сохранении графиков.
    /// </summary>
    /// <param name="filenames">Список путей к сохранённым файлам.</param>
    void DisplayChartsSaved(IReadOnlyList<string> filenames);

    /// <summary>
    /// Отображает сообщение об ошибке.
    /// </summary>
    /// <param name="message">Текст ошибки.</param>
    void DisplayError(string message);

    /// <summary>
    /// Отображает сообщение об отмене теста.
    /// </summary>
    void DisplayCancelled();

    /// <summary>
    /// Запрашивает подтверждение запуска теста у пользователя.
    /// </summary>
    /// <param name="config">Конфигурация теста для отображения.</param>
    /// <returns>true, если пользователь подтвердил запуск.</returns>
    bool ConfirmStart(LoadTestConfiguration config);
}
