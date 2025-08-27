using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Text.Json;

namespace DocHub.Infrastructure.Services.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
}

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks;
    private readonly ILoggingService _loggingService;

    public MemoryCacheService(
        IMemoryCache cache,
        ILoggingService loggingService)
    {
        _cache = cache;
        _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
        _loggingService = loggingService;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                return value;
            }
            return default;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Cache", "Get", ex.Message, new { key, type = typeof(T).Name });
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new MemoryCacheEntryOptions();

            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration;
            }
            else
            {
                options.SlidingExpiration = TimeSpan.FromMinutes(30);
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
            }

            _cache.Set(key, value, options);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Cache", "Set", ex.Message, new { key, type = typeof(T).Name });
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            _cache.Remove(key);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Cache", "Remove", ex.Message, new { key });
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        try
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                return value!;
            }

            var lockObj = _locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));
            await lockObj.WaitAsync();

            try
            {
                // Double check after acquiring lock
                if (_cache.TryGetValue(key, out value))
                {
                    return value!;
                }

                value = await factory();
                await SetAsync(key, value, expiration);
                return value;
            }
            finally
            {
                lockObj.Release();
                _locks.TryRemove(key, out _);
            }
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Cache", "GetOrSet", ex.Message, new { key, type = typeof(T).Name });
            return await factory();
        }
    }
}

public class CacheKeyBuilder
{
    public static string Build(string prefix, params object[] parameters)
    {
        var parts = new List<string> { prefix };
        parts.AddRange(parameters.Select(p => p?.ToString() ?? "null"));
        return string.Join(":", parts);
    }

    public static class Prefixes
    {
        public const string LetterTemplate = "lt";
        public const string EmailTemplate = "et";
        public const string Employee = "emp";
        public const string Admin = "adm";
        public const string Workflow = "wf";
        public const string Role = "role";
        public const string Permission = "perm";
    }
}
