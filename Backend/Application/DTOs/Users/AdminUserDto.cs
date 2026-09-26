namespace Application.DTOs.Users;

public record AdminUserDto(
    Guid Id,
    string Email,
    string Username,
    string? AvatarUrl,
    Guid RoleId,
    string RoleName,
    string? SubscriptionPlanName,
    DateTime CreatedAt);

public record RoleDto(Guid Id, string Name);

public record UpdateUserRoleDto(Guid RoleId);