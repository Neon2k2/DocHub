using Microsoft.EntityFrameworkCore.Storage;

namespace DocHub.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Begins a new transaction
    /// </summary>
    /// <returns>The transaction object</returns>
    Task<IDbContextTransaction> BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackAsync();

    /// <summary>
    /// Saves all changes made in this context to the database
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Executes an action within a transaction
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> ExecuteInTransactionAsync(Func<Task> action);

    /// <summary>
    /// Executes an action within a transaction and returns a result
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="action">The action to execute</param>
    /// <returns>The result of the action</returns>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action);
}
