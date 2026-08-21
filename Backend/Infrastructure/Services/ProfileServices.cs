using Application.DTOs.Users;
using Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Application.Enums;
using Domain.Entities.Users;

namespace Infrastructure.Services;

public class ProfileServices : IProfileServices
{

    private readonly SonaraDbContext _context;
    private readonly IBlobService _blobService;
    private readonly IMapper _mapper;

    public ProfileServices(SonaraDbContext context, IBlobService blobService, IMapper mapper)
    {
        _context = context;
        _blobService = blobService;
        _mapper = mapper;
    }

    public async Task<ProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new InvalidOperationException("User not found");

        return _mapper.Map<ProfileDto>(user);
    }

    public async Task<bool> SetAvatarAsync(Guid userId, IFormFile avatarFile)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new InvalidOperationException("User not found");

        string newUrl = await _blobService.ReplaceFileAsync(avatarFile, user.AvatarUrl, BlobFolder.Avatars);

        if(newUrl == null || newUrl == "")
            throw new InvalidOperationException("Failed to upload avatar");

        user.AvatarUrl = newUrl;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto profileDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new InvalidOperationException("User not found");

        var updatedUser = _mapper.Map(profileDto, user);

        if (profileDto.AvatarFile != null)
        {
            string newUrl = await _blobService.ReplaceFileAsync(profileDto.AvatarFile, user.AvatarUrl, BlobFolder.Avatars);

            if (string.IsNullOrEmpty(newUrl))
                throw new InvalidOperationException("Failed to upload avatar");

            user.AvatarUrl = newUrl;
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<ProfileDto>(updatedUser);
    }

    public async Task<string> UpdateAvatarAsync(Guid userId, IFormFile avatarFile)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new InvalidOperationException("User not found");

        string newUrl = await _blobService.ReplaceFileAsync(avatarFile, user.AvatarUrl, BlobFolder.Avatars);

        if (newUrl == null || newUrl == "")
            throw new InvalidOperationException("Failed to upload avatar");   

        return newUrl;
    }

    public async Task<ProfileDto> GetUserByUsernameAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username)
                ?? throw new InvalidOperationException("User not found");

        return _mapper.Map<ProfileDto>(user);
    }
}
