namespace Lagrange.Core.Exceptions;

public sealed class HttpServiceException(string service, string message, int? statusCode = null, int? businessCode = null, Exception? innerException = null) : Exception(message, innerException)
{
    public string Service { get; } = service;
    public int? StatusCode { get; } = statusCode;
    public int? BusinessCode { get; } = businessCode;
}
