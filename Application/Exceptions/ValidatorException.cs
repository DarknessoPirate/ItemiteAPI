using Domain.Exceptions;

namespace Application.Exceptions;

public class ValidatorException(string message, List<string> errors) : BaseException(message, statusCode:400, errors) { }