using Domain.Exceptions;

namespace Infrastructure.Exceptions;

public class StripeErrorException(string message, string? detailedMessage = null) : BaseException(message, statusCode: 500, detailedMessage)
{
    
}