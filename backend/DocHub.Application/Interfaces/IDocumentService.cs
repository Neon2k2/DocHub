using DocHub.Core.Entities;

namespace DocHub.Application.Interfaces
{
    public interface IDocumentService
    {
        Task<byte[]> GenerateLetterAsync(LetterTemplate template, Employee employee, Dictionary<string, object> data, DigitalSignature signature);
        Task<byte[]> GenerateLetterFromTemplateAsync(string templatePath, Dictionary<string, object> data);
        Task<string> SaveGeneratedLetterAsync(byte[] content, string fileName, string format = "PDF");
        Task<byte[]> ConvertToPdfAsync(byte[] documentContent, string sourceFormat);
        Task<Dictionary<string, object>> ExtractPlaceholdersAsync(string templatePath);
        Task<List<LetterTemplateField>> ExtractTemplateFieldsAsync(string templatePath);
        Task<bool> ValidateTemplateAsync(string templatePath);
        Task<string> GetTemplatePreviewAsync(string templatePath, Dictionary<string, object> sampleData);
        Task<byte[]> MergeDocumentsAsync(List<byte[]> documents);
        Task<byte[]> AddDigitalSignatureAsync(byte[] document, DigitalSignature signature, string position = "bottom-right");
        Task<string> GetDocumentInfoAsync(byte[] document);
        Task<bool> IsTemplateValidAsync(string templatePath);
        Task<List<string>> GetSupportedFormatsAsync();
        Task<byte[]> CompressDocumentAsync(byte[] document, int quality = 80);
        Task<byte[]> GenerateLetterPreviewAsync(LetterTemplate template, Employee employee, Dictionary<string, object> data, DigitalSignature signature);
    }
}
