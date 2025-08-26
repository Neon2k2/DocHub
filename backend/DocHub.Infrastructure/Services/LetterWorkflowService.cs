using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using DocHub.Application.DTOs;
using Microsoft.Extensions.Logging;
using DocHub.Infrastructure.Services.Email;

namespace DocHub.Infrastructure.Services;

public class LetterWorkflowService : ILetterWorkflowService
{
    private readonly IGeneratedLetterService _letterService;
    private readonly ILetterStatusService _statusService;
    private readonly IEmailService _emailService;
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<LetterWorkflowService> _logger;

    public LetterWorkflowService(
        IGeneratedLetterService letterService,
        ILetterStatusService statusService,
        IEmailService emailService,
        IEmployeeService employeeService,
        ILogger<LetterWorkflowService> logger)
    {
        _letterService = letterService;
        _statusService = statusService;
        _emailService = emailService;
        _employeeService = employeeService;
        _logger = logger;
    }

    public async Task<LetterWorkflowResult> ProcessLetterWorkflowAsync(LetterWorkflowRequest request)
    {
        var result = new LetterWorkflowResult
        {
            LetterId = request.LetterId,
            Success = false,
            StartedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting letter workflow for letter {LetterId}", request.LetterId);

            // Step 1: Update status to "Processing"
            await _statusService.UpdateLetterStatusAsync(request.LetterId, "Processing", "Workflow started");

            // Step 2: Generate letter if not already generated
            if (request.GenerateLetter)
            {
                var generationResult = await GenerateLetterAsync(request);
                if (!generationResult.Success)
                {
                    result.ErrorMessage = $"Letter generation failed: {generationResult.ErrorMessage}";
                    await _statusService.MarkLetterAsFailedAsync(request.LetterId, result.ErrorMessage);
                    return result;
                }
            }

            // Step 3: Send email if requested
            if (request.SendEmail)
            {
                var emailResult = await SendLetterEmailAsync(request);
                if (!emailResult.Success)
                {
                    result.ErrorMessage = $"Email sending failed: {emailResult.ErrorMessage}";
                    await _statusService.MarkLetterAsFailedAsync(request.LetterId, result.ErrorMessage);
                    return result;
                }

                result.EmailSent = true;
                result.EmailId = emailResult.EmailId;
            }

            // Step 4: Update status to "Completed"
            await _statusService.UpdateLetterStatusAsync(request.LetterId, "Completed", "Workflow completed successfully");

            result.Success = true;
            result.CompletedAt = DateTime.UtcNow;
            result.WorkflowDuration = result.CompletedAt.Value - result.StartedAt;

            _logger.LogInformation("Letter workflow completed successfully for {LetterId} in {Duration}", 
                request.LetterId, result.WorkflowDuration);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in letter workflow for {LetterId}", request.LetterId);
            result.ErrorMessage = ex.Message;
            await _statusService.MarkLetterAsFailedAsync(request.LetterId, ex.Message);
            return result;
        }
    }

    public async Task<BulkWorkflowResult> ProcessBulkWorkflowAsync(BulkWorkflowRequest request)
    {
        var result = new BulkWorkflowResult
        {
            OperationId = Guid.NewGuid().ToString(),
            Success = false,
            StartedAt = DateTime.UtcNow,
            Results = new List<LetterWorkflowResult>()
        };

        try
        {
            _logger.LogInformation("Starting bulk workflow for {Count} letters", request.LetterIds.Count);

            var tasks = request.LetterIds.Select(letterId => 
                ProcessLetterWorkflowAsync(new LetterWorkflowRequest
                {
                    LetterId = letterId,
                    GenerateLetter = request.GenerateLetter,
                    SendEmail = request.SendEmail,
                    EmailTemplate = request.EmailTemplate
                }));

            var workflowResults = await Task.WhenAll(tasks);
            result.Results.AddRange(workflowResults);

            result.Success = true;
            result.CompletedAt = DateTime.UtcNow;
            result.WorkflowDuration = result.CompletedAt.Value - result.StartedAt;
            result.SuccessfulCount = result.Results.Count(r => r.Success);
            result.FailedCount = result.Results.Count(r => !r.Success);

            _logger.LogInformation("Bulk workflow completed: {Successful}/{Total} successful", 
                result.SuccessfulCount, request.LetterIds.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk workflow");
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public async Task<LetterWorkflowStatus> GetWorkflowStatusAsync(string letterId)
    {
        try
        {
            var letter = await _letterService.GetByIdAsync(letterId);
            if (letter == null)
            {
                return new LetterWorkflowStatus
                {
                    LetterId = letterId,
                    Status = "NotFound",
                    ErrorMessage = "Letter not found"
                };
            }

            var statusHistory = await _statusService.GetLetterStatusHistoryAsync(letterId);
            var currentStatus = letter.Status;

            return new LetterWorkflowStatus
            {
                LetterId = letterId,
                Status = currentStatus,
                CurrentStep = GetCurrentWorkflowStep(currentStatus),
                StatusHistory = statusHistory,
                LastUpdated = letter.UpdatedAt,
                GeneratedAt = letter.GeneratedAt,
                SentAt = letter.SentAt,
                DeliveredAt = letter.DeliveredAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow status for {LetterId}", letterId);
            return new LetterWorkflowStatus
            {
                LetterId = letterId,
                Status = "Error",
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<LetterGenerationResult> GenerateLetterAsync(LetterWorkflowRequest request)
    {
        try
        {
            // This would call the actual letter generation logic
            // For now, return a success result
            return new LetterGenerationResult
            {
                Success = true,
                LetterId = request.LetterId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating letter {LetterId}", request.LetterId);
            return new LetterGenerationResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<EmailSendingResult> SendLetterEmailAsync(LetterWorkflowRequest request)
    {
        try
        {
            // Get letter and employee information
            var letter = await _letterService.GetByIdAsync(request.LetterId);
            if (letter == null)
            {
                return new EmailSendingResult
                {
                    Success = false,
                    ErrorMessage = "Letter not found"
                };
            }

            var employee = await _employeeService.GetByIdAsync(letter.EmployeeId);
            if (employee == null)
            {
                return new EmailSendingResult
                {
                    Success = false,
                    ErrorMessage = "Employee not found"
                };
            }

            // Prepare email content
            var emailRequest = new SendEmailRequest
            {
                GeneratedLetterId = request.LetterId,
                EmployeeId = letter.EmployeeId,
                Subject = request.EmailTemplate?.Subject ?? "Your Letter",
                Body = request.EmailTemplate?.Body ?? "Please find your letter attached.",
                AttachmentPaths = new List<string> { letter.LetterFilePath ?? "" }
            };

            // Send email
            var emailSent = await _emailService.SendEmailAsync(
                employee.Email, 
                emailRequest.Subject, 
                emailRequest.Body, 
                emailRequest.AttachmentPaths);
            
            if (emailSent)
            {
                // Mark letter as sent
                await _statusService.MarkLetterAsSentAsync(request.LetterId, Guid.NewGuid().ToString());
                
                return new EmailSendingResult
                {
                    Success = true,
                    EmailId = Guid.NewGuid().ToString()
                };
            }

            return new EmailSendingResult
            {
                Success = false,
                ErrorMessage = "Email sending failed"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending letter email for {LetterId}", request.LetterId);
            return new EmailSendingResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }



    private string GetCurrentWorkflowStep(string status)
    {
        return status switch
        {
            "Generated" => "Letter Generated",
            "Processing" => "Processing",
            "Sent" => "Email Sent",
            "Delivered" => "Delivered",
            "Failed" => "Failed",
            "Completed" => "Completed",
            _ => "Unknown"
        };
    }
}
