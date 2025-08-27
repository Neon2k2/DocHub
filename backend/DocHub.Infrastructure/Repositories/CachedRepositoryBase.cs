using DocHub.Core.Entities;
using DocHub.Core.Interfaces.Repositories;
using DocHub.Infrastructure.Services.Caching;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DocHub.Infrastructure.Repositories;

public abstract class CachedRepositoryBase<T> : GenericRepository<T>, IGenericRepository<T> where T : BaseEntity
{
    protected readonly ICacheService _cache;
    protected readonly string _cachePrefix;
    protected readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

    protected CachedRepositoryBase(
        ApplicationDbContext context,
        ICacheService cache,
        string cachePrefix) : base(context)
    {
        _cache = cache;
        _cachePrefix = cachePrefix;
    }

    protected virtual string BuildCacheKey(string key, params object[] parameters)
    {
        return CacheKeyBuilder.Build($"{_cachePrefix}:{key}", parameters);
    }

    public override async Task<T?> GetByIdAsync(string id)
    {
        var cacheKey = BuildCacheKey("id", id);
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            return await base.GetByIdAsync(id);
        }, _defaultExpiration);
    }

    public override async Task<IEnumerable<T>> GetAllAsync()
    {
        var cacheKey = BuildCacheKey("all");
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            return await base.GetAllAsync();
        }, _defaultExpiration);
    }

    public override async Task<T> AddAsync(T entity)
    {
        var result = await base.AddAsync(entity);
        await InvalidateListCacheAsync();
        return result;
    }

    public override async Task<T> UpdateAsync(T entity)
    {
        var result = await base.UpdateAsync(entity);
        await InvalidateCacheAsync(entity.Id);
        await InvalidateListCacheAsync();
        return result;
    }

    public override async Task DeleteAsync(T entity)
    {
        await base.DeleteAsync(entity);
        await InvalidateCacheAsync(entity.Id);
        await InvalidateListCacheAsync();
    }

    protected virtual async Task InvalidateCacheAsync(string id)
    {
        var cacheKey = BuildCacheKey("id", id);
        await _cache.RemoveAsync(cacheKey);
    }

    protected virtual async Task InvalidateListCacheAsync()
    {
        var cacheKey = BuildCacheKey("all");
        await _cache.RemoveAsync(cacheKey);
    }

    protected async Task<IEnumerable<T>> GetCachedByConditionAsync(
        Expression<Func<T, bool>> predicate,
        string cacheKey)
    {
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            return await GetQueryable()
                .Where(predicate)
                .ToListAsync();
        }, _defaultExpiration);
    }
}
