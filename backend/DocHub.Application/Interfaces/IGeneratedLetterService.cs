using DocHub.Core.Entities;
using DocHub.Application.DTOs;

namespace DocHub.Application.Interfaces
{
    public interface IGeneratedLetterService
    {
        Task<IEnumerable<GeneratedLetter>> GetAllAsync();
        Task<GeneratedLetter?> GetByIdAsync(string id);
        Task<GeneratedLetter?> GetByLetterNumberAsync(string letterNumber);
        Task<GeneratedLetter> CreateAsync(GeneratedLetter letter);
        Task<GeneratedLetter> UpdateAsync(string id, GeneratedLetter letter);
        Task<bool> DeleteAsync(string id);
        Task<IEnumerable<GeneratedLetter>> GetByEmployeeAsync(string employeeId);
        Task<IEnumerable<GeneratedLetter>> GetByTemplateAsync(string templateId);
        Task<IEnumerable<GeneratedLetter>> GetByStatusAsync(string status);
        Task<IEnumerable<GeneratedLetter>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<GeneratedLetter> GenerateLetterAsync(GenerateLetterRequest request);
        Task<bool> SendEmailAsync(string letterId, string emailId, List<string> attachmentPaths);
        Task<bool> ResendEmailAsync(string letterId);
        Task<bool> UpdateStatusAsync(string letterId, string status, string? errorMessage = null);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<GeneratedLetter>> GetPagedAsync(int page, int pageSize);
        Task<IEnumerable<GeneratedLetter>> SearchAsync(string searchTerm);
        
        // Additional methods needed by controllers
        Task<GeneratedLetter> GenerateBulkLettersAsync(BulkLetterGenerationRequest request);
        Task<string> SendBulkEmailsAsync(BulkEmailRequest request);
        Task<LetterGenerationStats> GetGenerationStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<bool> UpdateEmailStatusAsync(string letterId, string emailStatus, string? errorMessage = null);
        Task<IEnumerable<GeneratedLetter>> GetLettersByStatusAsync(string status);
        Task<IEnumerable<GeneratedLetter>> GetLettersByEmployeeAsync(string employeeId);
        Task<IEnumerable<GeneratedLetter>> GetLettersByTemplateAsync(string templateId);
        Task<IEnumerable<GeneratedLetter>> GetAllGeneratedLettersAsync();
        Task<GeneratedLetter> GetGeneratedLetterByIdAsync(string id);
        Task<bool> DeleteGeneratedLetterAsync(string id);
        
        // Bulk operations methods
        Task<BulkLetterGenerationResult> GenerateBulkLettersAsync(BulkLetterGenerationRequestDto request);
        Task<BulkEmailSendingResult> SendBulkEmailsAsync(BulkEmailSendingRequest request);
        Task<BulkOperationStatusDto?> GetBulkOperationStatusAsync(string operationId);
        Task<bool> CancelBulkOperationAsync(string operationId);
        Task<BulkOperationRetryResult> RetryBulkOperationAsync(string operationId, RetryBulkOperationRequest request);
        Task<IEnumerable<BulkOperationHistoryDto>> GetBulkOperationHistoryAsync(BulkOperationHistoryFilter filter);
        Task<BulkOperationStatsDto> GetBulkOperationStatsAsync();
    }
}
