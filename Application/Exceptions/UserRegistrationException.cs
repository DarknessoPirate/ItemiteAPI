using Domain.Exceptions;

namespace Application.Exceptions;

public class UserRegistrationException(string message, List<string> errors) : BaseException(message, statusCode:400 ,errors)
{
    
}