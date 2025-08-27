using DocHub.Core.Entities;
using DocHub.Core.Interfaces.Repositories;
using DocHub.Infrastructure.Services.Caching;
using Microsoft.EntityFrameworkCore;

namespace DocHub.Infrastructure.Repositories;

public class LetterTemplateRepository : CachedRepositoryBase<LetterTemplate>, ILetterTemplateRepository
{
    public LetterTemplateRepository(
        ApplicationDbContext context,
        ICacheService cache) : base(context, cache, CacheKeyBuilder.Prefixes.LetterTemplate)
    {
    }

    public async Task<IEnumerable<LetterTemplate>> GetActiveTemplatesAsync()
    {
        var cacheKey = BuildCacheKey("active");
        return await GetCachedByConditionAsync(
            t => t.IsActive,
            cacheKey);
    }

    public async Task<LetterTemplate?> GetWithFieldsAsync(string id)
    {
        var cacheKey = BuildCacheKey("withFields", id);
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            return await GetQueryable()
                .Include(t => t.Fields)
                .FirstOrDefaultAsync(t => t.Id == id);
        }, _defaultExpiration);
    }
}

public class EmailTemplateRepository : CachedRepositoryBase<EmailTemplate>, IEmailTemplateRepository
{
    public EmailTemplateRepository(
        ApplicationDbContext context,
        ICacheService cache) : base(context, cache, CacheKeyBuilder.Prefixes.EmailTemplate)
    {
    }

    public async Task<IEnumerable<EmailTemplate>> GetByTypeAsync(string type)
    {
        var cacheKey = BuildCacheKey("type", type);
        return await GetCachedByConditionAsync(
            t => t.Type == type && t.IsActive,
            cacheKey);
    }
}

public class EmployeeRepository : CachedRepositoryBase<Employee>, IEmployeeRepository
{
    public EmployeeRepository(
        ApplicationDbContext context,
        ICacheService cache) : base(context, cache, CacheKeyBuilder.Prefixes.Employee)
    {
    }

    public async Task<Employee?> GetByEmailAsync(string email)
    {
        var cacheKey = BuildCacheKey("email", email);
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            return await GetQueryable()
                .FirstOrDefaultAsync(e => e.Email == email && e.IsActive);
        }, _defaultExpiration);
    }

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(string department)
    {
        var cacheKey = BuildCacheKey("dept", department);
        return await GetCachedByConditionAsync(
            e => e.Department == department && e.IsActive,
            cacheKey);
    }
}

public class AdminRepository : CachedRepositoryBase<Admin>, IAdminRepository
{
    public AdminRepository(
        ApplicationDbContext context,
        ICacheService cache) : base(context, cache, CacheKeyBuilder.Prefixes.Admin)
    {
    }

    public async Task<Admin?> GetByUsernameAsync(string username)
    {
        var cacheKey = BuildCacheKey("username", username);
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            return await GetQueryable()
                .FirstOrDefaultAsync(a => a.Username == username && a.IsActive);
        }, _defaultExpiration);
    }

    public async Task<IEnumerable<Admin>> GetByRoleAsync(string role)
    {
        var cacheKey = BuildCacheKey("role", role);
        return await GetCachedByConditionAsync(
            a => a.Role == role && a.IsActive,
            cacheKey);
    }
}
