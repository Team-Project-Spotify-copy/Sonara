using System.ComponentModel.DataAnnotations;
using Application.Validators;

namespace WebApp.Contracts;

public record AddTrackToPlaylistRequest(
    [Required]
    [NotEmptyGuid]
    Guid TrackId);

public record RegisterListenRequest(
    [Range(0, 24 * 60 * 60 * 1000)]
    int DurationListenedMs);

public record TrackBatchRequest(
    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    IReadOnlyList<Guid> Ids);
