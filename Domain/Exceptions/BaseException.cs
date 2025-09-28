namespace Domain.Exceptions;

public class BaseException : Exception
{
    public int StatusCode { get; set; }
    public List<string> Errors { get; set; } = [];

    protected BaseException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    protected BaseException(string message, int statusCode, List<string> errors) : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}