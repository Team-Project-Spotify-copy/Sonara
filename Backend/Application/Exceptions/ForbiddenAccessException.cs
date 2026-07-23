namespace Application.Exceptions;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "Access to this resource is denied..")
        : base(message)
    {
    }
}