namespace Application.Exceptions;

/// <summary>
/// Помилка валідації бізнес-правил. Мапиться на HTTP 400 разом зі словником помилок.
/// </summary>
public class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(string message = "One or more validation errors occurred.")
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string field, string error)
        : base("One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]> { [field] = new[] { error } };
    }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors, string message = "One or more validation errors occurred.")
        : base(message)
    {
        Errors = errors;
    }
}
