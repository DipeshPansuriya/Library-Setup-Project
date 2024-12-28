using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace InfraLib.Startup_Pro
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Log the exception details using Serilog
            LogException(exception);

            // Determine the status code based on the exception type
            int statusCode = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            // Create problem details
            ProblemDetails problemDetails = new()
            {
                Status = statusCode,
                Title = "An error occurred while processing your request.",
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };

            // Set the response status code and content type
            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";

            // Write the problem details to the response
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private void LogException(Exception exception)
        {
            // Log the main exception using Serilog
            Log.Error(exception, "An error occurred: {Message}", exception.Message);

            // Log inner exceptions if they exist
            Exception innerException = exception.InnerException;
            while (innerException != null)
            {
                Log.Error(innerException, "Inner exception: {Message}", innerException.Message);
                innerException = innerException.InnerException;
            }
        }
    }
}

