using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

public static class AddSignalR
{
    public static void AddSignalRBuilder(WebApplicationBuilder builder)
    {
        _ = builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });
        _ = builder.Services.AddSingleton<ISignalRMessageHub, SignalRMessageHub>();
    }

    public static void AddSignalREndPoint(IEndpointRouteBuilder endpoints)
    {
        _ = endpoints.MapHub<SignalRMessageHub>("/MessageHub");
    }
}