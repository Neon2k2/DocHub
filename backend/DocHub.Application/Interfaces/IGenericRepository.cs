using DocHub.Core.Entities;

namespace DocHub.Application.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(string id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<int> GetCountAsync();
        Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize);
        Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate);
        Task<T?> FirstOrDefaultAsync(Func<T, bool> predicate);
        Task<IEnumerable<T>> GetActiveAsync();
        Task<bool> ToggleStatusAsync(string id);
        Task<int> SaveChangesAsync();
        Task<IEnumerable<T>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<T>> SearchAsync(string searchTerm);
        Task<T?> GetFirstOrDefaultAsync(Func<T, bool> predicate);
    }
}
