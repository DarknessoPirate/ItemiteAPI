using Domain.Exceptions;

namespace Application.Exceptions;

public class ValidatorException(string message) : BaseException(message, statusCode:400) { }