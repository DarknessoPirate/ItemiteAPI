using Domain.Exceptions;

namespace Infrastructure.Exceptions;
public class BadRequestException : BaseException
{
    public BadRequestException(string message) 
        : base(message, statusCode: 400)
    {
    }
    
    public BadRequestException(string message, List<string> errors) 
        : base(message, statusCode: 400, errors)
    {
    }
}
