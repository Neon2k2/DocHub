using System.Text;

namespace DocHub.Application.Interfaces;

public interface ITextExtractionService
{
    Task<TextExtractionResult> ExtractTextAsync(string filePath, TextExtractionOptions? options = null);
    Task<TextExtractionResult> ExtractTextFromBytesAsync(byte[] fileData, string fileExtension, TextExtractionOptions? options = null);
}

public class TextExtractionOptions
{
    public Encoding? Encoding { get; set; }
    public bool ExtractMetadata { get; set; } = false;
    public bool PreserveFormatting { get; set; } = false;
    public int? MaxLength { get; set; }
    public List<string>? ExcludePatterns { get; set; }
    public bool IncludeHiddenContent { get; set; } = false;
}

public class TextExtractionResult
{
    public string FilePath { get; set; } = string.Empty;
    public string? ExtractedText { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExtractedAt { get; set; }
    public TimeSpan ProcessingDuration { get; set; }
    public string? Encoding { get; set; }
    public int WordCount { get; set; }
    public int CharacterCount { get; set; }
    public int LineCount { get; set; }
    public bool IsPlaceholder { get; set; } = false;
    public Dictionary<string, object>? Metadata { get; set; }
}
