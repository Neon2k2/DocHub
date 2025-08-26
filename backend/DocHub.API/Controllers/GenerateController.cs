using Microsoft.AspNetCore.Mvc;
using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using DocHub.Core.Entities;
using DocHub.Infrastructure.Services.Document;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenerateController : ControllerBase
{
    private readonly IGeneratedLetterService _letterService;
    private readonly ILetterTemplateService _templateService;
    private readonly IEmployeeService _employeeService;
    private readonly IDigitalSignatureService _signatureService;
    private readonly IDocumentService _documentService;
    private readonly ILogger<GenerateController> _logger;

    public GenerateController(
        IGeneratedLetterService letterService,
        ILetterTemplateService templateService,
        IEmployeeService employeeService,
        IDigitalSignatureService signatureService,
        IDocumentService documentService,
        ILogger<GenerateController> logger)
    {
        _letterService = letterService;
        _templateService = templateService;
        _employeeService = employeeService;
        _signatureService = signatureService;
        _documentService = documentService;
        _logger = logger;
    }

    [HttpPost("single")]
    public async Task<ActionResult<GeneratedLetter>> GenerateSingleLetter(
        [FromBody] GenerateSingleLetterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate template
            var template = await _templateService.GetByIdAsync(request.TemplateId);
            if (template == null)
            {
                return BadRequest($"Template with id '{request.TemplateId}' not found");
            }

            // Validate employee
            var employee = await _employeeService.GetByIdAsync(request.EmployeeId);
            if (employee == null)
            {
                return BadRequest($"Employee with id '{request.EmployeeId}' not found");
            }

            // Generate letter
            var generateRequest = new GenerateLetterRequest
            {
                LetterTemplateId = request.TemplateId,
                EmployeeId = request.EmployeeId,
                DigitalSignatureId = request.DigitalSignatureId ?? "",
                FieldValues = request.Data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? "") ?? new Dictionary<string, string>()
            };
            
            var generatedLetter = await _letterService.GenerateLetterAsync(generateRequest);

            _logger.LogInformation("Single letter generated successfully: {LetterId}", generatedLetter.Id);
            return CreatedAtAction(nameof(GetGeneratedLetter), new { id = generatedLetter.Id }, generatedLetter);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating single letter");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<BulkGenerationResult>> GenerateBulkLetters(
        [FromBody] GenerateBulkLettersRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate template
            var template = await _templateService.GetByIdAsync(request.LetterTemplateId);
            if (template == null)
            {
                return BadRequest($"Template with id '{request.LetterTemplateId}' not found");
            }

            // Validate employees
            var employees = new List<Employee>();
            foreach (var employeeId in request.EmployeeIds)
            {
                var employee = await _employeeService.GetByIdAsync(employeeId);
                if (employee == null)
                {
                    return BadRequest($"Employee with id '{employeeId}' not found");
                }
                employees.Add(employee);
            }

            var result = new BulkGenerationResult
            {
                TemplateId = request.LetterTemplateId,
                TotalEmployees = request.EmployeeIds.Count,
                GeneratedLetters = new List<GeneratedLetter>(),
                Errors = new List<string>()
            };

            // Generate letters for each employee
            foreach (var employee in employees)
            {
                try
                {
                    var generateRequest = new GenerateLetterRequest
                    {
                        LetterTemplateId = request.LetterTemplateId,
                        EmployeeId = employee.Id,
                        DigitalSignatureId = request.DigitalSignatureId ?? "",
                        FieldValues = request.FieldValues ?? new Dictionary<string, string>()
                    };
                    
                    var generatedLetter = await _letterService.GenerateLetterAsync(generateRequest);

                    result.GeneratedLetters.Add(generatedLetter);
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Employee {employee.EmployeeId}: {ex.Message}";
                    result.Errors.Add(errorMessage);
                    _logger.LogWarning(ex, "Error generating letter for employee: {EmployeeId}", employee.Id);
                }
            }

            result.SuccessfullyGenerated = result.GeneratedLetters.Count;
            result.FailedCount = result.Errors.Count;
            result.ProcessedAt = DateTime.UtcNow;

            _logger.LogInformation("Bulk letter generation completed. {SuccessCount}/{TotalCount} letters generated successfully", 
                result.SuccessfullyGenerated, result.TotalEmployees);

            if (result.GeneratedLetters.Any())
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bulk letters");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("preview")]
    public async Task<ActionResult<LetterPreviewResult>> PreviewLetter(
        [FromBody] PreviewLetterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate template
            var template = await _templateService.GetByIdAsync(request.TemplateId);
            if (template == null)
            {
                return BadRequest($"Template with id '{request.TemplateId}' not found");
            }

            // Validate employee
            var employee = await _employeeService.GetByIdAsync(request.EmployeeId);
            if (employee == null)
            {
                return BadRequest($"Employee with id '{request.EmployeeId}' not found");
            }

            // Get signature
            DigitalSignature? signature = null;
            if (request.UseLatestSignature)
            {
                var signatures = await _signatureService.GetActiveSignaturesAsync();
                signature = signatures.FirstOrDefault();
            }
            else if (!string.IsNullOrEmpty(request.SignatureId))
            {
                signature = await _signatureService.GetByIdAsync(request.SignatureId);
            }

            if (signature == null)
            {
                return BadRequest("No signature available for preview");
            }

            // Generate preview
            var previewContent = await _documentService.GenerateLetterPreviewAsync(
                template, 
                employee, 
                request.Data, 
                signature);

            var result = new LetterPreviewResult
            {
                TemplateId = request.TemplateId,
                EmployeeId = request.EmployeeId,
                PreviewContent = Convert.ToBase64String(previewContent),
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating letter preview");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("templates")]
    public async Task<ActionResult<IEnumerable<LetterTemplate>>> GetAvailableTemplates()
    {
        try
        {
            var templates = await _templateService.GetActiveTemplatesAsync();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available templates");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("employees")]
    public async Task<ActionResult<IEnumerable<Employee>>> GetAvailableEmployees(
        [FromQuery] string? department = null,
        [FromQuery] string? designation = null)
    {
        try
        {
            IEnumerable<Employee> employees;

            if (!string.IsNullOrEmpty(department))
            {
                employees = await _employeeService.GetByDepartmentAsync(department);
            }
            else if (!string.IsNullOrEmpty(designation))
            {
                employees = await _employeeService.GetByDesignationAsync(designation);
            }
            else
            {
                employees = await _employeeService.GetAllAsync();
            }

            return Ok(employees.Where(e => e.IsActive));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available employees");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("signatures")]
    public async Task<ActionResult<IEnumerable<DigitalSignature>>> GetAvailableSignatures()
    {
        try
        {
            var signatures = await _signatureService.GetActiveSignaturesAsync();
            return Ok(signatures);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available signatures");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("generated/{id}")]
    public async Task<ActionResult<GeneratedLetter>> GetGeneratedLetter(string id)
    {
        try
        {
            var letter = await _letterService.GetByIdAsync(id);
            if (letter == null)
            {
                return NotFound();
            }
            return Ok(letter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting generated letter: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("download/{id}")]
    public async Task<IActionResult> DownloadLetter(string id)
    {
        try
        {
            var letter = await _letterService.GetByIdAsync(id);
            if (letter == null)
            {
                return NotFound("Letter not found");
            }

            if (string.IsNullOrEmpty(letter.LetterFilePath))
            {
                return BadRequest("Letter file not available for download");
            }

            var filePath = Path.Combine("wwwroot", letter.LetterFilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Letter file not found");
            }

            var fileName = $"{letter.LetterType}_{letter.LetterNumber}_{DateTime.UtcNow:yyyyMMdd}.pdf";
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(fileBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading letter: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

// Request and response models
public class GenerateSingleLetterRequest
{
    public string TemplateId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string? DigitalSignatureId { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public class PreviewLetterRequest
{
    public string TemplateId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string? DigitalSignatureId { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public bool UseLatestSignature { get; set; } = true;
    public string? SignatureId { get; set; }
}

public class BulkGenerationResult
{
    public string TemplateId { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public int SuccessfullyGenerated { get; set; }
    public int FailedCount { get; set; }
    public List<GeneratedLetter> GeneratedLetters { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}

public class LetterPreviewResult
{
    public string TemplateId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string PreviewContent { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}
