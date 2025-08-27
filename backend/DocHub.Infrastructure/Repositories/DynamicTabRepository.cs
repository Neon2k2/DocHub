using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using DocHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DocHub.Infrastructure.Repositories;

public class DynamicTabRepository : GenericRepository<DynamicTab>, IDynamicTabRepository
{
    private readonly DocHubDbContext _context;

    public DynamicTabRepository(DocHubDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DynamicTab>> GetActiveTabsAsync()
    {
        return await _context.DynamicTabs
            .Include(t => t.Fields)
            .Include(t => t.Template)
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<DynamicTab?> GetTabByNameAsync(string name)
    {
        return await _context.DynamicTabs
            .Include(t => t.Fields)
            .Include(t => t.Template)
            .FirstOrDefaultAsync(t => t.Name == name && t.IsActive);
    }

    public async Task<IEnumerable<DynamicTabField>> GetTabFieldsAsync(string tabId)
    {
        return await _context.DynamicTabFields
            .Where(f => f.DynamicTabId == tabId)
            .OrderBy(f => f.SortOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<DynamicTabData>> GetTabDataAsync(string tabId)
    {
        return await _context.DynamicTabData
            .Where(d => d.DynamicTabId == tabId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> TabNameExistsAsync(string name)
    {
        return await _context.DynamicTabs
            .AnyAsync(t => t.Name == name && t.IsActive);
    }

    public async Task<int> GetNextSortOrderAsync()
    {
        var maxOrder = await _context.DynamicTabs
            .Where(t => t.IsActive)
            .MaxAsync(t => (int?)t.SortOrder);
        return (maxOrder ?? 0) + 1;
    }

    public async Task ReorderTabsAsync(IEnumerable<(string Id, int NewOrder)> tabOrders)
    {
        foreach (var (id, newOrder) in tabOrders)
        {
            var tab = await _context.DynamicTabs.FindAsync(id);
            if (tab != null)
            {
                tab.SortOrder = newOrder;
                tab.UpdatedAt = DateTime.UtcNow;
            }
        }
        await _context.SaveChangesAsync();
    }
}
