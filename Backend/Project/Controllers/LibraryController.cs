using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/library")]
[Authorize]
public class LibraryController : ControllerBase
{
    private readonly ILibraryServices _libraryService;
    private readonly ICurrentUserService _currentUser;

    public LibraryController(ILibraryServices libraryService, ICurrentUserService currentUser)
    {
        _libraryService = libraryService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetLibrary()
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("The access token does not contain a valid user identifier.");

        var library = await _libraryService.GetLibraryAsync(userId);
        return Ok(library);
    }
}
