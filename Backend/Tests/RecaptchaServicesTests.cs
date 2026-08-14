using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Sonara.Tests.Infrastructure;
using Xunit;

namespace Sonara.Tests;

public class RecaptchaServicesTests
{
    private const string Action = "login_submit";

    private sealed class ExplodingHandler : HttpMessageHandler
    {
        public bool WasCalled { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            WasCalled = true;
            throw new InvalidOperationException(
                $"reCAPTCHA verification unexpectedly called out to {request.RequestUri}.");
        }
    }

    private sealed class StubEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Sonara.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private static (IRecaptchaServices Service, ExplodingHandler Handler) Create(
        string environmentName,
        string? secretKey)
    {
        var handler = new ExplodingHandler();
        var client = new HttpClient(handler);
        var config = TestConfiguration.Create(("Recaptcha:SecretKey", secretKey!));
        var env = new StubEnvironment { EnvironmentName = environmentName };

        return (new RecaptchaServices(client, config, env, NullLogger<RecaptchaServices>.Instance), handler);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("your-secret-key")]
    [InlineData("CHANGEME")]
    public async Task Development_without_a_real_secret_bypasses_verification(string secret)
    {
        var (service, handler) = Create(Environments.Development, secret);

        Assert.True(await service.VerifyTokenAsync("any-token-at-all", Action));
        Assert.False(handler.WasCalled);
    }

    [Fact]
    public async Task Development_bypasses_the_dev_token_even_when_a_secret_is_configured()
    {
        var (service, handler) = Create(Environments.Development, "a-real-looking-secret");

        Assert.True(await service.VerifyTokenAsync(RecaptchaServices.DevBypassToken, Action));
        Assert.False(handler.WasCalled);
    }

    [Fact]
    public async Task Development_still_verifies_a_real_token_when_a_secret_is_configured()
    {
        var (service, handler) = Create(Environments.Development, "a-real-looking-secret");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.VerifyTokenAsync("a-real-token", Action));
        Assert.True(handler.WasCalled);
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public async Task Outside_development_the_dev_token_is_never_trusted(string environmentName)
    {
        var (service, handler) = Create(environmentName, "a-real-looking-secret");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.VerifyTokenAsync(RecaptchaServices.DevBypassToken, Action));
        Assert.True(handler.WasCalled);
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public async Task Outside_development_a_missing_secret_does_not_disable_verification(string environmentName)
    {
        var (service, handler) = Create(environmentName, "");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.VerifyTokenAsync("any-token-at-all", Action));
        Assert.True(handler.WasCalled);
    }

    [Fact]
    public async Task An_empty_token_is_rejected_without_any_network_call()
    {
        var (service, handler) = Create(Environments.Development, "");

        Assert.False(await service.VerifyTokenAsync("", Action));
        Assert.False(handler.WasCalled);
    }
}
