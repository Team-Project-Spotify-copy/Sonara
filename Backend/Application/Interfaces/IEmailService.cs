namespace Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
    Task SendSubscriptionSuccessEmailAsync(string toEmail, string username, string planName, DateTime expiresAt, CancellationToken ct = default);
    Task SendPasswordResetEmailAsync(string toEmail, string username, string code, CancellationToken ct = default);
    Task SendSubscriptionExpiredEmailAsync(string toEmail, string username, CancellationToken ct = default);
}