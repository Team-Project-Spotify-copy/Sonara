using MediatR;
using Application.Interfaces;

namespace Application.Commands.Auth;

public record ForgotPasswordCommand(string Email) : IRequest<bool>;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private static readonly TimeSpan CodeTtl = TimeSpan.FromMinutes(10);

    private readonly IAuthRepository _authRepository;
    private readonly ICacheService _cache;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IAuthRepository authRepository, ICacheService cache, IEmailService emailService)
    {
        _authRepository = authRepository;
        _cache = cache;
        _emailService = emailService;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _authRepository.GetUserByEmailAsync(request.Email);

        if (user is null)
        {
            return true;
        }

        var code = System.Security.Cryptography.RandomNumberGenerator.GetInt32(1000, 10000).ToString();
        await _cache.SetAsync($"password-reset:code:{user.Id}", code, CodeTtl, cancellationToken);

        await _emailService.SendEmailAsync(
            user.Email,
            "Password Reset Code for Sonara",
            $"""
            <p>Hello, {user.Username}!</p>
            <p>Your password reset code: <strong>{code}</strong></p>
            <p>The code is valid for 10 minutes. If this wasn't you, please ignore this email.</p>
            """,
            cancellationToken);

        return true;
    }
}