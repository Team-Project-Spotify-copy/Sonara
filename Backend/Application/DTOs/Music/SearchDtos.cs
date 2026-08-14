namespace Application.DTOs.Music;

/// <summary>Одна секція результатів пошуку: сторінка елементів + загальна кількість збігів.</summary>
public class SearchSectionDto<T>
{
    public List<T> Items { get; set; } = new();

    /// <summary>Загальна кількість збігів у цій секції (може бути більшою за Items.Count).</summary>
    public int Total { get; set; }
}

/// <summary>Нормалізована відповідь пошуку по каталогу.</summary>
public class SearchResponseDto
{
    /// <summary>Нормалізований (обрізаний) запит, який фактично виконувався.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Ліміт елементів на секцію, який був застосований.</summary>
    public int Limit { get; set; }

    public SearchSectionDto<TrackDto> Tracks { get; set; } = new();
    public SearchSectionDto<ArtistSummaryDto> Artists { get; set; } = new();
    public SearchSectionDto<AlbumSummaryDto> Albums { get; set; } = new();
    public SearchSectionDto<PlaylistSummaryDto> Playlists { get; set; } = new();

    /// <summary>Сума Total усіх секцій.</summary>
    public int TotalResults => Tracks.Total + Artists.Total + Albums.Total + Playlists.Total;
}
