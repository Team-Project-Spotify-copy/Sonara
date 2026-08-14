namespace Application.DTOs.Music;

/// <summary>Спосіб, у який плеєр отримує доступ до медіа.</summary>
public enum TrackStreamMode
{
    /// <summary>Публічне (або вже доступне) посилання на сховище; строку дії немає.</summary>
    DirectUrl = 0,

    /// <summary>Тимчасове підписане посилання (Azure Blob SAS) з обмеженим часом життя.</summary>
    SignedUrl = 1
}

/// <summary>
/// Результат розвʼязання джерела відтворення. Фізичне розташування у сховищі
/// назовні не розкривається: фронтенд отримує лише готове до відтворення посилання.
/// </summary>
public class TrackStreamDto
{
    public Guid TrackId { get; set; }

    /// <summary>Абсолютний URL, який можна передати в HTMLAudioElement.src.</summary>
    public string Url { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/octet-stream";

    public int DurationMs { get; set; }

    public TrackStreamMode Mode { get; set; }

    /// <summary>Момент, коли посилання перестане бути дійсним. null для <see cref="TrackStreamMode.DirectUrl"/>.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>true, якщо джерело підтримує HTTP Range (перемотування в плеєрі).</summary>
    public bool SupportsRangeRequests { get; set; } = true;
}
