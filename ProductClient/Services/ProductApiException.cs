using System.Net;

namespace ProductClient.Services;

public sealed class ProductApiException(HttpStatusCode? statusCode, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public HttpStatusCode? StatusCode { get; } = statusCode;
}
