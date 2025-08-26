using Microsoft.AspNetCore.Mvc;
using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileProcessingController : ControllerBase
{
    private readonly IFileValidationService _fileValidationService;
    private readonly IFileCompressionService _fileCompressionService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IExcelService _excelService;
    private readonly ILogger<FileProcessingController> _logger;

    public FileProcessingController(
        IFileValidationService fileValidationService,
        IFileCompressionService fileCompressionService,
        IFileStorageService fileStorageService,
        IExcelService excelService,
        ILogger<FileProcessingController> logger)
    {
        _fileValidationService = fileValidationService;
        _fileCompressionService = fileCompressionService;
        _fileStorageService = fileStorageService;
        _excelService = excelService;
        _logger = logger;
    }

    #region File Validation

    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<FileValidationResult>>> ValidateFile(
        [FromForm] IFormFile file,
        [FromForm] FileValidationOptions options)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<FileValidationResult>
                {
                    Success = false,
                    Message = "No file provided"
                });
            }

            var validationResult = await _fileValidationService.ValidateFileAsync(file, options);
            
            return Ok(new ApiResponse<FileValidationResult>
            {
                Success = true,
                Data = validationResult,
                Message = validationResult.IsValid ? "File validation successful" : "File validation failed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file {FileName}", file?.FileName);
            return StatusCode(500, new ApiResponse<FileValidationResult>
            {
                Success = false,
                Message = "Error during file validation"
            });
        }
    }

    [HttpPost("validate-batch")]
    public async Task<ActionResult<ApiResponse<IEnumerable<FileValidationResult>>>> ValidateFiles(
        [FromForm] List<IFormFile> files,
        [FromForm] FileValidationOptions options)
    {
        try
        {
            if (files == null || !files.Any())
            {
                return BadRequest(new ApiResponse<IEnumerable<FileValidationResult>>
                {
                    Success = false,
                    Message = "No files provided"
                });
            }

            var validationResults = await _fileValidationService.ValidateFilesAsync(files, options);
            
            return Ok(new ApiResponse<IEnumerable<FileValidationResult>>
            {
                Success = true,
                Data = validationResults,
                Message = $"Validated {files.Count} files"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating batch files");
            return StatusCode(500, new ApiResponse<IEnumerable<FileValidationResult>>
            {
                Success = false,
                Message = "Error during batch file validation"
            });
        }
    }

    #endregion

    #region File Compression

    [HttpPost("compress")]
    public async Task<ActionResult<ApiResponse<byte[]>>> CompressFile(
        [FromForm] IFormFile file,
        [FromForm] FileCompressionLevel level = FileCompressionLevel.Optimal)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = "No file provided"
                });
            }

            var fileData = new byte[file.Length];
            using (var stream = file.OpenReadStream())
            {
                await stream.ReadAsync(fileData, 0, (int)file.Length);
            }

            var compressedData = await _fileCompressionService.CompressFileAsync(fileData, file.FileName, level);
            
            return File(compressedData, "application/zip", $"{Path.GetFileNameWithoutExtension(file.FileName)}_compressed.zip");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing file {FileName}", file?.FileName);
            return StatusCode(500, new ApiResponse<byte[]>
            {
                Success = false,
                Message = "Error during file compression"
            });
        }
    }

    [HttpPost("compress-multiple")]
    public async Task<ActionResult<ApiResponse<byte[]>>> CompressMultipleFiles(
        [FromForm] List<IFormFile> files,
        [FromForm] FileCompressionLevel level = FileCompressionLevel.Optimal)
    {
        try
        {
            if (files == null || !files.Any())
            {
                return BadRequest(new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = "No files provided"
                });
            }

            var filesDict = new Dictionary<string, byte[]>();
            foreach (var file in files)
            {
                var fileData = new byte[file.Length];
                using (var stream = file.OpenReadStream())
                {
                    await stream.ReadAsync(fileData, 0, (int)file.Length);
                }
                filesDict[file.FileName] = fileData;
            }

            var compressedData = await _fileCompressionService.CompressMultipleFilesAsync(filesDict, level);
            
            return File(compressedData, "application/zip", "multiple_files_compressed.zip");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing multiple files");
            return StatusCode(500, new ApiResponse<byte[]>
            {
                Success = false,
                Message = "Error during multiple file compression"
            });
        }
    }

    [HttpPost("decompress")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, byte[]>>>> DecompressFile(
        [FromForm] IFormFile archiveFile)
    {
        try
        {
            if (archiveFile == null || archiveFile.Length == 0)
            {
                return BadRequest(new ApiResponse<Dictionary<string, byte[]>>
                {
                    Success = false,
                    Message = "No archive file provided"
                });
            }

            var archiveData = new byte[archiveFile.Length];
            using (var stream = archiveFile.OpenReadStream())
            {
                await stream.ReadAsync(archiveData, 0, (int)archiveFile.Length);
            }

            var decompressedFiles = await _fileCompressionService.DecompressArchiveAsync(archiveData);
            
            return Ok(new ApiResponse<Dictionary<string, byte[]>>
            {
                Success = true,
                Data = decompressedFiles,
                Message = $"Decompressed {decompressedFiles.Count} files"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decompressing archive {FileName}", archiveFile?.FileName);
            return StatusCode(500, new ApiResponse<Dictionary<string, byte[]>>
            {
                Success = false,
                Message = "Error during archive decompression"
            });
        }
    }

    #endregion

    #region File Storage

    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<FileUploadResult>>> UploadFile(
        [FromForm] IFormFile file,
        [FromForm] FileUploadOptions options)
    {
        try
        {
            // Input validation
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<FileUploadResult>.ErrorResult("No file provided"));
            }

            // File size validation (100MB limit from configuration)
            var maxFileSizeBytes = 100 * 1024 * 1024; // 100MB
            if (file.Length > maxFileSizeBytes)
            {
                return BadRequest(ApiResponse<FileUploadResult>.ErrorResult(
                    $"File size exceeds maximum allowed size of 100MB. Current size: {file.Length / (1024 * 1024)}MB"));
            }

            // File extension validation
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".csv", ".zip", ".rar" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(ApiResponse<FileUploadResult>.ErrorResult(
                    $"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}"));
            }

            // File name security check
            if (file.FileName.Contains("..") || file.FileName.Contains("/") || file.FileName.Contains("\\"))
            {
                return BadRequest(ApiResponse<FileUploadResult>.ErrorResult("Invalid file name detected"));
            }

            var uploadResult = await _fileStorageService.UploadFileAsync(file, options);
            
            if (uploadResult.Success)
            {
                return Ok(new ApiResponse<FileUploadResult>
                {
                    Success = true,
                    Data = uploadResult,
                    Message = "File uploaded successfully"
                });
            }
            else
            {
                return BadRequest(new ApiResponse<FileUploadResult>
                {
                    Success = false,
                    Data = uploadResult,
                    Message = uploadResult.ErrorMessage ?? "File upload failed"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", file?.FileName);
            return StatusCode(500, new ApiResponse<FileUploadResult>
            {
                Success = false,
                Message = "Error during file upload"
            });
        }
    }

    [HttpPost("upload-batch")]
    public async Task<ActionResult<ApiResponse<FileUploadResult>>> UploadFiles(
        [FromForm] List<IFormFile> files,
        [FromForm] FileUploadOptions options)
    {
        try
        {
            if (files == null || !files.Any())
            {
                return BadRequest(new ApiResponse<FileUploadResult>
                {
                    Success = false,
                    Message = "No files provided"
                });
            }

            var uploadResult = await _fileStorageService.UploadFilesAsync(files, options);
            
            return Ok(new ApiResponse<FileUploadResult>
            {
                Success = true,
                Data = uploadResult,
                Message = $"Batch upload completed. {uploadResult.Metadata["SuccessfulUploads"]} successful, {uploadResult.Metadata["FailedUploads"]} failed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading batch files");
            return StatusCode(500, new ApiResponse<FileUploadResult>
            {
                Success = false,
                Message = "Error during batch file upload"
            });
        }
    }

    [HttpGet("download/{fileId}")]
    public async Task<ActionResult> DownloadFile(string fileId)
    {
        try
        {
            var downloadResult = await _fileStorageService.DownloadFileAsync(fileId);
            
            if (!downloadResult.Success)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = downloadResult.ErrorMessage ?? "File not found"
                });
            }

            return File(downloadResult.FileData, downloadResult.ContentType, downloadResult.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FileId}", fileId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error during file download"
            });
        }
    }

    [HttpGet("info/{fileId}")]
    public async Task<ActionResult<ApiResponse<DocHub.Application.Interfaces.FileInfo>>> GetFileInfo(string fileId)
    {
        try
        {
            var fileInfo = await _fileStorageService.GetFileInfoAsync(fileId);
            
            if (fileInfo == null)
            {
                return NotFound(new ApiResponse<DocHub.Application.Interfaces.FileInfo>
                {
                    Success = false,
                    Message = "File not found"
                });
            }

            return Ok(new ApiResponse<DocHub.Application.Interfaces.FileInfo>
            {
                Success = true,
                Data = fileInfo,
                Message = "File information retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info for {FileId}", fileId);
            return StatusCode(500, new ApiResponse<DocHub.Application.Interfaces.FileInfo>
            {
                Success = false,
                Message = "Error retrieving file information"
            });
        }
    }

    [HttpDelete("{fileId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteFile(string fileId)
    {
        try
        {
            var result = await _fileStorageService.DeleteFileAsync(fileId);
            
            if (result)
            {
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "File deleted successfully"
                });
            }
            else
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "File not found or could not be deleted"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId}", fileId);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Error during file deletion"
            });
        }
    }

    [HttpPost("search")]
    public async Task<ActionResult<ApiResponse<FileSearchResult>>> SearchFiles(
        [FromBody] FileSearchOptions options)
    {
        try
        {
            var searchResult = await _fileStorageService.SearchFilesAsync(options);
            
            return Ok(new ApiResponse<FileSearchResult>
            {
                Success = true,
                Data = searchResult,
                Message = $"Found {searchResult.TotalCount} files"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching files");
            return StatusCode(500, new ApiResponse<FileSearchResult>
            {
                Success = false,
                Message = "Error during file search"
            });
        }
    }

    #endregion

    #region File Processing

    [HttpPost("process/{fileId}")]
    public async Task<ActionResult<ApiResponse<FileProcessingResult>>> ProcessFile(
        string fileId,
        [FromBody] FileProcessingOptions options)
    {
        try
        {
            var processingResult = await _fileStorageService.ProcessFileAsync(fileId, options);
            
            if (processingResult.Success)
            {
                return Ok(new ApiResponse<FileProcessingResult>
                {
                    Success = true,
                    Data = processingResult,
                    Message = "File processed successfully"
                });
            }
            else
            {
                return BadRequest(new ApiResponse<FileProcessingResult>
                {
                    Success = false,
                    Data = processingResult,
                    Message = processingResult.ErrorMessage ?? "File processing failed"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file {FileId}", fileId);
            return StatusCode(500, new ApiResponse<FileProcessingResult>
            {
                Success = false,
                Message = "Error during file processing"
            });
        }
    }

    #endregion

    #region Excel Processing

    [HttpPost("excel/process")]
    public async Task<ActionResult<ApiResponse<ExcelProcessingResult>>> ProcessExcelFile(
        [FromForm] IFormFile file,
        [FromForm] ExcelProcessingOptions options)
    {
        try
        {
            // Input validation
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<ExcelProcessingResult>
                {
                    Success = false,
                    Message = "No Excel file provided"
                });
            }

            // Validate file size (100MB limit from configuration)
            var maxFileSizeBytes = 100 * 1024 * 1024; // 100MB
            if (file.Length > maxFileSizeBytes)
            {
                return BadRequest(new ApiResponse<ExcelProcessingResult>
                {
                    Success = false,
                    Message = $"File size exceeds maximum allowed size of 100MB. Current size: {file.Length / (1024 * 1024)}MB"
                });
            }

            // Validate file extension
            var allowedExtensions = new[] { ".xlsx", ".xls", ".csv" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new ApiResponse<ExcelProcessingResult>
                {
                    Success = false,
                    Message = $"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}"
                });
            }

            // Process the Excel file using the file name and options
            var processingResult = await _excelService.ProcessExcelDataAsync(file.FileName, options);
            
            return Ok(new ApiResponse<ExcelProcessingResult>
            {
                Success = true,
                Data = processingResult,
                Message = "Excel file processed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Excel file {FileName}", file?.FileName);
            return StatusCode(500, new ApiResponse<ExcelProcessingResult>
            {
                Success = false,
                Message = "Error during Excel file processing"
            });
        }
    }

    [HttpGet("excel/history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExcelProcessingHistoryDto>>>> GetExcelProcessingHistory()
    {
        try
        {
            var history = await _excelService.GetProcessingHistoryAsync();
            
            return Ok(new ApiResponse<IEnumerable<ExcelProcessingHistoryDto>>
            {
                Success = true,
                Data = history,
                Message = "Processing history retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Excel processing history");
            return StatusCode(500, new ApiResponse<IEnumerable<ExcelProcessingHistoryDto>>
            {
                Success = false,
                Message = "Error retrieving processing history"
            });
        }
    }

    [HttpGet("excel/stats")]
    public async Task<ActionResult<ApiResponse<ExcelProcessingStatsDto>>> GetExcelProcessingStats()
    {
        try
        {
            var stats = await _excelService.GetProcessingStatsAsync();
            
            return Ok(new ApiResponse<ExcelProcessingStatsDto>
            {
                Success = true,
                Data = stats,
                Message = "Processing stats retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Excel processing stats");
            return StatusCode(500, new ApiResponse<ExcelProcessingStatsDto>
            {
                Success = false,
                Message = "Error retrieving processing stats"
            });
        }
    }

    [HttpPost("excel/export")]
    public async Task<ActionResult> ExportEmployeeData(
        [FromBody] EmployeeExportRequest request)
    {
        try
        {
            var exportData = await _excelService.ExportEmployeeDataAsync(request.EmployeeIds, request.Fields);
            
            return File(exportData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "employee_export.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting employee data");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error during employee data export"
            });
        }
    }

    #endregion
}

public class EmployeeExportRequest
{
    public List<string> EmployeeIds { get; set; } = new();
    public List<string> Fields { get; set; } = new();
}
