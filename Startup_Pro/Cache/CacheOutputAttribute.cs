//-----------------------------------------------------------------------
// <copyright file="CacheOutputAttribute.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using KLSPL.Community.Common.Infrastructure;
using KLSPL.Community.Common.Infrastructure.Startup_Proj.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class OutputCacheAttribute : ActionFilterAttribute
{
    readonly TimeSpan _cacheDuration;
    readonly string _contextKey;

    public OutputCacheAttribute(int durationInSeconds, string contextKey)
    {
        _cacheDuration = TimeSpan.FromSeconds(durationInSeconds);
        _contextKey = contextKey;
    }

    string GenerateCacheKey(ActionExecutingContext context)
    {
        StringBuilder keyBuilder = new StringBuilder();
        _ = keyBuilder.Append(context.HttpContext.Request.Path);

        foreach (var (key, value) in context.ActionArguments.OrderBy(x => x.Key))
        {
            _ = keyBuilder.Append($"|{key}:{value}");
        }

        return keyBuilder.ToString();
    }

    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if((AppSettings.enableCache.UseRedisCache == true) ||
            (AppSettings.enableCache.UseMemoryCache == true))
        {
            ICacheService cacheService = context.HttpContext.RequestServices
                .GetService<ICacheService>();
            string cacheKey = GenerateCacheKey(context);
            string cachedResponse = cacheService.Get<string>(cacheKey);

            if(cachedResponse != null)
            {
                context.Result = new ContentResult
                {
                    Content = cachedResponse,
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }
            else
            {
                ActionExecutedContext executedContext = await next();
                if((executedContext.Result is ObjectResult objectResult) &&
                    (objectResult.StatusCode == 200) && objectResult.Value != null)
                {
                    string response = (objectResult.Value.GetType() == typeof(string))
                        ? objectResult.Value.ToString()
                        : GenericFunction.ObjectToJson(objectResult.Value);
                    cacheService.Add(
                        response,
                        cacheKey,
                        cacheDuration: _cacheDuration,
                        contextKey: _contextKey);
                }
            }
        }
        else
        {
            _ = await next();
        }
    }
}