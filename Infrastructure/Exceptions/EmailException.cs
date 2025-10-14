using Domain.Exceptions;

namespace Infrastructure.Exceptions;

public class EmailException: BaseException
{
    public EmailException(string message) : base(message: message, statusCode: 503){}
    
    public EmailException(string message, List<string> errors) : base(message: message,errors: errors ,statusCode: 503){}
}