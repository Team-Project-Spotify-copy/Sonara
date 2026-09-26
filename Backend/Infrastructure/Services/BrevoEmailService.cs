using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class BrevoEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public BrevoEmailService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        var apiKey = _configuration["Brevo__ApiKey"] ?? _configuration["Brevo:ApiKey"];
        var senderEmail = _configuration["Brevo__SenderEmail"] ?? _configuration["Brevo:SenderEmail"] ?? "sonara@app.com";
        var senderName = _configuration["Brevo__SenderName"] ?? _configuration["Brevo:SenderName"] ?? "Sonara";

        var emailData = new
        {
            sender = new { email = senderEmail, name = senderName },
            to = new[] { new { email = toEmail } },
            subject = subject,
            htmlContent = htmlBody
        };

        var jsonContent = JsonSerializer.Serialize(emailData);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("api-key", apiKey);
        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"Failed to send email via Brevo API: {errorContent}");
        }
    }

    public Task SendSubscriptionSuccessEmailAsync(string toEmail, string username, string planName, DateTime expiresAt, CancellationToken ct = default)
    {
        string subject = "Дякуємо за покупку підписки на Sonara!";

        string htmlBody = $"""
            <div style="font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                <h2 style="color: #1DB954; text-align: center;">Вітаємо, {username}! 🎉</h2>
                <p style="font-size: 16px;">Ваша підписка <strong>{planName}</strong> успішно активована через блокчейн!</p>
                <p style="font-size: 16px;">Тепер вам доступні всі переваги преміум-акаунта без жодних обмежень.</p>
                <div style="background-color: #f9f9f9; padding: 15px; border-radius: 6px; margin: 20px 0;">
                    <p style="margin: 5px 0;"><strong>План:</strong> {planName}</p>
                    <p style="margin: 5px 0;"><strong>Термін дії до:</strong> {expiresAt:dd.MM.yyyy HH:mm} UTC</p>
                </div>
                <p style="font-size: 14px; color: #666;">Дякуємо, що ви з нами. Насолоджуйтесь музикою разом із Sonara!</p>
                <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;">
                <p style="font-size: 12px; color: #999; text-align: center;">Це автоматичне повідомлення, будь ласка, не відповідайте на нього.</p>
            </div>
         """;

        return SendEmailAsync(toEmail, subject, htmlBody, ct);
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string username, string code, CancellationToken ct = default)
    {
        string subject = "Відновлення пароля для Sonara";

        string htmlBody = $"""
            <div style="font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                <h2 style="color: #1DB954; text-align: center;">Відновлення пароля 🔒</h2>
                <p style="font-size: 16px;">Привіт, <strong>{username}</strong>!</p>
                <p style="font-size: 16px;">Ми отримали запит на скидання пароля для вашого акаунта в Sonara. Ваш код підтвердження:</p>
            
                <div style="background-color: #f9f9f9; padding: 20px; border-radius: 6px; margin: 20px 0; text-align: center;">
                    <span style="font-size: 28px; font-weight: bold; letter-spacing: 4px; color: #1DB954;">{code}</span>
                </div>
            
                <p style="font-size: 14px; color: #666;">Цей код діє протягом <strong>10 хвилин</strong>.</p>
                <p style="font-size: 14px; color: #666;">Якщо ви не запитували зміну пароля, просто проігноруйте цей лист.</p>
            
                <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;">
                <p style="font-size: 12px; color: #999; text-align: center;">Це автоматичне повідомлення, будь ласка, не відповідайте на нього.</p>
            </div>
        """;

        return SendEmailAsync(toEmail, subject, htmlBody, ct);
    }

    public Task SendSubscriptionExpiredEmailAsync(string toEmail, string username, CancellationToken ct = default)
    {
        string subject = "Ваша підписка на Sonara завершилася";

        string htmlBody = $"""
            <div style="font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                <h2 style="color: #E53935; text-align: center;">Підписку завершено ⏳</h2>
                <p style="font-size: 16px;">Привіт, <strong>{username}</strong>!</p>
                <p style="font-size: 16px;">Термін дії вашої преміум-підписки на Sonara закінчився, і ваш акаунт було переведено на тарифний план <strong>Free</strong>.</p>
            
                <div style="background-color: #f9f9f9; padding: 15px; border-radius: 6px; margin: 20px 0;">
                    <p style="margin: 5px 0;"><strong>Що змінилося?</strong> Вам більше недоступні ексклюзивні преміум-можливості, але ви й надалі можете слухати музику з базовими умовами.</p>
                </div>
            
                <p style="font-size: 16px;">Бажаєте повернути преміум? Ви будь-коли можете поновити підписку через блокчейн у своєму профілі.</p>
            
                <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;">
                <p style="font-size: 12px; color: #999; text-align: center;">Це автоматичне повідомлення, будь ласка, не відповідайте на нього.</p>
            </div>
        """;

        return SendEmailAsync(toEmail, subject, htmlBody, ct);
    }
}