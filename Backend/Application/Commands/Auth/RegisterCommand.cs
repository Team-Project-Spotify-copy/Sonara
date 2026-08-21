using MediatR;
using Domain.Entities.Users;
using Application.Interfaces;

namespace Application.Commands.Auth;

public record RegisterCommand(string Email, string Username, string Password, string token) : IRequest<Guid>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Guid>
{
    private readonly IAuthRepository _authRepository;

    public RegisterCommandHandler(IAuthRepository authRepository) => _authRepository = authRepository;

    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _authRepository.EmailExistsAsync(request.Email))
            throw new InvalidOperationException("Email is already registered.");

        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = await _authRepository.GetDefaultRoleIdAsync(),
            ActiveSubscriptionId = null
        };

        var freeSub = await _authRepository.CreateDefaultSubscriptionForUserAsync(user.Id);

        user.ActiveSubscriptionId = freeSub.Id;

        await _authRepository.AddUserAsync(user);
        await _authRepository.SaveChangesAsync();

        return user.Id;
    }
}
