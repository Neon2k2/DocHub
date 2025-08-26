using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using DocHub.Core.Entities;
using DocHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace DocHub.Infrastructure.Services;

public class EmailHistoryService : IEmailHistoryService
{
    private readonly DocHubDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<EmailHistoryService> _logger;
    private readonly IEmailService _emailService; // Added this field

    public EmailHistoryService(DocHubDbContext context, IMapper mapper, ILogger<EmailHistoryService> logger, IEmailService emailService) // Added emailService to constructor
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _emailService = emailService; // Initialize the new field
    }

    public async Task<EmailHistoryDto> CreateEmailHistoryAsync(CreateEmailHistoryRequest request)
    {
        try
        {
            var emailHistory = new EmailHistory
            {
                Subject = request.Subject,
                ToEmail = request.ToEmail,
                CcEmail = request.CcEmail,
                BccEmail = request.BccEmail,
                Body = request.Body,
                GeneratedLetterId = request.GeneratedLetterId,
                EmployeeId = request.EmployeeId,
                EmailProvider = request.EmailProvider,
                Status = "Pending",
                SentAt = DateTime.UtcNow,
                CreatedBy = "System" // This will be set by the BaseEntity
            };

            _context.EmailHistories.Add(emailHistory);
            await _context.SaveChangesAsync();

            // Add attachments if any
            if (request.Attachments?.Any() == true)
            {
                foreach (var attachmentRequest in request.Attachments)
                {
                    var attachment = new DocHub.Core.Entities.EmailAttachment
                    {
                        FileName = attachmentRequest.FileName,
                        FileType = attachmentRequest.FileType,
                        FilePath = attachmentRequest.FilePath,
                        FileSize = attachmentRequest.FileSize
                    };
                    
                    emailHistory.Attachments.Add(attachment);
                }
                await _context.SaveChangesAsync();
            }

            return await GetEmailHistoryByIdAsync(emailHistory.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email history");
            throw;
        }
    }

    public async Task<EmailHistoryDto> GetEmailHistoryByIdAsync(string id)
    {
        try
        {
            var emailHistory = await _context.EmailHistories
                .Include(eh => eh.Attachments)
                .Include(eh => eh.Employee)
                .Include(eh => eh.GeneratedLetter)
                .FirstOrDefaultAsync(eh => eh.Id == id);

            if (emailHistory == null)
                return null;

            var dto = _mapper.Map<EmailHistoryDto>(emailHistory);
            
            // Populate additional fields
            if (emailHistory.Employee != null)
            {
                dto.EmployeeName = $"{emailHistory.Employee.FirstName} {emailHistory.Employee.LastName}".Trim();
            }
            
            if (emailHistory.GeneratedLetter != null)
            {
                dto.LetterType = emailHistory.GeneratedLetter.LetterType;
            }

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email history by ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<EmailHistoryDto>> GetEmailHistoryByEmployeeAsync(string employeeId)
    {
        try
        {
            var emailHistories = await _context.EmailHistories
                .Include(eh => eh.Attachments)
                .Include(eh => eh.Employee)
                .Include(eh => eh.GeneratedLetter)
                .Where(eh => eh.EmployeeId == employeeId)
                .OrderByDescending(eh => eh.CreatedAt)
                .ToListAsync();

            var dtos = new List<EmailHistoryDto>();
            foreach (var emailHistory in emailHistories)
            {
                var dto = _mapper.Map<EmailHistoryDto>(emailHistory);
                
                if (emailHistory.Employee != null)
                {
                    dto.EmployeeName = $"{emailHistory.Employee.FirstName} {emailHistory.Employee.LastName}".Trim();
                }
                
                if (emailHistory.GeneratedLetter != null)
                {
                    dto.LetterType = emailHistory.GeneratedLetter.LetterType;
                }
                
                dtos.Add(dto);
            }

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email history for employee: {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<IEnumerable<EmailHistoryDto>> GetEmailHistoryByStatusAsync(string status)
    {
        try
        {
            var emailHistories = await _context.EmailHistories
                .Include(eh => eh.Attachments)
                .Include(eh => eh.Employee)
                .Include(eh => eh.GeneratedLetter)
                .Where(eh => eh.Status == status)
                .OrderByDescending(eh => eh.CreatedAt)
                .ToListAsync();

            var dtos = new List<EmailHistoryDto>();
            foreach (var emailHistory in emailHistories)
            {
                var dto = _mapper.Map<EmailHistoryDto>(emailHistory);
                
                if (emailHistory.Employee != null)
                {
                    dto.EmployeeName = $"{emailHistory.Employee.FirstName} {emailHistory.Employee.LastName}".Trim();
                }
                
                if (emailHistory.GeneratedLetter != null)
                {
                    dto.LetterType = emailHistory.GeneratedLetter.LetterType;
                }
                
                dtos.Add(dto);
            }

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email history by status: {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<EmailHistoryDto>> GetEmailHistoryByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var emailHistories = await _context.EmailHistories
                .Include(eh => eh.Attachments)
                .Include(eh => eh.Employee)
                .Include(eh => eh.GeneratedLetter)
                .Where(eh => eh.CreatedAt >= startDate && eh.CreatedAt <= endDate)
                .OrderByDescending(eh => eh.CreatedAt)
                .ToListAsync();

            var dtos = new List<EmailHistoryDto>();
            foreach (var emailHistory in emailHistories)
            {
                var dto = _mapper.Map<EmailHistoryDto>(emailHistory);
                
                if (emailHistory.Employee != null)
                {
                    dto.EmployeeName = $"{emailHistory.Employee.FirstName} {emailHistory.Employee.LastName}".Trim();
                }
                
                if (emailHistory.GeneratedLetter != null)
                {
                    dto.LetterType = emailHistory.GeneratedLetter.LetterType;
                }
                
                dtos.Add(dto);
            }

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email history by date range: {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }

    public async Task<EmailHistoryDto> UpdateEmailStatusAsync(string id, string status, string? errorMessage = null)
    {
        try
        {
            var emailHistory = await _context.EmailHistories.FindAsync(id);
            if (emailHistory == null)
                throw new ArgumentException($"Email history with ID {id} not found");

            emailHistory.Status = status;
            emailHistory.UpdatedAt = DateTime.UtcNow;

            switch (status.ToLower())
            {
                case "sent":
                    emailHistory.SentAt = DateTime.UtcNow;
                    break;
                case "delivered":
                    emailHistory.DeliveredAt = DateTime.UtcNow;
                    break;
                case "failed":
                    emailHistory.ErrorMessage = errorMessage;
                    break;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Email status updated to {Status} for {Id}", status, id);

            return _mapper.Map<EmailHistoryDto>(emailHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email status for {EmailId}", id);
            throw;
        }
    }

    public async Task<bool> ResendEmailAsync(string emailHistoryId, ResendEmailRequest request)
    {
        try
        {
            var emailHistory = await _context.EmailHistories
                .Include(eh => eh.Attachments)
                .FirstOrDefaultAsync(eh => eh.Id == emailHistoryId);

            if (emailHistory == null)
                throw new ArgumentException($"Email history with ID {emailHistoryId} not found");

            // Prepare email content
            var subject = request.Subject ?? emailHistory.Subject;
            var body = request.Body ?? emailHistory.Body;
            var toEmail = emailHistory.ToEmail;

            // Get attachment paths
            var attachmentPaths = emailHistory.Attachments
                .Select(a => a.FilePath)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            // Add additional attachments if provided
            if (request.AdditionalAttachments != null && request.AdditionalAttachments.Any())
            {
                attachmentPaths.AddRange(request.AdditionalAttachments.Select(a => a.FilePath));
            }

            // Send email
            var emailSent = await _emailService.SendEmailAsync(
                toEmail,
                subject,
                body,
                attachmentPaths
            );

            if (emailSent)
            {
                // Update status
                emailHistory.Status = "Resent";
                emailHistory.LastRetryAt = DateTime.UtcNow;
                emailHistory.RetryCount = emailHistory.RetryCount + 1;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Email resent successfully: {Id}", emailHistory.Id);
            }
            else
            {
                // Update status to failed
                emailHistory.Status = "Failed";
                emailHistory.ErrorMessage = "Email service failed to resend email";

                await _context.SaveChangesAsync();
                _logger.LogWarning("Email resend failed: {Id}", emailHistory.Id);
            }

            return emailSent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email {EmailId}", emailHistoryId);
            throw;
        }
    }

    public async Task<IEnumerable<EmailHistoryDto>> GetFailedEmailsAsync()
    {
        try
        {
            var failedEmails = await _context.EmailHistories
                .Include(eh => eh.Attachments)
                .Include(eh => eh.Employee)
                .Include(eh => eh.GeneratedLetter)
                .Where(eh => eh.Status == "Failed")
                .OrderByDescending(eh => eh.FailedAt)
                .ToListAsync();

            var dtos = new List<EmailHistoryDto>();
            foreach (var emailHistory in failedEmails)
            {
                var dto = _mapper.Map<EmailHistoryDto>(emailHistory);
                
                if (emailHistory.Employee != null)
                {
                    dto.EmployeeName = $"{emailHistory.Employee.FirstName} {emailHistory.Employee.LastName}".Trim();
                }
                
                if (emailHistory.GeneratedLetter != null)
                {
                    dto.LetterType = emailHistory.GeneratedLetter.LetterType;
                }
                
                dtos.Add(dto);
            }

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting failed emails");
            throw;
        }
    }

    public async Task<bool> RetryFailedEmailAsync(string emailHistoryId)
    {
        try
        {
            var emailHistory = await _context.EmailHistories.FindAsync(emailHistoryId);
            if (emailHistory == null || emailHistory.Status != "Failed")
                return false;

            emailHistory.Status = "Pending";
            emailHistory.RetryCount++;
            emailHistory.LastRetryAt = DateTime.UtcNow;
            emailHistory.UpdatedAt = DateTime.UtcNow;
            emailHistory.UpdatedBy = "System"; // TODO: Get from current user context

            await _context.SaveChangesAsync();
            _logger.LogInformation("Failed email retry initiated for ID: {Id}", emailHistoryId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying failed email for ID: {Id}", emailHistoryId);
            return false;
        }
    }

    public async Task<EmailHistoryDto> GetEmailHistoryByEmailIdAsync(string emailId)
    {
        try
        {
            var emailHistory = await _context.EmailHistories
                .Include(eh => eh.Attachments)
                .Include(eh => eh.Employee)
                .Include(eh => eh.GeneratedLetter)
                .FirstOrDefaultAsync(eh => eh.EmailId == emailId);

            if (emailHistory == null)
                return null;

            var dto = _mapper.Map<EmailHistoryDto>(emailHistory);
            
            if (emailHistory.Employee != null)
            {
                dto.EmployeeName = $"{emailHistory.Employee.FirstName} {emailHistory.Employee.LastName}".Trim();
            }
            
            if (emailHistory.GeneratedLetter != null)
            {
                dto.LetterType = emailHistory.GeneratedLetter.LetterType;
            }

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email history by email ID: {EmailId}", emailId);
            throw;
        }
    }

    public async Task<IEnumerable<EmailHistoryDto>> SearchEmailHistoryAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<EmailHistoryDto>();

            var emailHistories = await _context.EmailHistories
                .Include(eh => eh.Attachments)
                .Include(eh => eh.Employee)
                .Include(eh => eh.GeneratedLetter)
                .Where(eh => 
                    eh.Subject.Contains(searchTerm) ||
                    eh.ToEmail.Contains(searchTerm) ||
                    eh.Body.Contains(searchTerm) ||
                    (eh.Employee != null && 
                     (eh.Employee.FirstName.Contains(searchTerm) || 
                      eh.Employee.LastName.Contains(searchTerm) || 
                      eh.Employee.EmployeeId.Contains(searchTerm)))
                )
                .OrderByDescending(eh => eh.CreatedAt)
                .ToListAsync();

            var dtos = new List<EmailHistoryDto>();
            foreach (var emailHistory in emailHistories)
            {
                var dto = _mapper.Map<EmailHistoryDto>(emailHistory);
                
                if (emailHistory.Employee != null)
                {
                    dto.EmployeeName = $"{emailHistory.Employee.FirstName} {emailHistory.Employee.LastName}".Trim();
                }
                
                if (emailHistory.GeneratedLetter != null)
                {
                    dto.LetterType = emailHistory.GeneratedLetter.LetterType;
                }
                
                dtos.Add(dto);
            }

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching email history with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<EmailHistoryDto> AddAttachmentAsync(string emailHistoryId, AddEmailAttachmentRequest request)
    {
        try
        {
            var emailHistory = await _context.EmailHistories.FindAsync(emailHistoryId);
            if (emailHistory == null)
                throw new ArgumentException($"Email history with ID {emailHistoryId} not found");

            var attachment = new DocHub.Core.Entities.EmailAttachment
            {
                FileName = request.FileName,
                FileType = request.FileType,
                FilePath = request.FilePath,
                FileSize = request.FileSize,
                EmailHistoryId = emailHistoryId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System" // TODO: Get from current user context
            };

            _context.EmailAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Attachment added to email history {Id}: {FileName}", emailHistoryId, request.FileName);
            return await GetEmailHistoryByIdAsync(emailHistoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding attachment to email history {Id}", emailHistoryId);
            throw;
        }
    }

    public async Task<bool> RemoveAttachmentAsync(string emailHistoryId, string attachmentId)
    {
        try
        {
            var attachment = await _context.EmailAttachments
                .FirstOrDefaultAsync(ea => ea.Id == attachmentId && ea.EmailHistoryId == emailHistoryId);

            if (attachment == null)
                return false;

            _context.EmailAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Attachment {AttachmentId} removed from email history {Id}", attachmentId, emailHistoryId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing attachment {AttachmentId} from email history {Id}", attachmentId, emailHistoryId);
            return false;
        }
    }
}
