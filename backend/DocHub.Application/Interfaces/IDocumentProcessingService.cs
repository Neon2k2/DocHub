namespace DocHub.Application.Interfaces;

public interface IDocumentProcessingService
{
    /// <summary>
    /// Generates a letter from a template by replacing placeholders with actual values
    /// </summary>
    /// <param name="templatePath">Path to the Word template file</param>
    /// <param name="fieldValues">Dictionary of field names and their values</param>
    /// <param name="digitalSignaturePath">Optional path to digital signature image</param>
    /// <returns>Path to the generated document</returns>
    Task<string> GenerateLetterFromTemplateAsync(
        string templatePath, 
        Dictionary<string, object> fieldValues, 
        string? digitalSignaturePath = null);

    /// <summary>
    /// Converts a Word document to PDF format
    /// </summary>
    /// <param name="documentPath">Path to the Word document</param>
    /// <returns>Path to the generated PDF</returns>
    Task<string> ConvertToPdfAsync(string documentPath);

    /// <summary>
    /// Extracts template fields (placeholders) from a Word template
    /// </summary>
    /// <param name="templatePath">Path to the template file</param>
    /// <returns>Dictionary of field names and their placeholder text</returns>
    Task<Dictionary<string, string>> ExtractTemplateFieldsAsync(string templatePath);

    /// <summary>
    /// Creates a new template from scratch with specified fields
    /// </summary>
    /// <param name="templateName">Name of the template</param>
    /// <param name="fieldDefinitions">Dictionary of field names and their descriptions</param>
    /// <param name="description">Optional template description</param>
    /// <returns>Path to the created template</returns>
    Task<string> CreateTemplateFromScratchAsync(
        string templateName, 
        Dictionary<string, string> fieldDefinitions,
        string? description = null);

    /// <summary>
    /// Validates a template file for correctness and completeness
    /// </summary>
    /// <param name="templatePath">Path to the template file</param>
    /// <returns>True if template is valid, false otherwise</returns>
    Task<bool> ValidateTemplateAsync(string templatePath);

    /// <summary>
    /// Merges multiple Word documents into a single document
    /// </summary>
    /// <param name="documentPaths">List of document paths to merge</param>
    /// <returns>Path to the merged document</returns>
    Task<string> MergeDocumentsAsync(List<string> documentPaths);

    /// <summary>
    /// Extracts text content from a PDF file
    /// </summary>
    /// <param name="pdfPath">Path to the PDF file</param>
    /// <returns>Extracted text as byte array</returns>
    Task<byte[]> ExtractTextFromPdfAsync(string pdfPath);
}
