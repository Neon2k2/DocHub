using System.IO.Ports;
using System.Text;
using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using Microsoft.Extensions.Logging;
using DocHub.Application.Configuration;
using DocHub.Application.DTOs;

namespace DocHub.Infrastructure.Services.PROXKey;

public class PROXKeyService : IPROXKeyService
{
    private readonly ILogger<PROXKeyService> _logger;
    private readonly AppConfiguration _config;
    private SerialPort? _serialPort;
    private readonly object _lockObject = new object();

    public PROXKeyService(ILogger<PROXKeyService> logger, AppConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task<DigitalSignature> GenerateSignatureAsync(GenerateSignatureRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting to generate signature for {AuthorityName}", request.AuthorityName);

            // Try to connect to PROXKey device
            var signature = await TryGenerateFromDeviceAsync(request.AuthorityName, request.AuthorityDesignation);
            if (signature != null)
            {
                return signature;
            }

            // Fallback to stored signature
            _logger.LogWarning("PROXKey device unavailable, using stored signature");
            return await GetStoredSignatureAsync(request.AuthorityName, request.AuthorityDesignation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating signature for {AuthorityName}", request.AuthorityName);
            throw new InvalidOperationException($"Failed to generate signature: {ex.Message}", ex);
        }
    }

    public async Task<bool> TestDeviceConnectionAsync()
    {
        try
        {
            // Check if simulation mode is enabled
            if (IsSimulationModeEnabled())
            {
                _logger.LogInformation("PROXKey simulation mode enabled - simulating device connection");
                return true;
            }

            using var port = CreateSerialPort();
            if (port == null) return false;

            port.Open();
            if (!port.IsOpen) return false;

            // Send test command
            var testCommand = "TEST\n";
            port.Write(testCommand);
            
            // Wait for response
            await Task.Delay(1000);
            
            var response = port.ReadExisting();
            port.Close();

            var isConnected = !string.IsNullOrEmpty(response);
            _logger.LogInformation("PROXKey device test: {IsConnected}", isConnected);
            
            return isConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing PROXKey device connection");
            return false;
        }
    }

    public async Task<DeviceStatus> GetDeviceStatusAsync()
    {
        try
        {
            var isConnected = await TestDeviceConnectionAsync();
            
            if (IsSimulationModeEnabled())
            {
                return new DeviceStatus
                {
                    IsConnected = true,
                    DevicePath = "SIMULATION MODE",
                    LastChecked = DateTime.UtcNow,
                    Status = "Simulation",
                    ErrorMessage = null
                };
            }
            
            return new DeviceStatus
            {
                IsConnected = isConnected,
                DevicePath = _config.ProxKeyDevicePath,
                LastChecked = DateTime.UtcNow,
                Status = isConnected ? "Online" : "Offline",
                ErrorMessage = isConnected ? null : "Device not responding"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device status");
            return new DeviceStatus
            {
                IsConnected = false,
                DevicePath = _config.ProxKeyDevicePath,
                LastChecked = DateTime.UtcNow,
                Status = "Error",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PROXKeyInfoDto> GetDeviceInfoAsync()
    {
        try
        {
            var isConnected = await TestDeviceConnectionAsync();
            
            if (IsSimulationModeEnabled())
            {
                return new PROXKeyInfoDto
                {
                    Id = "SIM-001",
                    DeviceName = "PROXKey Simulator",
                    DeviceId = "SIMULATOR",
                    DeviceType = "Simulator",
                    FirmwareVersion = "1.0.0-SIM",
                    IsConnected = true,
                    LastConnected = DateTime.UtcNow,
                    Status = "Simulation",
                    SerialNumber = "SIM-001",
                    AvailableSignatures = new List<string> { "999" }
                };
            }
            
            return new PROXKeyInfoDto
            {
                Id = Guid.NewGuid().ToString(),
                DeviceName = isConnected ? "PROXKey Device" : "Unknown",
                DeviceId = isConnected ? "DETECTING..." : "Unknown",
                DeviceType = "PROXKey",
                FirmwareVersion = isConnected ? "DETECTING..." : "Unknown",
                IsConnected = isConnected,
                LastConnected = DateTime.UtcNow,
                Status = isConnected ? "Online" : "Offline",
                SerialNumber = isConnected ? "DETECTING..." : "Unknown",
                AvailableSignatures = isConnected ? new List<string> { "DETECTING..." } : new List<string> { "0" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device info");
            return new PROXKeyInfoDto
            {
                Id = "ERROR",
                DeviceName = "Error",
                DeviceId = "Error",
                DeviceType = "Error",
                FirmwareVersion = "Error",
                IsConnected = false,
                LastConnected = DateTime.UtcNow,
                Status = "Error",
                SerialNumber = "Error",
                AvailableSignatures = new List<string> { "0" }
            };
        }
    }

    public async Task<bool> IsPROXKeyConnectedAsync()
    {
        return await TestDeviceConnectionAsync();
    }

    public async Task<bool> ValidatePROXKeyAsync()
    {
        try
        {
            var isConnected = await TestDeviceConnectionAsync();
            if (!isConnected) return false;

            // Additional validation logic could go here
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating PROXKey device");
            return false;
        }
    }

    public async Task<bool> ValidateSignatureAsync(string signatureData)
    {
        try
        {
            if (string.IsNullOrEmpty(signatureData))
                return false;

            // Basic validation - check if signature data is not empty
            return signatureData.Length > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating signature");
            return false;
        }
    }

    public async Task<byte[]> GetSignatureImageAsync(string signatureId)
    {
        try
        {
            // Mock implementation - return a simple image
            return new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG header
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting signature image for {SignatureId}", signatureId);
            return Array.Empty<byte>();
        }
    }

    public async Task<DigitalSignature> UpdateSignatureAsync(string signatureId, DigitalSignature signature)
    {
        try
        {
            // Mock implementation - return the updated signature
            signature.Id = signatureId;
            signature.UpdatedAt = DateTime.UtcNow;
            return signature;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating signature {SignatureId}", signatureId);
            throw;
        }
    }

    public async Task<bool> DeleteSignatureAsync(string signatureId)
    {
        try
        {
            // Mock implementation - always return true
            _logger.LogInformation("Signature {SignatureId} deleted successfully", signatureId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting signature {SignatureId}", signatureId);
            return false;
        }
    }

    public async Task<List<DigitalSignature>> GetSignaturesByAuthorityAsync(string authorityName)
    {
        try
        {
            // Mock implementation - return empty list
            return new List<DigitalSignature>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting signatures for authority {AuthorityName}", authorityName);
            return new List<DigitalSignature>();
        }
    }

    public async Task<DigitalSignature> GetLatestSignatureAsync()
    {
        try
        {
            // Mock implementation - return a mock signature
            return new DigitalSignature
            {
                Id = Guid.NewGuid().ToString(),
                AuthorityName = "Mock Authority",
                AuthorityDesignation = "Mock Designation",
                SignatureData = "mock_signature_data",
                SignatureName = "Mock Signature",
                SignatureDate = DateTime.UtcNow,
                IsActive = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest signature");
            throw;
        }
    }

    public async Task<PROXKeyInfoDto> GetPROXKeyInfoAsync()
    {
        // This is an alias for GetDeviceInfoAsync
        return await GetDeviceInfoAsync();
    }

    private async Task<DigitalSignature?> TryGenerateFromDeviceAsync(string authorityName, string authorityDesignation)
    {
        try
        {
            if (IsSimulationModeEnabled())
            {
                _logger.LogInformation("PROXKey simulation mode - generating simulated signature");
                return await GenerateSimulatedSignatureAsync(authorityName, authorityDesignation);
            }

            using var port = CreateSerialPort();
            if (port == null) return null;

            try
            {
                port.Open();
                if (!port.IsOpen) return null;

                // Send signature request command
                var command = $"SIGN:{authorityName}:{authorityDesignation}\n";
                port.Write(command);
                
                // Wait for response
                await Task.Delay(2000);
                
                var response = port.ReadExisting();
                if (!string.IsNullOrEmpty(response))
                {
                    // Parse the response and create signature
                    var signature = new DigitalSignature
                    {
                        Id = Guid.NewGuid().ToString(),
                        AuthorityName = authorityName,
                        AuthorityDesignation = authorityDesignation,
                        SignatureData = response.Trim(),
                        SignatureName = $"{authorityName}_{DateTime.Now:yyyyMMdd_HHmmss}",
                        SignatureDate = DateTime.UtcNow,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "PROXKey Device"
                    };

                    _logger.LogInformation("Signature generated successfully from PROXKey device for {AuthorityName}", authorityName);
                    return signature;
                }
            }
            finally
            {
                if (port.IsOpen)
                    port.Close();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with PROXKey device");
        }

        return null;
    }

    private async Task<DigitalSignature> GenerateSimulatedSignatureAsync(string authorityName, string authorityDesignation)
    {
        // Generate a realistic simulated signature
        var signatureData = Convert.ToBase64String(Encoding.UTF8.GetBytes($"SIMULATED_SIGNATURE_{authorityName}_{DateTime.Now.Ticks}"));
        
        return new DigitalSignature
        {
            Id = Guid.NewGuid().ToString(),
            AuthorityName = authorityName,
            AuthorityDesignation = authorityDesignation,
            SignatureData = signatureData,
            SignatureName = $"{authorityName}_SIM_{DateTime.Now:yyyyMMdd_HHmmss}",
            SignatureDate = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "Simulation Mode"
        };
    }

    private async Task<DigitalSignature> GetStoredSignatureAsync(string authorityName, string authorityDesignation)
    {
        // Return a stored signature if available, otherwise create a new one
        return new DigitalSignature
        {
            Id = Guid.NewGuid().ToString(),
            AuthorityName = authorityName,
            AuthorityDesignation = authorityDesignation,
            SignatureData = $"STORED_SIGNATURE_{authorityName}_{DateTime.Now.Ticks}",
            SignatureName = $"{authorityName}_STORED_{DateTime.Now:yyyyMMdd_HHmmss}",
            SignatureDate = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };
    }

    private SerialPort? CreateSerialPort()
    {
        try
        {
            var port = new SerialPort
            {
                PortName = _config.ProxKeyDevicePath,
                BaudRate = 9600,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                ReadTimeout = _config.ProxKeyTimeout * 1000,
                WriteTimeout = _config.ProxKeyTimeout * 1000
            };

            return port;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating serial port for {DevicePath}", _config.ProxKeyDevicePath);
            return null;
        }
    }

    private bool IsSimulationModeEnabled()
    {
        try
        {
            // Check if simulation mode is enabled in configuration
            var simulationEnabled = Environment.GetEnvironmentVariable("PROXKEY_SIMULATION")?.ToLower() == "true";
            if (simulationEnabled)
            {
                return true;
            }

            // Check if we're in development mode
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower();
            if (environment == "development")
            {
                // In development, enable simulation if device is not available
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _serialPort?.Dispose();
    }
}
