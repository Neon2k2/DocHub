using DocHub.Application.DTOs;
using DocHub.Application.Interfaces;
using DocHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocHub.Core.Entities;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    private readonly DocHubDbContext _context;
    private readonly IExcelService _excelService;
    private readonly IDocumentService _documentService;
    private readonly ILogger<FileUploadController> _logger;
    private readonly IConfiguration _configuration;

    public FileUploadController(
        DocHubDbContext context,
        IExcelService excelService,
        IDocumentService documentService,
        ILogger<FileUploadController> logger,
        IConfiguration configuration)
    {
        _context = context;
        _excelService = excelService;
        _documentService = documentService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Upload Excel file with employee data
    /// </summary>
    [HttpPost("excel")]
    public async Task<ActionResult<ApiResponse<ExcelUploadResult>>> UploadExcelFile(IFormFile file, [FromForm] string department = "", [FromForm] string location = "")
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<ExcelUploadResult>.ValidationErrorResult("No file uploaded", new List<string> { "File is required" }));
            }

            // Validate file type
            var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions:Excel").Get<string[]>() ?? new[] { ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(ApiResponse<ExcelUploadResult>.ValidationErrorResult("Invalid file type", new List<string> { $"Only {string.Join(", ", allowedExtensions)} files are allowed" }));
            }

            // Validate file size
            var maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSize", 10 * 1024 * 1024); // Default 10MB
            if (file.Length > maxFileSize)
            {
                return BadRequest(ApiResponse<ExcelUploadResult>.ValidationErrorResult("File too large", new List<string> { $"File size must be less than {maxFileSize / (1024 * 1024)}MB" }));
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var uploadPath = Path.Combine("wwwroot", "uploads", "excel");
            var filePath = Path.Combine(uploadPath, fileName);

            // Ensure directory exists
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Process Excel file
            var employees = await _excelService.ProcessEmployeeExcelFileAsync(filePath);

            if (employees != null && employees.Any())
            {
                // Create upload record
                var uploadRecord = new FileUpload
                {
                    Id = Guid.NewGuid().ToString(),
                    FileName = file.FileName,
                    FilePath = $"/uploads/excel/{fileName}",
                    FileType = "excel",
                    FileSize = file.Length,
                    Department = department,
                    Location = location,
                    Status = "completed",
                    ProcessedRows = employees.Count,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "API User" // This should come from authentication context
                };

                _context.FileUploads.Add(uploadRecord);
                await _context.SaveChangesAsync();

                // Create ExcelUploadResult from employees
                var result = new ExcelUploadResult
                {
                    Success = true,
                    ProcessedRows = employees.Count,
                    Employees = employees,
                    Errors = new List<string>()
                };

                _logger.LogInformation("Excel file uploaded and processed successfully: {FileName}", file.FileName);
                return Ok(ApiResponse<ExcelUploadResult>.SuccessResult(result, "Excel file uploaded and processed successfully"));
            }
            else
            {
                _logger.LogWarning("Excel file processing failed: {FileName}, No employees found", file.FileName);
                return BadRequest(ApiResponse<ExcelUploadResult>.ErrorResult("Excel file processing failed", new List<string> { "No employees found in the file" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading Excel file: {FileName}", file?.FileName);
            return StatusCode(500, ApiResponse<ExcelUploadResult>.ErrorResult("Error uploading Excel file", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Upload document template with enhanced validation
    /// </summary>
    [HttpPost("template")]
    public async Task<ActionResult<ApiResponse<TemplateUploadResult>>> UploadTemplate(
        IFormFile file, 
        [FromForm] string templateName, 
        [FromForm] string letterType,
        [FromForm] string description = "",
        [FromForm] string category = "",
        [FromForm] string dataSource = "upload")
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<TemplateUploadResult>.ValidationErrorResult("No file uploaded", new List<string> { "File is required" }));
            }

            if (string.IsNullOrWhiteSpace(templateName))
            {
                return BadRequest(ApiResponse<TemplateUploadResult>.ValidationErrorResult("Template name is required", new List<string> { "Template name is required" }));
            }

            if (string.IsNullOrWhiteSpace(letterType))
            {
                return BadRequest(ApiResponse<TemplateUploadResult>.ValidationErrorResult("Letter type is required", new List<string> { "Letter type is required" }));
            }

            // Validate file type
            var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions:Template").Get<string[]>() ?? new[] { ".docx", ".doc" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(ApiResponse<TemplateUploadResult>.ValidationErrorResult("Invalid file type", new List<string> { $"Only {string.Join(", ", allowedExtensions)} files are allowed" }));
            }

            // Validate file size
            var maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSize", 10 * 1024 * 1024); // Default 10MB
            if (file.Length > maxFileSize)
            {
                return BadRequest(ApiResponse<TemplateUploadResult>.ValidationErrorResult("File too large", new List<string> { $"File size must be less than {maxFileSize / (1024 * 1024)}MB" }));
            }

            // Check if template name already exists
            if (await _context.LetterTemplates.AnyAsync(t => t.Name == templateName))
            {
                return BadRequest(ApiResponse<TemplateUploadResult>.ValidationErrorResult("Template name already exists", new List<string> { "Template name must be unique" }));
            }

            // Generate unique filename
            var fileName = $"{templateName}_{Guid.NewGuid()}{fileExtension}";
            var uploadPath = Path.Combine("wwwroot", "uploads", "templates");
            var filePath = Path.Combine(uploadPath, fileName);

            // Ensure directory exists
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Validate template file
            var isValidTemplate = await _documentService.ValidateTemplateAsync(filePath);
            if (!isValidTemplate)
            {
                // Clean up invalid file
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                return BadRequest(ApiResponse<TemplateUploadResult>.ValidationErrorResult("Invalid template file", new List<string> { "The uploaded file is not a valid document template" }));
            }

            // Extract template fields
            var templateFields = await _documentService.ExtractTemplateFieldsAsync(filePath);

            // Create template record
            var template = new LetterTemplate
            {
                Id = Guid.NewGuid().ToString(),
                Name = templateName,
                LetterType = letterType,
                TemplateContent = $"Template: {templateName}",
                TemplateFilePath = $"/uploads/templates/{fileName}",
                Description = description,
                Category = category,
                DataSource = dataSource,
                IsActive = true,
                SortOrder = await GetNextTemplateSortOrderAsync(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "API User" // This should come from authentication context
            };

            _context.LetterTemplates.Add(template);

            // Add template fields
            if (templateFields.Any())
            {
                var fields = templateFields.Select((field, index) => new LetterTemplateField
                {
                    Id = Guid.NewGuid().ToString(),
                    LetterTemplateId = template.Id,
                    FieldName = field.FieldName,
                    FieldType = field.FieldType ?? "text",
                    IsRequired = field.IsRequired,
                    SortOrder = index + 1,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "API User"
                }).ToList();

                _context.LetterTemplateFields.AddRange(fields);
            }

            await _context.SaveChangesAsync();

            // Create upload record
            var uploadRecord = new FileUpload
            {
                Id = Guid.NewGuid().ToString(),
                FileName = file.FileName,
                FilePath = $"/uploads/templates/{fileName}",
                FileType = "template",
                FileSize = file.Length,
                Status = "completed",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "API User"
            };

            _context.FileUploads.Add(uploadRecord);
            await _context.SaveChangesAsync();

            var result = new TemplateUploadResult
            {
                TemplateId = template.Id,
                TemplateName = template.Name,
                FilePath = template.TemplateFilePath,
                Fields = templateFields.Select(f => f.FieldName).ToList(),
                Message = "Template uploaded successfully"
            };

            _logger.LogInformation("Template uploaded successfully: {TemplateName}", templateName);
            return Ok(ApiResponse<TemplateUploadResult>.SuccessResult(result, "Template uploaded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading template: {TemplateName}", templateName);
            return StatusCode(500, ApiResponse<TemplateUploadResult>.ErrorResult("Error uploading template", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Upload signature image with enhanced validation
    /// </summary>
    [HttpPost("signature")]
    public async Task<ActionResult<ApiResponse<SignatureUploadResult>>> UploadSignature(
        IFormFile file, 
        [FromForm] string authorityName, 
        [FromForm] string designation,
        [FromForm] string notes = "")
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<SignatureUploadResult>.ValidationErrorResult("No file uploaded", new List<string> { "File is required" }));
            }

            if (string.IsNullOrWhiteSpace(authorityName))
            {
                return BadRequest(ApiResponse<SignatureUploadResult>.ValidationErrorResult("Authority name is required", new List<string> { "Authority name is required" }));
            }

            if (string.IsNullOrWhiteSpace(designation))
            {
                return BadRequest(ApiResponse<SignatureUploadResult>.ValidationErrorResult("Designation is required", new List<string> { "Designation is required" }));
            }

            // Validate file type
            var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions:Signature").Get<string[]>() ?? new[] { ".png", ".jpg", ".jpeg" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(ApiResponse<SignatureUploadResult>.ValidationErrorResult("Invalid file type", new List<string> { $"Only {string.Join(", ", allowedExtensions)} files are allowed" }));
            }

            // Validate file size
            var maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSize", 5 * 1024 * 1024); // Default 5MB for images
            if (file.Length > maxFileSize)
            {
                return BadRequest(ApiResponse<SignatureUploadResult>.ValidationErrorResult("File too large", new List<string> { $"File size must be less than {maxFileSize / (1024 * 1024)}MB" }));
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var uploadPath = Path.Combine("wwwroot", "uploads", "signatures");
            var filePath = Path.Combine(uploadPath, fileName);

            // Ensure directory exists
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create signature record
            var signature = new DigitalSignature
            {
                Id = Guid.NewGuid().ToString(),
                AuthorityName = authorityName,
                AuthorityDesignation = designation,
                SignatureImagePath = $"/uploads/signatures/{fileName}",
                Notes = notes,
                IsActive = true,
                SortOrder = await GetNextSignatureSortOrderAsync(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "API User" // This should come from authentication context
            };

            _context.DigitalSignatures.Add(signature);

            // Create upload record
            var uploadRecord = new FileUpload
            {
                Id = Guid.NewGuid().ToString(),
                FileName = file.FileName,
                FilePath = $"/uploads/signatures/{fileName}",
                FileType = "signature",
                FileSize = file.Length,
                Status = "completed",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "API User"
            };

            _context.FileUploads.Add(uploadRecord);
            await _context.SaveChangesAsync();

            var result = new SignatureUploadResult
            {
                SignatureId = signature.Id,
                AuthorityName = signature.AuthorityName,
                AuthorityDesignation = signature.AuthorityDesignation,
                ImagePath = signature.SignatureImagePath,
                Message = "Signature uploaded successfully"
            };

            _logger.LogInformation("Signature uploaded successfully: {AuthorityName}", authorityName);
            return Ok(ApiResponse<SignatureUploadResult>.SuccessResult(result, "Signature uploaded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading signature for {AuthorityName}", authorityName);
            return StatusCode(500, ApiResponse<SignatureUploadResult>.ErrorResult("Error uploading signature", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get upload history
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<List<FileUploadHistory>>>> GetUploadHistory(
        [FromQuery] string? fileType = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.FileUploads.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(fileType))
                query = query.Where(u => u.FileType == fileType);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(u => u.Status == status);

            if (fromDate.HasValue)
                query = query.Where(u => u.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(u => u.CreatedAt <= toDate.Value);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var uploads = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var history = uploads.Select(u => new FileUploadHistory
            {
                Id = u.Id,
                FileName = u.FileName,
                FileType = u.FileType,
                FileSize = u.FileSize,
                Status = u.Status,
                Department = u.Department,
                Location = u.Location,
                ProcessedRows = u.ProcessedRows,
                CreatedAt = u.CreatedAt,
                CreatedBy = u.CreatedBy
            }).ToList();

            var result = new ApiResponse<List<FileUploadHistory>>
            {
                Success = true,
                Data = history,
                Message = $"Retrieved {history.Count} uploads",
                Pagination = new PaginationInfo
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upload history");
            return StatusCode(500, ApiResponse<List<FileUploadHistory>>.ErrorResult("Error retrieving upload history", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Delete uploaded file
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUpload(string id)
    {
        try
        {
            var upload = await _context.FileUploads.FindAsync(id);
            if (upload == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResult("Upload not found", new List<string> { "Upload ID not found" }));
            }

            // Delete physical file
            var filePath = Path.Combine("wwwroot", upload.FilePath.TrimStart('/'));
                            if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

            // Remove from database
            _context.FileUploads.Remove(upload);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Upload deleted successfully: {Id}", id);
            return Ok(ApiResponse<bool>.SuccessResult(true, "Upload deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting upload {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Error deleting upload", new List<string> { ex.Message }));
        }
    }

    private async Task<int> GetNextTemplateSortOrderAsync()
    {
        var maxSortOrder = await _context.LetterTemplates
            .MaxAsync(t => (int?)t.SortOrder) ?? 0;
        return maxSortOrder + 1;
    }

    private async Task<int> GetNextSignatureSortOrderAsync()
    {
        var maxSortOrder = await _context.DigitalSignatures
            .MaxAsync(s => (int?)s.SortOrder) ?? 0;
        return maxSortOrder + 1;
    }
}

// Enhanced result models
public class ExcelUploadResult
{
    public bool Success { get; set; }
    public int ProcessedRows { get; set; }
    public int CreatedEmployees { get; set; }
    public int UpdatedEmployees { get; set; }
    public List<Employee> Employees { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class TemplateUploadResult
{
    public string TemplateId { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public List<string> Fields { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class SignatureUploadResult
{
    public string SignatureId { get; set; } = string.Empty;
    public string AuthorityName { get; set; } = string.Empty;
    public string AuthorityDesignation { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class FileUploadHistory
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Location { get; set; }
    public int? ProcessedRows { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}


