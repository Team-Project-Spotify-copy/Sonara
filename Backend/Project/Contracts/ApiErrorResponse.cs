namespace WebApp.Contracts;

public class ApiErrorResponse
{
    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Code { get; set; } = "error";

    public IDictionary<string, string[]>? Errors { get; set; }

    public string? TraceId { get; set; }
}
