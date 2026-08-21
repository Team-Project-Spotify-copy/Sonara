namespace Application.DTOs.Music;

public class TrackLikeStateDto
{
    public Guid TrackId { get; set; }
    public bool IsLiked { get; set; }
    public long LikesCount { get; set; }

    public DateTime? LikedAt { get; set; }
}

public enum ListenRecordStatus
{
    Recorded = 0,

    TooShort = 1,

    Throttled = 2
}

public class ListenRegistrationDto
{
    public Guid TrackId { get; set; }
    public ListenRecordStatus Status { get; set; }

    public bool Recorded => Status == ListenRecordStatus.Recorded;

    public long PlaysCount { get; set; }

    public DateTime? ListenedAt { get; set; }

    public int RequiredListenedMs { get; set; }
}

public class ListeningHistoryEntryDto
{
    public Guid Id { get; set; }
    public DateTime ListenedAt { get; set; }
    public int? DurationListenedMs { get; set; }
    public TrackDto Track { get; set; } = new();
}
