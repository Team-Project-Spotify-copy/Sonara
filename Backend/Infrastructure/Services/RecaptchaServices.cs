using Application.DTOs.Recaptcha;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class RecaptchaServices : IRecaptchaServices
{
    private readonly HttpClient _httpClient;
    private readonly string _secretKey;

    public RecaptchaServices(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _secretKey = configuration["Recaptcha:SecretKey"] ?? "";
    }

    public async Task<bool> VerifyTokenAsync(string token, string expectedAction)
    {
        if (string.IsNullOrEmpty(token)) return false;

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