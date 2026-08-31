using MediatR;
using Application.Interfaces;
using Application.Exceptions;

namespace Application.Commands.Auth;

public record ResetPasswordCommand(string Email, string ResetToken, string NewPassword, string ConfirmPassword) : IRequest<bool>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IAuthRepository _authRepository;
    private readonly ICacheService _cache;

    public ResetPasswordCommandHandler(IAuthRepository authRepository, ICacheService cache)
    {
        _authRepository = authRepository;
        _cache = cache;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new ValidationException("confirmPassword", "Passwords do not match.");
        }

        if (request.NewPassword.Length < 8)
        {
            throw new ValidationException("newPassword", "Password must be at least 8 characters long.");
        }

        var user = await _authRepository.GetUserByEmailAsync(request.Email);
        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid password reset request.");
        }

        var tokenKey = $"password-reset:token:{user.Id}";
        var storedToken = await _cache.GetAsync<string>(tokenKey, cancellationToken);

        if (storedToken is null || storedToken != request.ResetToken)
        {
            throw new UnauthorizedAccessException("Invalid or expired password reset token.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        await _authRepository.RevokeAllRefreshTokensAsync(user.Id);
        await _authRepository.SaveChangesAsync();

        await _cache.RemoveAsync(tokenKey, cancellationToken);

        return true;
    }
}