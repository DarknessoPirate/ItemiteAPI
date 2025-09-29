using Domain.Exceptions;

namespace Infrastructure.Exceptions;

public class UnauthorizedException(string message) : BaseException(message, statusCode: 401)
{
    
}