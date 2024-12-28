//-----------------------------------------------------------------------
// <copyright file="CacheServiceMemory.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using KLSPL.Community.Common.Infrastructure.Startup_Proj.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

public class CacheServiceMemory : ICacheService
{
    static List<string> _cacheKeys = new List<string>();
    readonly Dictionary<string, List<string>> _contextKeyMap;
    readonly IHttpContextAccessor _httpContextAccessor;
    readonly IMemoryCache _memoryCache;
    readonly TimeSpan DefaultExpirationTime = TimeSpan.FromDays(7);

    public CacheServiceMemory(IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor)
    {
        _memoryCache = memoryCache;
        _contextKeyMap = new Dictionary<string, List<string>>();
        _httpContextAccessor = httpContextAccessor;
    }

    public List<string> GetAllCacheKeys(string keyword = null)
    {
        List<string> keys = _cacheKeys.Distinct().ToList();

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

        _ = _memoryCache.Set(cacheKey, item, (TimeSpan)cacheDuration);
        _cacheKeys.Add(cacheKey);

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
        _ = _memoryCache.TryGetValue(cacheKey, out T cacheEntry);
        return cacheEntry;
    }

    public void Remove(string cacheKey, bool generateCacheKey = false)
    {
        Remove(new List<string> { cacheKey }, generateCacheKey);
    }

    public void Remove(List<string> cacheKey, bool generateCacheKey = false)
    {
        string generatedKey = generateCacheKey ? ($"_{CacheService.GenerateCacheKey(_httpContextAccessor)}") : string.Empty;
        foreach (string item in cacheKey)
        {
            _memoryCache.Remove($"{item}{generatedKey}");
        }
        _ = _cacheKeys.RemoveAll(t => cacheKey.Contains(t));
    }

    public void RemoveAll()
    {
        Remove(_cacheKeys);
    }

    public void RemoveByContextKey(string contextKey)
    {
        RemoveByContextKey(new List<string> { contextKey });
    }

    public void RemoveByContextKey(List<string> contextKey)
    {
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