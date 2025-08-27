using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using DocHub.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.IO;

namespace DocHub.Infrastructure.Services;

public class DigitalSignatureWorkflowService : IDigitalSignatureWorkflowService
{
    private readonly ILogger<DigitalSignatureWorkflowService> _logger;
    private readonly IDigitalSignatureService _digitalSignatureService;
    private readonly IFileStorageService _fileStorageService;

    public DigitalSignatureWorkflowService(
        ILogger<DigitalSignatureWorkflowService> logger,
        IDigitalSignatureService digitalSignatureService,
        IFileStorageService fileStorageService)
    {
        _logger = logger;
        _digitalSignatureService = digitalSignatureService;
        _fileStorageService = fileStorageService;
    }

    public async Task<DigitalSignature> GenerateSignatureFromPendriveAsync(
        string pendrivePath,
        string userId,
        string? signaturePurpose = null)
    {
        try
        {
            _logger.LogInformation("Generating digital signature from pendrive: {PendrivePath}", pendrivePath);

            // Validate pendrive path
            if (!Directory.Exists(pendrivePath))
                throw new DirectoryNotFoundException($"Pendrive path not found: {pendrivePath}");

            // Look for signature files on pendrive
            var signatureFiles = FindSignatureFilesOnPendrive(pendrivePath);
            if (!signatureFiles.Any())
                throw new InvalidOperationException("No signature files found on pendrive");

            // Generate signature hash from pendrive content
            var signatureHash = await GeneratePendriveSignatureHashAsync(pendrivePath);

            // Create digital signature record
            var digitalSignature = new DigitalSignature
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                SignatureType = "pendrive",
                SignatureHash = signatureHash,
                SignaturePurpose = signaturePurpose ?? "document_signing",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddYears(1), // Signatures expire after 1 year
                Metadata = JsonSerializer.Serialize(new
                {
                    PendrivePath = pendrivePath,
                    SignatureFiles = signatureFiles,
                    GeneratedAt = DateTime.UtcNow,
                    DeviceInfo = GetDeviceInfo(pendrivePath)
                })
            };

            // Save signature to database
            await _digitalSignatureService.CreateAsync(digitalSignature);

            _logger.LogInformation("Digital signature generated successfully: {SignatureId}", digitalSignature.Id);
            return digitalSignature;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating signature from pendrive: {PendrivePath}", pendrivePath);
            throw;
        }
    }

    public async Task<bool> ValidateSignatureAsync(string signatureId, string documentHash)
    {
        try
        {
            _logger.LogInformation("Validating digital signature: {SignatureId}", signatureId);

            var signature = await _digitalSignatureService.GetByIdAsync(signatureId);
            if (signature == null)
            {
                _logger.LogWarning("Signature not found: {SignatureId}", signatureId);
                return false;
            }

            // Check if signature is active and not expired
            if (!signature.IsActive || signature.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Signature is inactive or expired: {SignatureId}", signatureId);
                return false;
            }

            // Validate signature hash
            var isValid = await ValidateSignatureHashAsync(signature, documentHash);

            _logger.LogInformation("Signature validation result: {SignatureId} - {IsValid}", signatureId, isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating signature: {SignatureId}", signatureId);
            return false;
        }
    }

    public async Task<DigitalSignature> GetLatestUserSignatureAsync(string userId)
    {
        try
        {
            var signatures = await _digitalSignatureService.GetByUserIdAsync(userId);
            var latestSignature = signatures
                .Where(s => s.IsActive && s.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefault();

            if (latestSignature == null)
            {
                _logger.LogWarning("No active signature found for user: {UserId}", userId);
                throw new InvalidOperationException("No active signature found for user");
            }

            return latestSignature;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest signature for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<DigitalSignature> RenewSignatureAsync(string signatureId, string userId)
    {
        try
        {
            _logger.LogInformation("Renewing digital signature: {SignatureId}", signatureId);

            var existingSignature = await _digitalSignatureService.GetByIdAsync(signatureId);
            if (existingSignature == null)
                throw new InvalidOperationException($"Signature not found: {signatureId}");

            if (existingSignature.UserId != userId)
                throw new UnauthorizedAccessException("User not authorized to renew this signature");

            // Create new signature with extended expiry
            var renewedSignature = new DigitalSignature
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                SignatureType = existingSignature.SignatureType,
                SignatureHash = existingSignature.SignatureHash,
                SignaturePurpose = existingSignature.SignaturePurpose,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddYears(1),
                Metadata = existingSignature.Metadata
            };

            // Deactivate old signature
            existingSignature.IsActive = false;
            existingSignature.UpdatedAt = DateTime.UtcNow;
            await _digitalSignatureService.UpdateAsync(existingSignature.Id, existingSignature);

            // Save new signature
            await _digitalSignatureService.CreateAsync(renewedSignature);

            _logger.LogInformation("Signature renewed successfully: {NewSignatureId}", renewedSignature.Id);
            return renewedSignature;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing signature: {SignatureId}", signatureId);
            throw;
        }
    }

    public async Task<bool> RevokeSignatureAsync(string signatureId, string userId, string reason)
    {
        try
        {
            _logger.LogInformation("Revoking digital signature: {SignatureId}", signatureId);

            var signature = await _digitalSignatureService.GetByIdAsync(signatureId);
            if (signature == null)
                return false;

            if (signature.UserId != userId)
                throw new UnauthorizedAccessException("User not authorized to revoke this signature");

            // Revoke signature
            signature.IsActive = false;
            signature.RevokedAt = DateTime.UtcNow;
            signature.RevokedBy = userId;
            signature.RevocationReason = reason;
            signature.UpdatedAt = DateTime.UtcNow;

            await _digitalSignatureService.UpdateAsync(signatureId, signature);

            _logger.LogInformation("Signature revoked successfully: {SignatureId}", signatureId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking signature: {SignatureId}", signatureId);
            throw;
        }
    }

    public async Task<IEnumerable<DigitalSignature>> GetSignatureHistoryAsync(string userId)
    {
        try
        {
            var signatures = await _digitalSignatureService.GetByUserIdAsync(userId);
            return signatures.OrderByDescending(s => s.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting signature history for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<SignatureValidationResult> ValidateDocumentSignatureAsync(
        string documentPath,
        string signatureId)
    {
        try
        {
            _logger.LogInformation("Validating document signature: {DocumentPath}", documentPath);

            // Calculate document hash
            var documentHash = await CalculateDocumentHashAsync(documentPath);

            // Validate signature
            var isValid = await ValidateSignatureAsync(signatureId, documentHash);

            var result = new SignatureValidationResult
            {
                DocumentPath = documentPath,
                SignatureId = signatureId,
                DocumentHash = documentHash,
                IsValid = isValid,
                ValidatedAt = DateTime.UtcNow
            };

            if (isValid)
            {
                var signature = await _digitalSignatureService.GetByIdAsync(signatureId);
                result.SignatureDetails = new Dictionary<string, string>
                {
                    { "UserId", signature?.UserId?.ToString() ?? "" },
                    { "CreatedAt", signature?.CreatedAt.ToString() ?? "" },
                    { "ExpiresAt", signature?.ExpiresAt.ToString() ?? "" },
                    { "SignatureType", signature?.SignatureType?.ToString() ?? "" }
                };
            }

            _logger.LogInformation("Document signature validation completed: {DocumentPath} - {IsValid}",
                documentPath, isValid);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating document signature: {DocumentPath}", documentPath);
            throw;
        }
    }

    private List<string> FindSignatureFilesOnPendrive(string pendrivePath)
    {
        var signatureFiles = new List<string>();
        var signatureExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".svg", ".gif" };

        try
        {
            var files = Directory.GetFiles(pendrivePath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file).ToLowerInvariant();
                if (signatureExtensions.Contains(extension))
                {
                    signatureFiles.Add(file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error scanning pendrive for signature files: {PendrivePath}", pendrivePath);
        }

        return signatureFiles;
    }

    private async Task<string> GeneratePendriveSignatureHashAsync(string pendrivePath)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var sha256 = SHA256.Create();
                var hashBuilder = new StringBuilder();

                // Get pendrive volume info
                var driveInfo = new DriveInfo(pendrivePath);
                if (driveInfo.IsReady)
                {
                    var volumeInfo = $"{driveInfo.VolumeLabel}_{driveInfo.DriveFormat}_{driveInfo.TotalSize}";
                    var volumeBytes = Encoding.UTF8.GetBytes(volumeInfo);
                    var volumeHash = sha256.ComputeHash(volumeBytes);
                    hashBuilder.Append(Convert.ToBase64String(volumeHash));
                }

                // Get file listing and modification times
                var files = Directory.GetFiles(pendrivePath, "*.*", SearchOption.AllDirectories);
                foreach (var file in files.OrderBy(f => f))
                {
                    var fileInfo = new System.IO.FileInfo(file);
                    var fileData = $"{fileInfo.Name}_{fileInfo.Length}_{fileInfo.LastWriteTimeUtc:yyyyMMddHHmmss}";
                    var fileBytes = Encoding.UTF8.GetBytes(fileData);
                    var fileHash = sha256.ComputeHash(fileBytes);
                    hashBuilder.Append(Convert.ToBase64String(fileHash));
                }

                // Generate final hash
                var finalData = Encoding.UTF8.GetBytes(hashBuilder.ToString());
                var finalHash = sha256.ComputeHash(finalData);

                return Convert.ToBase64String(finalHash);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating pendrive signature hash");
            throw;
        }
    }

    private async Task<bool> ValidateSignatureHashAsync(DigitalSignature signature, string documentHash)
    {
        try
        {
            return await Task.Run(() =>
            {
                // For pendrive signatures, we validate based on the signature hash
                // In a real implementation, you might want to implement more sophisticated validation

                if (string.IsNullOrEmpty(signature.SignatureHash))
                    return false;

                // Basic validation - in production, implement proper cryptographic validation
                return !string.IsNullOrEmpty(signature.SignatureHash) && signature.IsActive;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating signature hash");
            return false;
        }
    }

    private async Task<string> CalculateDocumentHashAsync(string documentPath)
    {
        try
        {
            using var sha256 = SHA256.Create();
            using var fileStream = File.OpenRead(documentPath);
            var hash = await sha256.ComputeHashAsync(fileStream);
            return Convert.ToBase64String(hash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating document hash: {DocumentPath}", documentPath);
            throw;
        }
    }

    private string GetDeviceInfo(string pendrivePath)
    {
        try
        {
            var driveInfo = new DriveInfo(pendrivePath);
            if (driveInfo.IsReady)
            {
                return JsonSerializer.Serialize(new
                {
                    DriveName = driveInfo.Name,
                    VolumeLabel = driveInfo.VolumeLabel,
                    DriveFormat = driveInfo.DriveFormat,
                    DriveType = driveInfo.DriveType.ToString(),
                    TotalSize = driveInfo.TotalSize,
                    AvailableFreeSpace = driveInfo.AvailableFreeSpace
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting device info for: {PendrivePath}", pendrivePath);
        }

        return "{}";
    }
}


