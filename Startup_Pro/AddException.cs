using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
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
                        (c) =>
                        {
                            IExceptionHandlerFeature exception = c.Features
                                .Get<IExceptionHandlerFeature>();
                            HttpStatusCode statusCode = exception.Error.GetType().Name switch
                            {
                                "ArgumentException" => HttpStatusCode.BadRequest,
                                _ => HttpStatusCode.ServiceUnavailable
                            };

                            c.Response.StatusCode = (int)statusCode;

                            byte[] content = (AppSettings.EnvironmentName.ToLower() != "dev")
                                ? Encoding.UTF8.GetBytes($"Error [{exception.Error.Message}]")
                                : Encoding.UTF8
                                            .GetBytes($"Error : Kindly contact system administrator.");
                            _ = c.Response.Body.WriteAsync(content, 0, content.Length);

                            return Task.CompletedTask;
                        }
                });
        }
    }
}