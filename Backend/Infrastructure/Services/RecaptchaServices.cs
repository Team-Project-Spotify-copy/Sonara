using Application.DTOs.Recaptcha;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class RecaptchaServices : IRecaptchaServices
{
    internal const string DevBypassToken = "dev-dummy-token";

    private static readonly string[] PlaceholderSecrets =
    [
        "your-secret-key",
        "your_secret_key",
        "changeme",
        "todo",
        "secret"
    ];

    private readonly HttpClient _httpClient;
    private readonly string _secretKey;
    private readonly bool _isDevelopment;
    private readonly ILogger<RecaptchaServices> _logger;

    public RecaptchaServices(
        HttpClient httpClient,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<RecaptchaServices> logger)
    {
        _httpClient = httpClient;
        _secretKey = configuration["Recaptcha:SecretKey"] ?? "";
        _isDevelopment = environment.IsDevelopment();
        _logger = logger;
    }

    private bool IsSecretMissing =>
        string.IsNullOrWhiteSpace(_secretKey)
        || PlaceholderSecrets.Contains(_secretKey.Trim().ToLowerInvariant());

    private bool ShouldBypass(string token) =>
        _isDevelopment && (token == DevBypassToken || IsSecretMissing);

    public async Task<bool> VerifyTokenAsync(string token, string expectedAction)
    {
        if (string.IsNullOrEmpty(token)) return false;

        if (ShouldBypass(token))
        {
            _logger.LogWarning(
                "reCAPTCHA verification bypassed for action {Action}: Development environment " +
                "with {Reason}. This never happens outside Development.",
                expectedAction,
                token == DevBypassToken ? "the dev bypass token" : "no secret key configured");

            return true;
        }

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("secret", _secretKey),
            new KeyValuePair<string, string>("response", token)
        });

        var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

        if (!response.IsSuccessStatusCode) return false;

        var jsonString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RecaptchaResponse>(jsonString);

        return result != null &&
               result.Success &&
               result.Action == expectedAction &&
               result.Score >= 0.5f;
    }
}
