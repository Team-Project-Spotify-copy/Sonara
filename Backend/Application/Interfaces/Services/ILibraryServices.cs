using Application.DTOs.Library;

namespace Application.Interfaces.Services;

public interface ILibraryServices
{
    Task<List<LibraryItemDto>> GetLibraryAsync(Guid userId);
}
