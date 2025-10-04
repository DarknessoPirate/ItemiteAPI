using Domain.Exceptions;

namespace Infrastructure.Exceptions;

public class InvalidTokenException(string message) : BaseException(message, statusCode: 401)
{
}


