using DocHub.Core.Entities;
using DocHub.Infrastructure.Data;
using DocHub.Infrastructure.Repositories;
using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DocHub.Infrastructure.Services;

public class LetterTemplateService : ILetterTemplateService
{
    private readonly DocHubDbContext _context;
    private readonly IGenericRepository<LetterTemplate> _repository;
    private readonly ILogger<LetterTemplateService> _logger;

    public LetterTemplateService(DocHubDbContext context, IGenericRepository<LetterTemplate> repository, ILogger<LetterTemplateService> logger)
    {
        _context = context;
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<LetterTemplate>> GetAllAsync()
    {
        return await _context.LetterTemplates
            .Include(t => t.Fields)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<LetterTemplate> GetByIdAsync(string id)
    {
        return await _context.LetterTemplates
            .Include(t => t.Fields)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<LetterTemplate> CreateAsync(LetterTemplate template)
    {
        if (await ExistsAsync(template.Name))
        {
            throw new InvalidOperationException($"Template with name '{template.Name}' already exists.");
        }

        template.Id = Guid.NewGuid().ToString();
        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;

        if (template.SortOrder == 0)
        {
            template.SortOrder = await GetNextSortOrderAsync();
        }

        var result = await _repository.AddAsync(template);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<LetterTemplate> UpdateAsync(string id, LetterTemplate template)
    {
        var existingTemplate = await GetByIdAsync(id);
        if (existingTemplate == null)
        {
            throw new InvalidOperationException($"Template with id '{id}' not found.");
        }

        if (template.Name != existingTemplate.Name && await ExistsAsync(template.Name))
        {
            throw new InvalidOperationException($"Template with name '{template.Name}' already exists.");
        }

        existingTemplate.Name = template.Name;
        existingTemplate.LetterType = template.LetterType;
        existingTemplate.Description = template.Description;
        existingTemplate.TemplateContent = template.TemplateContent;
        existingTemplate.TemplateFilePath = template.TemplateFilePath;
        existingTemplate.DataSource = template.DataSource;
        existingTemplate.DatabaseQuery = template.DatabaseQuery;
        existingTemplate.IsActive = template.IsActive;
        existingTemplate.SortOrder = template.SortOrder;
        existingTemplate.UpdatedAt = DateTime.UtcNow;

        var result = await _repository.UpdateAsync(existingTemplate);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var template = await GetByIdAsync(id);
        if (template == null)
        {
            return false;
        }

        // Check if template has generated letters
        var hasGeneratedLetters = await _context.GeneratedLetters
            .AnyAsync(gl => gl.LetterTemplateId == id);

        if (hasGeneratedLetters)
        {
            throw new InvalidOperationException("Cannot delete template that has generated letters.");
        }

        var result = await _repository.DeleteAsync(id);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<IEnumerable<LetterTemplate>> GetActiveTemplatesAsync()
    {
        return await _context.LetterTemplates
            .Include(t => t.Fields)
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<LetterTemplate> GetByNameAsync(string name)
    {
        return await _context.LetterTemplates
            .Include(t => t.Fields)
            .FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<IEnumerable<LetterTemplate>> GetByDataSourceAsync(string dataSource)
    {
        return await _context.LetterTemplates
            .Include(t => t.Fields)
            .Where(t => t.DataSource == dataSource && t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string name)
    {
        return await _context.LetterTemplates.AnyAsync(t => t.Name == name);
    }

    public async Task<int> GetNextSortOrderAsync()
    {
        var maxSortOrder = await _context.LetterTemplates
            .MaxAsync(t => (int?)t.SortOrder) ?? 0;
        return maxSortOrder + 1;
    }

    // Template field methods
    public async Task<IEnumerable<LetterTemplateFieldDto>> GetTemplateFieldsAsync(string templateId)
    {
        try
        {
            var template = await _context.LetterTemplates
                .Include(t => t.Fields)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template?.Fields == null)
                return Enumerable.Empty<LetterTemplateFieldDto>();

            return template.Fields
                .OrderBy(f => f.SortOrder)
                .Select(f => new LetterTemplateFieldDto
                {
                    Id = f.Id.ToString(),
                    FieldName = f.FieldName,
                    DisplayName = f.DisplayName,
                    DataType = f.DataType,
                    IsRequired = f.IsRequired,
                    ValidationRules = f.ValidationRules,
                    SortOrder = f.SortOrder,
                    LetterTemplateId = f.LetterTemplateId.ToString()
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template fields for template {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<LetterTemplateFieldDto?> GetTemplateFieldAsync(string id)
    {
        try
        {
            var field = await _context.LetterTemplateFields
                .FirstOrDefaultAsync(f => f.Id.ToString() == id);

            if (field == null)
                return null;

            return new LetterTemplateFieldDto
            {
                Id = field.Id.ToString(),
                FieldName = field.FieldName,
                DisplayName = field.DisplayName,
                DataType = field.DataType,
                IsRequired = field.IsRequired,
                ValidationRules = field.ValidationRules,
                SortOrder = field.SortOrder,
                LetterTemplateId = field.LetterTemplateId.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template field {Id}", id);
            throw;
        }
    }

    public async Task<LetterTemplateFieldDto> CreateTemplateFieldAsync(CreateLetterTemplateFieldDto createDto)
    {
        try
        {
            var field = new LetterTemplateField
            {
                FieldName = createDto.FieldName,
                DisplayName = createDto.DisplayName,
                DataType = createDto.DataType,
                IsRequired = createDto.IsRequired,
                ValidationRules = createDto.ValidationRules,
                SortOrder = createDto.SortOrder,
                LetterTemplateId = createDto.LetterTemplateId
            };

            _context.LetterTemplateFields.Add(field);
            await _context.SaveChangesAsync();

            return new LetterTemplateFieldDto
            {
                Id = field.Id.ToString(),
                FieldName = field.FieldName,
                DisplayName = field.DisplayName,
                DataType = field.DataType,
                IsRequired = field.IsRequired,
                ValidationRules = field.ValidationRules,
                SortOrder = field.SortOrder,
                LetterTemplateId = field.LetterTemplateId.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template field");
            throw;
        }
    }

    public async Task<LetterTemplateFieldDto?> UpdateTemplateFieldAsync(string id, UpdateLetterTemplateFieldDto updateDto)
    {
        try
        {
            var field = await _context.LetterTemplateFields
                .FirstOrDefaultAsync(f => f.Id.ToString() == id);

            if (field == null)
                return null;

            if (!string.IsNullOrWhiteSpace(updateDto.FieldName))
                field.FieldName = updateDto.FieldName;
            
            if (!string.IsNullOrWhiteSpace(updateDto.DisplayName))
                field.DisplayName = updateDto.DisplayName;
            
            if (!string.IsNullOrWhiteSpace(updateDto.DataType))
                field.DataType = updateDto.DataType;
            
            if (updateDto.IsRequired.HasValue)
                field.IsRequired = updateDto.IsRequired.Value;
            
            if (!string.IsNullOrWhiteSpace(updateDto.ValidationRules))
                field.ValidationRules = updateDto.ValidationRules;
            
            if (updateDto.SortOrder.HasValue)
                field.SortOrder = updateDto.SortOrder.Value;

            await _context.SaveChangesAsync();

            return new LetterTemplateFieldDto
            {
                Id = field.Id.ToString(),
                FieldName = field.FieldName,
                DisplayName = field.DisplayName,
                DataType = field.DataType,
                IsRequired = field.IsRequired,
                ValidationRules = field.ValidationRules,
                SortOrder = field.SortOrder,
                LetterTemplateId = field.LetterTemplateId.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template field {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteTemplateFieldAsync(string id)
    {
        try
        {
            var field = await _context.LetterTemplateFields
                .FirstOrDefaultAsync(f => f.Id.ToString() == id);

            if (field == null)
                return false;

            _context.LetterTemplateFields.Remove(field);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template field {Id}", id);
            throw;
        }
    }

    public async Task<bool> ReorderTemplateFieldsAsync(List<FieldReorderDto> reorderDtos)
    {
        try
        {
            foreach (var reorderDto in reorderDtos)
            {
                var field = await _context.LetterTemplateFields
                    .FirstOrDefaultAsync(f => f.Id.ToString() == reorderDto.Id);

                if (field != null)
                {
                    field.SortOrder = reorderDto.NewSortOrder;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering template fields");
            throw;
        }
    }
}
