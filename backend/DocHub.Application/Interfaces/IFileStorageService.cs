using Microsoft.AspNetCore.Http;

namespace DocHub.Application.Interfaces;

public interface IFileStorageService
{
    Task<FileUploadResult> UploadFileAsync(IFormFile file, FileUploadOptions options);
    Task<FileUploadResult> UploadFilesAsync(IEnumerable<IFormFile> files, FileUploadOptions options);
    Task<FileDownloadResult> DownloadFileAsync(string fileId);
    Task<FileDownloadResult> DownloadFileAsync(string fileId, string version);
    Task<bool> DeleteFileAsync(string fileId);
    Task<bool> DeleteFileVersionAsync(string fileId, string version);
    Task<FileInfo?> GetFileInfoAsync(string fileId);
    Task<IEnumerable<FileVersionInfo>> GetFileVersionsAsync(string fileId);
    Task<FileSearchResult> SearchFilesAsync(FileSearchOptions options);
    Task<bool> UpdateFileMetadataAsync(string fileId, FileMetadata metadata);
    Task<FileProcessingResult> ProcessFileAsync(string fileId, FileProcessingOptions options);
}

public class FileUploadOptions
{
    public string? Folder { get; set; }
    public bool GenerateThumbnail { get; set; } = false;
    public bool EnableVersioning { get; set; } = true;
    public string? CustomFileName { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public bool CompressAfterUpload { get; set; } = false;
    public FileCompressionLevel CompressionLevel { get; set; } = FileCompressionLevel.Optimal;
}

public class FileUploadResult
{
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string? ThumbnailPath { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class FileDownloadResult
{
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
}

public class FileInfo
{
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
    public bool IsDeleted { get; set; }
    public string? ThumbnailPath { get; set; }
}

public class FileVersionInfo
{
    public string Version { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Hash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? ChangeDescription { get; set; }
}

public class FileSearchOptions
{
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long? MinSize { get; set; }
    public long? MaxSize { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? CreatedBy { get; set; }
    public string? Folder { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class FileSearchResult
{
    public IEnumerable<FileInfo> Files { get; set; } = Enumerable.Empty<FileInfo>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class FileMetadata
{
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public Dictionary<string, string> CustomProperties { get; set; } = new();
}

public class FileProcessingOptions
{
    public bool GenerateThumbnail { get; set; } = false;
    public bool ExtractText { get; set; } = false;
    public bool CompressFile { get; set; } = false;
    public FileCompressionLevel CompressionLevel { get; set; } = FileCompressionLevel.Optimal;
    public bool ValidateFile { get; set; } = true;
    public FileValidationOptions? ValidationOptions { get; set; }
}

public class FileProcessingResult
{
    public string FileId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? ExtractedText { get; set; }
    public string? CompressedFilePath { get; set; }
    public long? CompressedSize { get; set; }
    public double? CompressionRatio { get; set; }
    public FileValidationResult? ValidationResult { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingDuration { get; set; }
}
