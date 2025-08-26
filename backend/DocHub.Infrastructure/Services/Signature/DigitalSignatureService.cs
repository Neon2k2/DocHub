using DocHub.Application.DTOs;
using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using DocHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DocHub.Infrastructure.Services.Signature;

public class SignatureService : ISignatureService
{
    private readonly DocHubDbContext _context;
    private readonly ILogger<SignatureService> _logger;

    public SignatureService(DocHubDbContext context, ILogger<SignatureService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DigitalSignatureDto> GenerateSignatureAsync(string authorityName, string authorityDesignation, string signatureData)
    {
        try
        {
            // Generate a unique signature ID
            var signatureId = Guid.NewGuid().ToString();
            
            // Create signature image path (this would typically come from a pendrive device)
            var signatureImagePath = $"/uploads/signatures/signature_{signatureId}.png";
            
            // Create new digital signature
            var signature = new DigitalSignature
            {
                Id = signatureId,
                AuthorityName = authorityName,
                AuthorityDesignation = authorityDesignation,
                SignatureImagePath = signatureImagePath,
                SignatureData = signatureData,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };
            
            _context.DigitalSignatures.Add(signature);
            await _context.SaveChangesAsync();
            
            // Convert to DTO
            return new DigitalSignatureDto
            {
                Id = signature.Id,
                AuthorityName = signature.AuthorityName,
                AuthorityDesignation = signature.AuthorityDesignation,
                SignatureImagePath = signature.SignatureImagePath,
                SignatureData = signature.SignatureData,
                IsActive = signature.IsActive,
                SortOrder = signature.SortOrder,
                CreatedAt = signature.CreatedAt,
                CreatedBy = signature.CreatedBy,
                UpdatedAt = signature.UpdatedAt,
                UpdatedBy = signature.UpdatedBy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating signature for {AuthorityName}", authorityName);
            throw new Exception($"Error generating signature: {ex.Message}");
        }
    }

    public async Task<DigitalSignatureDto> GetLatestSignatureAsync()
    {
        try
        {
            var latestSignature = await _context.DigitalSignatures
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
            
            if (latestSignature == null)
                throw new InvalidOperationException("No active signatures found");
            
            return new DigitalSignatureDto
            {
                Id = latestSignature.Id,
                AuthorityName = latestSignature.AuthorityName,
                AuthorityDesignation = latestSignature.AuthorityDesignation,
                SignatureImagePath = latestSignature.SignatureImagePath,
                SignatureData = latestSignature.SignatureData,
                IsActive = latestSignature.IsActive,
                SortOrder = latestSignature.SortOrder,
                CreatedAt = latestSignature.CreatedAt,
                CreatedBy = latestSignature.CreatedBy,
                UpdatedAt = latestSignature.UpdatedAt,
                UpdatedBy = latestSignature.UpdatedBy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest signature");
            throw new Exception($"Error getting latest signature: {ex.Message}");
        }
    }

    public async Task<DigitalSignatureDto> GetSignatureByAuthorityAsync(string authorityName)
    {
        try
        {
            var signature = await _context.DigitalSignatures
                .Where(s => s.AuthorityName == authorityName && s.IsActive)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
            
            if (signature == null)
                throw new InvalidOperationException($"No active signature found for authority: {authorityName}");
            
            return new DigitalSignatureDto
            {
                Id = signature.Id,
                AuthorityName = signature.AuthorityName,
                AuthorityDesignation = signature.AuthorityDesignation,
                SignatureImagePath = signature.SignatureImagePath,
                SignatureData = signature.SignatureData,
                IsActive = signature.IsActive,
                SortOrder = signature.SortOrder,
                CreatedAt = signature.CreatedAt,
                CreatedBy = signature.CreatedBy,
                UpdatedAt = signature.UpdatedAt,
                UpdatedBy = signature.UpdatedBy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting signature for authority {AuthorityName}", authorityName);
            throw new Exception($"Error getting signature for authority: {ex.Message}");
        }
    }

    public async Task<bool> ValidateSignatureAsync(string signatureData)
    {
        try
        {
            // Basic validation - check if signature data exists and is not empty
            if (string.IsNullOrWhiteSpace(signatureData))
                return false;
            
            // Check if signature exists in database
            var signatureExists = await _context.DigitalSignatures
                .AnyAsync(s => s.SignatureData == signatureData && s.IsActive);
            
            return signatureExists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating signature");
            return false;
        }
    }

    public async Task<string> ApplySignatureToDocumentAsync(byte[] documentBytes, DigitalSignatureDto signature)
    {
        try
        {
            // Generate unique filename for signed document
            var fileName = $"signed_document_{signature.Id}_{DateTime.Now:yyyyMMddHHmmss}.docx";
            var filePath = Path.Combine("wwwroot", "uploads", "signed", fileName);
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory!);
            
            // Save the signed document
            await File.WriteAllBytesAsync(filePath, documentBytes);
            
            return $"/uploads/signed/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying signature to document");
            throw new Exception($"Error applying signature to document: {ex.Message}");
        }
    }
}
