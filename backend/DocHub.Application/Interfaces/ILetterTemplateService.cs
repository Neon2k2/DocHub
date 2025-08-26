using DocHub.Core.Entities;
using DocHub.Application.DTOs;

namespace DocHub.Application.Interfaces;

public interface ILetterTemplateService
{
    Task<IEnumerable<LetterTemplate>> GetAllAsync();
    Task<LetterTemplate> GetByIdAsync(string id);
    Task<LetterTemplate> CreateAsync(LetterTemplate template);
    Task<LetterTemplate> UpdateAsync(string id, LetterTemplate template);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<LetterTemplate>> GetActiveTemplatesAsync();
    Task<LetterTemplate> GetByNameAsync(string name);
    Task<IEnumerable<LetterTemplate>> GetByDataSourceAsync(string dataSource);
            Task<bool> ExistsAsync(string name);
        Task<int> GetNextSortOrderAsync();
        
        // Template field methods
        Task<IEnumerable<LetterTemplateFieldDto>> GetTemplateFieldsAsync(string templateId);
        Task<LetterTemplateFieldDto?> GetTemplateFieldAsync(string id);
        Task<LetterTemplateFieldDto> CreateTemplateFieldAsync(CreateLetterTemplateFieldDto createDto);
        Task<LetterTemplateFieldDto?> UpdateTemplateFieldAsync(string id, UpdateLetterTemplateFieldDto updateDto);
        Task<bool> DeleteTemplateFieldAsync(string id);
        Task<bool> ReorderTemplateFieldsAsync(List<FieldReorderDto> reorderDtos);
    }
