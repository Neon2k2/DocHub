using Microsoft.AspNetCore.Mvc;
using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using DocHub.Core.Entities;
using Microsoft.Extensions.Logging;
using DocHub.Infrastructure.Services.PROXKey;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DigitalSignatureController : ControllerBase
{
    private readonly IPROXKeyService _proxKeyService;
    private readonly ISignatureService _signatureService;
    private readonly IDigitalSignatureService _digitalSignatureService;
    private readonly ILogger<DigitalSignatureController> _logger;

    public DigitalSignatureController(
        IPROXKeyService proxKeyService,
        ISignatureService signatureService,
        IDigitalSignatureService digitalSignatureService,
        ILogger<DigitalSignatureController> logger)
    {
        _proxKeyService = proxKeyService;
        _signatureService = signatureService;
        _digitalSignatureService = digitalSignatureService;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Get all digital signatures with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DigitalSignature>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var signatures = await _digitalSignatureService.GetPagedAsync(page, pageSize);
            var totalCount = await _digitalSignatureService.GetTotalCountAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page", page.ToString());
            Response.Headers.Add("X-PageSize", pageSize.ToString());

            return Ok(signatures);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all digital signatures");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get active digital signatures
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<DigitalSignature>>> GetActive()
    {
        try
        {
            var signatures = await _digitalSignatureService.GetActiveSignaturesAsync();
            return Ok(signatures);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active signatures");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get latest signature path
    /// </summary>
    [HttpGet("latest")]
    public async Task<ActionResult<string>> GetLatestSignaturePath()
    {
        try
        {
            var signaturePath = await _digitalSignatureService.GetLatestSignaturePathAsync();
            if (string.IsNullOrEmpty(signaturePath))
            {
                return NotFound("No active signatures found");
            }
            return Ok(new { signaturePath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest signature path");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get digital signature by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DigitalSignature>> GetById(string id)
    {
        try
        {
            var signature = await _digitalSignatureService.GetByIdAsync(id);
            if (signature == null)
            {
                return NotFound();
            }
            return Ok(signature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting signature by id: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region PROXKey Operations

    /// <summary>
    /// Get PROXKey device information
    /// </summary>
    [HttpGet("device-info")]
    public async Task<ActionResult<ApiResponse<PROXKeyInfoDto>>> GetDeviceInfo()
    {
        try
        {
            var deviceInfo = await _proxKeyService.GetDeviceInfoAsync();

            _logger.LogInformation("PROXKey device info retrieved successfully");

            return Ok(ApiResponse<PROXKeyInfoDto>.SuccessResult(deviceInfo, "Device information retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving PROXKey device info");
            return StatusCode(500, ApiResponse<PROXKeyInfoDto>.ErrorResult("Error retrieving device information", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Generate digital signature using PROXKey device
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<ApiResponse<DigitalSignatureDto>>> GenerateSignature([FromBody] GenerateSignatureRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.AuthorityName))
            {
                return BadRequest(ApiResponse<DigitalSignatureDto>.ValidationErrorResult("Authority name is required", new List<string> { "Authority name is required" }));
            }

            if (string.IsNullOrWhiteSpace(request.AuthorityDesignation))
            {
                return BadRequest(ApiResponse<DigitalSignatureDto>.ValidationErrorResult("Authority designation is required", new List<string> { "Authority designation is required" }));
            }

            _logger.LogInformation("Generating digital signature for {AuthorityName}", request.AuthorityName);

            var signature = await _proxKeyService.GenerateSignatureAsync(request);

            _logger.LogInformation("Digital signature generated successfully for {AuthorityName}. Signature ID: {SignatureId}", 
                request.AuthorityName, signature.Id);

            // Convert entity to DTO
            var signatureDto = new DigitalSignatureDto
            {
                Id = signature.Id,
                SignatureName = signature.SignatureName,
                AuthorityName = signature.AuthorityName,
                AuthorityDesignation = signature.AuthorityDesignation,
                SignatureImagePath = signature.SignatureImagePath,
                SignatureData = signature.SignatureData,
                SignatureDate = signature.SignatureDate,
                IsActive = signature.IsActive,
                SortOrder = signature.SortOrder,
                CreatedAt = signature.CreatedAt,
                UpdatedAt = signature.UpdatedAt,
                CreatedBy = signature.CreatedBy,
                UpdatedBy = signature.UpdatedBy
            };

            return Ok(ApiResponse<DigitalSignatureDto>.SuccessResult(signatureDto, "Digital signature generated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating digital signature for {AuthorityName}", request.AuthorityName);
            return StatusCode(500, ApiResponse<DigitalSignatureDto>.ErrorResult("Error generating digital signature", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Validate digital signature
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateSignature([FromBody] ValidateSignatureRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.SignatureData))
            {
                return BadRequest(ApiResponse<bool>.ValidationErrorResult("Signature data is required", new List<string> { "Signature data is required" }));
            }

            _logger.LogInformation("Validating digital signature");

            var isValid = await _proxKeyService.ValidateSignatureAsync(request.SignatureData);

            _logger.LogInformation("Digital signature validation completed. Result: {IsValid}", isValid);

            return Ok(ApiResponse<bool>.SuccessResult(isValid, "Signature validation completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating digital signature");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Error validating signature", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get signature image
    /// </summary>
    [HttpGet("image/{signatureId}")]
    public async Task<ActionResult> GetSignatureImage(string signatureId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(signatureId))
            {
                return BadRequest("Signature ID is required");
            }

            _logger.LogInformation("Retrieving signature image for signature {SignatureId}", signatureId);

            var imageBytes = await _proxKeyService.GetSignatureImageAsync(signatureId);

            _logger.LogInformation("Signature image retrieved successfully for signature {SignatureId}", signatureId);

            return File(imageBytes, "image/png", $"signature_{signatureId}.png");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving signature image for signature {SignatureId}", signatureId);
            return StatusCode(500, "Error retrieving signature image");
        }
    }

    #endregion

    #region Update Operations

    /// <summary>
    /// Update digital signature
    /// </summary>
    [HttpPut("{signatureId}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSignature(string signatureId, [FromBody] UpdateDigitalSignatureDto updateDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(signatureId))
            {
                return BadRequest(ApiResponse<bool>.ValidationErrorResult("Signature ID is required", new List<string> { "Signature ID is required" }));
            }

            if (string.IsNullOrWhiteSpace(updateDto.AuthorityName))
            {
                return BadRequest(ApiResponse<bool>.ValidationErrorResult("Authority name is required", new List<string> { "Authority name is required" }));
            }

            if (string.IsNullOrWhiteSpace(updateDto.AuthorityDesignation))
            {
                return BadRequest(ApiResponse<bool>.ValidationErrorResult("Authority designation is required", new List<string> { "Authority designation is required" }));
            }

            _logger.LogInformation("Updating digital signature {SignatureId}", signatureId);

            // Convert DTO to entity
            var signature = new DigitalSignature
            {
                Id = signatureId,
                SignatureName = updateDto.SignatureName,
                AuthorityName = updateDto.AuthorityName,
                AuthorityDesignation = updateDto.AuthorityDesignation,
                SignatureImagePath = updateDto.SignatureImagePath,
                SignatureData = updateDto.SignatureData,
                SignatureDate = updateDto.SignatureDate,
                IsActive = updateDto.IsActive,
                SortOrder = updateDto.SortOrder
            };

            var updatedSignature = await _proxKeyService.UpdateSignatureAsync(signatureId, signature);

            if (updatedSignature != null)
            {
                _logger.LogInformation("Digital signature updated successfully: {SignatureId}", signatureId);
                return Ok(ApiResponse<bool>.SuccessResult(true, "Digital signature updated successfully"));
            }
            else
            {
                _logger.LogWarning("Failed to update digital signature: {SignatureId}", signatureId);
                return BadRequest(ApiResponse<bool>.ErrorResult("Failed to update digital signature", new List<string> { "Update operation failed" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating digital signature {SignatureId}", signatureId);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Error updating digital signature", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Delete digital signature
    /// </summary>
    [HttpDelete("{signatureId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSignature(string signatureId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(signatureId))
            {
                return BadRequest(ApiResponse<bool>.ValidationErrorResult("Signature ID is required", new List<string> { "Signature ID is required" }));
            }

            _logger.LogInformation("Deleting digital signature {SignatureId}", signatureId);

            var isDeleted = await _digitalSignatureService.DeleteAsync(signatureId);

            if (isDeleted)
            {
                _logger.LogInformation("Digital signature deleted successfully: {SignatureId}", signatureId);
                return Ok(ApiResponse<bool>.SuccessResult(true, "Digital signature deleted successfully"));
            }
            else
            {
                _logger.LogWarning("Failed to delete digital signature: {SignatureId}", signatureId);
                return BadRequest(ApiResponse<bool>.ErrorResult("Failed to delete digital signature", new List<string> { "Delete operation failed" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting digital signature {SignatureId}", signatureId);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Error deleting digital signature", new List<string> { ex.Message }));
        }
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Bulk update signature status
    /// </summary>
    [HttpPut("bulk-status")]
    public async Task<ActionResult<ApiResponse<BulkOperationResult>>> BulkUpdateStatus([FromBody] BulkSignatureStatusUpdateRequest request)
    {
        try
        {
            if (request?.SignatureIds == null || !request.SignatureIds.Any())
            {
                return BadRequest(ApiResponse<BulkOperationResult>.ValidationErrorResult("Signature IDs are required", new List<string> { "At least one signature ID is required" }));
            }

            _logger.LogInformation("Bulk updating status for {Count} signatures", request.SignatureIds.Count);

            var results = new List<SignatureUpdateResult>();
            var successCount = 0;
            var failureCount = 0;

            foreach (var signatureId in request.SignatureIds)
    {
        try
        {
                    var signature = await _digitalSignatureService.GetByIdAsync(signatureId);
                    if (signature != null)
                    {
                        signature.IsActive = request.IsActive;
                        signature.UpdatedAt = DateTime.UtcNow;
                        signature.UpdatedBy = "System"; // TODO: Get from current user context

                        await _digitalSignatureService.UpdateAsync(signatureId, signature);
                        successCount++;

                        results.Add(new SignatureUpdateResult
                        {
                            SignatureId = signatureId,
                            Success = true,
                            Message = "Status updated successfully"
                        });
                    }
                    else
                    {
                        failureCount++;
                        results.Add(new SignatureUpdateResult
                        {
                            SignatureId = signatureId,
                            Success = false,
                            Message = "Signature not found"
                        });
                    }
        }
        catch (Exception ex)
        {
                    failureCount++;
                    results.Add(new SignatureUpdateResult
                    {
                        SignatureId = signatureId,
                        Success = false,
                        Message = $"Error: {ex.Message}"
                    });
                }
            }

            var bulkResult = new BulkOperationResult
            {
                TotalRequested = request.SignatureIds.Count,
                SuccessfulCount = successCount,
                FailedCount = failureCount,
                Results = results
            };

            _logger.LogInformation("Bulk status update completed: {SuccessCount}/{TotalCount} successful", successCount, request.SignatureIds.Count);

            return Ok(ApiResponse<BulkOperationResult>.SuccessResult(bulkResult, $"Bulk status update completed: {successCount}/{request.SignatureIds.Count} successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk status update");
            return StatusCode(500, ApiResponse<BulkOperationResult>.ErrorResult("Error during bulk status update", new List<string> { ex.Message }));
        }
    }

    #endregion
}

#region DTOs

public class ValidateSignatureRequest
{
    public string SignatureData { get; set; } = string.Empty;
}

public class BulkSignatureStatusUpdateRequest
{
    public List<string> SignatureIds { get; set; } = new List<string>();
    public bool IsActive { get; set; }
}

public class SignatureUpdateResult
{
    public string SignatureId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class BulkOperationResult
{
    public int TotalRequested { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public List<SignatureUpdateResult> Results { get; set; } = new List<SignatureUpdateResult>();
}

#endregion
