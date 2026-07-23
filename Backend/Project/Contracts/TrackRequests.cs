namespace WebApp.Contracts;

public record AddTrackToPlaylistRequest(Guid TrackId);

public record RegisterListenRequest(int? DurationListenedMs);