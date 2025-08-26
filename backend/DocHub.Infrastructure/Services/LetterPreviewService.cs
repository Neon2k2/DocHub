using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using DocHub.Core.Entities;
using DocHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace DocHub.Infrastructure.Services;

public class LetterPreviewService : ILetterPreviewService
{
    private readonly DocHubDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<LetterPreviewService> _logger;
    private readonly IEmployeeService _employeeService; // Added IEmployeeService

    public LetterPreviewService(DocHubDbContext context, IMapper mapper, ILogger<LetterPreviewService> logger, IEmployeeService employeeService) // Modified constructor
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _employeeService = employeeService; // Initialize IEmployeeService
    }

    public async Task<LetterPreviewDto> GeneratePreviewAsync(string letterTemplateId, string employeeId, string? digitalSignatureId = null)
    {
        try
        {
            // Get the letter template
            var template = await _context.LetterTemplates
                .Include(lt => lt.Fields)
                .FirstOrDefaultAsync(lt => lt.Id == letterTemplateId && lt.IsActive);

            if (template == null)
                throw new ArgumentException($"Letter template with ID {letterTemplateId} not found or inactive");

            // Get the employee
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
                throw new ArgumentException($"Employee with ID {employeeId} not found");

            // Get the digital signature (use latest if not specified)
            if (string.IsNullOrEmpty(digitalSignatureId))
            {
                var latestSignature = await _context.DigitalSignatures
                    .Where(ds => ds.IsActive)
                    .OrderByDescending(ds => ds.SortOrder)
                    .ThenByDescending(ds => ds.CreatedAt)
                    .FirstOrDefaultAsync();
                
                digitalSignatureId = latestSignature?.Id;
            }

            // Generate preview content (this would typically involve template processing)
            var previewContent = await GeneratePreviewContentAsync(template, employee, digitalSignatureId);

            // Create or update the preview
            var existingPreview = await _context.LetterPreviews
                .FirstOrDefaultAsync(lp => lp.LetterTemplateId == letterTemplateId && lp.EmployeeId == employeeId);

            LetterPreview preview;
            if (existingPreview != null)
            {
                // Update existing preview
                existingPreview.PreviewContent = previewContent;
                existingPreview.DigitalSignatureId = digitalSignatureId;
                existingPreview.LastGeneratedAt = DateTime.UtcNow;
                existingPreview.UpdatedAt = DateTime.UtcNow;
                existingPreview.UpdatedBy = "System"; // TODO: Get from current user context
                preview = existingPreview;
            }
            else
            {
                // Create new preview
                preview = new LetterPreview
                {
                    LetterType = template.LetterType,
                    LetterTemplateId = letterTemplateId,
                    EmployeeId = employeeId,
                    DigitalSignatureId = digitalSignatureId,
                    PreviewContent = previewContent,
                    LastGeneratedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System" // TODO: Get from current user context
                };

                _context.LetterPreviews.Add(preview);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Letter preview generated for template {TemplateId} and employee {EmployeeId}", 
                letterTemplateId, employeeId);

            return await GetPreviewAsync(letterTemplateId, employeeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating preview for template {TemplateId} and employee {EmployeeId}", 
                letterTemplateId, employeeId);
            throw;
        }
    }

    public async Task<LetterPreviewDto> GetPreviewAsync(string letterTemplateId, string employeeId)
    {
        try
        {
            var preview = await _context.LetterPreviews
                .Include(lp => lp.LetterTemplate)
                .Include(lp => lp.Employee)
                .Include(lp => lp.DigitalSignature)
                .Include(lp => lp.Attachments)
                .FirstOrDefaultAsync(lp => lp.LetterTemplateId == letterTemplateId && lp.EmployeeId == employeeId);

            if (preview == null)
                return null;

            var dto = _mapper.Map<LetterPreviewDto>(preview);
            
            // Populate additional fields
            if (preview.Employee != null)
            {
                dto.EmployeeName = $"{preview.Employee.FirstName} {preview.Employee.LastName}".Trim();
            }
            
            if (preview.DigitalSignature != null)
            {
                dto.AuthorityName = preview.DigitalSignature.AuthorityName;
                dto.AuthorityDesignation = preview.DigitalSignature.AuthorityDesignation;
            }

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting preview for template {TemplateId} and employee {EmployeeId}", 
                letterTemplateId, employeeId);
            throw;
        }
    }

    public async Task<LetterPreviewDto> UpdatePreviewAsync(string previewId, UpdatePreviewRequest request)
    {
        try
        {
            var preview = await _context.LetterPreviews.FindAsync(previewId);
            if (preview == null)
                throw new ArgumentException($"Letter preview with ID {previewId} not found");

            if (request.PreviewContent != null)
                preview.PreviewContent = request.PreviewContent;

            if (request.PreviewFilePath != null)
                preview.PreviewFilePath = request.PreviewFilePath;

            if (request.PreviewImagePath != null)
                preview.PreviewImagePath = request.PreviewImagePath;

            if (request.DigitalSignatureId != null)
                preview.DigitalSignatureId = request.DigitalSignatureId;

            preview.IsActive = request.IsActive;
            preview.UpdatedAt = DateTime.UtcNow;
            preview.UpdatedBy = "System"; // TODO: Get from current user context

            await _context.SaveChangesAsync();

            _logger.LogInformation("Letter preview updated: {PreviewId}", previewId);

            return await GetPreviewWithAttachmentsAsync(previewId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating preview {PreviewId}", previewId);
            throw;
        }
    }

    public async Task<bool> DeletePreviewAsync(string previewId)
    {
        try
        {
            var preview = await _context.LetterPreviews.FindAsync(previewId);
            if (preview == null)
                return false;

            _context.LetterPreviews.Remove(preview);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Letter preview deleted: {PreviewId}", previewId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting preview {PreviewId}", previewId);
            return false;
        }
    }

    public async Task<IEnumerable<LetterPreviewDto>> GetPreviewsByEmployeeAsync(string employeeId)
    {
        try
        {
            var previews = await _context.LetterPreviews
                .Include(lp => lp.LetterTemplate)
                .Include(lp => lp.Employee)
                .Include(lp => lp.DigitalSignature)
                .Where(lp => lp.EmployeeId == employeeId && lp.IsActive)
                .OrderByDescending(lp => lp.LastGeneratedAt)
                .ToListAsync();

            var dtos = new List<LetterPreviewDto>();
            foreach (var preview in previews)
            {
                var dto = _mapper.Map<LetterPreviewDto>(preview);
                
                if (preview.Employee != null)
                {
                    dto.EmployeeName = $"{preview.Employee.FirstName} {preview.Employee.LastName}".Trim();
                }
                
                if (preview.DigitalSignature != null)
                {
                    dto.AuthorityName = preview.DigitalSignature.AuthorityName;
                    dto.AuthorityDesignation = preview.DigitalSignature.AuthorityDesignation;
                }
                
                dtos.Add(dto);
            }

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting previews for employee {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<IEnumerable<LetterPreviewDto>> GetPreviewsByLetterTypeAsync(string letterType)
    {
        try
        {
            var previews = await _context.LetterPreviews
                .Include(lp => lp.LetterTemplate)
                .Include(lp => lp.Employee)
                .Include(lp => lp.DigitalSignature)
                .Where(lp => lp.LetterType == letterType && lp.IsActive)
                .OrderByDescending(lp => lp.LastGeneratedAt)
                .ToListAsync();

            var dtos = new List<LetterPreviewDto>();
            foreach (var preview in previews)
            {
                var dto = _mapper.Map<LetterPreviewDto>(preview);
                
                if (preview.Employee != null)
                {
                    dto.EmployeeName = $"{preview.Employee.FirstName} {preview.Employee.LastName}".Trim();
                }
                
                if (preview.DigitalSignature != null)
                {
                    dto.AuthorityName = preview.DigitalSignature.AuthorityName;
                    dto.AuthorityDesignation = preview.DigitalSignature.AuthorityDesignation;
                }
                
                dtos.Add(dto);
            }

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting previews for letter type {LetterType}", letterType);
            throw;
        }
    }

    public async Task<LetterPreviewDto> GetLatestPreviewAsync(string letterTemplateId, string employeeId)
    {
        try
        {
            var preview = await _context.LetterPreviews
                .Include(lp => lp.LetterTemplate)
                .Include(lp => lp.Employee)
                .Include(lp => lp.DigitalSignature)
                .Include(lp => lp.Attachments)
                .Where(lp => lp.LetterTemplateId == letterTemplateId && lp.EmployeeId == employeeId && lp.IsActive)
                .OrderByDescending(lp => lp.LastGeneratedAt)
                .FirstOrDefaultAsync();

            if (preview == null)
                return null;

            var dto = _mapper.Map<LetterPreviewDto>(preview);
            
            if (preview.Employee != null)
            {
                dto.EmployeeName = $"{preview.Employee.FirstName} {preview.Employee.LastName}".Trim();
            }
            
            if (preview.DigitalSignature != null)
            {
                dto.AuthorityName = preview.DigitalSignature.AuthorityName;
                dto.AuthorityDesignation = preview.DigitalSignature.AuthorityDesignation;
            }

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest preview for template {TemplateId} and employee {EmployeeId}", 
                letterTemplateId, employeeId);
            throw;
        }
    }

    public async Task<bool> RegeneratePreviewWithLatestSignatureAsync(string previewId)
    {
        try
        {
            var preview = await _context.LetterPreviews
                .Include(lp => lp.LetterTemplate)
                .Include(lp => lp.Employee)
                .FirstOrDefaultAsync(lp => lp.Id == previewId);

            if (preview == null)
                return false;

            // Get the latest digital signature
            var latestSignature = await _context.DigitalSignatures
                .Where(ds => ds.IsActive)
                .OrderByDescending(ds => ds.SortOrder)
                .ThenByDescending(ds => ds.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestSignature != null)
            {
                preview.DigitalSignatureId = latestSignature.Id;
                preview.LastGeneratedAt = DateTime.UtcNow;
                preview.UpdatedAt = DateTime.UtcNow;
                preview.UpdatedBy = "System"; // TODO: Get from current user context

                await _context.SaveChangesAsync();

                _logger.LogInformation("Preview regenerated with latest signature: {PreviewId}", previewId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating preview {PreviewId} with latest signature", previewId);
            return false;
        }
    }

    public async Task<IEnumerable<LetterPreviewDto>> GetPreviewsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var previews = await _context.LetterPreviews
                .Include(lp => lp.LetterTemplate)
                .Include(lp => lp.Employee)
                .Include(lp => lp.DigitalSignature)
                .Where(lp => lp.LastGeneratedAt >= startDate && lp.LastGeneratedAt <= endDate && lp.IsActive)
                .OrderByDescending(lp => lp.LastGeneratedAt)
                .ToListAsync();

            var dtos = new List<LetterPreviewDto>();
            foreach (var preview in previews)
            {
                var dto = _mapper.Map<LetterPreviewDto>(preview);
                
                if (preview.Employee != null)
                {
                    dto.EmployeeName = $"{preview.Employee.FirstName} {preview.Employee.LastName}".Trim();
                }
                
                if (preview.DigitalSignature != null)
                {
                    dto.AuthorityName = preview.DigitalSignature.AuthorityName;
                    dto.AuthorityDesignation = preview.DigitalSignature.AuthorityDesignation;
                }
                
                dtos.Add(dto);
            }

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting previews by date range: {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }

    public async Task<LetterPreviewDto> ClonePreviewAsync(string previewId, string newEmployeeId)
    {
        try
        {
            var sourcePreview = await _context.LetterPreviews
                .Include(lp => lp.LetterTemplate)
                .Include(lp => lp.DigitalSignature)
                .FirstOrDefaultAsync(lp => lp.Id == previewId);

            if (sourcePreview == null)
                throw new ArgumentException($"Source preview with ID {previewId} not found");

            var newEmployee = await _context.Employees.FindAsync(newEmployeeId);
            if (newEmployee == null)
                throw new ArgumentException($"New employee with ID {newEmployeeId} not found");

            // Generate new preview content for the new employee
            var newPreviewContent = await GeneratePreviewContentAsync(sourcePreview.LetterTemplate, newEmployee, sourcePreview.DigitalSignatureId);

            var clonedPreview = new LetterPreview
            {
                LetterType = sourcePreview.LetterType,
                LetterTemplateId = sourcePreview.LetterTemplateId,
                EmployeeId = newEmployeeId,
                DigitalSignatureId = sourcePreview.DigitalSignatureId,
                PreviewContent = newPreviewContent,
                LastGeneratedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System" // TODO: Get from current user context
            };

            _context.LetterPreviews.Add(clonedPreview);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Preview cloned from {SourcePreviewId} to employee {NewEmployeeId}", previewId, newEmployeeId);

            return await GetPreviewAsync(sourcePreview.LetterTemplateId, newEmployeeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning preview {PreviewId} for employee {NewEmployeeId}", previewId, newEmployeeId);
            throw;
        }
    }

    public async Task<bool> BulkGeneratePreviewsAsync(BulkPreviewRequest request)
    {
        try
        {
            var results = new List<PreviewGenerationResult>();
            var successfullyGenerated = 0;
            var failed = 0;

            foreach (var employeeId in request.EmployeeIds)
            {
                try
                {
                    var preview = await GeneratePreviewAsync(
                        request.LetterTemplateId, 
                        employeeId, 
                        request.UseLatestSignature ? null : request.DigitalSignatureId);

                    if (preview != null)
                    {
                        successfullyGenerated++;
                        results.Add(new PreviewGenerationResult
                        {
                            EmployeeId = employeeId,
                            EmployeeName = preview.EmployeeName,
                            Success = true,
                            PreviewId = preview.Id,
                            PreviewFilePath = preview.PreviewFilePath
                        });
                    }
                    else
                    {
                        failed++;
                        results.Add(new PreviewGenerationResult
                        {
                            EmployeeId = employeeId,
                            Success = false,
                            ErrorMessage = "Failed to generate preview"
                        });
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    results.Add(new PreviewGenerationResult
                    {
                        EmployeeId = employeeId,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            _logger.LogInformation("Bulk preview generation completed: {SuccessfullyGenerated} successful, {Failed} failed", 
                successfullyGenerated, failed);

            return successfullyGenerated > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk preview generation for template {TemplateId}", request.LetterTemplateId);
            return false;
        }
    }

    public async Task<LetterPreviewDto> GetPreviewWithAttachmentsAsync(string previewId)
    {
        try
        {
            var preview = await _context.LetterPreviews
                .Include(lp => lp.LetterTemplate)
                .Include(lp => lp.Employee)
                .Include(lp => lp.DigitalSignature)
                .Include(lp => lp.Attachments)
                .FirstOrDefaultAsync(lp => lp.Id == previewId);

            if (preview == null)
                return null;

            var dto = _mapper.Map<LetterPreviewDto>(preview);
            
            if (preview.Employee != null)
            {
                dto.EmployeeName = $"{preview.Employee.FirstName} {preview.Employee.LastName}".Trim();
            }
            
            if (preview.DigitalSignature != null)
            {
                dto.AuthorityName = preview.DigitalSignature.AuthorityName;
                dto.AuthorityDesignation = preview.DigitalSignature.AuthorityDesignation;
            }

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting preview with attachments {PreviewId}", previewId);
            throw;
        }
    }

    private async Task<string> GeneratePreviewContentAsync(LetterTemplate template, Employee employee, string? digitalSignatureId)
    {
        try
        {
            // This is a simplified preview generation
            // In a real implementation, you would use a template engine like Razor, Handlebars, or similar
            var content = template.TemplateContent;

            // Replace basic placeholders (this is simplified - you'd want a proper template engine)
            content = content.Replace("{{EmployeeName}}", $"{employee.FirstName} {employee.LastName}".Trim());
            content = content.Replace("{{EmployeeId}}", employee.EmployeeId);
            content = content.Replace("{{Department}}", employee.Department ?? "");
            content = content.Replace("{{Designation}}", employee.Designation ?? "");
            content = content.Replace("{{Email}}", employee.Email);
            content = content.Replace("{{PhoneNumber}}", employee.PhoneNumber ?? "");
            content = content.Replace("{{CurrentDate}}", DateTime.Now.ToString("MMMM dd, yyyy"));

            // Add signature placeholder if digital signature is available
            if (!string.IsNullOrEmpty(digitalSignatureId))
            {
                var signature = await _context.DigitalSignatures.FindAsync(digitalSignatureId);
                if (signature != null)
                {
                    content = content.Replace("{{AuthorityName}}", signature.AuthorityName);
                    content = content.Replace("{{AuthorityDesignation}}", signature.AuthorityDesignation);
                    content = content.Replace("{{SignatureDate}}", DateTime.Now.ToString("MMMM dd, yyyy"));
                }
            }

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating preview content for template {TemplateId} and employee {EmployeeId}", 
                template.Id, employee.Id);
            return template.TemplateContent; // Return original template content as fallback
        }
    }

    // Bulk preview generation method
    public async Task<BulkPreviewGenerationResult> GenerateBulkPreviewsAsync(BulkPreviewGenerationRequest request)
    {
        try
        {
            var operationId = Guid.NewGuid().ToString();
            var results = new List<PreviewGenerationItemResult>();
            var successfullyGenerated = 0;
            var failed = 0;

            foreach (var employeeId in request.EmployeeIds)
            {
                try
                {
                    // Get employee information
                    var employee = await _employeeService.GetByIdAsync(employeeId);
                    if (employee == null)
                    {
                        throw new InvalidOperationException($"Employee with ID {employeeId} not found");
                    }

                    // Generate preview for this employee
                    var previewRequest = new GeneratePreviewRequest
                    {
                        LetterTemplateId = request.LetterTemplateId,
                        EmployeeId = employeeId,
                        FieldValues = request.FieldValues
                    };
                    
                    var preview = await GeneratePreviewAsync(previewRequest);
                    
                    var result = new PreviewGenerationItemResult
                    {
                        EmployeeId = employeeId,
                        EmployeeName = employee.Name,
                        Success = true,
                        PreviewId = preview.Id
                    };
                    
                    results.Add(result);
                    successfullyGenerated++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating preview for employee {EmployeeId}", employeeId);
                    
                    var result = new PreviewGenerationItemResult
                    {
                        EmployeeId = employeeId,
                        EmployeeName = "Unknown Employee",
                        Success = false,
                        ErrorMessage = ex.Message
                    };
                    
                    results.Add(result);
                    failed++;
                }
            }

            _logger.LogInformation("Bulk preview generation completed. Success: {Success}, Failed: {Failed}", 
                successfullyGenerated, failed);

            return new BulkPreviewGenerationResult
            {
                OperationId = operationId,
                TotalRequested = request.EmployeeIds.Count,
                SuccessfullyGenerated = successfullyGenerated,
                Failed = failed,
                Results = results,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk preview generation");
            throw;
        }
    }
}
