using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using Microsoft.Extensions.Logging;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using System.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Interactive;
using Syncfusion.Pdf.Security;
using Syncfusion.Pdf.Parsing;
using System.Text;
using System.Text.RegularExpressions;

namespace DocHub.Infrastructure.Services.DocumentProcessing;

public class SynfusionDocumentService : IDocumentProcessingService
{
    private readonly ILogger<SynfusionDocumentService> _logger;
    private readonly IFileStorageService _fileStorageService;

    public SynfusionDocumentService(
        ILogger<SynfusionDocumentService> logger,
        IFileStorageService fileStorageService)
    {
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    public async Task<string> GenerateLetterFromTemplateAsync(
        string templatePath,
        Dictionary<string, object> fieldValues,
        string? digitalSignaturePath = null)
    {
        try
        {
            _logger.LogInformation("Generating letter from template: {TemplatePath}", templatePath);

            // Load the Word template
            using var wordDocument = new WordDocument(templatePath);

            // Replace placeholders with actual values
            ReplacePlaceholders(wordDocument, fieldValues);

            // Add digital signature if provided
            if (!string.IsNullOrEmpty(digitalSignaturePath))
            {
                await AddDigitalSignatureAsync(wordDocument, digitalSignaturePath);
            }

            // Generate unique filename
            var outputFileName = $"Letter_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.docx";
            var outputPath = Path.Combine(Path.GetTempPath(), outputFileName);

            // Save the generated document
            using var fileStream = new FileStream(outputPath, FileMode.Create);
            wordDocument.Save(fileStream, FormatType.Docx);

            _logger.LogInformation("Letter generated successfully: {OutputPath}", outputPath);
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating letter from template: {TemplatePath}", templatePath);
            throw;
        }
    }

    public async Task<string> ConvertToPdfAsync(string documentPath)
    {
        try
        {
            _logger.LogInformation("Converting document to PDF: {DocumentPath}", documentPath);

            // Load the Word document
            using var wordDocument = new WordDocument(documentPath);

            // Generate unique filename
            var outputFileName = $"Document_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.pdf";
            var outputPath = Path.Combine(Path.GetTempPath(), outputFileName);

            // Convert to PDF using DocIORenderer
            using var fileStream = new FileStream(outputPath, FileMode.Create);
            using var docIORenderer = new DocIORenderer();
            using var pdfDoc = docIORenderer.ConvertToPDF(wordDocument);
            await Task.Run(() => pdfDoc.Save(fileStream));

            _logger.LogInformation("Document converted to PDF successfully: {OutputPath}", outputPath);
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting document to PDF: {DocumentPath}", documentPath);
            throw;
        }
    }

    public async Task<Dictionary<string, string>> ExtractTemplateFieldsAsync(string templatePath)
    {
        try
        {
            _logger.LogInformation("Extracting template fields from: {TemplatePath}", templatePath);

            using var wordDocument = new WordDocument(templatePath);
            var fields = new Dictionary<string, string>();

            await Task.Run(() =>
            {
                // Extract content controls (placeholders)
                foreach (IWSection section in wordDocument.Sections)
                {
                    foreach (IWParagraph paragraph in section.Paragraphs)
                    {
                        foreach (Entity entity in paragraph.ChildEntities)
                        {
                            if (entity is WTextRange textRange)
                            {
                                var text = textRange.Text;
                                if (text.StartsWith("{{") && text.EndsWith("}}"))
                                {
                                    var fieldName = text.Trim('{', '}');
                                    fields[fieldName] = text;
                                }
                            }
                        }
                    }
                }

                // Extract bookmarks
                foreach (Bookmark bookmark in wordDocument.Bookmarks)
                {
                    fields[bookmark.Name] = $"{{{{{bookmark.Name}}}}}";
                }
            });

            _logger.LogInformation("Extracted {FieldCount} template fields", fields.Count);
            return fields;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting template fields from: {TemplatePath}", templatePath);
            throw;
        }
    }

    public async Task<string> CreateTemplateFromScratchAsync(
        string templateName,
        Dictionary<string, string> fieldDefinitions,
        string? description = null)
    {
        try
        {
            _logger.LogInformation("Creating new template: {TemplateName}", templateName);

            var wordDocument = new WordDocument();

            await Task.Run(() =>
            {
                // Add title
                var titleParagraph = wordDocument.LastSection.AddParagraph();
                titleParagraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;
                var titleText = titleParagraph.AppendText(templateName);
                titleText.CharacterFormat.FontSize = 18;
                titleText.CharacterFormat.Bold = true;

                // Add description if provided
                if (!string.IsNullOrEmpty(description))
                {
                    var descParagraph = wordDocument.LastSection.AddParagraph();
                    descParagraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;
                    var descText = descParagraph.AppendText(description);
                    descText.CharacterFormat.FontSize = 12;
                    descText.CharacterFormat.Italic = true;
                }

                // Add fields
                foreach (var field in fieldDefinitions)
                {
                    var fieldParagraph = wordDocument.LastSection.AddParagraph();
                    fieldParagraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Left;

                    var labelText = fieldParagraph.AppendText($"{field.Key}: ");
                    labelText.CharacterFormat.Bold = true;

                    var placeholderText = fieldParagraph.AppendText($"{{{{{field.Value}}}}}");
                    placeholderText.CharacterFormat.FontSize = 12;
                    placeholderText.CharacterFormat.TextColor = Syncfusion.Drawing.Color.Gray;
                }
            });

            // Generate unique filename
            var outputFileName = $"{templateName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.docx";
            var outputPath = Path.Combine(Path.GetTempPath(), outputFileName);

            // Save the template
            using var fileStream = new FileStream(outputPath, FileMode.Create);
            await Task.Run(() => wordDocument.Save(fileStream, FormatType.Docx));

            _logger.LogInformation("Template created successfully: {OutputPath}", outputPath);
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template: {TemplateName}", templateName);
            throw;
        }
    }

    public async Task<bool> ValidateTemplateAsync(string templatePath)
    {
        try
        {
            _logger.LogInformation("Validating template: {TemplatePath}", templatePath);

            using var wordDocument = new WordDocument(templatePath);

            // Check if document has content
            if (wordDocument.Sections.Count == 0)
            {
                _logger.LogWarning("Template has no sections: {TemplatePath}", templatePath);
                return false;
            }

            // Check if document has placeholders
            var fields = await ExtractTemplateFieldsAsync(templatePath);
            if (fields.Count == 0)
            {
                _logger.LogWarning("Template has no placeholders: {TemplatePath}", templatePath);
                return false;
            }

            _logger.LogInformation("Template validation successful: {TemplatePath}", templatePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating template: {TemplatePath}", templatePath);
            return false;
        }
    }

    public async Task<string> MergeDocumentsAsync(List<string> documentPaths)
    {
        try
        {
            _logger.LogInformation("Merging {Count} documents", documentPaths.Count);

            if (documentPaths.Count == 0)
                throw new ArgumentException("No documents provided for merging");

            if (documentPaths.Count == 1)
                return documentPaths[0];

            var mergedDocument = new WordDocument(documentPaths[0]);

            await Task.Run(() =>
            {
                // Merge additional documents
                for (int i = 1; i < documentPaths.Count; i++)
                {
                    using var sourceDocument = new WordDocument(documentPaths[i]);
                    mergedDocument.ImportContent(sourceDocument, ImportOptions.UseDestinationStyles);
                }
            });

            // Generate unique filename
            var outputFileName = $"Merged_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.docx";
            var outputPath = Path.Combine(Path.GetTempPath(), outputFileName);

            // Save the merged document
            using var fileStream = new FileStream(outputPath, FileMode.Create);
            await Task.Run(() => mergedDocument.Save(fileStream, FormatType.Docx));

            _logger.LogInformation("Documents merged successfully: {OutputPath}", outputPath);
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging documents");
            throw;
        }
    }

    public async Task<byte[]> ExtractTextFromPdfAsync(string pdfPath)
    {
        try
        {
            _logger.LogInformation("Extracting text from PDF: {PdfPath}", pdfPath);

            using var stream = new FileStream(pdfPath, FileMode.Open, FileAccess.Read);
            using var pdfDocument = new PdfLoadedDocument(stream);
            var textBuilder = new StringBuilder();

            // Extract text from each page
            await Task.Run(() =>
            {
                for (int i = 0; i < pdfDocument.Pages.Count; i++)
                {
                    var page = pdfDocument.Pages[i] as PdfLoadedPage;
                    if (page != null)
                    {
                        textBuilder.AppendLine(page.ExtractText());
                    }
                }
            });

            var extractedText = textBuilder.ToString();
            _logger.LogInformation("Text extracted successfully from PDF: {PdfPath}", pdfPath);

            return Encoding.UTF8.GetBytes(extractedText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from PDF: {PdfPath}", pdfPath);
            throw;
        }
    }

    private void ReplacePlaceholders(WordDocument document, Dictionary<string, object> fieldValues)
    {
        foreach (var field in fieldValues)
        {
            var placeholder = $"{{{{{field.Key}}}}}";
            var value = field.Value?.ToString() ?? string.Empty;

            // Replace text in the document
            document.Replace(placeholder, value, false, true);
        }
    }

    private async Task AddDigitalSignatureAsync(WordDocument document, string signaturePath)
    {
        try
        {
            await Task.Run(() =>
            {
                // Add signature image to the document
                var lastSection = document.LastSection;
                var signatureParagraph = lastSection.AddParagraph();
                signatureParagraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Right;

                // Add signature image
                if (File.Exists(signaturePath))
                {
                    using var signatureStream = File.OpenRead(signaturePath);
                    var signatureImage = signatureParagraph.AppendPicture(signatureStream);
                    signatureImage.Height = 60;
                    signatureImage.Width = 120;
                }

                // Add signature date
                var dateParagraph = lastSection.AddParagraph();
                dateParagraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Right;
                var dateText = dateParagraph.AppendText($"Date: {DateTime.UtcNow:dd/MM/yyyy}");
                dateText.CharacterFormat.FontSize = 10;
                dateText.CharacterFormat.Italic = true;

                _logger.LogInformation("Digital signature added successfully");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding digital signature");
            throw;
        }
    }
}
