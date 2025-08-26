using DocHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace DocHub.Infrastructure.Services;

public class FileValidationService : IFileValidationService
{
    private readonly ILogger<FileValidationService> _logger;
    private readonly IConfiguration _configuration;

    public FileValidationService(ILogger<FileValidationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<FileValidationResult> ValidateFileAsync(IFormFile file, FileValidationOptions options)
    {
        var result = new FileValidationResult
        {
            FileName = file.FileName,
            FileSize = file.Length,
            IsValid = true,
            ValidationErrors = new List<string>(),
            Warnings = new List<string>(),
            SuggestedAction = null
        };

        try
        {
            // Check file size
            if (!IsValidFileSize(file.Length, options.MaxFileSizeInBytes))
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"File size exceeds maximum allowed size of {options.MaxFileSizeInBytes / (1024 * 1024)}MB");
            }

            // Check file extension
            if (!IsValidFileExtension(file.FileName, options.AllowedExtensions))
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"File extension not allowed. Allowed extensions: {string.Join(", ", options.AllowedExtensions)}");
            }

            // Check for viruses (if enabled)
            if (options.EnableVirusScanning)
            {
                if (!await IsFileVirusFreeAsync(file))
                {
                    result.IsValid = false;
                    result.ValidationErrors.Add("File failed virus scanning");
                }
            }

            // Check for corruption (if enabled)
            if (options.EnableCorruptionCheck)
            {
                if (await IsFileCorruptedAsync(file))
                {
                    result.IsValid = false;
                    result.ValidationErrors.Add("File appears to be corrupted");
                }
            }

            // Check file content type if specified
            if (options.AllowedContentTypes?.Any() == true)
            {
                if (!options.AllowedContentTypes.Contains(file.ContentType))
                {
                    result.IsValid = false;
                    result.ValidationErrors.Add($"Content type '{file.ContentType}' not allowed");
                }
            }

            // Check for compressed files
            if (!options.AllowCompressedFiles && IsCompressedFile(file.FileName))
            {
                result.Warnings.Add("Compressed files are not recommended for this operation");
                result.SuggestedAction = "Extract the file contents before uploading";
            }

            result.ValidationCompletedAt = DateTime.UtcNow;
            result.ValidationDuration = result.ValidationCompletedAt - result.ValidationStartedAt;

            if (result.IsValid)
            {
                _logger.LogInformation("File {FileName} passed all validation checks", file.FileName);
            }
            else
            {
                _logger.LogWarning("File {FileName} failed validation: {Errors}", 
                    file.FileName, string.Join("; ", result.ValidationErrors));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file {FileName}", file.FileName);
            result.IsValid = false;
            result.ValidationErrors.Add($"Validation error: {ex.Message}");
            return result;
        }
    }

    public async Task<IEnumerable<FileValidationResult>> ValidateFilesAsync(IEnumerable<IFormFile> files, FileValidationOptions options)
    {
        var results = new List<FileValidationResult>();
        var tasks = files.Select(file => ValidateFileAsync(file, options));
        
        var validationResults = await Task.WhenAll(tasks);
        results.AddRange(validationResults);
        
        return results;
    }

    public bool IsValidFileExtension(string fileName, string[] allowedExtensions)
    {
        if (string.IsNullOrEmpty(fileName) || allowedExtensions == null || !allowedExtensions.Any())
            return false;

        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        return allowedExtensions.Any(ext => ext.ToLowerInvariant() == fileExtension);
    }

    public bool IsValidFileSize(long fileSize, long maxSizeInBytes)
    {
        return fileSize > 0 && fileSize <= maxSizeInBytes;
    }

    public async Task<bool> IsFileVirusFreeAsync(IFormFile file)
    {
        try
        {
            // Check file size first (very large files might be suspicious)
            if (file.Length > 100 * 1024 * 1024) // 100MB limit
            {
                _logger.LogWarning("File {FileName} exceeds size limit for virus scanning", file.FileName);
                return false;
            }

            // Check file extension against known dangerous types
            var dangerousExtensions = new[] { ".exe", ".bat", ".cmd", ".com", ".pif", ".scr", ".vbs", ".js", ".jar" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (dangerousExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("File {FileName} has potentially dangerous extension {Extension}", file.FileName, fileExtension);
                return false;
            }

            // TODO: In production, integrate with actual antivirus service
            // For now, implement basic heuristic checks
            
            // Check file header for common executable signatures
            using var stream = file.OpenReadStream();
            var header = new byte[8];
            await stream.ReadAsync(header, 0, 8);
            
            // Check for MZ header (Windows executable)
            if (header[0] == 0x4D && header[1] == 0x5A)
            {
                _logger.LogWarning("File {FileName} contains executable header", file.FileName);
                return false;
            }
            
            // Check for ELF header (Linux executable)
            if (header[0] == 0x7F && header[1] == 0x45 && header[2] == 0x4C && header[3] == 0x46)
            {
                _logger.LogWarning("File {FileName} contains ELF executable header", file.FileName);
                return false;
            }

            // Reset stream position for actual processing
            stream.Position = 0;
            
            _logger.LogInformation("File {FileName} passed virus scanning checks", file.FileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during virus scanning for file {FileName}", file.FileName);
            return false; // Fail safe - reject file if scanning fails
        }
    }

    public async Task<bool> IsFileCorruptedAsync(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            
            // Check if file is empty
            if (file.Length == 0)
            {
                _logger.LogWarning("File {FileName} is empty", file.FileName);
                return true; // Empty files are considered corrupted for our use case
            }

            // Read file in chunks to check for corruption
            var buffer = new byte[8192]; // 8KB chunks
            var totalBytesRead = 0L;
            var bytesRead = 0;
            
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                totalBytesRead += bytesRead;
                
                // Check for null bytes or corrupted data patterns
                for (int i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] == 0x00 && i > 0 && buffer[i-1] == 0x00)
                    {
                        // Multiple consecutive null bytes might indicate corruption
                        _logger.LogWarning("File {FileName} contains suspicious null byte pattern", file.FileName);
                        return true;
                    }
                }
            }
            
            // Verify we read the expected amount of data
            if (totalBytesRead != file.Length)
            {
                _logger.LogWarning("File {FileName} size mismatch: expected {Expected}, read {Actual}", 
                    file.FileName, file.Length, totalBytesRead);
                return true;
            }
            
            // Reset stream position for actual processing
            stream.Position = 0;
            
            _logger.LogInformation("File {FileName} passed corruption check", file.FileName);
            return false; // File is not corrupted
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file corruption for {FileName}", file.FileName);
            return true; // Assume corrupted if we can't read it
        }
    }

    private bool IsCompressedFile(string fileName)
    {
        var compressedExtensions = new[] { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2" };
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        return compressedExtensions.Contains(fileExtension);
    }
}
