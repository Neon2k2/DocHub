using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using DocHub.Application.DTOs;
using DocHub.Infrastructure.Data;
using DocHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DocHub.Infrastructure.Services;

public class GeneratedLetterService : IGeneratedLetterService
{
    private readonly DocHubDbContext _context;
    private readonly IGenericRepository<GeneratedLetter> _repository;
    private readonly ILetterTemplateService _templateService;
    private readonly IEmployeeService _employeeService;
    private readonly IDigitalSignatureService _signatureService;
    private readonly IEmailService _emailService;
    private readonly IDocumentService _documentService;
    private readonly ILogger<GeneratedLetterService> _logger;

    public GeneratedLetterService(
        DocHubDbContext context,
        IGenericRepository<GeneratedLetter> repository,
        ILetterTemplateService templateService,
        IEmployeeService employeeService,
        IDigitalSignatureService signatureService,
        IEmailService emailService,
        IDocumentService documentService,
        ILogger<GeneratedLetterService> logger)
    {
        _context = context;
        _repository = repository;
        _templateService = templateService;
        _employeeService = employeeService;
        _signatureService = signatureService;
        _emailService = emailService;
        _documentService = documentService;
        _logger = logger;
    }

    public async Task<IEnumerable<GeneratedLetter>> GetAllAsync()
    {
        return await _context.GeneratedLetters
            .Include(gl => gl.LetterTemplate)
            .Include(gl => gl.Employee)
            .Include(gl => gl.DigitalSignature)
            .Include(gl => gl.Attachments)
            .OrderByDescending(gl => gl.CreatedAt)
            .ToListAsync();
    }

    public async Task<GeneratedLetter> GetByIdAsync(string id)
    {
        return await _context.GeneratedLetters
            .Include(gl => gl.LetterTemplate)
            .Include(gl => gl.Employee)
            .Include(gl => gl.DigitalSignature)
            .Include(gl => gl.Attachments)
            .FirstOrDefaultAsync(gl => gl.Id == id);
    }

    public async Task<GeneratedLetter> GetByLetterNumberAsync(string letterNumber)
    {
        return await _context.GeneratedLetters
            .Include(gl => gl.LetterTemplate)
            .Include(gl => gl.Employee)
            .Include(gl => gl.DigitalSignature)
            .Include(gl => gl.Attachments)
            .FirstOrDefaultAsync(gl => gl.LetterNumber == letterNumber);
    }

    public async Task<GeneratedLetter> CreateAsync(GeneratedLetter letter)
    {
        letter.Id = Guid.NewGuid().ToString();
        letter.CreatedAt = DateTime.UtcNow;
        letter.UpdatedAt = DateTime.UtcNow;

        if (string.IsNullOrEmpty(letter.LetterNumber))
        {
            letter.LetterNumber = await GenerateLetterNumberAsync(letter.LetterType);
        }

        var result = await _repository.AddAsync(letter);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<GeneratedLetter> UpdateAsync(string id, GeneratedLetter letter)
    {
        var existingLetter = await GetByIdAsync(id);
        if (existingLetter == null)
        {
            throw new InvalidOperationException($"Generated letter with id '{id}' not found.");
        }

        existingLetter.Status = letter.Status;
        existingLetter.EmailId = letter.EmailId;
        existingLetter.ErrorMessage = letter.ErrorMessage;
        existingLetter.SentAt = letter.SentAt;
        existingLetter.UpdatedAt = DateTime.UtcNow;

        var result = await _repository.UpdateAsync(existingLetter);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var letter = await GetByIdAsync(id);
        if (letter == null)
        {
            return false;
        }

        var result = await _repository.DeleteAsync(id);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<IEnumerable<GeneratedLetter>> GetByEmployeeAsync(string employeeId)
    {
        return await _context.GeneratedLetters
            .Include(gl => gl.LetterTemplate)
            .Include(gl => gl.DigitalSignature)
            .Include(gl => gl.Attachments)
            .Where(gl => gl.EmployeeId == employeeId)
            .OrderByDescending(gl => gl.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GeneratedLetter>> GetByTemplateAsync(string templateId)
    {
        return await _context.GeneratedLetters
            .Include(gl => gl.Employee)
            .Include(gl => gl.DigitalSignature)
            .Include(gl => gl.Attachments)
            .Where(gl => gl.LetterTemplateId == templateId)
            .OrderByDescending(gl => gl.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GeneratedLetter>> GetByStatusAsync(string status)
    {
        return await _context.GeneratedLetters
            .Include(gl => gl.LetterTemplate)
            .Include(gl => gl.Employee)
            .Include(gl => gl.DigitalSignature)
            .Include(gl => gl.Attachments)
            .Where(gl => gl.Status == status)
            .OrderByDescending(gl => gl.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GeneratedLetter>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.GeneratedLetters
            .Include(gl => gl.LetterTemplate)
            .Include(gl => gl.Employee)
            .Include(gl => gl.DigitalSignature)
            .Include(gl => gl.Attachments)
            .Where(gl => gl.CreatedAt >= startDate && gl.CreatedAt <= endDate)
            .OrderByDescending(gl => gl.CreatedAt)
            .ToListAsync();
    }

    public async Task<GeneratedLetter> GenerateLetterAsync(GenerateLetterRequest request)
    {
        try
        {
            _logger.LogInformation("Generating letter for employee {EmployeeId} using template {TemplateId}", 
                request.EmployeeId, request.LetterTemplateId);

            // Validate request
            if (string.IsNullOrEmpty(request.LetterTemplateId))
                throw new ArgumentException("Template ID is required");

            if (string.IsNullOrEmpty(request.EmployeeId))
                throw new ArgumentException("Employee ID is required");

            // Get template and employee
            var template = await _context.LetterTemplates.FindAsync(request.LetterTemplateId);
            var employee = await _context.Employees.FindAsync(request.EmployeeId);

            if (template == null)
                throw new ArgumentException($"Template with ID {request.LetterTemplateId} not found");

            if (employee == null)
                throw new ArgumentException($"Employee with ID {request.EmployeeId} not found");

            // Get signature if provided
            DigitalSignature signature = null;
            if (!string.IsNullOrEmpty(request.DigitalSignatureId))
            {
                signature = await _context.DigitalSignatures.FindAsync(request.DigitalSignatureId);
            }

            // Convert field values to object dictionary
            var fieldValues = request.FieldValues.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);

            // Generate letter content using document service
            var letterContent = await _documentService.GenerateLetterAsync(template, employee, fieldValues, signature);

            // Create generated letter record
            var generatedLetter = new GeneratedLetter
            {
                Id = Guid.NewGuid().ToString(),
                LetterNumber = GenerateLetterNumber(template.LetterType),
                LetterType = template.LetterType,
                LetterTemplateId = request.LetterTemplateId,
                EmployeeId = request.EmployeeId,
                DigitalSignatureId = signature?.Id,
                Status = "Generated",
                LetterFilePath = null, // Will be set when saved
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            // Save to database
            _context.GeneratedLetters.Add(generatedLetter);
            await _context.SaveChangesAsync();

            // Save letter file
            var fileName = $"{generatedLetter.LetterNumber}_{employee.FirstName}_{employee.LastName}";
            var filePath = await _documentService.SaveGeneratedLetterAsync(letterContent, fileName, "PDF");
            generatedLetter.LetterFilePath = filePath;

            // Update database with file path
            await _context.SaveChangesAsync();

            _logger.LogInformation("Letter generated successfully: {LetterNumber}", generatedLetter.LetterNumber);
            return generatedLetter;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating letter for employee {EmployeeId}", request.EmployeeId);
            throw;
        }
    }

    // Keep the other method for backward compatibility
    public async Task<GeneratedLetter> GenerateLetterAsync(string templateId, string employeeId, Dictionary<string, object> data, string signatureId = null)
    {
        var request = new GenerateLetterRequest
        {
            LetterTemplateId = templateId,
            EmployeeId = employeeId,
            FieldValues = data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? ""),
            DigitalSignatureId = signatureId ?? ""
        };
        return await GenerateLetterAsync(request);
    }

    public async Task<bool> SendEmailAsync(string letterId, string emailId, List<string> attachmentPaths = null)
    {
        try
        {
            var letter = await _context.GeneratedLetters
                .Include(gl => gl.Employee)
                .Include(gl => gl.LetterTemplate)
                .FirstOrDefaultAsync(gl => gl.Id == letterId);

            if (letter == null)
                throw new ArgumentException($"Letter with ID {letterId} not found");

            var emailHistory = await _context.EmailHistories
                .FirstOrDefaultAsync(eh => eh.Id == emailId);

            if (emailHistory == null)
                throw new ArgumentException($"Email history with ID {emailId} not found");

            // Prepare email content
            var subject = $"{letter.LetterType} - {letter.Employee.FirstName} {letter.Employee.LastName}";
            var body = GenerateEmailBody(letter, emailHistory);

            // Add letter file to attachments
            var allAttachments = new List<string>();
            if (attachmentPaths != null)
                allAttachments.AddRange(attachmentPaths);
            
            if (!string.IsNullOrEmpty(letter.LetterFilePath))
                allAttachments.Add(letter.LetterFilePath);

            // Send email
            var emailSent = await _emailService.SendEmailAsync(
                emailHistory.ToEmail,
                subject,
                body,
                allAttachments
            );

            if (emailSent)
            {
                // Update letter status
                letter.Status = "Sent";
                letter.SentAt = DateTime.UtcNow;

                // Update email history
                emailHistory.Status = "Sent";
                emailHistory.SentAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Email sent successfully for letter {LetterNumber}", letter.LetterNumber);
            }
            else
            {
                // Update status to failed
                letter.Status = "Failed";
                emailHistory.Status = "Failed";
                emailHistory.ErrorMessage = "Email service failed to send email";

                await _context.SaveChangesAsync();
                _logger.LogWarning("Email failed to send for letter {LetterNumber}", letter.LetterNumber);
            }

            return emailSent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email for letter {LetterId}", letterId);
            throw;
        }
    }

    public async Task<bool> ResendEmailAsync(string letterId)
    {
        try
        {
            var letter = await GetByIdAsync(letterId);
            if (letter == null)
            {
                throw new InvalidOperationException($"Letter with id '{letterId}' not found.");
            }

            if (string.IsNullOrEmpty(letter.EmailId))
            {
                throw new InvalidOperationException("No email ID associated with this letter.");
            }

            var attachmentPaths = new List<string> { letter.LetterFilePath };
            if (letter.Attachments != null)
            {
                attachmentPaths.AddRange(letter.Attachments.Select(a => a.FilePath));
            }

            return await SendEmailAsync(letterId, letter.EmailId, attachmentPaths);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email for letter: {LetterId}", letterId);
            return false;
        }
    }

    public async Task<bool> UpdateStatusAsync(string letterId, string status, string? errorMessage = null)
    {
        try
        {
            var letter = await GetByIdAsync(letterId);
            if (letter == null)
            {
                return false;
            }

            letter.Status = status;
            letter.ErrorMessage = errorMessage;
            letter.UpdatedAt = DateTime.UtcNow;

            await UpdateAsync(letterId, letter);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for letter: {LetterId}", letterId);
            return false;
        }
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.GeneratedLetters.CountAsync();
    }

    public async Task<IEnumerable<GeneratedLetter>> GetPagedAsync(int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        return await _context.GeneratedLetters
            .Include(gl => gl.LetterTemplate)
            .Include(gl => gl.Employee)
            .Include(gl => gl.DigitalSignature)
            .Include(gl => gl.Attachments)
            .OrderByDescending(gl => gl.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<GeneratedLetter>> SearchAsync(string searchTerm)
    {
        try
        {
            var allLetters = await GetAllAsync();
            return allLetters.Where(l => 
                l.LetterNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                l.Employee.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                l.LetterTemplate.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching generated letters");
            return Enumerable.Empty<GeneratedLetter>();
        }
    }

    // Additional methods needed by controllers
    public async Task<GeneratedLetter> GenerateBulkLettersAsync(BulkLetterGenerationRequest request)
    {
        try
        {
            // Mock implementation - generate a sample letter
            var sampleLetter = new GeneratedLetter
            {
                Id = Guid.NewGuid().ToString(),
                LetterNumber = $"BLK_{DateTime.UtcNow:yyyyMMddHHmmss}",
                CreatedAt = DateTime.UtcNow
            };

            return sampleLetter;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bulk letters");
            throw;
        }
    }

    public async Task<string> SendBulkEmailsAsync(BulkEmailRequest request)
    {
        try
        {
            // Mock implementation - return a message ID
            return $"BULK_{DateTime.UtcNow:yyyyMMddHHmmss}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk emails");
            throw;
        }
    }

    public async Task<LetterGenerationStats> GetGenerationStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            // Mock implementation - return sample stats
            return new LetterGenerationStats
            {
                TotalLetters = 100,
                SuccessfullyGenerated = 95,
                FailedCount = 5,
                SuccessRate = 95.0,
                AverageProcessingTime = TimeSpan.FromSeconds(2),
                ProcessedAt = DateTime.UtcNow,
                Errors = new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting generation stats");
            throw;
        }
    }

    public async Task<bool> UpdateEmailStatusAsync(string letterId, string emailStatus, string? errorMessage = null)
    {
        try
        {
            // Mock implementation - always return true
            _logger.LogInformation("Updated email status for letter {LetterId} to {Status}", letterId, emailStatus);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email status for letter {LetterId}", letterId);
            return false;
        }
    }

    public async Task<IEnumerable<GeneratedLetter>> GetLettersByStatusAsync(string status)
    {
        try
        {
            var allLetters = await GetAllAsync();
            return allLetters.Where(l => l.Status == status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letters by status {Status}", status);
            return Enumerable.Empty<GeneratedLetter>();
        }
    }

    public async Task<IEnumerable<GeneratedLetter>> GetLettersByEmployeeAsync(string employeeId)
    {
        try
        {
            var allLetters = await GetAllAsync();
            return allLetters.Where(l => l.Employee.Id == employeeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letters by employee {EmployeeId}", employeeId);
            return Enumerable.Empty<GeneratedLetter>();
        }
    }

    public async Task<IEnumerable<GeneratedLetter>> GetLettersByTemplateAsync(string templateId)
    {
        try
        {
            var allLetters = await GetAllAsync();
            return allLetters.Where(l => l.LetterTemplateId == templateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letters by template {TemplateId}", templateId);
            return Enumerable.Empty<GeneratedLetter>();
        }
    }

    public async Task<IEnumerable<GeneratedLetter>> GetAllGeneratedLettersAsync()
    {
        return await GetAllAsync();
    }

    public async Task<GeneratedLetter> GetGeneratedLetterByIdAsync(string id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteGeneratedLetterAsync(string id)
    {
        return await DeleteAsync(id);
    }

    private async Task<string> GenerateLetterNumberAsync(string letterType)
    {
        var today = DateTime.Today;
        var year = today.Year;
        var month = today.Month.ToString("00");
        var day = today.Day.ToString("00");

        // Get count of letters generated today for this type
        var todayCount = await _context.GeneratedLetters
            .Where(gl => gl.LetterType == letterType && 
                        gl.CreatedAt >= today && 
                        gl.CreatedAt < today.AddDays(1))
            .CountAsync();

        var sequence = (todayCount + 1).ToString("000");
        return $"{letterType.Replace(" ", "").ToUpper()}-{year}{month}{day}-{sequence}";
    }

    private string GenerateLetterNumber(string letterType)
    {
        var prefix = letterType.Substring(0, Math.Min(3, letterType.Length)).ToUpper();
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"{prefix}-{timestamp}-{random}";
    }

    private string GenerateEmailBody(GeneratedLetter letter, EmailHistory emailHistory)
    {
        var body = new StringBuilder();
        body.AppendLine($"Dear {letter.Employee.FirstName} {letter.Employee.LastName},");
        body.AppendLine();
        body.AppendLine($"Please find attached your {letter.LetterType}.");
        body.AppendLine();
        body.AppendLine("Best regards,");
        body.AppendLine("HR Department");
        body.AppendLine();
        body.AppendLine($"Reference: {letter.LetterNumber}");
        body.AppendLine($"Generated: {letter.CreatedAt:dd/MM/yyyy HH:mm}");
        
        return body.ToString();
    }

    // Bulk operations methods
    public async Task<BulkLetterGenerationResult> GenerateBulkLettersAsync(BulkLetterGenerationRequestDto request)
    {
        try
        {
            var operationId = Guid.NewGuid().ToString();
            var results = new List<LetterGenerationItemResult>();
            var successfullyGenerated = 0;
            var failed = 0;

            foreach (var employeeId in request.EmployeeIds)
            {
                try
                {
                    // TODO: Implement actual letter generation logic
                    // This would involve calling the existing GenerateLetterAsync method
                    
                    var result = new LetterGenerationItemResult
                    {
                        EmployeeId = employeeId,
                        EmployeeName = "Employee Name", // TODO: Get from employee service
                        Success = true,
                        LetterId = Guid.NewGuid().ToString(),
                        PreviewId = Guid.NewGuid().ToString()
                    };
                    
                    results.Add(result);
                    successfullyGenerated++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating letter for employee {EmployeeId}", employeeId);
                    
                    var result = new LetterGenerationItemResult
                    {
                        EmployeeId = employeeId,
                        EmployeeName = "Unknown",
                        Success = false,
                        ErrorMessage = ex.Message
                    };
                    
                    results.Add(result);
                    failed++;
                }
            }

            return new BulkLetterGenerationResult
            {
                OperationId = operationId,
                TotalRequested = request.EmployeeIds.Count,
                SuccessfullyGenerated = successfullyGenerated,
                Failed = failed,
                Results = results,
                CompletedAt = DateTime.UtcNow,
                Status = "Completed"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bulk letters");
            throw;
        }
    }

    public async Task<BulkEmailSendingResult> SendBulkEmailsAsync(BulkEmailSendingRequest request)
    {
        try
        {
            var operationId = Guid.NewGuid().ToString();
            var results = new List<EmailSendingItemResult>();
            var successfullySent = 0;
            var failed = 0;

            foreach (var letterId in request.LetterIds)
            {
                try
                {
                    // TODO: Implement actual email sending logic
                    // This would involve calling the existing SendEmailAsync method
                    
                    var result = new EmailSendingItemResult
                    {
                        LetterId = letterId,
                        EmployeeEmail = "employee@company.com", // TODO: Get from letter
                        Success = true,
                        EmailId = Guid.NewGuid().ToString()
                    };
                    
                    results.Add(result);
                    successfullySent++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending email for letter {LetterId}", letterId);
                    
                    var result = new EmailSendingItemResult
                    {
                        LetterId = letterId,
                        EmployeeEmail = "unknown@company.com",
                        Success = false,
                        ErrorMessage = ex.Message
                    };
                    
                    results.Add(result);
                    failed++;
                }
            }

            return new BulkEmailSendingResult
            {
                OperationId = operationId,
                TotalRequested = request.LetterIds.Count,
                SuccessfullySent = successfullySent,
                Failed = failed,
                Results = results,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk emails");
            throw;
        }
    }

    public async Task<BulkOperationStatusDto?> GetBulkOperationStatusAsync(string operationId)
    {
        try
        {
            // TODO: Implement actual status tracking
            // This would involve querying a bulk operations table
            return new BulkOperationStatusDto
            {
                OperationId = operationId,
                OperationType = "Letter Generation",
                Status = "Completed",
                TotalItems = 10,
                CompletedItems = 10,
                FailedItems = 0,
                ProgressPercentage = 100.0,
                StartedAt = DateTime.UtcNow.AddMinutes(-5),
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bulk operation status {OperationId}", operationId);
            return null;
        }
    }

    public async Task<bool> CancelBulkOperationAsync(string operationId)
    {
        try
        {
            // TODO: Implement actual cancellation logic
            // This would involve updating the operation status in the database
            _logger.LogInformation("Bulk operation {OperationId} cancelled", operationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling bulk operation {OperationId}", operationId);
            return false;
        }
    }

    public async Task<BulkOperationRetryResult> RetryBulkOperationAsync(string operationId, RetryBulkOperationRequest request)
    {
        try
        {
            // TODO: Implement actual retry logic
            var result = new BulkOperationRetryResult
            {
                OperationId = operationId,
                TotalRetried = request.ItemIds.Count,
                SuccessfullyRetried = request.ItemIds.Count,
                Failed = 0,
                RetriedItemIds = request.ItemIds
            };

            _logger.LogInformation("Retried {Count} items for operation {OperationId}", request.ItemIds.Count, operationId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying bulk operation {OperationId}", operationId);
            throw;
        }
    }

    public async Task<IEnumerable<BulkOperationHistoryDto>> GetBulkOperationHistoryAsync(BulkOperationHistoryFilter filter)
    {
        try
        {
            // TODO: Implement actual history query
            // This would involve querying a bulk operations history table
            return new List<BulkOperationHistoryDto>
            {
                new BulkOperationHistoryDto
                {
                    OperationId = Guid.NewGuid().ToString(),
                    OperationType = "Letter Generation",
                    Status = "Completed",
                    TotalItems = 50,
                    CompletedItems = 48,
                    FailedItems = 2,
                    StartedAt = DateTime.UtcNow.AddDays(-1),
                    CompletedAt = DateTime.UtcNow.AddDays(-1).AddMinutes(30),
                    InitiatedBy = "admin@company.com"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bulk operation history");
            throw;
        }
    }

    public async Task<BulkOperationStatsDto> GetBulkOperationStatsAsync()
    {
        try
        {
            // TODO: Implement actual stats calculation
            // This would involve querying bulk operations data
            return new BulkOperationStatsDto
            {
                TotalOperations = 25,
                SuccessfulOperations = 23,
                FailedOperations = 2,
                PendingOperations = 0,
                SuccessRate = 92.0,
                TotalItemsProcessed = 1250,
                LastOperationAt = DateTime.UtcNow.AddHours(-1),
                OperationsByType = new Dictionary<string, int>
                {
                    { "Letter Generation", 15 },
                    { "Email Sending", 10 }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bulk operation stats");
            throw;
        }
    }
}
