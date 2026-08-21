namespace Application.DTOs.Music;

public enum TrackStreamMode
{
    DirectUrl = 0,

    SignedUrl = 1
}

public class TrackStreamDto
{
    public Guid TrackId { get; set; }

    public string Url { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/octet-stream";

    public int DurationMs { get; set; }

    public TrackStreamMode Mode { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool SupportsRangeRequests { get; set; } = true;
}
