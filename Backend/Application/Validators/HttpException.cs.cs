using System.Net;

namespace Application.Validators;

[Serializable]
public class HttpException : Exception
{
    public HttpStatusCode StatusCode { get; set; }

    public HttpException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpException(string message, HttpStatusCode statusCode, Exception inner)
        : base(message, inner)
    {
        StatusCode = statusCode;
    }
}
