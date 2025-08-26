using Microsoft.AspNetCore.Mvc;
using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using DocHub.Core.Entities;
using DocHub.Infrastructure.Services.Excel;
using Microsoft.AspNetCore.Http;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IExcelService _excelService;
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<UploadController> _logger;

    public UploadController(
        IExcelService excelService,
        IEmployeeService employeeService,
        ILogger<UploadController> logger)
    {
        _excelService = excelService;
        _employeeService = employeeService;
        _logger = logger;
    }

    [HttpPost("excel")]
    public async Task<ActionResult<UploadResult>> UploadExcel(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (!IsValidExcelFile(file))
            {
                return BadRequest("Invalid file format. Please upload an Excel file (.xlsx, .xls)");
            }

            _logger.LogInformation("Processing Excel file: {FileName}, Size: {Size} bytes", 
                file.FileName, file.Length);

            // Process the Excel file
            var employees = await _excelService.ProcessEmployeeDataAsync(file.OpenReadStream());
            
            if (!employees.Any())
            {
                return BadRequest("No valid employee data found in the Excel file");
            }

            // Save employees to database
            var savedEmployees = new List<Employee>();
            var errors = new List<string>();

            foreach (var employee in employees)
            {
                try
                {
                    var savedEmployee = await _employeeService.CreateAsync(employee);
                    savedEmployees.Add(savedEmployee);
                }
                catch (InvalidOperationException ex)
                {
                    errors.Add($"Employee {employee.EmployeeId}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    errors.Add($"Employee {employee.EmployeeId}: Unexpected error - {ex.Message}");
                }
            }

            var result = new UploadResult
            {
                TotalRecords = employees.Count,
                SuccessfullyProcessed = savedEmployees.Count,
                FailedRecords = errors.Count,
                Errors = errors,
                ProcessedAt = DateTime.UtcNow
            };

            if (savedEmployees.Any())
            {
                _logger.LogInformation("Excel upload completed. {SuccessCount}/{TotalCount} employees processed successfully", 
                    savedEmployees.Count, employees.Count);
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Excel upload");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("excel/validate")]
    public async Task<ActionResult<ExcelValidationResult>> ValidateExcel(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (!IsValidExcelFile(file))
            {
                return BadRequest("Invalid file format. Please upload an Excel file (.xlsx, .xls)");
            }

            // Validate the Excel file without saving
            var validationResult = await _excelService.ValidateEmployeeDataFromStreamAsync(file.OpenReadStream());
            
            return Ok(validationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Excel file");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("excel/template")]
    public async Task<IActionResult> DownloadTemplate()
    {
        try
        {
            var templateData = await _excelService.GenerateEmployeeTemplateAsync();
            
            var fileName = $"Employee_Template_{DateTime.UtcNow:yyyyMMdd}.xlsx";
            
            return File(templateData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Excel template");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult<UploadStatus>> GetUploadStatus()
    {
        try
        {
            var status = new UploadStatus
            {
                LastUploadAt = DateTime.UtcNow.AddHours(-2), // Mock data
                TotalUploads = 15, // Mock data
                SuccessRate = 95.5, // Mock data
                LastUploadFile = "employees_20241201.xlsx" // Mock data
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upload status");
            return StatusCode(500, "Internal server error");
        }
    }

    private bool IsValidExcelFile(IFormFile file)
    {
        var allowedExtensions = new[] { ".xlsx", ".xls" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        return allowedExtensions.Contains(fileExtension);
    }
}

// Response models
public class UploadResult
{
    public int TotalRecords { get; set; }
    public int SuccessfullyProcessed { get; set; }
    public int FailedRecords { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}

public class UploadStatus
{
    public DateTime? LastUploadAt { get; set; }
    public int TotalUploads { get; set; }
    public double SuccessRate { get; set; }
    public string? LastUploadFile { get; set; }
}
