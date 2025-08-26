using DocHub.Application.DTOs;

namespace DocHub.Application.Interfaces;

public interface IEmailHistoryService
{
    Task<EmailHistoryDto> CreateEmailHistoryAsync(CreateEmailHistoryRequest request);
    Task<EmailHistoryDto?> GetEmailHistoryByIdAsync(string id);
    Task<IEnumerable<EmailHistoryDto>> GetEmailHistoryByEmployeeAsync(string employeeId);
    Task<IEnumerable<EmailHistoryDto>> GetEmailHistoryByStatusAsync(string status);
    Task<IEnumerable<EmailHistoryDto>> GetEmailHistoryByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<EmailHistoryDto?> UpdateEmailStatusAsync(string id, string status, string? errorMessage = null);
    Task<bool> ResendEmailAsync(string emailHistoryId, ResendEmailRequest request);
    Task<IEnumerable<EmailHistoryDto>> GetFailedEmailsAsync();
    Task<bool> RetryFailedEmailAsync(string emailHistoryId);
    Task<EmailHistoryDto> GetEmailHistoryByEmailIdAsync(string emailId);
    Task<IEnumerable<EmailHistoryDto>> SearchEmailHistoryAsync(string searchTerm);
    Task<EmailHistoryDto> AddAttachmentAsync(string emailHistoryId, AddEmailAttachmentRequest request);
    Task<bool> RemoveAttachmentAsync(string emailHistoryId, string attachmentId);
}
