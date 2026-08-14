using Application.DTOs.Music;

namespace Application.Interfaces.Services;

/// <summary>
/// Розвʼязання джерела відтворення треку. Єдине місце, яке знає, де фізично лежить медіа.
/// </summary>
public interface ITrackStreamService
{
    /// <summary>
    /// Повертає готове до відтворення посилання.
    /// Кидає <see cref="Exceptions.NotFoundException"/>, якщо треку немає,
    /// <see cref="Exceptions.MediaUnavailableException"/>, якщо медіа не привʼязане,
    /// <see cref="Exceptions.StorageUnavailableException"/>, якщо сховище недоступне.
    /// </summary>
    Task<TrackStreamDto> ResolveAsync(Guid trackId, CancellationToken ct = default);
}
