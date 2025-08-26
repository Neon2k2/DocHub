using DocHub.Application.DTOs;

namespace DocHub.Application.Interfaces;

public interface IDynamicTabService
{
    Task<IEnumerable<DynamicTabDto>> GetAllActiveTabsAsync();
    Task<DynamicTabDto?> GetTabByIdAsync(string id);
    Task<DynamicTabDto?> GetTabByNameAsync(string name);
    Task<DynamicTabDto> CreateTabAsync(CreateDynamicTabDto createDto);
    Task<DynamicTabDto> UpdateTabAsync(string id, UpdateDynamicTabDto updateDto);
    Task<bool> DeleteTabAsync(string id);
    Task<bool> ToggleTabStatusAsync(string id);
    Task<IEnumerable<DynamicTabDto>> GetTabsByDataSourceAsync(string dataSource);
    Task<bool> ReorderTabsAsync(List<TabReorderDto> reorderDtos);
}
