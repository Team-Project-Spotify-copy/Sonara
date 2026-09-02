using MediatR;
using Application.Interfaces;

namespace Application.Commands.Auth;

public record VerifyResetCodeCommand(string Email, string Code) : IRequest<string>;

public class VerifyResetCodeCommandHandler : IRequestHandler<VerifyResetCodeCommand, string>
{
    private static readonly TimeSpan ResetTokenTtl = TimeSpan.FromMinutes(15);

    private readonly IAuthRepository _authRepository;
    private readonly ICacheService _cache;

    public VerifyResetCodeCommandHandler(IAuthRepository authRepository, ICacheService cache)
    {
        _authRepository = authRepository;
        _cache = cache;
    }

    public async Task<string> Handle(VerifyResetCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _authRepository.GetUserByEmailAsync(request.Email);
        if (user is null)
        {
            throw new UnauthorizedAccessException("Wrong email or code.");
        }

        var codeKey = $"password-reset:code:{user.Id}";
        var storedCode = await _cache.GetAsync<string>(codeKey, cancellationToken);

        if (storedCode is null || storedCode != request.Code)
        {
            throw new UnauthorizedAccessException("Wrong email or code.");
        }

        await _cache.RemoveAsync(codeKey, cancellationToken);

        var resetToken = Guid.NewGuid().ToString("N");
        await _cache.SetAsync($"password-reset:token:{user.Id}", resetToken, ResetTokenTtl, cancellationToken);

        return resetToken;
    }
}