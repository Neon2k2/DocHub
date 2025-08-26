using Microsoft.AspNetCore.Mvc;
using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PROXKeyController : ControllerBase
{
    private readonly IPROXKeyService _proxKeyService;
    private readonly ILogger<PROXKeyController> _logger;

    public PROXKeyController(
        IPROXKeyService proxKeyService,
        ILogger<PROXKeyController> logger)
    {
        _proxKeyService = proxKeyService;
        _logger = logger;
    }

    [HttpGet("status")]
    public async Task<ActionResult<PROXKeyStatus>> GetDeviceStatus()
    {
        try
        {
            var isConnected = await _proxKeyService.IsPROXKeyConnectedAsync();
            var deviceInfo = await _proxKeyService.GetPROXKeyInfoAsync();
            var isValid = await _proxKeyService.ValidatePROXKeyAsync();

            var status = new PROXKeyStatus
            {
                IsConnected = isConnected,
                IsValid = isValid,
                DeviceInfo = deviceInfo,
                LastChecked = DateTime.UtcNow
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PROXKey device status");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("info")]
    public async Task<ActionResult<PROXKeyInfoDto>> GetDeviceInfo()
    {
        try
        {
            var deviceInfo = await _proxKeyService.GetPROXKeyInfoAsync();
            return Ok(deviceInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PROXKey device info");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("test-connection")]
    public async Task<ActionResult<ConnectionTestResult>> TestConnection()
    {
        try
        {
            var isConnected = await _proxKeyService.IsPROXKeyConnectedAsync();
            var isValid = await _proxKeyService.ValidatePROXKeyAsync();

            var result = new ConnectionTestResult
            {
                IsConnected = isConnected,
                IsValid = isValid,
                TestedAt = DateTime.UtcNow,
                Message = isConnected && isValid ? "Device connected and validated successfully" : "Device connection or validation failed"
            };

            if (result.IsConnected && result.IsValid)
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
            _logger.LogError(ex, "Error testing PROXKey connection");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("generate-signature")]
    public async Task<ActionResult<SignatureGenerationResult>> GenerateSignature(
        [FromBody] GenerateSignatureRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check device connection first
            if (!await _proxKeyService.IsPROXKeyConnectedAsync())
            {
                return BadRequest(new SignatureGenerationResult
                {
                    Success = false,
                    Message = "PROXKey device is not connected",
                    GeneratedAt = DateTime.UtcNow
                });
            }

            // Validate device
            if (!await _proxKeyService.ValidatePROXKeyAsync())
            {
                return BadRequest(new SignatureGenerationResult
                {
                    Success = false,
                    Message = "PROXKey device validation failed",
                    GeneratedAt = DateTime.UtcNow
                });
            }

            // Generate signature
            var signatureData = await _proxKeyService.GenerateSignatureAsync(request);

            if (signatureData != null && !string.IsNullOrEmpty(signatureData.SignatureData))
            {
                var result = new SignatureGenerationResult
                {
                    Success = true,
                    AuthorityName = request.AuthorityName,
                    AuthorityDesignation = request.AuthorityDesignation,
                    SignatureSize = signatureData.SignatureData?.Length ?? 0,
                    GeneratedAt = DateTime.UtcNow,
                    Message = "Signature generated successfully"
                };

                _logger.LogInformation("Signature generated successfully for {AuthorityName}", request.AuthorityName);
                return Ok(result);
            }
            else
            {
                return BadRequest(new SignatureGenerationResult
                {
                    Success = false,
                    Message = "Failed to generate signature data",
                    GeneratedAt = DateTime.UtcNow
                });
            }
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new SignatureGenerationResult
            {
                Success = false,
                Message = ex.Message,
                GeneratedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating signature from PROXKey");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("signature/{authorityName}")]
    public async Task<IActionResult> GetSignatureImage(string authorityName)
    {
        try
        {
            var signatureData = await _proxKeyService.GetSignatureImageAsync(authorityName);
            
            if (signatureData != null && signatureData.Length > 0)
            {
                var fileName = $"signature_{authorityName.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
                return File(signatureData, "image/png", fileName);
            }
            else
            {
                return NotFound("Signature image not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting signature image for {AuthorityName}", authorityName);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("validate")]
    public async Task<ActionResult<PROXKeyValidationResult>> ValidateDevice()
    {
        try
        {
            var isConnected = await _proxKeyService.IsPROXKeyConnectedAsync();
            var isValid = await _proxKeyService.ValidatePROXKeyAsync();

            var result = new PROXKeyValidationResult
            {
                IsConnected = isConnected,
                IsValid = isValid,
                ValidatedAt = DateTime.UtcNow,
                Status = isConnected && isValid ? "Valid" : "Invalid",
                Message = isConnected && isValid ? "Device is connected and valid" : "Device connection or validation failed"
            };

            if (result.IsConnected && result.IsValid)
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
            _logger.LogError(ex, "Error validating PROXKey device");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("diagnostics")]
    public async Task<ActionResult<DiagnosticsResult>> RunDiagnostics()
    {
        try
        {
            var diagnostics = new DiagnosticsResult
            {
                Timestamp = DateTime.UtcNow,
                Tests = new List<DiagnosticTest>()
            };

            // Test 1: Connection
            var connectionTest = new DiagnosticTest
            {
                Name = "Device Connection",
                Status = "Running"
            };

            try
            {
                var isConnected = await _proxKeyService.IsPROXKeyConnectedAsync();
                connectionTest.Status = isConnected ? "Passed" : "Failed";
                connectionTest.Message = isConnected ? "Device is connected" : "Device is not connected";
                connectionTest.Duration = 100; // Mock duration
            }
            catch (Exception ex)
            {
                connectionTest.Status = "Error";
                connectionTest.Message = ex.Message;
                connectionTest.Duration = 0;
            }

            diagnostics.Tests.Add(connectionTest);

            // Test 2: Validation
            var validationTest = new DiagnosticTest
            {
                Name = "Device Validation",
                Status = "Running"
            };

            try
            {
                var isValid = await _proxKeyService.ValidatePROXKeyAsync();
                validationTest.Status = isValid ? "Passed" : "Failed";
                validationTest.Message = isValid ? "Device validation successful" : "Device validation failed";
                validationTest.Duration = 200; // Mock duration
            }
            catch (Exception ex)
            {
                validationTest.Status = "Error";
                validationTest.Message = ex.Message;
                validationTest.Duration = 0;
            }

            diagnostics.Tests.Add(validationTest);

            // Test 3: Signature Generation (Test mode)
            var signatureTest = new DiagnosticTest
            {
                Name = "Signature Generation",
                Status = "Running"
            };

            try
            {
                var testRequest = new GenerateSignatureRequest
                {
                    AuthorityName = "Test Authority",
                    AuthorityDesignation = "Test Designation"
                };
                var testSignature = await _proxKeyService.GenerateSignatureAsync(testRequest);
                signatureTest.Status = testSignature != null && !string.IsNullOrEmpty(testSignature.SignatureData) ? "Passed" : "Failed";
                signatureTest.Message = testSignature != null && !string.IsNullOrEmpty(testSignature.SignatureData) ? "Signature generation successful" : "Signature generation failed";
                signatureTest.Duration = 1500; // Mock duration
            }
            catch (Exception ex)
            {
                signatureTest.Status = "Error";
                signatureTest.Message = ex.Message;
                signatureTest.Duration = 0;
            }

            diagnostics.Tests.Add(signatureTest);

            // Calculate overall status
            var passedTests = diagnostics.Tests.Count(t => t.Status == "Passed");
            var totalTests = diagnostics.Tests.Count;
            diagnostics.OverallStatus = passedTests == totalTests ? "Healthy" : "Issues Detected";
            diagnostics.SuccessRate = totalTests > 0 ? Math.Round((double)passedTests / totalTests * 100, 2) : 0;

            return Ok(diagnostics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running PROXKey diagnostics");
            return StatusCode(500, "Internal server error");
        }
    }
}
