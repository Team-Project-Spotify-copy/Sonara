namespace Application.DTOs.Music;

public class SearchSectionDto<T>
{
    public List<T> Items { get; set; } = new();

    public int Total { get; set; }
}

public class SearchResponseDto
{
    public string Query { get; set; } = string.Empty;

    public int Limit { get; set; }

    public SearchSectionDto<TrackDto> Tracks { get; set; } = new();
    public SearchSectionDto<ArtistSummaryDto> Artists { get; set; } = new();
    public SearchSectionDto<AlbumSummaryDto> Albums { get; set; } = new();
    public SearchSectionDto<PlaylistSummaryDto> Playlists { get; set; } = new();

    public int TotalResults => Tracks.Total + Artists.Total + Albums.Total + Playlists.Total;
}
