using Domain.Exceptions;

namespace Infrastructure.Exceptions;

public class BadRequestException(string message) : BaseException(message, statusCode: 400)
{
    
}