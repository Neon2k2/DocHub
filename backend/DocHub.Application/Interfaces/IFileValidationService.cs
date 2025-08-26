using Microsoft.AspNetCore.Http;

namespace DocHub.Application.Interfaces;

public interface IFileValidationService
{
    Task<FileValidationResult> ValidateFileAsync(IFormFile file, FileValidationOptions options);
    Task<IEnumerable<FileValidationResult>> ValidateFilesAsync(IEnumerable<IFormFile> files, FileValidationOptions options);
    bool IsValidFileExtension(string fileName, string[] allowedExtensions);
    bool IsValidFileSize(long fileSize, long maxSizeInBytes);
    Task<bool> IsFileVirusFreeAsync(IFormFile file);
    Task<bool> IsFileCorruptedAsync(IFormFile file);
}

public class FileValidationOptions
{
    public long MaxFileSizeInBytes { get; set; } = 10 * 1024 * 1024; // 10MB default
    public string[] AllowedExtensions { get; set; } = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".csv" };
    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
    public bool EnableVirusScanning { get; set; } = true;
    public bool EnableCorruptionCheck { get; set; } = true;
    public bool AllowCompressedFiles { get; set; } = false;
}

public class FileValidationResult
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool IsValid { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? SuggestedAction { get; set; }
    public DateTime ValidationStartedAt { get; set; } = DateTime.UtcNow;
    public DateTime ValidationCompletedAt { get; set; }
    public TimeSpan ValidationDuration { get; set; }
}
