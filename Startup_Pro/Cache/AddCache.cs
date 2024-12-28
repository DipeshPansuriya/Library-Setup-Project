//-----------------------------------------------------------------------
// <copyright file="AddCache.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using KLSPL.Community.Common.Infrastructure;
using KLSPL.Community.Common.Infrastructure.Startup_Proj.Cache;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

public static class AddCache
{
    public static void AddCacheBuilder(WebApplicationBuilder builder)
    {
        _ = builder.Services.AddMemoryCache();

        if(AppSettings.enableCache.UseRedisCache)
        {
            _ = builder.Services
                .AddStackExchangeRedisCache(
                    options =>
                    {
                        options.Configuration = AppSettings.enableCache.RedisCacheURL;
                        options.InstanceName = AppSettings.EnvironmentName;
                        options.ConfigurationOptions.AllowAdmin = true;
                    });
        }

        if(AppSettings.enableCache.UseMemoryCache && AppSettings.enableCache.UseRedisCache)
        {
            _ = builder.Services.AddSingleton<ICacheService, CacheService>();
            _ = builder.Services
                .AddSingleton<IConnectionMultiplexer>(
                    ConnectionMultiplexer.Connect(AppSettings.enableCache.RedisCacheURL));
        }
        else if(AppSettings.enableCache.UseMemoryCache)
        {
            _ = builder.Services.AddSingleton<ICacheService, CacheServiceMemory>();
        }
        else if(AppSettings.enableCache.UseRedisCache)
        {
            _ = builder.Services.AddSingleton<ICacheService, CacheServiceRedis>();
            _ = builder.Services
                .AddSingleton<IConnectionMultiplexer>(
                    ConnectionMultiplexer.Connect(AppSettings.enableCache.RedisCacheURL));
        }
        else
        {
            _ = builder.Services.AddSingleton<ICacheService, CacheServiceDefault>();
        }
    }
}