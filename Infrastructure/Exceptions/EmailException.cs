using Domain.Exceptions;

namespace Infrastructure.Exceptions;

public class EmailException(string message, List<string> errors) : BaseException(message, statusCode:503 ,errors)
{
    
}