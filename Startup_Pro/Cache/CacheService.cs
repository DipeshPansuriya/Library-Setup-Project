//-----------------------------------------------------------------------
// <copyright file="CacheService.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using KLSPL.Community.Common.Infrastructure;
using KLSPL.Community.Common.Infrastructure.Startup_Proj.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System.Net;
using System.Text;

public class CacheService : ICacheService
{
    static List<string> _cacheKeys = new List<string>();
    readonly Dictionary<string, List<string>> _contextKeyMap;
    readonly string _contextKeyPrefix = "context_key:";
    readonly IHttpContextAccessor _httpContextAccessor;
    readonly IMemoryCache _memoryCache;
    readonly IConnectionMultiplexer _redisConnection;
    readonly IDatabase _redisDatabase;
    readonly TimeSpan DefaultExpirationTime = TimeSpan.FromDays(7);

    public CacheService(
        IConnectionMultiplexer redisConnection,
        IMemoryCache memoryCache,
        IHttpContextAccessor httpContextAccessor)
    {
        _memoryCache = memoryCache;
        _contextKeyMap = new Dictionary<string, List<string>>();
        _redisConnection = redisConnection;
        _redisDatabase = _redisConnection.GetDatabase();
        _httpContextAccessor = httpContextAccessor;
    }

    void AddCacheKey(string cacheKey)
    {
        if (!_cacheKeys.Contains(cacheKey))
        {
            _cacheKeys.Add(cacheKey);
        }
    }

    public static string GenerateCacheKey(IHttpContextAccessor _httpContextAccessor)
    {
        if ((_httpContextAccessor != null) &&
            (_httpContextAccessor.HttpContext != null) &&
            (_httpContextAccessor.HttpContext.Request != null))
        {
            HttpContext context = _httpContextAccessor.HttpContext;
            StringBuilder keyBuilder = new StringBuilder();
            _ = keyBuilder.Append(context.Request.Path);

            if (context.Request.Query.Count > 0)
            {
                foreach (var (key, value) in context.Request.Query.OrderBy(x => x.Key))
                {
                    _ = keyBuilder.Append($"|{key}:{value}");
                }
            }
            if (context.Request.HasFormContentType)
            {
                foreach (var (key, value) in context.Request.Form.OrderBy(x => x.Key))
                {
                    _ = keyBuilder.Append($"|{key}:{value}");
                }
            }

            return keyBuilder.ToString();
        }
        return default;
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

        keys.AddRange(_cacheKeys);
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
        if (cacheDuration == null)
        {
            cacheDuration = DefaultExpirationTime;
        }

        AddCacheKey(cacheKey);

        if (IsRediscConnect())
        {
            string serializedData = GenericFunction.ClassToJson<T>(item);
            _ = _redisDatabase.StringSet(cacheKey, serializedData, cacheDuration);

            if (!string.IsNullOrEmpty(contextKey))
            {
                string contextKeySet = $"{_contextKeyPrefix}{contextKey}";
                _ = _redisDatabase.SetAdd(contextKeySet, cacheKey);
            }
        }

        _ = _memoryCache.Set(cacheKey, item, (TimeSpan)cacheDuration);

        if (!string.IsNullOrEmpty(contextKey))
        {
            if (!_contextKeyMap.ContainsKey(contextKey))
            {
                _contextKeyMap[contextKey] = new List<string>();
            }
            _contextKeyMap[contextKey].Add(cacheKey);
        }
    }

    public T Get<T>(string cacheKey, bool generateCacheKey = false)
    {
        cacheKey = $"{cacheKey}{(generateCacheKey ? ($"_{CacheService.GenerateCacheKey(_httpContextAccessor)}") : string.Empty)}";
        if (IsRediscConnect())
        {
            RedisValue cacheEntry = _redisDatabase.StringGet(cacheKey);
            if (cacheEntry.HasValue)
            {
                return GenericFunction.JsonToClass<T>(cacheEntry);
            }
        }

        _ = _memoryCache.TryGetValue(cacheKey, out T cacheEntryData);
        if (cacheEntryData != null)
        {
            return cacheEntryData;
        }

        return default;
    }

    public void Remove(string cacheKey, bool generateCacheKey = false)
    {
        Remove(new List<string> { cacheKey }, generateCacheKey);
    }

    public void Remove(List<string> cacheKey, bool generateCacheKey = false)
    {
        string generatedKey = generateCacheKey ? ($"_{CacheService.GenerateCacheKey(_httpContextAccessor)}") : string.Empty;
        if (IsRediscConnect())
        {
            foreach (string item in cacheKey)
            {
                if (_redisDatabase.KeyExists($"{item}{generatedKey}"))
                {
                    _ = _redisDatabase.KeyDelete($"{item}{generatedKey}");
                }
            }
        }

        foreach (string item in cacheKey)
        {
            _memoryCache.Remove($"{item}{generatedKey}");
        }

        _ = _cacheKeys.RemoveAll(t => cacheKey.Contains(t));
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
        Remove(_cacheKeys);
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

        foreach (string item in contextKey)
        {
            if (_contextKeyMap.ContainsKey(item))
            {
                foreach (string cacheKey in _contextKeyMap[item])
                {
                    _memoryCache.Remove(cacheKey);
                }
                _ = _contextKeyMap.Remove(item);
            }
        }
    }
}

public class CacheServiceDefault : ICacheService
{
    public void Add<T>(
        T item,
        string cacheKey,
        string contextKey,
        bool generateCacheKey,
        TimeSpan? cacheDuration)
    {
    }

    public T Get<T>(string cacheKey, bool generateCacheKey)
    {
        return default;
    }

    public void Remove(string cacheKey, bool generateCacheKey)
    {
    }

    public void Remove(List<string> cacheKey, bool generateCacheKey)
    {
    }

    public void RemoveAll()
    {
    }

    public void RemoveByContextKey(string contextKey)
    {
    }

    public void RemoveByContextKey(List<string> contextKey)
    {
    }

    public List<string> GetAllCacheKeys()
    {
        return null;
    }

    public List<string> GetAllCacheKeys(string keyword = null)
    {
        throw new NotImplementedException();
    }
}