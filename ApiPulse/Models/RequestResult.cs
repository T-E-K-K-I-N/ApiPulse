namespace ApiPulse.Models;

/// <summary>
/// Представляет результат выполнения одного HTTP-запроса во время нагрузочного тестирования.
/// </summary>
public sealed record RequestResult
{
    /// <summary>
    /// Время ответа сервера в миллисекундах.
    /// </summary>
    public required long ResponseTimeMs { get; init; }

    /// <summary>
    /// Указывает, был ли запрос успешным (HTTP статус 2xx).
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// HTTP-код статуса ответа сервера.
    /// </summary>
    public required int StatusCode { get; init; }

    /// <summary>
    /// Сообщение об ошибке, если запрос завершился неудачей.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Временная метка выполнения запроса.
    /// </summary>
    public required DateTime Timestamp { get; init; }
}
