using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace InfraLib.Startup_Pro
{
    public static class AddCORS
    {
        public static void Builder(WebApplicationBuilder builder)
        {
            _ = builder.Services
                .AddCors(
                    options => options.AddPolicy(
                        "CorsPolicy",
                        builder => builder.WithOrigins(AppSettings.AllowedOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .WithExposedHeaders("X-Pagination")));
        }

        public static void App(WebApplication app)
        {
            _ = app.UseCors("CorsPolicy");


            _ = app.Use(
                async (context, next) =>
                {
                    if (context.Request.Method != HttpMethods.Options)
                    {
                        context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                        await next.Invoke();
                    }
                });
        }
    }
}