namespace Application.Interfaces.Services;


/// TODO: зараз реал≥зац≥€ (в WebApp) читаЇ Guid ≥з заголовка X-User-Id Ч тимчасово, без JWT. 
/// ѕеределать потом, коли буде реал≥зована авторизац≥€ через JWT.

public interface ICurrentUserService
{
    Guid? UserId { get; }
}