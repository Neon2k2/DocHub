using DocHub.Core.Entities;
using DocHub.Infrastructure.Data;
using DocHub.Infrastructure.Repositories;
using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DocHub.Infrastructure.Services;

public class DigitalSignatureService : IDigitalSignatureService
{
    private readonly DocHubDbContext _context;
    private readonly IGenericRepository<DigitalSignature> _repository;
    private readonly IPROXKeyService _proxKeyService;
    private readonly ILogger<DigitalSignatureService> _logger;

    public DigitalSignatureService(
        DocHubDbContext context,
        IGenericRepository<DigitalSignature> repository,
        IPROXKeyService proxKeyService,
        ILogger<DigitalSignatureService> logger)
    {
        _context = context;
        _repository = repository;
        _proxKeyService = proxKeyService;
        _logger = logger;
    }

    public async Task<IEnumerable<DigitalSignature>> GetAllAsync()
    {
        return await _context.DigitalSignatures
            .OrderBy(s => s.SortOrder)
            .ToListAsync();
    }

    public async Task<DigitalSignature?> GetByIdAsync(string id)
    {
        return await _context.DigitalSignatures.FindAsync(id);
    }

    public async Task<DigitalSignature> CreateAsync(DigitalSignature signature)
    {
        if (await ExistsAsync(signature.AuthorityName))
        {
            throw new InvalidOperationException($"Signature for authority '{signature.AuthorityName}' already exists.");
        }

        signature.Id = Guid.NewGuid().ToString();
        signature.CreatedAt = DateTime.UtcNow;
        signature.UpdatedAt = DateTime.UtcNow;

        if (signature.SortOrder == 0)
        {
            signature.SortOrder = await GetNextSortOrderAsync();
        }

        var result = await _repository.AddAsync(signature);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<DigitalSignature> UpdateAsync(string id, DigitalSignature signature)
    {
        DigitalSignature? existingSignature = await GetByIdAsync(id);
        if (existingSignature == null)
        {
            throw new InvalidOperationException($"Signature with id '{id}' not found.");
        }

        if (signature.AuthorityName != existingSignature.AuthorityName && await ExistsAsync(signature.AuthorityName))
        {
            throw new InvalidOperationException($"Signature for authority '{signature.AuthorityName}' already exists.");
        }

        existingSignature.AuthorityName = signature.AuthorityName;
        existingSignature.AuthorityDesignation = signature.AuthorityDesignation;
        existingSignature.SignatureImagePath = signature.SignatureImagePath;
        existingSignature.SignatureData = signature.SignatureData;
        existingSignature.IsActive = signature.IsActive;
        existingSignature.SortOrder = signature.SortOrder;
        existingSignature.UpdatedAt = DateTime.UtcNow;

        var result = await _repository.UpdateAsync(existingSignature);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        DigitalSignature? signature = await GetByIdAsync(id);
        if (signature == null)
        {
            return false;
        }

        // Check if signature has generated letters
        var hasGeneratedLetters = await _context.GeneratedLetters
            .AnyAsync(gl => gl.DigitalSignatureId == id);

        if (hasGeneratedLetters)
        {
            throw new InvalidOperationException("Cannot delete signature that has generated letters.");
        }

        var result = await _repository.DeleteAsync(id);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<IEnumerable<DigitalSignature>> GetActiveSignaturesAsync()
    {
        return await _context.DigitalSignatures
            .Where(s => s.IsActive)
            .OrderBy(s => s.SortOrder)
            .ToListAsync();
    }

    public async Task<DigitalSignature?> GetByAuthorityNameAsync(string authorityName)
    {
        return await _context.DigitalSignatures
            .FirstOrDefaultAsync(s => s.AuthorityName == authorityName);
    }

    public async Task<bool> ExistsAsync(string authorityName)
    {
        return await _context.DigitalSignatures.AnyAsync(s => s.AuthorityName == authorityName);
    }

    public async Task<bool> ToggleActiveAsync(string id)
    {
        DigitalSignature? signature = await GetByIdAsync(id);
        if (signature == null)
        {
            return false;
        }

        signature.IsActive = !signature.IsActive;
        signature.UpdatedAt = DateTime.UtcNow;

        await UpdateAsync(id, signature);
        return true;
    }

    public async Task<DigitalSignature> GenerateSignatureFromPROXKeyAsync(string authorityName, string authorityDesignation)
    {
        try
        {
            _logger.LogInformation("Generating signature from PROXKey for authority: {AuthorityName}", authorityName);

            // Generate signature using PROXKey service
            var request = new GenerateSignatureRequest
            {
                AuthorityName = authorityName,
                AuthorityDesignation = authorityDesignation
            };
            var signatureData = await _proxKeyService.GenerateSignatureAsync(request);

            if (signatureData == null || string.IsNullOrEmpty(signatureData.SignatureData))
            {
                throw new InvalidOperationException("Failed to generate signature from PROXKey.");
            }

            // Save signature image to file system
            var fileName = $"signature_{authorityName.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
            var filePath = Path.Combine("wwwroot", "signatures", fileName);

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            // Convert signature data to bytes if it's base64 string
            byte[] signatureBytes;
            try
            {
                signatureBytes = Convert.FromBase64String(signatureData.SignatureData);
            }
            catch
            {
                // If not base64, treat as raw bytes
                signatureBytes = System.Text.Encoding.UTF8.GetBytes(signatureData.SignatureData);
            }

            // Save the signature image
            await File.WriteAllBytesAsync(filePath, signatureBytes);

            // Create signature record
            var signature = new DigitalSignature
            {
                AuthorityName = authorityName,
                AuthorityDesignation = authorityDesignation,
                SignatureImagePath = $"/signatures/{fileName}",
                SignatureData = signatureData.SignatureData,
                IsActive = true,
                CreatedBy = "PROXKey System"
            };

            var createdSignature = await CreateAsync(signature);

            _logger.LogInformation("Signature generated successfully for authority: {AuthorityName}", authorityName);
            return createdSignature;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating signature from PROXKey for authority: {AuthorityName}", authorityName);
            throw;
        }
    }

    public async Task<byte[]> GetSignatureImageAsync(string id)
    {
        DigitalSignature? signature = await GetByIdAsync(id);
        if (signature == null)
        {
            throw new InvalidOperationException($"Signature with id '{id}' not found.");
        }

        if (string.IsNullOrEmpty(signature.SignatureImagePath))
        {
            throw new InvalidOperationException("Signature image path is not set.");
        }

        var filePath = Path.Combine("wwwroot", signature.SignatureImagePath.TrimStart('/'));
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Signature image file not found: {filePath}");
        }

        return await File.ReadAllBytesAsync(filePath);
    }

    public async Task<string> GetLatestSignaturePathAsync()
    {
        var latestSignature = await _context.DigitalSignatures
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        return latestSignature?.SignatureImagePath ?? string.Empty;
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.DigitalSignatures.CountAsync();
    }

    public async Task<IEnumerable<DigitalSignature>> GetPagedAsync(int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        return await _context.DigitalSignatures
            .OrderBy(s => s.SortOrder)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    private async Task<int> GetNextSortOrderAsync()
    {
        var maxSortOrder = await _context.DigitalSignatures
            .MaxAsync(s => (int?)s.SortOrder) ?? 0;
        return maxSortOrder + 1;
    }

    public async Task<IEnumerable<DigitalSignature>> GetByUserIdAsync(string userId)
    {
        return await _context.DigitalSignatures
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }
}
