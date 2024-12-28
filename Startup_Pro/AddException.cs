using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Net;
using System.Text;

namespace InfraLib.Startup_Pro
{
    public static class AddException
    {
        public static void Builder(WebApplicationBuilder builder)
        {
            _ = builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            _ = builder.Services.AddProblemDetails();
        }

        public static void App(WebApplication app)
        {
            _ = app.UseExceptionHandler(
                new ExceptionHandlerOptions
                {
                    ExceptionHandler =
                        async (c) =>
                        {
                            IExceptionHandlerFeature exceptionFeature = c.Features
                                .Get<IExceptionHandlerFeature>();
                            Exception exception = exceptionFeature?.Error;
                            HttpStatusCode statusCode = exception?.GetType().Name switch
                            {
                                "ArgumentException" => HttpStatusCode.BadRequest,
                                _ => HttpStatusCode.ServiceUnavailable
                            };

                            c.Response.StatusCode = (int)statusCode;

                            byte[] content = Encoding.UTF8.GetBytes($"Error [{exception?.Message}]");
                            await c.Response.Body.WriteAsync(content, 0, content.Length);

                            if (exception != null)
                            {
                                Log.Error(exception, "An error occurred: {Message}", exception.Message);
                                if (exception.InnerException != null)
                                {
                                    Log.Error(exception.InnerException, "Inner exception: {Message}", exception.InnerException.Message);
                                }
                            }
                        }
                });
        }
    }
}
