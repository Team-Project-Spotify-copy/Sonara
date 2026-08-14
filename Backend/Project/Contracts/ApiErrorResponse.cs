namespace WebApp.Contracts;

/// <summary>
/// Єдиний формат помилки для всього API: і для винятків, і для помилок валідації моделі.
/// Внутрішні деталі (стек, повідомлення інфраструктури) сюди ніколи не потрапляють.
/// </summary>
public class ApiErrorResponse
{
    public int StatusCode { get; set; }

    /// <summary>Повідомлення, придатне для показу користувачеві.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Стабільний код помилки для гілок логіки на фронтенді.</summary>
    public string Code { get; set; } = "error";

    /// <summary>Помилки по полях. Заповнюється лише для 400.</summary>
    public IDictionary<string, string[]>? Errors { get; set; }

    /// <summary>Ідентифікатор запиту для звірки з логами сервера.</summary>
    public string? TraceId { get; set; }
}
