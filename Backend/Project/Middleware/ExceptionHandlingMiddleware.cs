namespace WebApp;

using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Application.Exceptions;
using WebApp.Contracts;
using ValidationException = Application.Exceptions.ValidationException;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = JsonSerializerOptions.Web;

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug("Request {Path} was aborted by the client.", context.Request.Path);
            return;
        }

        var (statusCode, code) = Map(exception);

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "An unexpected error occurred in the Sonara application");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {ExceptionType}", exception.GetType().Name);
        }

        if (context.Response.HasStarted)
        {
            _logger.LogError("The response has already started; the error could not be written to the body.");
            return;
        }

        context.Response.Clear();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ApiErrorResponse
        {
            StatusCode = (int)statusCode,
            Code = code,
            Message = statusCode switch
            {
                HttpStatusCode.InternalServerError =>
                    "Oops, something went wrong on the Sonara server. We are already working on it!",
                HttpStatusCode.BadGateway =>
                    "Media storage is temporarily unavailable. Please try again in a moment.",
                _ => exception.Message
            },
            Errors = exception is ValidationException { Errors.Count: > 0 } validation
                ? validation.Errors.ToDictionary(e => e.Key, e => e.Value)
                : null,
            TraceId = Activity.Current?.Id ?? context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
    }

    private static (HttpStatusCode StatusCode, string Code) Map(Exception exception) => exception switch
    {
        ValidationException => (HttpStatusCode.BadRequest, "validation_failed"),
        ArgumentException => (HttpStatusCode.BadRequest, "bad_request"),
        NotFoundException => (HttpStatusCode.NotFound, "not_found"),
        ForbiddenAccessException => (HttpStatusCode.Forbidden, "forbidden"),
        UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "unauthorized"),
        MediaUnavailableException => (HttpStatusCode.Conflict, "media_unavailable"),
        ConflictException => (HttpStatusCode.Conflict, "conflict"),
        StorageUnavailableException => (HttpStatusCode.BadGateway, "storage_unavailable"),
        OperationCanceledException => ((HttpStatusCode)499, "client_closed_request"),
        _ => (HttpStatusCode.InternalServerError, "internal_error")
    };
}
