using Domain.Exceptions;

namespace Infrastructure.Exceptions;

public class CloudinaryException(string message) : BaseException(message, statusCode: 500)
{
    
}