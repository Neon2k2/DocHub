using DocHub.Application.DTOs;
using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using DocHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace DocHub.Infrastructure.Services;

public class DynamicTabService : IDynamicTabService
{
    private readonly DocHubDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<DynamicTabService> _logger;

    public DynamicTabService(DocHubDbContext context, IMapper mapper, ILogger<DynamicTabService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<DynamicTabDto>> GetAllActiveTabsAsync()
    {
        try
        {
            var tabs = await _context.DynamicTabs
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DynamicTabDto>>(tabs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all active tabs");
            throw;
        }
    }

    public async Task<DynamicTabDto?> GetTabByIdAsync(string id)
    {
        try
        {
            var tab = await _context.DynamicTabs.FindAsync(id);
            return _mapper.Map<DynamicTabDto>(tab);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tab by id: {Id}", id);
            throw;
        }
    }

    public async Task<DynamicTabDto?> GetTabByNameAsync(string name)
    {
        try
        {
            var tab = await _context.DynamicTabs
                .FirstOrDefaultAsync(t => t.Name == name && t.IsActive);
            return _mapper.Map<DynamicTabDto>(tab);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tab by name: {Name}", name);
            throw;
        }
    }

    public async Task<DynamicTabDto> CreateTabAsync(CreateDynamicTabDto createDto)
    {
        try
        {
            // Check if tab with same name already exists
            var existingTab = await _context.DynamicTabs
                .FirstOrDefaultAsync(t => t.Name == createDto.Name);

            if (existingTab != null)
                throw new InvalidOperationException($"Tab with name '{createDto.Name}' already exists");

            var dynamicTab = new DynamicTab
            {
                Name = createDto.Name,
                DisplayName = createDto.DisplayName,
                Description = createDto.Description,
                Icon = createDto.Icon,
                Color = createDto.Color,
                DataSource = createDto.DataSource,
                DatabaseQuery = createDto.DatabaseQuery,
                IsActive = true,
                SortOrder = await GetNextSortOrderAsync(),
                IsAdminOnly = createDto.IsAdminOnly,
                RequiredPermission = createDto.RequiredPermission
            };

            _context.DynamicTabs.Add(dynamicTab);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Dynamic tab created successfully: {Id}", dynamicTab.Id);
            return _mapper.Map<DynamicTabDto>(dynamicTab);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dynamic tab: {Name}", createDto.Name);
            throw;
        }
    }

    public async Task<DynamicTabDto> UpdateTabAsync(string id, UpdateDynamicTabDto updateDto)
    {
        try
        {
            var dynamicTab = await _context.DynamicTabs.FindAsync(id);
            if (dynamicTab == null)
                throw new ArgumentException($"Dynamic tab with ID {id} not found");

            // Update properties
            dynamicTab.DisplayName = updateDto.DisplayName;
            dynamicTab.Description = updateDto.Description;
            dynamicTab.Icon = updateDto.Icon;
            dynamicTab.Color = updateDto.Color;
            dynamicTab.DataSource = updateDto.DataSource;
            dynamicTab.DatabaseQuery = updateDto.DatabaseQuery;
            dynamicTab.IsActive = updateDto.IsActive;
            dynamicTab.SortOrder = updateDto.SortOrder;
            dynamicTab.IsAdminOnly = updateDto.IsAdminOnly;
            dynamicTab.RequiredPermission = updateDto.RequiredPermission;
            dynamicTab.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Dynamic tab updated successfully: {Id}", id);

            return _mapper.Map<DynamicTabDto>(dynamicTab);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating dynamic tab: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteTabAsync(string id)
    {
        try
        {
            var dynamicTab = await _context.DynamicTabs.FindAsync(id);
            if (dynamicTab == null)
                throw new ArgumentException($"Dynamic tab with ID {id} not found");

            // Check if tab is being used by any templates
            var isUsed = await _context.LetterTemplates.AnyAsync(t => t.LetterType == dynamicTab.Name);
            if (isUsed)
            {
                throw new InvalidOperationException($"Cannot delete tab '{dynamicTab.Name}' as it is being used by letter templates");
            }

            _context.DynamicTabs.Remove(dynamicTab);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Dynamic tab deleted successfully: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting dynamic tab: {Id}", id);
            throw;
        }
    }

    public async Task<bool> ToggleTabStatusAsync(string id)
    {
        try
        {
            var dynamicTab = await _context.DynamicTabs.FindAsync(id);
            if (dynamicTab == null)
                throw new ArgumentException($"Dynamic tab with ID {id} not found");

            dynamicTab.IsActive = !dynamicTab.IsActive;
            dynamicTab.UpdatedAt = DateTime.UtcNow;
            dynamicTab.UpdatedBy = "System";

            await _context.SaveChangesAsync();
            _logger.LogInformation("Dynamic tab {Id} active status toggled to: {IsActive}", id, dynamicTab.IsActive);

            return dynamicTab.IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling dynamic tab active status: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<DynamicTabDto>> GetTabsByDataSourceAsync(string dataSource)
    {
        try
        {
            var tabs = await _context.DynamicTabs
                .Where(t => t.DataSource == dataSource && t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DynamicTabDto>>(tabs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tabs by data source: {DataSource}", dataSource);
            throw;
        }
    }

    public async Task<bool> ReorderTabsAsync(List<TabReorderDto> reorderDtos)
    {
        try
        {
            foreach (var reorderDto in reorderDtos)
            {
                var tab = await _context.DynamicTabs.FindAsync(reorderDto.Id);
                if (tab != null)
                {
                    tab.SortOrder = reorderDto.NewSortOrder;
                    tab.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reordered {Count} tabs", reorderDtos.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering tabs");
            throw;
        }
    }

    private async Task<int> GetNextSortOrderAsync()
    {
        var maxSortOrder = await _context.DynamicTabs
            .Where(t => t.IsActive)
            .MaxAsync(t => (int?)t.SortOrder) ?? 0;

        return maxSortOrder + 1;
    }
}
