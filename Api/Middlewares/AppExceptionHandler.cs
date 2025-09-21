using System.Text.Json;
using Api.Exceptions;
using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Api.Middlewares;

public class AppExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int statusCode;
        string message;
        List<string> errorList = [];

        if (exception is BaseException ex)
        {
            statusCode = ex.StatusCode;
            message = ex.Message;
            errorList = ex.Errors;
        }
        else
        {
            statusCode = 500;
            message = exception.Message;
        }
        

        
        var errorResponse = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            Errors = errorList
        };
        
        httpContext.Response.ContentType = "application/json";
        var jsonResponse = JsonSerializer.Serialize(errorResponse);
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsync(jsonResponse, cancellationToken: cancellationToken);
        
        return true;
    }
}