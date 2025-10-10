using Domain.Exceptions;

namespace Application.Exceptions;

public class ForbiddenException(string message) : BaseException(message, statusCode: 403) { }