//-----------------------------------------------------------------------
// <copyright file="ICacheService.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace KLSPL.Community.Common.Infrastructure.Startup_Proj.Cache
{
    public interface ICacheService
    {
        void Add<T>(
            T item,
            string cacheKey,
            string contextKey,
            bool generateCacheKey = false,
            TimeSpan? cacheDuration = null);

        T Get<T>(string cacheKey, bool generateCacheKey = false);

        void Remove(string cacheKey, bool generateCacheKey = false);

        void Remove(List<string> cacheKey, bool generateCacheKey = false);

        void RemoveAll();

        void RemoveByContextKey(string contextKey);

        void RemoveByContextKey(List<string> contextKey);

        List<string> GetAllCacheKeys(string keyword = null);
    }
}