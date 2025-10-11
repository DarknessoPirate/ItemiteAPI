using Domain.Exceptions;

namespace Infrastructure.Exceptions;

public class MediaServiceException(string message) : BaseException(message: message, statusCode: 500 )
{
    
}