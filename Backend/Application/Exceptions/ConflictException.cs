namespace Application.Exceptions;

/// <summary>
/// Стан ресурсу не дозволяє виконати операцію (дублювання звʼязку, недоступне медіа тощо).
/// Мапиться на HTTP 409.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message = "The request conflicts with the current state of the resource.")
        : base(message)
    {
    }
}

/// <summary>
/// Трек існує, але відтворити його неможливо: медіа не завантажене або втрачене.
/// Мапиться на HTTP 409 і відрізняється від 404 (трек не знайдено).
/// </summary>
public class MediaUnavailableException : ConflictException
{
    public Guid TrackId { get; }

    public MediaUnavailableException(Guid trackId)
        : base($"Track ({trackId}) has no playable media.")
    {
        TrackId = trackId;
    }
}

/// <summary>
/// Зовнішнє сховище медіа недоступне або повернуло помилку. Мапиться на HTTP 502.
/// Технічні деталі назовні не віддаються — лише логуються.
/// </summary>
public class StorageUnavailableException : Exception
{
    public StorageUnavailableException(string message = "Media storage is temporarily unavailable.", Exception? inner = null)
        : base(message, inner)
    {
    }
}
