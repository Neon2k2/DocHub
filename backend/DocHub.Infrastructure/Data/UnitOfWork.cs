using DocHub.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace DocHub.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly DocHubDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(DocHubDbContext context)
    {
        _context = context;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync();
        return _currentTransaction;
    }

    public async Task CommitAsync()
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction to commit");
        }

        try
        {
            await _context.SaveChangesAsync();
            await _currentTransaction.CommitAsync();
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally
        {
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync();
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<bool> ExecuteInTransactionAsync(Func<Task> action)
    {
        using var transaction = await BeginTransactionAsync();
        try
        {
            await action();
            await CommitAsync();
            return true;
        }
        catch
        {
            await RollbackAsync();
            return false;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
    {
        using var transaction = await BeginTransactionAsync();
        try
        {
            var result = await action();
            await CommitAsync();
            return result;
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}
