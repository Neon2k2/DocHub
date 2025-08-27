using DocHub.Application.DTOs;

namespace DocHub.Application.Interfaces
{
    public interface ILetterTemplateService
    {
        Task<IEnumerable<LetterTemplateDto>> GetAllAsync();
        Task<LetterTemplateDto?> GetByIdAsync(string id);
        Task<LetterTemplateDto> CreateAsync(LetterTemplateDto template);
        Task<LetterTemplateDto> UpdateAsync(string id, LetterTemplateDto template);
        Task<bool> DeleteAsync(string id);
        Task<IEnumerable<LetterTemplateDto>> GetActiveTemplatesAsync();
        Task<LetterTemplateDto?> GetByNameAsync(string name);
        Task<IEnumerable<LetterTemplateDto>> GetByDataSourceAsync(string dataSource);
        Task<bool> ExistsAsync(string name);
        Task<int> GetNextSortOrderAsync();

        // Template field methods
        Task<IEnumerable<LetterTemplateFieldDto>> GetTemplateFieldsAsync(string templateId);
        Task<LetterTemplateFieldDto> AddTemplateFieldAsync(string templateId, LetterTemplateFieldDto field);
        Task<bool> UpdateTemplateFieldAsync(string templateId, string fieldId, LetterTemplateFieldDto field);
        Task<bool> DeleteTemplateFieldAsync(string templateId, string fieldId);
        Task<bool> ReorderTemplateFieldsAsync(string templateId, List<string> fieldIds);
        // Additional field operations
        Task<LetterTemplateFieldDto?> GetTemplateFieldAsync(string id);
        Task<LetterTemplateFieldDto> CreateTemplateFieldAsync(CreateLetterTemplateFieldDto createDto);
        Task<LetterTemplateFieldDto?> UpdateTemplateFieldAsync(string id, UpdateLetterTemplateFieldDto updateDto);
        Task<bool> DeleteTemplateFieldAsync(string id);
        Task<bool> ReorderTemplateFieldsAsync(List<FieldReorderDto> reorderDtos);
    }
}
