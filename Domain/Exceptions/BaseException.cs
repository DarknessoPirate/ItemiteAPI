namespace Domain.Exceptions;

public class BaseException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; set; } = statusCode;
    public List<string> Errors { get; set; } = [];
}