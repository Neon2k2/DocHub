using DocHub.Application.DTOs;
using DocHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExcelDataController : ControllerBase
{
    private readonly IExcelService _excelService;
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<ExcelDataController> _logger;

    public ExcelDataController(
        IExcelService excelService,
        IEmployeeService employeeService,
        ILogger<ExcelDataController> logger)
    {
        _excelService = excelService;
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>
    /// Validate Excel data before processing
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<ExcelValidationResult>>> ValidateExcelData(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<ExcelValidationResult>.ValidationErrorResult("No file uploaded", new List<string> { "File is required" }));
            }

            var validationResult = await _excelService.ValidateExcelDataAsync(file);
            return Ok(ApiResponse<ExcelDataValidationResult>.SuccessResult(validationResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Excel data");
            return StatusCode(500, ApiResponse<ExcelValidationResult>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Process Excel data and import employees
    /// </summary>
    [HttpPost("process")]
    public async Task<ActionResult<ApiResponse<ExcelProcessingResult>>> ProcessExcelData([FromBody] ProcessExcelRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var processingResult = await _excelService.ProcessExcelDataAsync(request.FilePath, request.Options);
            return Ok(ApiResponse<ExcelProcessingResult>.SuccessResult(processingResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Excel data");
            return StatusCode(500, ApiResponse<ExcelProcessingResult>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Get Excel processing history
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<ExcelProcessingHistoryDto>>> GetProcessingHistory()
    {
        try
        {
            var history = await _excelService.GetProcessingHistoryAsync();
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Excel processing history");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get Excel template for employee data
    /// </summary>
    [HttpGet("template")]
    public async Task<IActionResult> DownloadExcelTemplate()
    {
        try
        {
            var templateBytes = await _excelService.GenerateEmployeeTemplateAsync();
            var fileName = $"Employee_Template_{DateTime.Now:yyyyMMdd}.xlsx";
            
            return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Excel template");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Export employee data to Excel
    /// </summary>
    [HttpPost("export")]
    public async Task<IActionResult> ExportEmployeeData([FromBody] ExportEmployeeRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var excelBytes = await _excelService.ExportEmployeeDataAsync(request.EmployeeIds, request.Fields);
            var fileName = $"Employee_Export_{DateTime.Now:yyyyMMdd}.xlsx";
            
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting employee data");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get Excel processing statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<ExcelProcessingStatsDto>> GetProcessingStats()
    {
        try
        {
            var stats = await _excelService.GetProcessingStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Excel processing stats");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Retry failed Excel processing
    /// </summary>
    [HttpPost("retry/{processingId}")]
    public async Task<ActionResult<ApiResponse<ExcelProcessingResult>>> RetryProcessing(string processingId)
    {
        try
        {
            var result = await _excelService.RetryProcessingAsync(processingId);
            if (result == null)
                return NotFound(ApiResponse<ExcelProcessingResult>.ErrorResult("Processing record not found"));

            return Ok(ApiResponse<ExcelProcessingResult>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying Excel processing {ProcessingId}", processingId);
            return StatusCode(500, ApiResponse<ExcelProcessingResult>.ErrorResult("Internal server error"));
        }
    }
}


