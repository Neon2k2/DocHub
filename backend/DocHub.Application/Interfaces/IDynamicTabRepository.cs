using DocHub.Core.Entities;

namespace DocHub.Application.Interfaces;

public interface IDynamicTabRepository : IGenericRepository<DynamicTab>
{
    Task<IEnumerable<DynamicTab>> GetActiveTabsAsync();
    Task<DynamicTab?> GetTabByNameAsync(string name);
    Task<IEnumerable<DynamicTabField>> GetTabFieldsAsync(string tabId);
    Task<IEnumerable<DynamicTabData>> GetTabDataAsync(string tabId);
    Task<bool> TabNameExistsAsync(string name);
    Task<int> GetNextSortOrderAsync();
    Task ReorderTabsAsync(IEnumerable<(string Id, int NewOrder)> tabOrders);
}
