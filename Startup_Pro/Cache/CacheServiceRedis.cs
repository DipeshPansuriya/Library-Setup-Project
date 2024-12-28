//-----------------------------------------------------------------------
// <copyright file="CacheServiceRedis.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using KLSPL.Community.Common.Infrastructure;
using KLSPL.Community.Common.Infrastructure.Startup_Proj.Cache;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;
using System.Net;

public class CacheServiceRedis : ICacheService
{
    readonly string _contextKeyPrefix = "context_key:";
    readonly IHttpContextAccessor _httpContextAccessor;
    readonly IConnectionMultiplexer _redisConnection;
    readonly IDatabase _redisDatabase;
    readonly TimeSpan DefaultExpirationTime = TimeSpan.FromDays(7);

    public CacheServiceRedis(
        IConnectionMultiplexer redisConnection,
        IHttpContextAccessor httpContextAccessor)
    {
        _redisConnection = redisConnection;
        _redisDatabase = _redisConnection.GetDatabase();
        _httpContextAccessor = httpContextAccessor;
    }

    bool IsRediscConnect()
    {
        try
        {
            IDatabase db = _redisConnection.GetDatabase();
            TimeSpan pong = db.Ping();
            return pong.TotalMilliseconds > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public List<string> GetAllCacheKeys(string keyword = null)
    {
        List<string> keys = new List<string>();
        if (IsRediscConnect())
        {
            EndPoint[] endpoints = _redisConnection.GetEndPoints(true);
            foreach (EndPoint endpoint in endpoints)
            {
                IServer server = _redisConnection.GetServer(endpoint);
                keys = server.Keys().Select(a => a.ToString()).ToList();
            }
        }

        keys = keys.Distinct().ToList();

        return string.IsNullOrEmpty(keyword) ? keys : keys.Where(a => a.ToLower().Contains(keyword.ToLower())).ToList();
    }

    public void Add<T>(
        T item,
        string cacheKey,
        string contextKey,
        bool generateCacheKey = false,
        TimeSpan? cacheDuration = null)
    {
        cacheKey = $"{cacheKey}{(generateCacheKey ? ($"_{CacheService.GenerateCacheKey(_httpContextAccessor)}") : string.Empty)}";
        if (IsRediscConnect())
        {
            if (cacheDuration == null)
            {
                cacheDuration = DefaultExpirationTime;
            }

            string serializedData = GenericFunction.ClassToJson<T>(item);
            _ = _redisDatabase.StringSet(cacheKey, serializedData, cacheDuration);

            if (!string.IsNullOrEmpty(contextKey))
            {
                string contextKeySet = $"{_contextKeyPrefix}{contextKey}";
                _ = _redisDatabase.SetAdd(contextKeySet, cacheKey);
            }
        }
    }

    public T Get<T>(string cacheKey, bool generateCacheKey = false)
    {
        cacheKey = $"{cacheKey}{(generateCacheKey ? ($"_{CacheService.GenerateCacheKey(_httpContextAccessor)}") : string.Empty)}";
        if (IsRediscConnect())
        {
            RedisValue cacheEntry = _redisDatabase.StringGet(cacheKey);
            return cacheEntry.HasValue ? GenericFunction.JsonToClass<T>(cacheEntry) : default;
        }

        return default;
    }

    public void Remove(string cacheKey, bool generateCacheKey = false)
    {
        Remove(new List<string> { cacheKey }, generateCacheKey);
    }

    public void Remove(List<string> cacheKey, bool generateCacheKey = false)
    {
        if (IsRediscConnect())
        {
            string generatedKey = generateCacheKey ? ($"_{CacheService.GenerateCacheKey(_httpContextAccessor)}") : string.Empty;
            foreach (string item in cacheKey)
            {
                _ = _redisDatabase.KeyDelete($"{item}{generatedKey}");
            }
        }
    }

    public void RemoveAll()
    {
        if (IsRediscConnect())
        {
            EndPoint[] endpoints = _redisConnection.GetEndPoints(true);
            foreach (EndPoint endpoint in endpoints)
            {
                IServer server = _redisConnection.GetServer(endpoint);
                server.FlushDatabase();
                server.FlushAllDatabases();
            }
        }
    }

    public void RemoveByContextKey(string contextKey)
    {
        RemoveByContextKey(new List<string> { contextKey });
    }

    public void RemoveByContextKey(List<string> contextKey)
    {
        if (IsRediscConnect())
        {
            foreach (string item in contextKey)
            {
                string contextKeySet = $"{_contextKeyPrefix}{item}";
                RedisValue[] cacheKeys = _redisDatabase.SetMembers(contextKeySet);

                foreach (RedisValue cacheKey in cacheKeys)
                {
                    _ = _redisDatabase.KeyDelete(cacheKey.ToString());
                }

                _ = _redisDatabase.KeyDelete(contextKeySet);
            }
        }
    }
}