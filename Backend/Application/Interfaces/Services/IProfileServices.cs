using Application.DTOs.Users;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services;

public interface IProfileService
{
    Task<ProfileDto> GetProfileAsync(Guid userId, CancellationToken ct = default);

    Task<ProfileDto> GetUserByUsernameAsync(Guid userId, string username, CancellationToken ct = default);

    Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto profileDto, CancellationToken ct = default);

    Task<bool> SetAvatarAsync(Guid userId, IFormFile avatarFile, CancellationToken ct = default);

    Task<bool> FollowOnUser(Guid userId, string username, CancellationToken ct = default);
    Task<bool> UnFollowOnUser(Guid userId, string username, CancellationToken ct = default);

}
