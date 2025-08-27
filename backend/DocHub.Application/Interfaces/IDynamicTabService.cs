using DocHub.Application.DTOs.DynamicTabs;

namespace DocHub.Application.Interfaces;

public interface IDynamicTabService
{
    /// <summary>
    /// Creates a new dynamic tab with specified configuration
    /// </summary>
    Task<DynamicTabDto> CreateTabAsync(CreateDynamicTabDto createDto);

    /// <summary>
    /// Updates an existing dynamic tab
    /// </summary>
    Task<DynamicTabDto> UpdateTabAsync(string id, UpdateDynamicTabDto updateDto);

    /// <summary>
    /// Gets a dynamic tab by its ID
    /// </summary>
    Task<DynamicTabDto?> GetTabByIdAsync(string id);

    /// <summary>
    /// Gets a dynamic tab by its name
    /// </summary>
    Task<DynamicTabDto?> GetTabByNameAsync(string name);

    /// <summary>
    /// Gets all active dynamic tabs
    /// </summary>
    Task<IEnumerable<DynamicTabDto>> GetAllActiveTabsAsync();

    /// <summary>
    /// Deletes a dynamic tab (soft delete)
    /// </summary>
    Task<bool> DeleteTabAsync(string id, string deletedBy);

    /// <summary>
    /// Adds data to a specific tab
    /// </summary>
    Task<DynamicTabDataDto> AddTabDataAsync(string tabId, AddTabDataDto dataDto);

    /// <summary>
    /// Gets data for a specific tab
    /// </summary>
    Task<IEnumerable<DynamicTabDataDto>> GetTabDataAsync(string tabId);

    /// <summary>
    /// Reorders the tabs based on the provided order information
    /// </summary>
    Task<bool> ReorderTabsAsync(IEnumerable<TabReorderDto> reorderDtos);
}
