using Domain.Exceptions;

namespace Infrastructure.Exceptions;

public class NotFoundException(string message) : BaseException(message, statusCode: 404)
{
}