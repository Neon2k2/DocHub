using DocHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using Path = System.IO.Path;
using File = System.IO.File;

namespace DocHub.Infrastructure.Services;

public class TextExtractionService : ITextExtractionService
{
    private readonly ILogger<TextExtractionService> _logger;
    private readonly IConfiguration _configuration;

    public TextExtractionService(ILogger<TextExtractionService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<TextExtractionResult> ExtractTextAsync(string filePath, TextExtractionOptions? options = null)
    {
        var result = new TextExtractionResult
        {
            FilePath = filePath,
            Success = false,
            ExtractedAt = DateTime.UtcNow
        };

        try
        {
            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            var startTime = DateTime.UtcNow;

            switch (fileExtension)
            {
                case ".txt":
                case ".csv":
                    result = await ExtractFromTextFileAsync(filePath, options);
                    break;
                case ".pdf":
                    result = await ExtractFromPdfAsync(filePath, options);
                    break;
                case ".doc":
                case ".docx":
                    result = await ExtractFromWordDocumentAsync(filePath, options);
                    break;
                case ".xls":
                case ".xlsx":
                    result = await ExtractFromExcelAsync(filePath, options);
                    break;
                case ".rtf":
                    result = await ExtractFromRtfAsync(filePath, options);
                    break;
                case ".html":
                case ".htm":
                    result = await ExtractFromHtmlAsync(filePath, options);
                    break;
                case ".xml":
                    result = await ExtractFromXmlAsync(filePath, options);
                    break;
                case ".json":
                    result = await ExtractFromJsonAsync(filePath, options);
                    break;
                default:
                    result.ErrorMessage = $"Text extraction not supported for file type: {fileExtension}";
                    _logger.LogWarning("Text extraction attempted for unsupported file type: {FileType}", fileExtension);
                    break;
            }

            if (result.Success)
            {
                result.ProcessingDuration = DateTime.UtcNow - startTime;
                result.WordCount = CountWords(result.ExtractedText);
                result.CharacterCount = result.ExtractedText?.Length ?? 0;
                result.LineCount = CountLines(result.ExtractedText);

                _logger.LogInformation("Text extracted successfully from {FilePath} in {Duration}",
                    filePath, result.ProcessingDuration);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from file {FilePath}", filePath);
            result.ErrorMessage = ex.Message;
            result.Success = false;
        }

        return result;
    }

    public async Task<TextExtractionResult> ExtractTextFromBytesAsync(byte[] fileData, string fileExtension, TextExtractionOptions? options = null)
    {
        var result = new TextExtractionResult
        {
            Success = false,
            ExtractedAt = DateTime.UtcNow
        };

        try
        {
            // Create temporary file
            var tempPath = Path.GetTempFileName();
            await File.WriteAllBytesAsync(tempPath, fileData);

            try
            {
                result = await ExtractTextAsync(tempPath, options);
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from bytes");
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private async Task<TextExtractionResult> ExtractFromTextFileAsync(string filePath, TextExtractionOptions? options)
    {
        try
        {
            var encoding = options?.Encoding ?? Encoding.UTF8;
            var text = await File.ReadAllTextAsync(filePath, encoding);

            return new TextExtractionResult
            {
                FilePath = filePath,
                ExtractedText = text,
                Success = true,
                ExtractedAt = DateTime.UtcNow,
                Encoding = encoding.WebName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from text file {FilePath}", filePath);
            return new TextExtractionResult
            {
                FilePath = filePath,
                Success = false,
                ErrorMessage = ex.Message,
                ExtractedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<TextExtractionResult> ExtractFromPdfAsync(string filePath, TextExtractionOptions? options)
    {
        try
        {
            // Use iText7 for PDF text extraction
            using var pdfReader = new PdfReader(filePath);
            using var pdfDoc = new PdfDocument(pdfReader);

            var text = new StringBuilder();
            var listener = new LocationTextExtractionStrategy();

            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                var pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), listener);
                text.AppendLine(pageText);
            }

            return new TextExtractionResult
            {
                FilePath = filePath,
                ExtractedText = text.ToString(),
                Success = true,
                ExtractedAt = DateTime.UtcNow,
                Encoding = "UTF-8",

                IsPlaceholder = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from PDF {FilePath}", filePath);
            return new TextExtractionResult
            {
                FilePath = filePath,
                Success = false,
                ErrorMessage = ex.Message,
                ExtractedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<TextExtractionResult> ExtractFromWordDocumentAsync(string filePath, TextExtractionOptions? options)
    {
        try
        {
            // Use DocumentFormat.OpenXml for Word document text extraction
            using var document = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(filePath, false);
            var body = document.MainDocumentPart?.Document.Body;

            if (body == null)
            {
                throw new InvalidOperationException("Document body is null");
            }

            var text = body.InnerText;

            return new TextExtractionResult
            {
                FilePath = filePath,
                ExtractedText = text,
                Success = true,
                ExtractedAt = DateTime.UtcNow,
                Encoding = "UTF-8",
                IsPlaceholder = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from Word document {FilePath}", filePath);
            return new TextExtractionResult
            {
                FilePath = filePath,
                Success = false,
                ErrorMessage = ex.Message,
                ExtractedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<TextExtractionResult> ExtractFromExcelAsync(string filePath, TextExtractionOptions? options)
    {
        try
        {
            // Use EPPlus for Excel text extraction
            using var package = new OfficeOpenXml.ExcelPackage(new System.IO.FileInfo(filePath));
            var text = new StringBuilder();

            foreach (var worksheet in package.Workbook.Worksheets)
            {
                text.AppendLine($"Worksheet: {worksheet.Name}");

                var dimension = worksheet.Dimension;
                if (dimension != null)
                {
                    for (int row = dimension.Start.Row; row <= dimension.End.Row; row++)
                    {
                        var rowText = new List<string>();
                        for (int col = dimension.Start.Column; col <= dimension.End.Column; col++)
                        {
                            var cellValue = worksheet.Cells[row, col].Value?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(cellValue))
                            {
                                rowText.Add(cellValue);
                            }
                        }
                        if (rowText.Any())
                        {
                            text.AppendLine(string.Join("\t", rowText));
                        }
                    }
                }
                text.AppendLine();
            }

            return new TextExtractionResult
            {
                FilePath = filePath,
                ExtractedText = text.ToString(),
                Success = true,
                ExtractedAt = DateTime.UtcNow,
                Encoding = "UTF-8",
                IsPlaceholder = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from Excel file {FilePath}", filePath);
            return new TextExtractionResult
            {
                FilePath = filePath,
                Success = false,
                ErrorMessage = ex.Message,
                ExtractedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<TextExtractionResult> ExtractFromRtfAsync(string filePath, TextExtractionOptions? options)
    {
        try
        {
            var rtfContent = await File.ReadAllTextAsync(filePath);
            var plainText = ConvertRtfToPlainText(rtfContent);

            return new TextExtractionResult
            {
                FilePath = filePath,
                ExtractedText = plainText,
                Success = true,
                ExtractedAt = DateTime.UtcNow,
                Encoding = "UTF-8"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from RTF file {FilePath}", filePath);
            return new TextExtractionResult
            {
                FilePath = filePath,
                Success = false,
                ErrorMessage = ex.Message,
                ExtractedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<TextExtractionResult> ExtractFromHtmlAsync(string filePath, TextExtractionOptions? options)
    {
        try
        {
            var htmlContent = await File.ReadAllTextAsync(filePath);
            var plainText = ConvertHtmlToPlainText(htmlContent);

            return new TextExtractionResult
            {
                FilePath = filePath,
                ExtractedText = plainText,
                Success = true,
                ExtractedAt = DateTime.UtcNow,
                Encoding = "UTF-8"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from HTML file {FilePath}", filePath);
            return new TextExtractionResult
            {
                FilePath = filePath,
                Success = false,
                ErrorMessage = ex.Message,
                ExtractedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<TextExtractionResult> ExtractFromXmlAsync(string filePath, TextExtractionOptions? options)
    {
        try
        {
            var xmlContent = await File.ReadAllTextAsync(filePath);
            var plainText = ConvertXmlToPlainText(xmlContent);

            return new TextExtractionResult
            {
                FilePath = filePath,
                ExtractedText = plainText,
                Success = true,
                ExtractedAt = DateTime.UtcNow,
                Encoding = "UTF-8"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from XML file {FilePath}", filePath);
            return new TextExtractionResult
            {
                FilePath = filePath,
                Success = false,
                ErrorMessage = ex.Message,
                ExtractedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<TextExtractionResult> ExtractFromJsonAsync(string filePath, TextExtractionOptions? options)
    {
        try
        {
            var jsonContent = await File.ReadAllTextAsync(filePath);
            var plainText = ConvertJsonToPlainText(jsonContent);

            return new TextExtractionResult
            {
                FilePath = filePath,
                ExtractedText = plainText,
                Success = true,
                ExtractedAt = DateTime.UtcNow,
                Encoding = "UTF-8"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from JSON file {FilePath}", filePath);
            return new TextExtractionResult
            {
                FilePath = filePath,
                Success = false,
                ErrorMessage = ex.Message,
                ExtractedAt = DateTime.UtcNow
            };
        }
    }

    private string ConvertRtfToPlainText(string rtfContent)
    {
        try
        {
            // Basic RTF to plain text conversion
            // Remove RTF control words and keep only text content
            var plainText = Regex.Replace(rtfContent, @"\\[a-z]+\d*", "");
            plainText = Regex.Replace(plainText, @"\{|\}", "");
            plainText = Regex.Replace(plainText, @"\\'[0-9a-f]{2}", "");
            plainText = Regex.Replace(plainText, @"\\\*\\", "");
            plainText = Regex.Replace(plainText, @"\\\s", " ");

            // Clean up extra whitespace
            plainText = Regex.Replace(plainText, @"\s+", " ");
            plainText = plainText.Trim();

            return plainText;
        }
        catch
        {
            return "Error converting RTF to plain text";
        }
    }

    private string ConvertHtmlToPlainText(string htmlContent)
    {
        try
        {
            // Basic HTML to plain text conversion
            var plainText = Regex.Replace(htmlContent, @"<[^>]+>", "");
            plainText = Regex.Replace(plainText, @"&nbsp;", " ");
            plainText = Regex.Replace(plainText, @"&amp;", "&");
            plainText = Regex.Replace(plainText, @"&lt;", "<");
            plainText = Regex.Replace(plainText, @"&gt;", ">");
            plainText = Regex.Replace(plainText, @"&quot;", "\"");
            plainText = Regex.Replace(plainText, @"&#39;", "'");

            // Clean up extra whitespace
            plainText = Regex.Replace(plainText, @"\s+", " ");
            plainText = plainText.Trim();

            return plainText;
        }
        catch
        {
            return "Error converting HTML to plain text";
        }
    }

    private string ConvertXmlToPlainText(string xmlContent)
    {
        try
        {
            // Basic XML to plain text conversion
            var plainText = Regex.Replace(xmlContent, @"<[^>]+>", "");
            plainText = Regex.Replace(plainText, @"&nbsp;", " ");
            plainText = Regex.Replace(plainText, @"&amp;", "&");
            plainText = Regex.Replace(plainText, @"&lt;", "<");
            plainText = Regex.Replace(plainText, @"&gt;", ">");
            plainText = Regex.Replace(plainText, @"&quot;", "\"");
            plainText = Regex.Replace(plainText, @"&#39;", "'");

            // Clean up extra whitespace
            plainText = Regex.Replace(plainText, @"\s+", " ");
            plainText = plainText.Trim();

            return plainText;
        }
        catch
        {
            return "Error converting XML to plain text";
        }
    }

    private string ConvertJsonToPlainText(string jsonContent)
    {
        try
        {
            // Basic JSON to plain text conversion
            var plainText = jsonContent;

            // Remove quotes around property names and values
            plainText = Regex.Replace(plainText, @"""([^""]+)""\s*:", "$1: ");
            plainText = Regex.Replace(plainText, @":\s*""([^""]+)""", ": $1");

            // Remove brackets and braces
            plainText = Regex.Replace(plainText, @"[\[\]{}]", "");

            // Clean up extra whitespace
            plainText = Regex.Replace(plainText, @"\s+", " ");
            plainText = plainText.Trim();

            return plainText;
        }
        catch
        {
            return "Error converting JSON to plain text";
        }
    }

    private int CountWords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private int CountLines(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
