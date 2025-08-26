using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using DocHub.Application.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using System.Text;

namespace DocHub.Infrastructure.Services.Document
{
    public class SyncfusionDocumentService : IDocumentService
    {
        private readonly ILogger<SyncfusionDocumentService> _logger;
        private readonly AppConfiguration _config;

        public SyncfusionDocumentService(ILogger<SyncfusionDocumentService> logger, AppConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<byte[]> GenerateLetterAsync(LetterTemplate template, Employee employee, Dictionary<string, object> data, DigitalSignature signature)
        {
            try
            {
                _logger.LogInformation("Generating letter for employee {EmployeeName} using template {TemplateName}", 
                    employee.FirstName + " " + employee.LastName, template.Name);

                // For now, generate a simple text document that can be converted to PDF later
                var content = new StringBuilder();
                content.AppendLine($"{template.LetterType} - {employee.FirstName} {employee.LastName}");
                content.AppendLine("=".PadRight(50, '='));
                content.AppendLine();
                content.AppendLine($"Employee ID: {employee.EmployeeId}");
                content.AppendLine($"Name: {employee.FirstName} {employee.LastName}");
                content.AppendLine($"Department: {employee.Department ?? "N/A"}");
                content.AppendLine($"Designation: {employee.Designation ?? "N/A"}");
                content.AppendLine($"Email: {employee.Email}");
                content.AppendLine();
                
                // Add template content with data replacement
                var templateContent = template.TemplateContent ?? "";
                var processedContent = ReplacePlaceholders(templateContent, employee, data);
                content.AppendLine(processedContent);
                
                // Add signature if available
                if (signature != null)
                {
                    content.AppendLine();
                    content.AppendLine("=".PadRight(50, '='));
                    content.AppendLine($"Signed by: {signature.AuthorityName}");
                    content.AppendLine($"Designation: {signature.AuthorityDesignation}");
                    content.AppendLine($"Date: {DateTime.Now:dd/MM/yyyy}");
                }
                
                var result = System.Text.Encoding.UTF8.GetBytes(content.ToString());
                _logger.LogInformation("Letter generated successfully for employee {EmployeeName}", 
                    employee.FirstName + " " + employee.LastName);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating letter from template");
                throw;
            }
        }

        public async Task<byte[]> GenerateLetterFromTemplateAsync(string templatePath, Dictionary<string, object> data)
        {
            try
            {
                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException($"Template not found: {templatePath}");
                }

                _logger.LogInformation("Generating document from template: {TemplatePath}", templatePath);

                // For now, read the template and replace placeholders
                var templateContent = await File.ReadAllTextAsync(templatePath);
                
                foreach (var item in data)
                {
                    templateContent = templateContent.Replace($"${{{item.Key}}}", item.Value?.ToString() ?? "");
                }

                var result = System.Text.Encoding.UTF8.GetBytes(templateContent);
                _logger.LogInformation("Document generated successfully from template: {TemplatePath}", templatePath);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating document from template: {TemplatePath}", templatePath);
                throw;
            }
        }

        public async Task<string> SaveGeneratedLetterAsync(byte[] content, string fileName, string format = "PDF")
        {
            try
            {
                var directory = Path.Combine("wwwroot", "generated");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                var filePath = Path.Combine(directory, $"{fileName}.{format.ToLower()}");
                await File.WriteAllBytesAsync(filePath, content);
                
                _logger.LogInformation("Generated letter saved to: {FilePath}", filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving generated letter");
                throw;
            }
        }

        public async Task<bool> ValidateTemplateAsync(string templatePath)
        {
            try
            {
                if (!File.Exists(templatePath))
                {
                    return false;
                }

                // Simple validation - check if file exists and has content
                var fileInfo = new FileInfo(templatePath);
                return fileInfo.Length > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating template: {TemplatePath}", templatePath);
                return false;
            }
        }

        private string ReplacePlaceholders(string content, Employee employee, Dictionary<string, object> data)
        {
            var result = content;
            
            // Replace employee placeholders
            result = result.Replace("${EmployeeName}", $"{employee.FirstName} {employee.LastName}");
            result = result.Replace("${EmployeeId}", employee.EmployeeId);
            result = result.Replace("${Department}", employee.Department ?? "");
            result = result.Replace("${Designation}", employee.Designation ?? "");
            result = result.Replace("${Email}", employee.Email);
            result = result.Replace("${PhoneNumber}", employee.PhoneNumber ?? "");
            
            // Replace custom data placeholders
            if (data != null)
            {
                foreach (var item in data)
                {
                    result = result.Replace($"${{{item.Key}}}", item.Value?.ToString() ?? "");
                }
            }
            
            // Replace date placeholders
            result = result.Replace("${CurrentDate}", DateTime.Now.ToString("dd/MM/yyyy"));
            result = result.Replace("${CurrentYear}", DateTime.Now.Year.ToString());
            
            return result;
        }

        public async Task<byte[]> ConvertToPdfAsync(byte[] documentContent, string sourceFormat)
        {
            try
            {
                _logger.LogInformation("Converting document from {SourceFormat} to PDF", sourceFormat);
                
                if (sourceFormat.ToLower() == "pdf")
                {
                    return documentContent; // Already PDF
                }
                
                // For now, return a simple PDF representation
                // In production, you would use Syncfusion to convert various formats
                var content = $"Converted from {sourceFormat} to PDF\n\nContent length: {documentContent.Length} bytes";
                return System.Text.Encoding.UTF8.GetBytes(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting document to PDF from format: {SourceFormat}", sourceFormat);
                throw;
            }
        }

        public async Task<Dictionary<string, object>> ExtractPlaceholdersAsync(string templatePath)
        {
            try
            {
                if (!File.Exists(templatePath))
                {
                    return new Dictionary<string, object>();
                }

                var placeholders = new Dictionary<string, object>();
                
                // Extract placeholders from template content
                var content = await File.ReadAllTextAsync(templatePath);
                var matches = Regex.Matches(content, @"\$\{([^}]+)\}");
                
                foreach (Match match in matches)
                {
                    var placeholder = match.Groups[1].Value;
                    if (!placeholders.ContainsKey(placeholder))
                    {
                        placeholders[placeholder] = $"Sample value for {placeholder}";
                    }
                }
                
                return placeholders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting placeholders from template: {TemplatePath}", templatePath);
                return new Dictionary<string, object>();
            }
        }

        public async Task<List<LetterTemplateField>> ExtractTemplateFieldsAsync(string templatePath)
        {
            try
            {
                var placeholders = await ExtractPlaceholdersAsync(templatePath);
                var fields = new List<LetterTemplateField>();
                
                foreach (var placeholder in placeholders)
                {
                    fields.Add(new LetterTemplateField
                    {
                        Id = Guid.NewGuid().ToString(),
                        FieldName = placeholder.Key,
                        DisplayName = placeholder.Key.Replace("_", " ").Replace("-", " "),
                        DataType = "Text",
                        IsRequired = true,
                        DefaultValue = "",
                        HelpText = $"Field for {placeholder.Key}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                
                return fields;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting template fields: {TemplatePath}", templatePath);
                return new List<LetterTemplateField>();
            }
        }

        public async Task<string> GetTemplatePreviewAsync(string templatePath, Dictionary<string, object> sampleData)
        {
            try
            {
                var documentBytes = await GenerateLetterFromTemplateAsync(templatePath, sampleData);
                var previewPath = await SaveGeneratedLetterAsync(documentBytes, $"preview_{Guid.NewGuid()}", "txt");
                
                return previewPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating template preview: {TemplatePath}", templatePath);
                throw;
            }
        }

        public async Task<byte[]> MergeDocumentsAsync(List<byte[]> documents)
        {
            try
            {
                if (documents == null || documents.Count == 0)
                    throw new ArgumentException("No documents provided for merging");

                // For now, return the first document
                // In production, you would use Syncfusion to merge PDFs
                _logger.LogInformation("Merging {Count} documents", documents.Count);
                return documents.First();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging documents");
                throw;
            }
        }

        public async Task<byte[]> AddDigitalSignatureAsync(byte[] document, DigitalSignature signature, string position = "bottom-right")
        {
            try
            {
                // For now, return the original document
                // In production, you would use Syncfusion to add digital signatures
                _logger.LogInformation("Digital signature would be added to document for authority: {AuthorityName}", signature.AuthorityName);
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding digital signature");
                throw;
            }
        }

        public async Task<string> GetDocumentInfoAsync(byte[] document)
        {
            try
            {
                var info = new
                {
                    FileSize = document.Length,
                    CreationDate = DateTime.Now,
                    Format = "Unknown"
                };
                
                return System.Text.Json.JsonSerializer.Serialize(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document info");
                return "{}";
            }
        }

        public async Task<bool> IsTemplateValidAsync(string templatePath)
        {
            return await ValidateTemplateAsync(templatePath);
        }

        public async Task<List<string>> GetSupportedFormatsAsync()
        {
            return await Task.FromResult(new List<string> { "PDF", "DOCX", "DOC", "TXT" });
        }

        public async Task<byte[]> CompressDocumentAsync(byte[] document, int quality = 80)
        {
            // Mock implementation - return original document
            return document;
        }

        public async Task<byte[]> GenerateLetterPreviewAsync(LetterTemplate template, Employee employee, Dictionary<string, object> data, DigitalSignature signature)
        {
            try
            {
                // Generate a preview version of the letter
                var content = new StringBuilder();
                content.AppendLine($"PREVIEW - Letter Type: {template.LetterType}");
                content.AppendLine($"Employee: {employee.FirstName} {employee.LastName}");
                content.AppendLine($"Employee ID: {employee.EmployeeId}");
                content.AppendLine($"Department: {employee.Department ?? ""}");
                content.AppendLine($"Designation: {employee.Designation ?? ""}");
                
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        content.AppendLine($"{item.Key}: {item.Value}");
                    }
                }
                
                if (signature != null)
                {
                    content.AppendLine($"\nSigned by: {signature.AuthorityName}");
                    content.AppendLine($"Designation: {signature.AuthorityDesignation}");
                    content.AppendLine($"Date: {signature.SignatureDate:dd/MM/yyyy}");
                }
                
                return System.Text.Encoding.UTF8.GetBytes(content.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating letter preview");
                throw;
            }
        }
    }
}
