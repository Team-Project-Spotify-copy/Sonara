using System.Text.Json;
using Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using WebApp;
using Xunit;
using ValidationException = Application.Exceptions.ValidationException;

namespace Sonara.Tests;

public class ExceptionHandlingMiddlewareTests
{
    private static async Task<(int StatusCode, JsonElement Body)> RunAsync(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exception,
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var payload = await new StreamReader(context.Response.Body).ReadToEndAsync();

        return (context.Response.StatusCode, JsonDocument.Parse(payload).RootElement);
    }

    [Fact]
    public async Task NotFound_maps_to_404_with_a_stable_code()
    {
        var (status, body) = await RunAsync(new NotFoundException("Track", Guid.Empty));

        Assert.Equal(StatusCodes.Status404NotFound, status);
        Assert.Equal("not_found", body.GetProperty("code").GetString());
        Assert.Equal(404, body.GetProperty("statusCode").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task Forbidden_and_unauthorized_map_to_their_own_statuses()
    {
        var (forbidden, _) = await RunAsync(new ForbiddenAccessException());
        var (unauthorized, _) = await RunAsync(new UnauthorizedAccessException("no token"));

        Assert.Equal(StatusCodes.Status403Forbidden, forbidden);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorized);
    }

    [Fact]
    public async Task Missing_media_maps_to_409_and_is_distinct_from_not_found()
    {
        var (status, body) = await RunAsync(new MediaUnavailableException(Guid.NewGuid()));

        Assert.Equal(StatusCodes.Status409Conflict, status);
        Assert.Equal("media_unavailable", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Storage_failures_map_to_502_and_hide_the_underlying_error()
    {
        var (status, body) = await RunAsync(
            new StorageUnavailableException(inner: new InvalidOperationException("account key rejected")));

        Assert.Equal(StatusCodes.Status502BadGateway, status);
        Assert.Equal("storage_unavailable", body.GetProperty("code").GetString());
        Assert.DoesNotContain("account key", body.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Validation_errors_are_returned_per_field()
    {
        var (status, body) = await RunAsync(new ValidationException("Name", "Playlist name is required."));

        Assert.Equal(StatusCodes.Status400BadRequest, status);
        Assert.Equal("validation_failed", body.GetProperty("code").GetString());
        Assert.Equal("Playlist name is required.", body.GetProperty("errors").GetProperty("Name")[0].GetString());
    }

    [Fact]
    public async Task Unexpected_errors_never_leak_internal_details()
    {
        var (status, body) = await RunAsync(new InvalidOperationException("connection string Host=prod;Password=hunter2"));

        Assert.Equal(StatusCodes.Status500InternalServerError, status);
        Assert.Equal("internal_error", body.GetProperty("code").GetString());
        Assert.DoesNotContain("hunter2", body.GetProperty("message").GetString());
        Assert.False(body.TryGetProperty("stackTrace", out _));
    }

    [Fact]
    public async Task The_error_body_uses_camelCase_like_every_other_response()
    {
        var (_, body) = await RunAsync(new NotFoundException("Track", Guid.Empty));

        Assert.True(body.TryGetProperty("statusCode", out _));
        Assert.False(body.TryGetProperty("StatusCode", out _));
    }
}
