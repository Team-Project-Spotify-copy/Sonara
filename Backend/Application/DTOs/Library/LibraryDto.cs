namespace Application.DTOs.Library;

using Application.Enums;    

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
public class LibraryDto
{
    public List<LibraryItemDto> Items { get; set; } = new List<LibraryItemDto>();
}

public class LibraryCreateDto
{
    public Guid Id { get; set; }
}
