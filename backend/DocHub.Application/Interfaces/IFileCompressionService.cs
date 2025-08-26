namespace DocHub.Application.Interfaces;

public interface IFileCompressionService
{
    Task<byte[]> CompressFileAsync(byte[] fileData, string fileName, FileCompressionLevel level = FileCompressionLevel.Optimal);
    Task<byte[]> CompressMultipleFilesAsync(Dictionary<string, byte[]> files, FileCompressionLevel level = FileCompressionLevel.Optimal);
    Task<byte[]> DecompressFileAsync(byte[] compressedData, string fileName);
    Task<Dictionary<string, byte[]>> DecompressArchiveAsync(byte[] archiveData);
    Task<bool> IsCompressedFileAsync(byte[] fileData);
    Task<long> GetCompressedSizeAsync(byte[] originalData, FileCompressionLevel level = FileCompressionLevel.Optimal);
}

public enum FileCompressionLevel
{
    NoCompression = 0,
    Fastest = 1,
    Optimal = 2,
    SmallestSize = 3
}
