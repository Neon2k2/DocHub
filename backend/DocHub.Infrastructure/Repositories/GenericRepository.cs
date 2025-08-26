using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using DocHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DocHub.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly DocHubDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<GenericRepository<T>> _logger;

        public GenericRepository(DocHubDbContext context, ILogger<GenericRepository<T>> logger)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _logger = logger;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Where(e => !e.IsDeleted)
                    .OrderBy(e => e.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all entities of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("ID cannot be null or empty", nameof(id));
                }

                var entity = await _dbSet
                    .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

                if (entity == null)
                {
                    _logger.LogWarning("Entity of type {EntityType} with ID {Id} not found", typeof(T).Name, id);
                }

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public async Task<T> AddAsync(T entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }

                entity.Id = Guid.NewGuid().ToString();
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.IsDeleted = false;

                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Entity of type {EntityType} with ID {Id} added successfully", typeof(T).Name, entity.Id);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<T> UpdateAsync(T entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }

                if (string.IsNullOrEmpty(entity.Id))
                {
                    throw new ArgumentException("Entity ID cannot be null or empty", nameof(entity));
                }

                var existingEntity = await _dbSet
                    .FirstOrDefaultAsync(e => e.Id == entity.Id && !e.IsDeleted);

                if (existingEntity == null)
                {
                    _logger.LogWarning("Entity of type {EntityType} with ID {Id} not found for update", typeof(T).Name, entity.Id);
                    throw new InvalidOperationException($"Entity with ID {entity.Id} not found");
                }

                // Update properties
                entity.UpdatedAt = DateTime.UtcNow;
                entity.CreatedAt = existingEntity.CreatedAt;
                entity.CreatedBy = existingEntity.CreatedBy;

                _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Entity of type {EntityType} with ID {Id} updated successfully", typeof(T).Name, entity.Id);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity of type {EntityType} with ID {Id}", typeof(T).Name, entity.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("ID cannot be null or empty", nameof(id));
                }

                var entity = await _dbSet
                    .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

                if (entity == null)
                {
                    _logger.LogWarning("Entity of type {EntityType} with ID {Id} not found for deletion", typeof(T).Name, id);
                    return false;
                }

                // Soft delete
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.DeletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Entity of type {EntityType} with ID {Id} deleted successfully", typeof(T).Name, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return false;
                }

                return await _dbSet
                    .AnyAsync(e => e.Id == id && !e.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
                return false;
            }
        }

        public async Task<int> GetCountAsync()
        {
            try
            {
                return await _dbSet
                    .Where(e => !e.IsDeleted)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of entities of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;

                var skip = (page - 1) * pageSize;

                return await _dbSet
                    .Where(e => !e.IsDeleted)
                    .OrderBy(e => e.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged entities of type {EntityType}. Page: {Page}, PageSize: {PageSize}", 
                    typeof(T).Name, page, pageSize);
                throw;
            }
        }

        public async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate)
        {
            try
            {
                return await Task.FromResult(_dbSet
                    .Where(e => !e.IsDeleted)
                    .AsEnumerable()
                    .Where(predicate)
                    .ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding entities of type {EntityType} with predicate", typeof(T).Name);
                throw;
            }
        }

        public async Task<T?> FirstOrDefaultAsync(Func<T, bool> predicate)
        {
            try
            {
                return await Task.FromResult(_dbSet
                    .Where(e => !e.IsDeleted)
                    .AsEnumerable()
                    .FirstOrDefault(predicate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding first entity of type {EntityType} with predicate", typeof(T).Name);
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetActiveAsync()
        {
            try
            {
                // This assumes entities have an IsActive property
                // If not, you can override this method in specific repositories
                if (typeof(T).GetProperty("IsActive") != null)
                {
                    return await _dbSet
                        .Where(e => !e.IsDeleted && EF.Property<bool>(e, "IsActive"))
                        .OrderBy(e => e.CreatedAt)
                        .ToListAsync();
                }

                // Fallback to non-deleted entities
                return await _dbSet
                    .Where(e => !e.IsDeleted)
                    .OrderBy(e => e.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active entities of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<bool> ToggleStatusAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("ID cannot be null or empty", nameof(id));
                }

                var entity = await _dbSet
                    .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

                if (entity == null)
                {
                    _logger.LogWarning("Entity of type {EntityType} with ID {Id} not found for status toggle", typeof(T).Name, id);
                    return false;
                }

                // Check if entity has IsActive property
                var isActiveProperty = typeof(T).GetProperty("IsActive");
                if (isActiveProperty == null)
                {
                    _logger.LogWarning("Entity of type {EntityType} does not have IsActive property", typeof(T).Name);
                    return false;
                }

                var currentValue = isActiveProperty.GetValue(entity);
                if (currentValue == null)
                {
                    _logger.LogWarning("IsActive property is null for entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
                    return false;
                }
                var currentStatus = (bool)currentValue;
                isActiveProperty.SetValue(entity, !currentStatus);
                entity.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Entity of type {EntityType} with ID {Id} status toggled from {OldStatus} to {NewStatus}", 
                    typeof(T).Name, id, currentStatus, !currentStatus);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status of entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes for entities of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _dbSet
                    .Where(e => !e.IsDeleted && e.CreatedAt >= startDate && e.CreatedAt <= endDate)
                    .OrderBy(e => e.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entities of type {EntityType} by date range {StartDate} to {EndDate}", 
                    typeof(T).Name, startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<T>> SearchAsync(string searchTerm)
        {
            try
            {
                var allEntities = await GetAllAsync();
                return allEntities.Where(e => e.Id.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching entities of type {EntityType}", typeof(T).Name);
                return Enumerable.Empty<T>();
            }
        }

        public async Task<T?> GetFirstOrDefaultAsync(Func<T, bool> predicate)
        {
            try
            {
                var allEntities = await GetAllAsync();
                return allEntities.FirstOrDefault(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting first or default entity of type {EntityType}", typeof(T).Name);
                return null;
            }
        }
    }
}
