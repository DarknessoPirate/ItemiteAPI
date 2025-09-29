using Domain.Exceptions;

namespace Infrastructure.Exceptions;

public class ConfigException(string message) : BaseException(message, statusCode: 500)
{
    
}