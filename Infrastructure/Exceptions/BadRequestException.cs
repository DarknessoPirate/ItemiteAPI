using Domain.Exceptions;

namespace Infrastructure.Exceptions;

public class BadRequestException(string message, List<string> errors) : BaseException(message, statusCode: 400, errors)
{
    
}