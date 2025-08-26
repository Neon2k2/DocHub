using DocHub.Application.DTOs;

namespace DocHub.Application.Interfaces;

public interface ILetterPreviewService
{
    Task<LetterPreviewDto> GeneratePreviewAsync(string letterTemplateId, string employeeId, string? digitalSignatureId = null);
    Task<LetterPreviewDto> GetPreviewAsync(string letterTemplateId, string employeeId);
    Task<LetterPreviewDto> UpdatePreviewAsync(string previewId, UpdatePreviewRequest request);
    Task<bool> DeletePreviewAsync(string previewId);
    Task<IEnumerable<LetterPreviewDto>> GetPreviewsByEmployeeAsync(string employeeId);
    Task<IEnumerable<LetterPreviewDto>> GetPreviewsByLetterTypeAsync(string letterType);
    Task<LetterPreviewDto> GetLatestPreviewAsync(string letterTemplateId, string employeeId);
    Task<bool> RegeneratePreviewWithLatestSignatureAsync(string previewId);
    Task<IEnumerable<LetterPreviewDto>> GetPreviewsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<LetterPreviewDto> ClonePreviewAsync(string previewId, string newEmployeeId);
            Task<bool> BulkGeneratePreviewsAsync(BulkPreviewRequest request);
        Task<LetterPreviewDto> GetPreviewWithAttachmentsAsync(string previewId);
        
        // Bulk preview generation method
        Task<BulkPreviewGenerationResult> GenerateBulkPreviewsAsync(BulkPreviewGenerationRequest request);
    }
