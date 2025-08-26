using DocHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace DocHub.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFileValidationService _fileValidationService;
    private readonly IFileCompressionService _fileCompressionService;
    private readonly IThumbnailService _thumbnailService;
    private readonly ITextExtractionService _textExtractionService;
    private readonly string _baseStoragePath;
    private readonly string _thumbnailPath;
    private readonly string _tempPath;

    public FileStorageService(
        ILogger<FileStorageService> logger,
        IConfiguration configuration,
        IFileValidationService fileValidationService,
        IFileCompressionService fileCompressionService,
        IThumbnailService thumbnailService,
        ITextExtractionService textExtractionService)
    {
        _logger = logger;
        _configuration = configuration;
        _fileValidationService = fileValidationService;
        _fileCompressionService = fileCompressionService;
        _thumbnailService = thumbnailService;
        _textExtractionService = textExtractionService;
        
        _baseStoragePath = _configuration["FileStorage:BasePath"] ?? "Storage/Files";
        _thumbnailPath = _configuration["FileStorage:ThumbnailPath"] ?? "Storage/Thumbnails";
        _tempPath = _configuration["FileStorage:TempPath"] ?? "Storage/Temp";
        
        // Ensure directories exist
        Directory.CreateDirectory(_baseStoragePath);
        Directory.CreateDirectory(_thumbnailPath);
        Directory.CreateDirectory(_tempPath);
    }

    public async Task<FileUploadResult> UploadFileAsync(IFormFile file, FileUploadOptions options)
    {
        var result = new FileUploadResult
        {
            FileName = options.CustomFileName ?? file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            Success = false
        };

        try
        {
            // Validate file if validation is enabled
            if (options.CompressAfterUpload)
            {
                var validationOptions = new FileValidationOptions
                {
                    MaxFileSizeInBytes = 100 * 1024 * 1024, // 100MB for compression
                    AllowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".csv", ".zip", ".rar" },
                    EnableVirusScanning = true,
                    EnableCorruptionCheck = true
                };

                var validationResult = await _fileValidationService.ValidateFileAsync(file, validationOptions);
                if (!validationResult.IsValid)
                {
                    result.ErrorMessage = $"File validation failed: {string.Join("; ", validationResult.ValidationErrors)}";
                    return result;
                }
            }

            // Generate unique file ID
            result.FileId = GenerateFileId();
            
            // Create folder structure
            var folderPath = Path.Combine(_baseStoragePath, options.Folder ?? "uploads", DateTime.Now.ToString("yyyy/MM"));
            Directory.CreateDirectory(folderPath);
            
            // Generate file path
            var fileName = $"{result.FileId}_{Path.GetFileNameWithoutExtension(result.FileName)}{Path.GetExtension(result.FileName)}";
            result.FilePath = Path.Combine(folderPath, fileName);
            
            // Save file
            using (var stream = new FileStream(result.FilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            // Generate file hash
            result.Hash = await GenerateFileHashAsync(result.FilePath);
            
            // Generate thumbnail if requested
            if (options.GenerateThumbnail)
            {
                result.ThumbnailPath = await _thumbnailService.GenerateThumbnailAsync(result.FilePath, result.FileId);
            }
            
            // Compress file if requested
            if (options.CompressAfterUpload)
            {
                var compressedData = await _fileCompressionService.CompressFileAsync(
                    await File.ReadAllBytesAsync(result.FilePath), 
                    fileName, 
                    options.CompressionLevel);
                
                var compressedPath = result.FilePath + ".compressed";
                await File.WriteAllBytesAsync(compressedPath, compressedData);
                
                // Replace original with compressed version
                File.Delete(result.FilePath);
                File.Move(compressedPath, result.FilePath);
                result.FileSize = compressedData.Length;
            }
            
            // Save metadata
            if (options.Metadata != null)
            {
                result.Metadata = options.Metadata;
            }
            
            result.Success = true;
            result.UploadedAt = DateTime.UtcNow;
            
            _logger.LogInformation("File {FileName} uploaded successfully with ID {FileId}", 
                result.FileName, result.FileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
            result.ErrorMessage = ex.Message;
            result.Success = false;
        }
        
        return result;
    }

    public async Task<FileUploadResult> UploadFilesAsync(IEnumerable<IFormFile> files, FileUploadOptions options)
    {
        var results = new List<FileUploadResult>();
        var tasks = files.Select(file => UploadFileAsync(file, options));
        
        var uploadResults = await Task.WhenAll(tasks);
        results.AddRange(uploadResults);
        
        // Return the first result as the main result (for backward compatibility)
        var mainResult = results.FirstOrDefault() ?? new FileUploadResult();
        
        // Add metadata about batch upload
        mainResult.Metadata["BatchUpload"] = "true";
        mainResult.Metadata["TotalFiles"] = results.Count.ToString();
        mainResult.Metadata["SuccessfulUploads"] = results.Count(r => r.Success).ToString();
        mainResult.Metadata["FailedUploads"] = results.Count(r => !r.Success).ToString();
        
        return mainResult;
    }

    public async Task<FileDownloadResult> DownloadFileAsync(string fileId)
    {
        var result = new FileDownloadResult
        {
            FileId = fileId,
            Success = false
        };

        try
        {
            var fileInfo = await GetFileInfoAsync(fileId);
            if (fileInfo == null || !File.Exists(fileInfo.FilePath))
            {
                result.ErrorMessage = "File not found";
                return result;
            }
            
            result.FileName = fileInfo.FileName;
            result.ContentType = fileInfo.ContentType;
            result.FileSize = fileInfo.FileSize;
            result.FileData = await File.ReadAllBytesAsync(fileInfo.FilePath);
            result.Success = true;
            result.DownloadedAt = DateTime.UtcNow;
            
            _logger.LogInformation("File {FileId} downloaded successfully", fileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FileId}", fileId);
            result.ErrorMessage = ex.Message;
        }
        
        return result;
    }

    public async Task<FileDownloadResult> DownloadFileAsync(string fileId, string version)
    {
        // For now, return the latest version
        // TODO: Implement version-specific download
        return await DownloadFileAsync(fileId);
    }

    public async Task<bool> DeleteFileAsync(string fileId)
    {
        try
        {
            var fileInfo = await GetFileInfoAsync(fileId);
            if (fileInfo == null || !File.Exists(fileInfo.FilePath))
            {
                return false;
            }
            
            // Move to deleted folder instead of permanent deletion
            var deletedFolder = Path.Combine(_baseStoragePath, "deleted", DateTime.Now.ToString("yyyy/MM"));
            Directory.CreateDirectory(deletedFolder);
            
            var deletedPath = Path.Combine(deletedFolder, $"{fileId}_{Path.GetFileName(fileInfo.FilePath)}");
            File.Move(fileInfo.FilePath, deletedPath);
            
            // Delete thumbnail if exists
            if (!string.IsNullOrEmpty(fileInfo.ThumbnailPath) && File.Exists(fileInfo.ThumbnailPath))
            {
                File.Delete(fileInfo.ThumbnailPath);
            }
            
            _logger.LogInformation("File {FileId} deleted successfully", fileId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId}", fileId);
            return false;
        }
    }

    public async Task<bool> DeleteFileVersionAsync(string fileId, string version)
    {
        // For now, delete the main file
        // TODO: Implement version-specific deletion
        return await DeleteFileAsync(fileId);
    }

    public async Task<DocHub.Application.Interfaces.FileInfo?> GetFileInfoAsync(string fileId)
    {
        try
        {
            // Search for file in storage
            var files = Directory.GetFiles(_baseStoragePath, "*", SearchOption.AllDirectories);
            var filePath = files.FirstOrDefault(f => f.Contains(fileId));
            
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return null;
            }
            
            var fileInfo = new DocHub.Application.Interfaces.FileInfo
            {
                FileId = fileId,
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                FileSize = new System.IO.FileInfo(filePath).Length,
                ContentType = GetContentTypeFromExtension(Path.GetExtension(filePath)),
                Hash = await GenerateFileHashAsync(filePath),
                CreatedAt = File.GetCreationTime(filePath),
                ModifiedAt = File.GetLastWriteTime(filePath),
                CreatedBy = "System", // TODO: Get from user context
                IsDeleted = false
            };
            
            // Check for thumbnail
            var thumbnailPath = Path.Combine(_thumbnailPath, $"{fileId}_thumb.jpg");
            if (File.Exists(thumbnailPath))
            {
                fileInfo.ThumbnailPath = thumbnailPath;
            }
            
            return fileInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info for {FileId}", fileId);
            return null;
        }
    }

    public async Task<IEnumerable<FileVersionInfo>> GetFileVersionsAsync(string fileId)
    {
        // For now, return single version
        // TODO: Implement versioning system
        var fileInfo = await GetFileInfoAsync(fileId);
        if (fileInfo == null)
        {
            return Enumerable.Empty<FileVersionInfo>();
        }
        
        return new List<FileVersionInfo>
        {
            new FileVersionInfo
            {
                Version = "1.0",
                FilePath = fileInfo.FilePath,
                FileSize = fileInfo.FileSize,
                Hash = fileInfo.Hash,
                CreatedAt = fileInfo.CreatedAt,
                CreatedBy = fileInfo.CreatedBy,
                ChangeDescription = "Initial version"
            }
        };
    }

    public async Task<FileSearchResult> SearchFilesAsync(FileSearchOptions options)
    {
        try
        {
            var allFiles = new List<DocHub.Application.Interfaces.FileInfo>();
            var files = Directory.GetFiles(_baseStoragePath, "*", SearchOption.AllDirectories);
            
            foreach (var filePath in files)
            {
                if (filePath.Contains("deleted")) continue; // Skip deleted files
                
                var fileInfo = await GetFileInfoAsync(Path.GetFileNameWithoutExtension(filePath).Split('_').First());
                if (fileInfo != null)
                {
                    allFiles.Add(fileInfo);
                }
            }
            
            // Apply filters
            var filteredFiles = allFiles.AsQueryable();
            
            if (!string.IsNullOrEmpty(options.FileName))
            {
                filteredFiles = filteredFiles.Where(f => f.FileName.Contains(options.FileName, StringComparison.OrdinalIgnoreCase));
            }
            
            if (!string.IsNullOrEmpty(options.ContentType))
            {
                filteredFiles = filteredFiles.Where(f => f.ContentType == options.ContentType);
            }
            
            if (options.MinSize.HasValue)
            {
                filteredFiles = filteredFiles.Where(f => f.FileSize >= options.MinSize.Value);
            }
            
            if (options.MaxSize.HasValue)
            {
                filteredFiles = filteredFiles.Where(f => f.FileSize <= options.MaxSize.Value);
            }
            
            if (options.CreatedAfter.HasValue)
            {
                filteredFiles = filteredFiles.Where(f => f.CreatedAt >= options.CreatedAfter.Value);
            }
            
            if (options.CreatedBefore.HasValue)
            {
                filteredFiles = filteredFiles.Where(f => f.CreatedAt <= options.CreatedBefore.Value);
            }
            
            if (!string.IsNullOrEmpty(options.CreatedBy))
            {
                filteredFiles = filteredFiles.Where(f => f.CreatedBy == options.CreatedBy);
            }
            
            if (!string.IsNullOrEmpty(options.Folder))
            {
                filteredFiles = filteredFiles.Where(f => f.FilePath.Contains(options.Folder));
            }
            
            // Apply sorting
            filteredFiles = options.SortBy?.ToLower() switch
            {
                "filename" => options.SortDescending ? filteredFiles.OrderByDescending(f => f.FileName) : filteredFiles.OrderBy(f => f.FileName),
                "filesize" => options.SortDescending ? filteredFiles.OrderByDescending(f => f.FileSize) : filteredFiles.OrderBy(f => f.FileSize),
                "createdat" => options.SortDescending ? filteredFiles.OrderByDescending(f => f.CreatedAt) : filteredFiles.OrderBy(f => f.CreatedAt),
                _ => options.SortDescending ? filteredFiles.OrderByDescending(f => f.CreatedAt) : filteredFiles.OrderBy(f => f.CreatedAt)
            };
            
            var totalCount = filteredFiles.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / options.PageSize);
            
            var pagedFiles = filteredFiles
                .Skip((options.Page - 1) * options.PageSize)
                .Take(options.PageSize)
                .ToList();
            
            return new FileSearchResult
            {
                Files = pagedFiles,
                TotalCount = totalCount,
                Page = options.Page,
                PageSize = options.PageSize,
                TotalPages = totalPages,
                HasNextPage = options.Page < totalPages,
                HasPreviousPage = options.Page > 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching files");
            return new FileSearchResult();
        }
    }

    public async Task<bool> UpdateFileMetadataAsync(string fileId, FileMetadata metadata)
    {
        try
        {
            var fileInfo = await GetFileInfoAsync(fileId);
            if (fileInfo == null)
            {
                return false;
            }
            
            // TODO: Save metadata to database or metadata file
            _logger.LogInformation("Metadata updated for file {FileId}", fileId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating metadata for file {FileId}", fileId);
            return false;
        }
    }

    public async Task<FileProcessingResult> ProcessFileAsync(string fileId, FileProcessingOptions options)
    {
        var result = new FileProcessingResult
        {
            FileId = fileId,
            Success = false,
            ProcessedAt = DateTime.UtcNow
        };

        try
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(fileId))
            {
                result.ErrorMessage = "File ID is required";
                _logger.LogWarning("File processing attempted with null or empty file ID");
                return result;
            }

            if (options == null)
            {
                result.ErrorMessage = "Processing options are required";
                _logger.LogWarning("File processing attempted with null options for file {FileId}", fileId);
                return result;
            }

            var startTime = DateTime.UtcNow;
            var fileInfo = await GetFileInfoAsync(fileId);
            
            if (fileInfo == null)
            {
                result.ErrorMessage = "File not found";
                _logger.LogWarning("File processing attempted for non-existent file {FileId}", fileId);
                return result;
            }

            // Verify file still exists on disk
            if (!File.Exists(fileInfo.FilePath))
            {
                result.ErrorMessage = "File no longer exists on disk";
                _logger.LogWarning("File {FileId} exists in database but not on disk: {FilePath}", fileId, fileInfo.FilePath);
                return result;
            }
            
            // Validate file if requested
            if (options.ValidateFile)
            {
                try
                {
                    // Basic file validation
                    var fileInfoFromDisk = new System.IO.FileInfo(fileInfo.FilePath);
                    if (fileInfoFromDisk.Length == 0)
                    {
                        result.ErrorMessage = "File is empty";
                        _logger.LogWarning("File {FileId} is empty: {FilePath}", fileId, fileInfo.FilePath);
                        return result;
                    }
                }
                catch (Exception validationEx)
                {
                    result.ErrorMessage = $"File validation failed: {validationEx.Message}";
                    _logger.LogWarning(validationEx, "File validation failed for {FileId}: {FilePath}", fileId, fileInfo.FilePath);
                    return result;
                }
            }
            
            // Generate thumbnail if requested
            if (options.GenerateThumbnail)
            {
                try
                {
                    result.ThumbnailPath = await _thumbnailService.GenerateThumbnailAsync(fileInfo.FilePath, fileId);
                    if (string.IsNullOrEmpty(result.ThumbnailPath))
                    {
                        _logger.LogWarning("Thumbnail generation failed for file {FileId}", fileId);
                    }
                }
                catch (Exception thumbnailEx)
                {
                    _logger.LogError(thumbnailEx, "Error generating thumbnail for file {FileId}", fileId);
                    // Don't fail the entire process for thumbnail generation
                }
            }
            
            // Extract text if requested
            if (options.ExtractText)
            {
                try
                {
                    var textResult = await _textExtractionService.ExtractTextAsync(fileInfo.FilePath);
                    if (textResult.Success)
                    {
                        result.ExtractedText = textResult.ExtractedText;
                    }
                    else
                    {
                        _logger.LogWarning("Text extraction failed for file {FileId}: {Error}", fileId, textResult.ErrorMessage);
                    }
                }
                catch (Exception textEx)
                {
                    _logger.LogError(textEx, "Error extracting text from file {FileId}", fileId);
                    // Don't fail the entire process for text extraction
                }
            }
            
            // Compress file if requested
            if (options.CompressFile)
            {
                try
                {
                    var fileData = await File.ReadAllBytesAsync(fileInfo.FilePath);
                    var compressedData = await _fileCompressionService.CompressFileAsync(fileData, fileInfo.FileName, options.CompressionLevel);
                    
                    var compressedPath = fileInfo.FilePath + ".compressed";
                    await File.WriteAllBytesAsync(compressedPath, compressedData);
                    
                    result.CompressedFilePath = compressedPath;
                    result.CompressedSize = compressedData.Length;
                    result.CompressionRatio = (double)compressedData.Length / fileData.Length;
                }
                catch (Exception compressionEx)
                {
                    _logger.LogError(compressionEx, "Error compressing file {FileId}", fileId);
                    result.ErrorMessage = "File compression failed";
                    return result;
                }
            }
            
            result.Success = true;
            result.ProcessingDuration = DateTime.UtcNow - startTime;
            
            _logger.LogInformation("File {FileId} processed successfully in {Duration}", 
                fileId, result.ProcessingDuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file {FileId}", fileId);
            result.ErrorMessage = ex.Message;
        }
        
        return result;
    }

    private string GenerateFileId()
    {
        return Guid.NewGuid().ToString("N");
    }

    private async Task<string> GenerateFileHashAsync(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }







    private string GetContentTypeFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".zip" => "application/zip",
            ".rar" => "application/vnd.rar",
            _ => "application/octet-stream"
        };
    }
}
