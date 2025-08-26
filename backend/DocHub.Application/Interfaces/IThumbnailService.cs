namespace DocHub.Application.Interfaces;

public interface IThumbnailService
{
    Task<string> GenerateThumbnailAsync(string filePath, string fileId, ThumbnailOptions? options = null);
    Task<string> GenerateThumbnailFromBytesAsync(byte[] fileData, string fileId, string fileExtension, ThumbnailOptions? options = null);
    Task<bool> DeleteThumbnailAsync(string fileId);
}

public class ThumbnailOptions
{
    public int? MaxWidth { get; set; }
    public int? MaxHeight { get; set; }
    public int? Quality { get; set; }
    public string? Format { get; set; }
    public bool MaintainAspectRatio { get; set; } = true;
    public bool GenerateMultipleSizes { get; set; } = false;
    public List<ThumbnailSize>? CustomSizes { get; set; }
}

public class ThumbnailSize
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string? Suffix { get; set; }
    public int Quality { get; set; } = 80;
}
