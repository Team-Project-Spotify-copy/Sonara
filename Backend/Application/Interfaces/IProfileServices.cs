using Application.DTOs.Users;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IProfileServices
{
    Task<ProfileDto> GetProfileAsync(Guid userId);

    Task<ProfileDto> GetUserByUsernameAsync(string username);

    Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto profileDto);

    Task<bool> SetAvatarAsync(Guid userId, IFormFile avatarFile);

}
