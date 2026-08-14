using System.ComponentModel.DataAnnotations;
using Application.Validators;

namespace WebApp.Contracts;

// Для record-типів MVC вимагає, щоб атрибути валідації стояли на ПАРАМЕТРІ первинного
// конструктора, а не на властивості: інакше ModelMetadata кидає InvalidOperationException
// ("validation metadata ... will be ignored"), і запит падає з 500 замість 400.
// Тому тут навмисно немає префікса [property: ...].

public record AddTrackToPlaylistRequest(
    [Required]
    [NotEmptyGuid]
    Guid TrackId);

/// <summary>
/// Фіксація завершеного прослуховування. Викликається один раз наприкінці відтворення
/// (або коли користувач перемикає трек), а НЕ на кожен тік прогресу.
/// </summary>
public record RegisterListenRequest(
    [Range(0, 24 * 60 * 60 * 1000)]
    int DurationListenedMs);

/// <summary>Пакетне читання треків за id - використовується для відновлення черги плеєра.</summary>
public record TrackBatchRequest(
    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    IReadOnlyList<Guid> Ids);
