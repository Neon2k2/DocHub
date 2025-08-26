using DocHub.Application.DTOs;
using DocHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileManagementController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileValidationService _fileValidationService;
    private readonly IFileCompressionService _fileCompressionService;
    private readonly IThumbnailService _thumbnailService;
    private readonly ITextExtractionService _textExtractionService;
    private readonly ILogger<FileManagementController> _logger;

    public FileManagementController(
        IFileStorageService fileStorageService,
        IFileValidationService fileValidationService,
        IFileCompressionService fileCompressionService,
        IThumbnailService thumbnailService,
        ITextExtractionService textExtractionService,
        ILogger<FileManagementController> logger)
    {
        _fileStorageService = fileStorageService;
        _fileValidationService = fileValidationService;
        _fileCompressionService = fileCompressionService;
        _thumbnailService = thumbnailService;
        _textExtractionService = textExtractionService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a single file with advanced options
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<FileUploadResult>>> UploadFile(
        IFormFile file,
        [FromForm] string? folder = null,
        [FromForm] bool generateThumbnail = false,
        [FromForm] bool compressAfterUpload = false,
        [FromForm] string compressionLevel = "Optimal",
        [FromForm] string? customFileName = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<FileUploadResult>.ValidationErrorResult("No file uploaded", new List<string> { "File is required" }));
            }

            var options = new FileUploadOptions
            {
                Folder = folder,
                GenerateThumbnail = generateThumbnail,
                CompressAfterUpload = compressAfterUpload,
                CompressionLevel = ParseCompressionLevel(compressionLevel),
                CustomFileName = customFileName,
                Metadata = new Dictionary<string, string>
                {
                    ["UploadedBy"] = User.Identity?.Name ?? "Unknown",
                    ["UploadMethod"] = "API",
                    ["OriginalFileName"] = file.FileName
                }
            };

            var result = await _fileStorageService.UploadFileAsync(file, options);

            if (result.Success)
            {
                _logger.LogInformation("File uploaded successfully: {FileName} -> {FileId}", file.FileName, result.FileId);
                return Ok(ApiResponse<FileUploadResult>.SuccessResult(result, "File uploaded successfully"));
            }
            else
            {
                _logger.LogWarning("File upload failed: {FileName} - {Error}", file.FileName, result.ErrorMessage);
                return BadRequest(ApiResponse<FileUploadResult>.ErrorResult("File upload failed", new List<string> { result.ErrorMessage ?? "Unknown error" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
            return StatusCode(500, ApiResponse<FileUploadResult>.ErrorResult("Error uploading file", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Upload multiple files with batch processing
    /// </summary>
    [HttpPost("upload/batch")]
    public async Task<ActionResult<ApiResponse<FileUploadResult>>> UploadFiles(
        List<IFormFile> files,
        [FromForm] string? folder = null,
        [FromForm] bool generateThumbnails = false,
        [FromForm] bool compressAfterUpload = false,
        [FromForm] string compressionLevel = "Optimal")
    {
        try
        {
            if (files == null || !files.Any())
            {
                return BadRequest(ApiResponse<FileUploadResult>.ValidationErrorResult("No files uploaded", new List<string> { "At least one file is required" }));
            }

            var options = new FileUploadOptions
            {
                Folder = folder,
                GenerateThumbnail = generateThumbnails,
                CompressAfterUpload = compressAfterUpload,
                CompressionLevel = ParseCompressionLevel(compressionLevel),
                Metadata = new Dictionary<string, string>
                {
                    ["UploadedBy"] = User.Identity?.Name ?? "Unknown",
                    ["UploadMethod"] = "Batch API",
                    ["FileCount"] = files.Count.ToString()
                }
            };

            var result = await _fileStorageService.UploadFilesAsync(files, options);

            if (result.Success)
            {
                _logger.LogInformation("Batch upload completed: {FileCount} files", files.Count);
                return Ok(ApiResponse<FileUploadResult>.SuccessResult(result, $"Batch upload completed: {files.Count} files"));
            }
            else
            {
                _logger.LogWarning("Batch upload failed: {Error}", result.ErrorMessage);
                return BadRequest(ApiResponse<FileUploadResult>.ErrorResult("Batch upload failed", new List<string> { result.ErrorMessage ?? "Unknown error" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch upload");
            return StatusCode(500, ApiResponse<FileUploadResult>.ErrorResult("Error in batch upload", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Download a file by ID
    /// </summary>
    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> DownloadFile(string fileId)
    {
        try
        {
            var result = await _fileStorageService.DownloadFileAsync(fileId);

            if (!result.Success)
            {
                return NotFound(ApiResponse<bool>.ErrorResult("File not found", new List<string> { result.ErrorMessage ?? "File not found" }));
            }

            _logger.LogInformation("File downloaded: {FileId} -> {FileName}", fileId, result.FileName);
            
            return File(result.FileData, result.ContentType, result.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FileId}", fileId);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Error downloading file", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get file information
    /// </summary>
    [HttpGet("info/{fileId}")]
    public async Task<ActionResult<ApiResponse<FileInfo>>> GetFileInfo(string fileId)
    {
        try
        {
            var fileInfo = await _fileStorageService.GetFileInfoAsync(fileId);

            if (fileInfo == null)
            {
                return NotFound(ApiResponse<FileInfo>.ErrorResult("File not found", new List<string> { "File ID not found" }));
            }

            _logger.LogInformation("File info retrieved: {FileId}", fileId);
            return Ok(ApiResponse<FileInfo>.SuccessResult(fileInfo, "File information retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info for {FileId}", fileId);
            return StatusCode(500, ApiResponse<FileInfo>.ErrorResult("Error getting file info", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Process a file with various options
    /// </summary>
    [HttpPost("process/{fileId}")]
    public async Task<ActionResult<ApiResponse<FileProcessingResult>>> ProcessFile(
        string fileId,
        [FromBody] FileProcessingRequest request)
    {
        try
        {
            var options = new FileProcessingOptions
            {
                GenerateThumbnail = request.GenerateThumbnail,
                ExtractText = request.ExtractText,
                CompressFile = request.CompressFile,
                CompressionLevel = request.CompressionLevel,
                ValidateFile = request.ValidateFile
            };

            var result = await _fileStorageService.ProcessFileAsync(fileId, options);

            if (result.Success)
            {
                _logger.LogInformation("File processed successfully: {FileId}", fileId);
                return Ok(ApiResponse<FileProcessingResult>.SuccessResult(result, "File processed successfully"));
            }
            else
            {
                _logger.LogWarning("File processing failed: {FileId} - {Error}", fileId, result.ErrorMessage);
                return BadRequest(ApiResponse<FileProcessingResult>.ErrorResult("File processing failed", new List<string> { result.ErrorMessage ?? "Unknown error" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file {FileId}", fileId);
            return StatusCode(500, ApiResponse<FileProcessingResult>.ErrorResult("Error processing file", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Generate thumbnail for a file
    /// </summary>
    [HttpPost("thumbnail/{fileId}")]
    public async Task<ActionResult<ApiResponse<string>>> GenerateThumbnail(
        string fileId,
        [FromBody] ThumbnailRequest request)
    {
        try
        {
            var fileInfo = await _fileStorageService.GetFileInfoAsync(fileId);
            if (fileInfo == null)
            {
                return NotFound(ApiResponse<string>.ErrorResult("File not found", new List<string> { "File ID not found" }));
            }

            var options = new ThumbnailOptions
            {
                MaxWidth = request.MaxWidth,
                MaxHeight = request.MaxHeight,
                Quality = request.Quality,
                Format = request.Format
            };

            var thumbnailPath = await _thumbnailService.GenerateThumbnailAsync(fileInfo.FilePath, fileId, options);

            if (!string.IsNullOrEmpty(thumbnailPath))
            {
                _logger.LogInformation("Thumbnail generated for file {FileId}", fileId);
                return Ok(ApiResponse<string>.SuccessResult(thumbnailPath, "Thumbnail generated successfully"));
            }
            else
            {
                _logger.LogWarning("Thumbnail generation failed for file {FileId}", fileId);
                return BadRequest(ApiResponse<string>.ErrorResult("Thumbnail generation failed", new List<string> { "Failed to generate thumbnail" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnail for file {FileId}", fileId);
            return StatusCode(500, ApiResponse<string>.ErrorResult("Error generating thumbnail", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Extract text from a file
    /// </summary>
    [HttpPost("extract-text/{fileId}")]
    public async Task<ActionResult<ApiResponse<TextExtractionResult>>> ExtractText(
        string fileId,
        [FromBody] TextExtractionRequest request)
    {
        try
        {
            var fileInfo = await _fileStorageService.GetFileInfoAsync(fileId);
            if (fileInfo == null)
            {
                return NotFound(ApiResponse<TextExtractionResult>.ErrorResult("File not found", new List<string> { "File ID not found" }));
            }

            var options = new TextExtractionOptions
            {
                Encoding = request.Encoding != null ? System.Text.Encoding.GetEncoding(request.Encoding) : null,
                ExtractMetadata = request.ExtractMetadata,
                PreserveFormatting = request.PreserveFormatting,
                MaxLength = request.MaxLength
            };

            var result = await _textExtractionService.ExtractTextAsync(fileInfo.FilePath, options);

            if (result.Success)
            {
                _logger.LogInformation("Text extracted from file {FileId}", fileId);
                return Ok(ApiResponse<TextExtractionResult>.SuccessResult(result, "Text extracted successfully"));
            }
            else
            {
                _logger.LogWarning("Text extraction failed for file {FileId}: {Error}", fileId, result.ErrorMessage);
                return BadRequest(ApiResponse<TextExtractionResult>.ErrorResult("Text extraction failed", new List<string> { result.ErrorMessage ?? "Unknown error" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from file {FileId}", fileId);
            return StatusCode(500, ApiResponse<TextExtractionResult>.ErrorResult("Error extracting text", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Search files with advanced filtering
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<FileSearchResult>>> SearchFiles(
        [FromQuery] string? fileName = null,
        [FromQuery] string? contentType = null,
        [FromQuery] long? minSize = null,
        [FromQuery] long? maxSize = null,
        [FromQuery] DateTime? createdAfter = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] string? createdBy = null,
        [FromQuery] string? folder = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        try
        {
            var options = new FileSearchOptions
            {
                FileName = fileName,
                ContentType = contentType,
                MinSize = minSize,
                MaxSize = maxSize,
                CreatedAfter = createdAfter,
                CreatedBefore = createdBefore,
                CreatedBy = createdBy,
                Folder = folder,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _fileStorageService.SearchFilesAsync(options);

            _logger.LogInformation("File search completed: {TotalCount} files found", result.TotalCount);
            return Ok(ApiResponse<FileSearchResult>.SuccessResult(result, $"Search completed: {result.TotalCount} files found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching files");
            return StatusCode(500, ApiResponse<FileSearchResult>.ErrorResult("Error searching files", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    [HttpDelete("{fileId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteFile(string fileId)
    {
        try
        {
            var success = await _fileStorageService.DeleteFileAsync(fileId);

            if (success)
            {
                // Also delete thumbnail if it exists
                await _thumbnailService.DeleteThumbnailAsync(fileId);
                
                _logger.LogInformation("File deleted successfully: {FileId}", fileId);
                return Ok(ApiResponse<bool>.SuccessResult(true, "File deleted successfully"));
            }
            else
            {
                _logger.LogWarning("File deletion failed: {FileId}", fileId);
                return BadRequest(ApiResponse<bool>.ErrorResult("File deletion failed", new List<string> { "File not found or already deleted" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId}", fileId);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Error deleting file", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Compress a file
    /// </summary>
    [HttpPost("compress/{fileId}")]
    public async Task<ActionResult<ApiResponse<FileCompressionResult>>> CompressFile(
        string fileId,
        [FromBody] FileCompressionRequest request)
    {
        try
        {
            var fileInfo = await _fileStorageService.GetFileInfoAsync(fileId);
            if (fileInfo == null)
            {
                return NotFound(ApiResponse<FileCompressionResult>.ErrorResult("File not found", new List<string> { "File ID not found" }));
            }

            var fileData = await System.IO.File.ReadAllBytesAsync(fileInfo.FilePath);
            var compressedData = await _fileCompressionService.CompressFileAsync(fileData, fileInfo.FileName, request.CompressionLevel);

            var result = new FileCompressionResult
            {
                FileId = fileId,
                OriginalSize = fileData.Length,
                CompressedSize = compressedData.Length,
                CompressionRatio = (double)compressedData.Length / fileData.Length,
                Success = true
            };

            _logger.LogInformation("File compressed successfully: {FileId}, ratio: {Ratio:P2}", fileId, result.CompressionRatio);
            return Ok(ApiResponse<FileCompressionResult>.SuccessResult(result, "File compressed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing file {FileId}", fileId);
            return StatusCode(500, ApiResponse<FileCompressionResult>.ErrorResult("Error compressing file", new List<string> { ex.Message }));
        }
    }

    private FileCompressionLevel ParseCompressionLevel(string level)
    {
        return level?.ToLower() switch
        {
            "no" or "none" => FileCompressionLevel.NoCompression,
            "fastest" => FileCompressionLevel.Fastest,
            "optimal" => FileCompressionLevel.Optimal,
            "smallest" => FileCompressionLevel.SmallestSize,
            _ => FileCompressionLevel.Optimal
        };
    }
}

// Request and response models
public class FileProcessingRequest
{
    public bool GenerateThumbnail { get; set; } = false;
    public bool ExtractText { get; set; } = false;
    public bool CompressFile { get; set; } = false;
    public FileCompressionLevel CompressionLevel { get; set; } = FileCompressionLevel.Optimal;
    public bool ValidateFile { get; set; } = true;
}

public class ThumbnailRequest
{
    public int? MaxWidth { get; set; }
    public int? MaxHeight { get; set; }
    public int? Quality { get; set; }
    public string? Format { get; set; }
}

public class TextExtractionRequest
{
    public string? Encoding { get; set; }
    public bool ExtractMetadata { get; set; } = false;
    public bool PreserveFormatting { get; set; } = false;
    public int? MaxLength { get; set; }
}

public class FileCompressionRequest
{
    public FileCompressionLevel CompressionLevel { get; set; } = FileCompressionLevel.Optimal;
}

public class FileCompressionResult
{
    public string FileId { get; set; } = string.Empty;
    public long OriginalSize { get; set; }
    public long CompressedSize { get; set; }
    public double CompressionRatio { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
