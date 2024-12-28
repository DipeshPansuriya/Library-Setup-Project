//-----------------------------------------------------------------------
// <copyright file="AddRateLimit.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using KLSPL.Community.Common.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Primitives;
using System.Threading.RateLimiting;

public class AddRateLimit
{
    static string GetRateLimitUserIdentifier(HttpContext _context)
    {
        StringValues authorization = _context.Request.Headers["authorization"];

        string authorizationToken = (authorization == StringValues.Empty)
            ? string.Empty
            : authorization.Single().Split(" ").Last();
        return string.Concat(_context.Request.Path, "_", authorizationToken);
    }

    static int GlobalLimiterPermitLimit(HttpContext _context, RateLimitPolicy rateLimitPolicy)
    {
        return rateLimitPolicy switch
        {
            RateLimitPolicy.DefaultRateLimiter => AppSettings.apiRateLimiter.DefaultFixedLimit,
            RateLimitPolicy.HighCapecityRateLimiter => AppSettings.apiRateLimiter.HighCapecityFixedLimit,
            RateLimitPolicy.ConcurrencyRateLimiter => AppSettings.apiRateLimiter.ConcurrencyRateLimit,
            RateLimitPolicy.GlobalLimiter => (_context.Request.Headers["authorization"] ==
                    StringValues.Empty)
                ? AppSettings.apiRateLimiter.DefaultFixedLimit
                : AppSettings.apiRateLimiter.DistributedFixedLimit
        };
    }

    static TimeSpan GlobalLimiterWindows(HttpContext _context, RateLimitPolicy rateLimitPolicy)
    {
        return rateLimitPolicy switch
        {
            RateLimitPolicy.DefaultRateLimiter => TimeSpan.FromSeconds(
                AppSettings.apiRateLimiter.DefaultFixedWindow),
            RateLimitPolicy.HighCapecityRateLimiter => TimeSpan.FromSeconds(
                AppSettings.apiRateLimiter.HighCapecityFixedWindow),
            RateLimitPolicy.GlobalLimiter => TimeSpan.FromSeconds(
                (_context.Request.Headers["authorization"] == StringValues.Empty)
                    ? AppSettings.apiRateLimiter.DefaultFixedWindow
                    : AppSettings.apiRateLimiter.DistributedFixedWindow)
        };
    }

    public static void AddRateLimiterapp(WebApplication app)
    {
        _ = app.UseRateLimiter();
    }

    public static void AddRateLimiterBuilder(WebApplicationBuilder builder)
    {
        #region Rate Limiting

        _ = builder.Services
            .AddRateLimiter(
                options =>
                {
                    _ = options.RejectionStatusCode = 429;

                    ConfigureFixedWindowLimiter(options, RateLimitPolicy.DefaultRateLimiter);
                    ConfigureFixedWindowLimiter(options, RateLimitPolicy.HighCapecityRateLimiter);
                    ConfigureFixedWindowLimiter(options, RateLimitPolicy.ConcurrencyRateLimiter);
                    options.GlobalLimiter = CreateGlobalLimiter();
                });
        #endregion Rate Limiting
    }

    public static void ConfigureFixedWindowLimiter(
        RateLimiterOptions options,
        RateLimitPolicy rateLimitPolicy)
    {
        if(rateLimitPolicy == RateLimitPolicy.DefaultRateLimiter)
        {
            _ = options.AddFixedWindowLimiter(
                "Default",
                opt =>
                {
                    opt.Window = GlobalLimiterWindows(null, RateLimitPolicy.DefaultRateLimiter);
                    opt.PermitLimit = GlobalLimiterPermitLimit(
                        null,
                        RateLimitPolicy.DefaultRateLimiter);
                });
        }
        else if(rateLimitPolicy == RateLimitPolicy.HighCapecityRateLimiter)
        {
            _ = options.AddFixedWindowLimiter(
                "HighCapecity",
                opt =>
                {
                    opt.Window = GlobalLimiterWindows(null, RateLimitPolicy.HighCapecityRateLimiter);
                    opt.PermitLimit = GlobalLimiterPermitLimit(
                        null,
                        RateLimitPolicy.HighCapecityRateLimiter);
                });
        }
        else if(rateLimitPolicy == RateLimitPolicy.ConcurrencyRateLimiter)
        {
            _ = options.AddConcurrencyLimiter(
                "Concurrency",
                opt => opt.PermitLimit =
                    GlobalLimiterPermitLimit(null, RateLimitPolicy.ConcurrencyRateLimiter));
        }
    }

    public static PartitionedRateLimiter<HttpContext> CreateGlobalLimiter()
    {
        return PartitionedRateLimiter.CreateChained(
            PartitionedRateLimiter.Create<HttpContext, string>(
                httpContext => RateLimitPartition.GetFixedWindowLimiter(
                    GetRateLimitUserIdentifier(httpContext),
                    partition => new FixedWindowRateLimiterOptions
            {
                Window = GlobalLimiterWindows(httpContext, RateLimitPolicy.GlobalLimiter),
                PermitLimit = GlobalLimiterPermitLimit(httpContext, RateLimitPolicy.GlobalLimiter),
            })));
    }
}

public enum RateLimitPolicy
{
    DefaultRateLimiter,
    HighCapecityRateLimiter,
    ConcurrencyRateLimiter,
    GlobalLimiter
}