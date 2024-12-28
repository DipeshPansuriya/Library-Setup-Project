using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace InfraLib.Startup_Pro
{
    public static class AddCORS
    {
        public static void Builder(WebApplicationBuilder builder, string[] AllowedOrigins)
        {
            _ = builder.Services
                .AddCors(
                    options => options.AddPolicy(
                        "CorsPolicy",
                        builder => builder.WithOrigins(AllowedOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .WithExposedHeaders("X-Pagination")));
        }

        public static void Builder(WebApplicationBuilder builder)
        {
            Builder(builder, new string[] { "*" });
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
