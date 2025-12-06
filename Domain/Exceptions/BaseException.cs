namespace Domain.Exceptions;

public class BaseException : Exception
{
    public string? DetailedMessage { get; set; } = null;
    public int StatusCode { get; set; }
    public List<string> Errors { get; set; } = [];

    protected BaseException(string message, int statusCode, string? detailedMessage = null) : base(message)
    {
        DetailedMessage = detailedMessage;
        StatusCode = statusCode;
    }

    protected BaseException(string message, int statusCode, List<string> errors) : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}