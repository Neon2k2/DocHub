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
    private readonly ILogger<DigitalSignatureController> _logger;

    public DigitalSignatureController(
        IPROXKeyService proxKeyService,
        ISignatureService signatureService,
        ILogger<DigitalSignatureController> logger)
    {
        _proxKeyService = proxKeyService;
        _signatureService = signatureService;
        _logger = logger;
    }

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
                return NotFound(ApiResponse<bool>.ErrorResult("Signature not found", new List<string> { "Signature ID not found" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating digital signature {SignatureId}", signatureId);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Error updating signature", new List<string> { ex.Message }));
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

            var success = await _proxKeyService.DeleteSignatureAsync(signatureId);

            if (success)
            {
                _logger.LogInformation("Digital signature deleted successfully: {SignatureId}", signatureId);
                return Ok(ApiResponse<bool>.SuccessResult(true, "Digital signature deleted successfully"));
            }
            else
            {
                return NotFound(ApiResponse<bool>.ErrorResult("Signature not found", new List<string> { "Signature ID not found" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting digital signature {SignatureId}", signatureId);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Error deleting signature", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get signatures by authority
    /// </summary>
    [HttpGet("by-authority/{authorityName}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DigitalSignatureDto>>>> GetSignaturesByAuthority(string authorityName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(authorityName))
            {
                return BadRequest(ApiResponse<IEnumerable<DigitalSignatureDto>>.ValidationErrorResult("Authority name is required", new List<string> { "Authority name is required" }));
            }

            _logger.LogInformation("Retrieving signatures for authority: {AuthorityName}", authorityName);

            var signatures = await _proxKeyService.GetSignaturesByAuthorityAsync(authorityName);

            _logger.LogInformation("Retrieved {Count} signatures for authority {AuthorityName}", signatures.Count(), authorityName);

            // Convert entities to DTOs
            var signatureDtos = signatures.Select(s => new DigitalSignatureDto
            {
                Id = s.Id,
                SignatureName = s.SignatureName,
                AuthorityName = s.AuthorityName,
                AuthorityDesignation = s.AuthorityDesignation,
                SignatureImagePath = s.SignatureImagePath,
                SignatureData = s.SignatureData,
                SignatureDate = s.SignatureDate,
                IsActive = s.IsActive,
                SortOrder = s.SortOrder,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedBy = s.UpdatedBy
            });

            return Ok(ApiResponse<IEnumerable<DigitalSignatureDto>>.SuccessResult(signatureDtos, "Signatures retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving signatures for authority {AuthorityName}", authorityName);
            return StatusCode(500, ApiResponse<IEnumerable<DigitalSignatureDto>>.ErrorResult("Error retrieving signatures", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get latest signature
    /// </summary>
    [HttpGet("latest")]
    public async Task<ActionResult<ApiResponse<DigitalSignatureDto>>> GetLatestSignature()
    {
        try
        {
            _logger.LogInformation("Retrieving latest digital signature");

            var signature = await _proxKeyService.GetLatestSignatureAsync();

            if (signature == null)
            {
                return NotFound(ApiResponse<DigitalSignatureDto>.ErrorResult("No signatures found", new List<string> { "No digital signatures exist" }));
            }

            _logger.LogInformation("Latest signature retrieved successfully. Signature ID: {SignatureId}", signature.Id);

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

            return Ok(ApiResponse<DigitalSignatureDto>.SuccessResult(signatureDto, "Latest signature retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest signature");
            return StatusCode(500, ApiResponse<DigitalSignatureDto>.ErrorResult("Error retrieving latest signature", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Test PROXKey device connection
    /// </summary>
    [HttpPost("test-connection")]
    public async Task<ActionResult<ApiResponse<DeviceConnectionTestResult>>> TestDeviceConnection()
    {
        try
        {
            _logger.LogInformation("Testing PROXKey device connection");

            var deviceInfo = await _proxKeyService.GetDeviceInfoAsync();
            var isConnected = deviceInfo.IsConnected;

            var result = new DeviceConnectionTestResult
            {
                IsConnected = isConnected,
                DeviceName = deviceInfo.DeviceName,
                SerialNumber = deviceInfo.SerialNumber,
                FirmwareVersion = deviceInfo.FirmwareVersion,
                TestTimestamp = DateTime.UtcNow,
                Status = isConnected ? "Connected" : "Disconnected"
            };

            _logger.LogInformation("PROXKey device connection test completed. Status: {Status}", result.Status);

            return Ok(ApiResponse<DeviceConnectionTestResult>.SuccessResult(result, "Device connection test completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing PROXKey device connection");
            return StatusCode(500, ApiResponse<DeviceConnectionTestResult>.ErrorResult("Error testing device connection", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get signature statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<SignatureStats>>> GetSignatureStats()
    {
        try
        {
            _logger.LogInformation("Retrieving digital signature statistics");

            var latestSignature = await _proxKeyService.GetLatestSignatureAsync();
            var deviceInfo = await _proxKeyService.GetDeviceInfoAsync();

            var stats = new SignatureStats
            {
                TotalSignatures = 0, // This would need to be implemented in the service
                ActiveSignatures = 0, // This would need to be implemented in the service
                DeviceConnected = deviceInfo.IsConnected,
                DeviceName = deviceInfo.DeviceName,
                AvailableSignatures = deviceInfo.AvailableSignatures?.Count ?? 0,
                LastSignatureDate = latestSignature?.CreatedAt,
                LastSignatureAuthority = latestSignature?.AuthorityName
            };

            _logger.LogInformation("Digital signature statistics retrieved successfully");

            return Ok(ApiResponse<SignatureStats>.SuccessResult(stats, "Signature statistics retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving signature statistics");
            return StatusCode(500, ApiResponse<SignatureStats>.ErrorResult("Error retrieving statistics", new List<string> { ex.Message }));
        }
    }
}

// Request and response models
public class ValidateSignatureRequest
{
    public string SignatureData { get; set; } = string.Empty;
}

public class DeviceConnectionTestResult
{
    public bool IsConnected { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string FirmwareVersion { get; set; } = string.Empty;
    public DateTime TestTimestamp { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SignatureStats
{
    public int TotalSignatures { get; set; }
    public int ActiveSignatures { get; set; }
    public bool DeviceConnected { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public int AvailableSignatures { get; set; }
    public DateTime? LastSignatureDate { get; set; }
    public string? LastSignatureAuthority { get; set; }
}
