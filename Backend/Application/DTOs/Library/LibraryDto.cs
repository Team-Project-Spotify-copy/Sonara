namespace Application.DTOs.Library;

public class LibraryItemDto
{
    public Guid Id { get; set; }
    public string? RouteKey { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? CoverUrl { get; set; }
    public string? AudioUrl { get; set; }
    public string? Kind { get; set; }
}

public class LibraryCreateDto
{
    public Guid Id { get; set; }
}
