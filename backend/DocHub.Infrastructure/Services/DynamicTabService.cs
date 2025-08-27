using DocHub.Application.DTOs.DynamicTabs;
using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using DocHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using AutoMapper;

namespace DocHub.Infrastructure.Services;

public class DynamicTabService : IDynamicTabService
{
    private readonly IDynamicTabRepository _tabRepository;
    private readonly ILogger<DynamicTabService> _logger;
    private readonly IMapper _mapper;
    private readonly IDocumentProcessingService _documentProcessingService;

    public DynamicTabService(
        IDynamicTabRepository tabRepository,
        ILogger<DynamicTabService> logger,
        IMapper mapper,
        IDocumentProcessingService documentProcessingService)
    {
        _tabRepository = tabRepository;
        _logger = logger;
        _mapper = mapper;
        _documentProcessingService = documentProcessingService;
    }

    public async Task<DynamicTab> CreateDynamicTabAsync(CreateDynamicTabRequest request)
    {
        try
        {
            _logger.LogInformation("Creating dynamic tab: {TabName}", request.Name);

            // Validate the request
            if (string.IsNullOrEmpty(request.Name))
                throw new ArgumentException("Tab name is required");

            // Check if tab with same name already exists
            var existingTab = await _context.DynamicTabs
                .FirstOrDefaultAsync(t => t.Name == request.Name && t.IsActive);

            if (existingTab != null)
                throw new InvalidOperationException($"Tab with name '{request.Name}' already exists");

            // Create the dynamic tab
            var dynamicTab = new DynamicTab
            {
                Name = request.Name,
                DisplayName = request.DisplayName ?? request.Name,
                Description = request.Description,
                TabType = request.TabType,
                DataSource = request.DataSource,
                DatabaseQuery = request.DatabaseQuery,
                ExcelMapping = request.ExcelMapping,
                TemplateId = request.TemplateId,
                FieldMappings = request.FieldMappings,
                SortOrder = request.SortOrder,
                Icon = request.Icon,
                Color = request.Color,
                Permissions = request.Permissions,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            // Add fields if provided
            if (request.Fields != null && request.Fields.Any())
            {
                foreach (var fieldRequest in request.Fields)
                {
                    var field = new DynamicTabField
                    {
                        FieldName = fieldRequest.FieldName,
                        DisplayName = fieldRequest.DisplayName,
                        DataType = fieldRequest.DataType,
                        IsRequired = fieldRequest.IsRequired,
                        IsEditable = fieldRequest.IsEditable,
                        IsVisible = fieldRequest.IsVisible,
                        ValidationRules = fieldRequest.ValidationRules,
                        DefaultValue = fieldRequest.DefaultValue,
                        SortOrder = fieldRequest.SortOrder,
                        ExcelColumnName = fieldRequest.ExcelColumnName,
                        DatabaseColumnName = fieldRequest.DatabaseColumnName
                    };
                    dynamicTab.Fields.Add(field);
                }
            }

            _context.DynamicTabs.Add(dynamicTab);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Dynamic tab created successfully: {TabId}", dynamicTab.Id);
            return dynamicTab;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dynamic tab: {TabName}", request.Name);
            throw;
        }
    }

    public async Task<DynamicTab?> GetDynamicTabAsync(string tabId)
    {
        try
        {
            return await _context.DynamicTabs
                .Include(t => t.Fields.OrderBy(f => f.SortOrder))
                .Include(t => t.Template)
                .FirstOrDefaultAsync(t => t.Id == tabId && t.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dynamic tab: {TabId}", tabId);
            throw;
        }
    }

    public async Task<IEnumerable<DynamicTab>> GetAllDynamicTabsAsync()
    {
        try
        {
            return await _context.DynamicTabs
                .Include(t => t.Fields.OrderBy(f => f.SortOrder))
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all dynamic tabs");
            throw;
        }
    }

    public async Task<DynamicTab> UpdateDynamicTabAsync(string tabId, UpdateDynamicTabRequest request)
    {
        try
        {
            _logger.LogInformation("Updating dynamic tab: {TabId}", tabId);

            var existingTab = await _context.DynamicTabs
                .Include(t => t.Fields)
                .FirstOrDefaultAsync(t => t.Id == tabId && t.IsActive);

            if (existingTab == null)
                throw new InvalidOperationException($"Tab with ID '{tabId}' not found");

            // Update basic properties
            if (!string.IsNullOrEmpty(request.DisplayName))
                existingTab.DisplayName = request.DisplayName;

            if (!string.IsNullOrEmpty(request.Description))
                existingTab.Description = request.Description;

            if (!string.IsNullOrEmpty(request.TabType))
                existingTab.TabType = request.TabType;

            if (!string.IsNullOrEmpty(request.DataSource))
                existingTab.DataSource = request.DataSource;

            if (request.DatabaseQuery != null)
                existingTab.DatabaseQuery = request.DatabaseQuery;

            if (request.ExcelMapping != null)
                existingTab.ExcelMapping = request.ExcelMapping;

            if (request.TemplateId != null)
                existingTab.TemplateId = request.TemplateId;

            if (request.FieldMappings != null)
                existingTab.FieldMappings = request.FieldMappings;

            if (request.SortOrder.HasValue)
                existingTab.SortOrder = request.SortOrder.Value;

            if (!string.IsNullOrEmpty(request.Icon))
                existingTab.Icon = request.Icon ?? existingTab.Icon;

            if (!string.IsNullOrEmpty(request.Color))
                existingTab.Color = request.Color;

            if (!string.IsNullOrEmpty(request.Permissions))
                existingTab.Permissions = request.Permissions;

            existingTab.UpdatedBy = request.UpdatedBy;
            existingTab.UpdatedAt = DateTime.UtcNow;

            // Update fields if provided
            if (request.Fields != null && request.Fields.Any())
            {
                // Remove existing fields
                _context.DynamicTabFields.RemoveRange(existingTab.Fields);

                // Add new fields
                foreach (var fieldRequest in request.Fields)
                {
                    var field = new DynamicTabField
                    {
                        FieldName = fieldRequest.FieldName,
                        DisplayName = fieldRequest.DisplayName,
                        DataType = fieldRequest.DataType,
                        IsRequired = fieldRequest.IsRequired,
                        IsEditable = fieldRequest.IsEditable,
                        IsVisible = fieldRequest.IsVisible,
                        ValidationRules = fieldRequest.ValidationRules,
                        DefaultValue = fieldRequest.DefaultValue,
                        SortOrder = fieldRequest.SortOrder,
                        ExcelColumnName = fieldRequest.ExcelColumnName,
                        DatabaseColumnName = fieldRequest.DatabaseColumnName
                    };
                    existingTab.Fields.Add(field);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Dynamic tab updated successfully: {TabId}", tabId);
            return existingTab;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating dynamic tab: {TabId}", tabId);
            throw;
        }
    }

    public async Task<bool> DeleteDynamicTabAsync(string tabId)
    {
        try
        {
            _logger.LogInformation("Deleting dynamic tab: {TabId}", tabId);

            var existingTab = await _context.DynamicTabs
                .FirstOrDefaultAsync(t => t.Id == tabId && t.IsActive);

            if (existingTab == null)
                return false;

            // Soft delete
            existingTab.IsActive = false;
            existingTab.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Dynamic tab deleted successfully: {TabId}", tabId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting dynamic tab: {TabId}", tabId);
            throw;
        }
    }

    public async Task<IEnumerable<DynamicTabData>> GetTabDataAsync(string tabId, string? dataSource = null)
    {
        try
        {
            var query = _context.DynamicTabData
                .Where(d => d.DynamicTabId == tabId && d.Status == "active");

            if (!string.IsNullOrEmpty(dataSource))
                query = query.Where(d => d.DataSource == dataSource);

            return await query
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tab data: {TabId}", tabId);
            throw;
        }
    }

    public async Task<DynamicTabData> AddTabDataAsync(string tabId, AddTabDataRequest request)
    {
        try
        {
            _logger.LogInformation("Adding data to tab: {TabId}", tabId);

            var tabData = new DynamicTabData
            {
                DynamicTabId = tabId,
                DataSource = request.DataSource,
                ExternalId = request.ExternalId,
                DataContent = JsonSerializer.Serialize(request.Data),
                Status = "active",
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.DynamicTabData.Add(tabData);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tab data added successfully: {DataId}", tabData.Id);
            return tabData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tab data: {TabId}", tabId);
            throw;
        }
    }

    public async Task<bool> ValidateTabConfigurationAsync(string tabId)
    {
        try
        {
            var tab = await GetDynamicTabAsync(tabId);
            if (tab == null)
                return false;

            // Validate tab configuration based on type
            switch (tab.TabType.ToLower())
            {
                case "letter":
                    return await ValidateLetterTabAsync(tab);
                case "database":
                    return await ValidateDatabaseTabAsync(tab);
                case "upload":
                    return await ValidateUploadTabAsync(tab);
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating tab configuration: {TabId}", tabId);
            return false;
        }
    }

    private async Task<bool> ValidateLetterTabAsync(DynamicTab tab)
    {
        if (string.IsNullOrEmpty(tab.TemplateId))
            return false;

        if (!tab.Fields.Any())
            return false;

        // Validate template exists
        var template = await _context.LetterTemplates
            .FirstOrDefaultAsync(t => t.Id == tab.TemplateId);

        return template != null;
    }

    private async Task<bool> ValidateDatabaseTabAsync(DynamicTab tab)
    {
        if (string.IsNullOrEmpty(tab.DatabaseQuery))
            return false;

        if (!tab.Fields.Any())
            return false;

        // Basic SQL validation (could be enhanced)
        var query = tab.DatabaseQuery.Trim().ToUpper();
        return query.StartsWith("SELECT") && !query.Contains("DROP") && !query.Contains("DELETE");
    }

    private async Task<bool> ValidateUploadTabAsync(DynamicTab tab)
    {
        if (string.IsNullOrEmpty(tab.ExcelMapping))
            return false;

        if (!tab.Fields.Any())
            return false;

        try
        {
            // Validate JSON mapping
            JsonSerializer.Deserialize<Dictionary<string, string>>(tab.ExcelMapping);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
