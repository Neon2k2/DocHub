using DocHub.Application.Interfaces;
using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace DocHub.Infrastructure.Services;

public class FileCompressionService : IFileCompressionService
{
    private readonly ILogger<FileCompressionService> _logger;

    public FileCompressionService(ILogger<FileCompressionService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> CompressFileAsync(byte[] fileData, string fileName, FileCompressionLevel level = FileCompressionLevel.Optimal)
    {
        try
        {
            using var outputStream = new MemoryStream();
            using var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true);
            
            var entry = archive.CreateEntry(fileName, GetCompressionLevel(level));
            using var entryStream = entry.Open();
            await entryStream.WriteAsync(fileData, 0, fileData.Length);
            
            outputStream.Position = 0;
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing file {FileName}", fileName);
            throw;
        }
    }

    public async Task<byte[]> CompressMultipleFilesAsync(Dictionary<string, byte[]> files, FileCompressionLevel level = FileCompressionLevel.Optimal)
    {
        try
        {
            using var outputStream = new MemoryStream();
            using var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true);
            
            foreach (var file in files)
            {
                var entry = archive.CreateEntry(file.Key, GetCompressionLevel(level));
                using var entryStream = entry.Open();
                await entryStream.WriteAsync(file.Value, 0, file.Value.Length);
            }
            
            outputStream.Position = 0;
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing multiple files");
            throw;
        }
    }

    public async Task<byte[]> DecompressFileAsync(byte[] compressedData, string fileName)
    {
        try
        {
            using var inputStream = new MemoryStream(compressedData);
            using var archive = new ZipArchive(inputStream, ZipArchiveMode.Read);
            
            var entry = archive.GetEntry(fileName);
            if (entry == null)
            {
                throw new FileNotFoundException($"File {fileName} not found in archive");
            }
            
            using var entryStream = entry.Open();
            using var outputStream = new MemoryStream();
            await entryStream.CopyToAsync(outputStream);
            
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decompressing file {FileName}", fileName);
            throw;
        }
    }

    public async Task<Dictionary<string, byte[]>> DecompressArchiveAsync(byte[] archiveData)
    {
        try
        {
            var files = new Dictionary<string, byte[]>();
            using var inputStream = new MemoryStream(archiveData);
            using var archive = new ZipArchive(inputStream, ZipArchiveMode.Read);
            
            foreach (var entry in archive.Entries)
            {
                using var entryStream = entry.Open();
                using var outputStream = new MemoryStream();
                await entryStream.CopyToAsync(outputStream);
                files[entry.FullName] = outputStream.ToArray();
            }
            
            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decompressing archive");
            throw;
        }
    }

    public Task<bool> IsCompressedFileAsync(byte[] fileData)
    {
        try
        {
            // Check for ZIP file signature
            if (fileData.Length >= 4)
            {
                var signature = BitConverter.ToUInt32(fileData, 0);
                if (signature == 0x04034B50) // ZIP file signature
                {
                    return Task.FromResult(true);
                }
            }
            
            // Check for GZIP signature
            if (fileData.Length >= 2)
            {
                var gzipSignature = BitConverter.ToUInt16(fileData, 0);
                if (gzipSignature == 0x8B1F) // GZIP signature
                {
                    return Task.FromResult(true);
                }
            }
            
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<long> GetCompressedSizeAsync(byte[] originalData, FileCompressionLevel level = FileCompressionLevel.Optimal)
    {
        try
        {
            var compressedData = await CompressFileAsync(originalData, "temp", level);
            return compressedData.Length;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating compressed size");
            return originalData.Length; // Return original size if compression fails
        }
    }

    private System.IO.Compression.CompressionLevel GetCompressionLevel(FileCompressionLevel level)
    {
        return level switch
        {
            FileCompressionLevel.NoCompression => System.IO.Compression.CompressionLevel.NoCompression,
            FileCompressionLevel.Fastest => System.IO.Compression.CompressionLevel.Fastest,
            FileCompressionLevel.Optimal => System.IO.Compression.CompressionLevel.Optimal,
            FileCompressionLevel.SmallestSize => System.IO.Compression.CompressionLevel.SmallestSize,
            _ => System.IO.Compression.CompressionLevel.Optimal
        };
    }
}
